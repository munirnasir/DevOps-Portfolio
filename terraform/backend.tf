# Remote state in Azure Storage. The values are intentionally omitted here so the same
# code works for every environment — supply them at init time with a per-env backend file:
#
#   terraform init -backend-config=environments/backend-dev.hcl
#
# (or run entirely locally by deleting this block).
terraform {
  backend "azurerm" {}
}
