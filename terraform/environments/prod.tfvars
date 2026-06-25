# Production environment — autoscaled, higher SKUs.
environment = "prod"
location    = "eastus"

# AKS
node_vm_size        = "Standard_D4s_v3"
enable_auto_scaling = true
node_min_count      = 3
node_max_count      = 6

# Registry
acr_sku = "Premium"

# PostgreSQL
postgres_sku_name   = "GP_Standard_D4s_v3"
postgres_storage_mb = 131072

tags = {
  cost-center = "production"
}
