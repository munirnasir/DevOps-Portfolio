# Backend config for the dev state. The storage account/container must already exist
# (see terraform/README.md for the one-time bootstrap command).
resource_group_name  = "rg-tfstate"
storage_account_name = "postfstate"
container_name       = "tfstate"
key                  = "dev.terraform.tfstate"
