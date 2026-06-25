provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }

  # Supplied via TF_VAR_subscription_id or the ARM_SUBSCRIPTION_ID env var.
  subscription_id = var.subscription_id
}

provider "random" {}
