locals {
  # e.g. "pos-dev" for human-readable names; "posdev" for names that forbid hyphens.
  name_prefix = "${var.project}-${var.environment}"
  base_name   = "${var.project}${var.environment}"

  tags = merge(
    {
      project     = var.project
      environment = var.environment
      managed-by  = "terraform"
    },
    var.tags
  )
}
