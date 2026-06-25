variable "subscription_id" {
  description = "Azure subscription ID to deploy into."
  type        = string
}

variable "project" {
  description = "Short project name, used as a naming prefix (lowercase, no spaces)."
  type        = string
  default     = "pos"

  validation {
    condition     = can(regex("^[a-z][a-z0-9]{1,11}$", var.project))
    error_message = "project must be 2-12 lowercase alphanumeric characters and start with a letter."
  }
}

variable "environment" {
  description = "Environment name (e.g. dev, staging, prod). Drives naming and is applied as a tag."
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9]{1,9}$", var.environment))
    error_message = "environment must be 2-10 lowercase alphanumeric characters and start with a letter."
  }
}

variable "location" {
  description = "Azure region (e.g. eastus, westeurope)."
  type        = string
  default     = "eastus"
}

variable "tags" {
  description = "Extra tags merged onto every resource."
  type        = map(string)
  default     = {}
}

# ---- Networking ----
variable "vnet_address_space" {
  description = "Address space for the virtual network."
  type        = list(string)
  default     = ["10.20.0.0/16"]
}

variable "aks_subnet_prefix" {
  description = "CIDR for the AKS nodes subnet."
  type        = string
  default     = "10.20.1.0/24"
}

variable "db_subnet_prefix" {
  description = "CIDR for the delegated PostgreSQL subnet."
  type        = string
  default     = "10.20.2.0/24"
}

# ---- AKS ----
variable "kubernetes_version" {
  description = "AKS Kubernetes version. Null tracks the region default."
  type        = string
  default     = null
}

variable "node_vm_size" {
  description = "VM size for the default node pool."
  type        = string
  default     = "Standard_D2s_v3"
}

variable "node_count" {
  description = "Node count when autoscaling is disabled; the initial count otherwise."
  type        = number
  default     = 2
}

variable "enable_auto_scaling" {
  description = "Enable cluster autoscaler on the default node pool."
  type        = bool
  default     = true
}

variable "node_min_count" {
  description = "Minimum nodes when autoscaling is enabled."
  type        = number
  default     = 2
}

variable "node_max_count" {
  description = "Maximum nodes when autoscaling is enabled."
  type        = number
  default     = 4
}

# ---- Container registry ----
variable "acr_sku" {
  description = "Azure Container Registry SKU (Basic, Standard, Premium)."
  type        = string
  default     = "Standard"
}

# ---- PostgreSQL ----
variable "postgres_sku_name" {
  description = "PostgreSQL Flexible Server SKU (e.g. B_Standard_B1ms, GP_Standard_D2s_v3)."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "PostgreSQL storage in MB."
  type        = number
  default     = 32768
}

variable "postgres_version" {
  description = "PostgreSQL major version."
  type        = string
  default     = "16"
}

variable "postgres_admin_username" {
  description = "PostgreSQL administrator login."
  type        = string
  default     = "posadmin"
}

variable "databases" {
  description = "Databases to create (one per microservice)."
  type        = list(string)
  default     = ["catalogdb", "salesdb", "identitydb"]
}
