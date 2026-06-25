resource "azurerm_key_vault" "this" {
  name                = var.name
  resource_group_name = var.resource_group_name
  location            = var.location
  tenant_id           = var.tenant_id
  sku_name            = "standard"

  # Deployer: full secret management.
  access_policy {
    tenant_id          = var.tenant_id
    object_id          = var.admin_object_id
    secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
  }

  # AKS kubelet identity: read-only (for the Key Vault CSI driver).
  access_policy {
    tenant_id          = var.tenant_id
    object_id          = var.reader_object_id
    secret_permissions = ["Get", "List"]
  }

  tags = var.tags
}

resource "azurerm_key_vault_secret" "this" {
  # Iterate over the (non-secret) key names; the values stay sensitive.
  for_each     = toset(nonsensitive(keys(var.secrets)))
  name         = each.value
  value        = var.secrets[each.value]
  key_vault_id = azurerm_key_vault.this.id
}
