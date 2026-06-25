resource "azurerm_kubernetes_cluster" "this" {
  name                = var.name
  resource_group_name = var.resource_group_name
  location            = var.location
  dns_prefix          = var.dns_prefix
  kubernetes_version  = var.kubernetes_version
  tags                = var.tags

  default_node_pool {
    name                 = "system"
    vm_size              = var.node_vm_size
    vnet_subnet_id       = var.vnet_subnet_id
    node_count           = var.node_count
    auto_scaling_enabled = var.enable_auto_scaling
    min_count            = var.enable_auto_scaling ? var.node_min_count : null
    max_count            = var.enable_auto_scaling ? var.node_max_count : null
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin = "azure"
    service_cidr   = "10.30.0.0/16"
    dns_service_ip = "10.30.0.10"
  }

  # CSI driver to project Key Vault secrets into pods.
  key_vault_secrets_provider {
    secret_rotation_enabled = true
  }
}
