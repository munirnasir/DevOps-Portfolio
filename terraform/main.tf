data "azurerm_client_config" "current" {}

# Short random suffix for globally-unique resource names (ACR, Key Vault, Postgres).
resource "random_string" "suffix" {
  length  = 5
  upper   = false
  special = false
  numeric = true
}

# Generated secrets — never committed, stored in Key Vault.
resource "random_password" "postgres_admin" {
  length           = 24
  special          = true
  override_special = "_%@"
}

resource "random_password" "jwt_signing_key" {
  length  = 64
  special = false
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.name_prefix}"
  location = var.location
  tags     = local.tags
}

module "network" {
  source = "./modules/network"

  name_prefix         = local.name_prefix
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  vnet_address_space  = var.vnet_address_space
  aks_subnet_prefix   = var.aks_subnet_prefix
  db_subnet_prefix    = var.db_subnet_prefix
  tags                = local.tags
}

module "acr" {
  source = "./modules/acr"

  name                = "${local.base_name}acr${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = var.acr_sku
  tags                = local.tags
}

module "postgres" {
  source = "./modules/postgres"

  name                = "${local.name_prefix}-pg-${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  delegated_subnet_id = module.network.db_subnet_id
  vnet_id             = module.network.vnet_id
  admin_username      = var.postgres_admin_username
  admin_password      = random_password.postgres_admin.result
  sku_name            = var.postgres_sku_name
  storage_mb          = var.postgres_storage_mb
  postgres_version    = var.postgres_version
  databases           = var.databases
  tags                = local.tags
}

module "aks" {
  source = "./modules/aks"

  name                = "${local.name_prefix}-aks"
  dns_prefix          = local.base_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  kubernetes_version  = var.kubernetes_version
  node_vm_size        = var.node_vm_size
  node_count          = var.node_count
  enable_auto_scaling = var.enable_auto_scaling
  node_min_count      = var.node_min_count
  node_max_count      = var.node_max_count
  vnet_subnet_id      = module.network.aks_subnet_id
  tags                = local.tags
}

module "keyvault" {
  source = "./modules/keyvault"

  name                = "${local.name_prefix}-kv-${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tenant_id           = data.azurerm_client_config.current.tenant_id

  # The deployer can manage secrets; the AKS kubelet identity can read them.
  admin_object_id  = data.azurerm_client_config.current.object_id
  reader_object_id = module.aks.kubelet_identity_object_id

  secrets = {
    postgres-admin-password = random_password.postgres_admin.result
    postgres-host           = module.postgres.fqdn
    jwt-signing-key         = random_password.jwt_signing_key.result
  }

  tags = local.tags
}

# Let AKS pull images from this ACR without registry credentials.
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                            = module.acr.id
  role_definition_name             = "AcrPull"
  principal_id                     = module.aks.kubelet_identity_object_id
  skip_service_principal_aad_check = true
}
