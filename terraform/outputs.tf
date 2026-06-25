output "resource_group_name" {
  description = "Resource group containing all environment resources."
  value       = azurerm_resource_group.this.name
}

output "acr_login_server" {
  description = "Container registry hostname to tag/push images to."
  value       = module.acr.login_server
}

output "aks_cluster_name" {
  description = "AKS cluster name."
  value       = module.aks.cluster_name
}

output "kube_config_command" {
  description = "Command to fetch kubeconfig for kubectl / deploy/k8s."
  value       = "az aks get-credentials --resource-group ${azurerm_resource_group.this.name} --name ${module.aks.cluster_name}"
}

output "postgres_fqdn" {
  description = "PostgreSQL Flexible Server FQDN (reachable from within the VNet)."
  value       = module.postgres.fqdn
}

output "postgres_databases" {
  description = "Databases created (one per microservice)."
  value       = var.databases
}

output "key_vault_name" {
  description = "Key Vault holding the generated DB password and JWT signing key."
  value       = module.keyvault.name
}

output "postgres_admin_username" {
  description = "PostgreSQL administrator login."
  value       = var.postgres_admin_username
}

output "postgres_admin_password" {
  description = "Generated PostgreSQL administrator password (also in Key Vault)."
  value       = random_password.postgres_admin.result
  sensitive   = true
}
