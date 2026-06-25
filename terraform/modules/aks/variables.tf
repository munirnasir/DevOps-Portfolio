variable "name" {
  type = string
}

variable "dns_prefix" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "kubernetes_version" {
  type    = string
  default = null
}

variable "node_vm_size" {
  type    = string
  default = "Standard_D2s_v3"
}

variable "node_count" {
  type    = number
  default = 2
}

variable "enable_auto_scaling" {
  type    = bool
  default = true
}

variable "node_min_count" {
  type    = number
  default = 2
}

variable "node_max_count" {
  type    = number
  default = 4
}

variable "vnet_subnet_id" {
  type = string
}

variable "tags" {
  type    = map(string)
  default = {}
}
