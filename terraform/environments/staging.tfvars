# Staging environment — production-like but modest scale.
environment = "staging"
location    = "eastus"

# AKS
node_vm_size        = "Standard_D2s_v3"
enable_auto_scaling = true
node_min_count      = 2
node_max_count      = 3

# Registry
acr_sku = "Standard"

# PostgreSQL
postgres_sku_name   = "GP_Standard_D2s_v3"
postgres_storage_mb = 65536

tags = {
  cost-center = "engineering"
}
