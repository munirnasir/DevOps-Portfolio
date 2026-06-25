variable "name" {
  description = "Globally-unique Key Vault name (3-24 chars, alphanumeric and hyphens)."
  type        = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "admin_object_id" {
  description = "Principal allowed to manage secrets (typically the deployer)."
  type        = string
}

variable "reader_object_id" {
  description = "Principal allowed to read secrets (typically the AKS kubelet identity)."
  type        = string
}

variable "secrets" {
  description = "Secrets to seed into the vault."
  type        = map(string)
  default     = {}
  sensitive   = true
}

variable "tags" {
  type    = map(string)
  default = {}
}
