# Dev environment — smallest, cheapest sizing.
# Provide the subscription via env var:  export TF_VAR_subscription_id=<id>
environment = "dev"
location    = "eastus"

# AKS
node_vm_size        = "Standard_B2s"
enable_auto_scaling = false
node_count          = 1

# Registry
acr_sku = "Basic"

# PostgreSQL
postgres_sku_name   = "B_Standard_B1ms"
postgres_storage_mb = 32768

tags = {
  cost-center = "engineering"
}
