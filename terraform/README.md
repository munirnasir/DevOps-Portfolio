# Terraform — Azure infrastructure for the Cash & Carry POS

Provisions the Azure infrastructure to host the application: an **AKS** cluster, **Azure
Database for PostgreSQL Flexible Server** (private, one DB per service), **Azure Container
Registry**, a **VNet**, and a **Key Vault** holding the generated DB password and JWT key.

This is **infra only** — deploy the app afterwards with the manifests in [`../deploy/k8s`](../deploy/k8s).

## Design: environment-agnostic & reusable

- **One root module** composes reusable child modules in [`modules/`](modules)
  (`network`, `acr`, `aks`, `postgres`, `keyvault`). No code is duplicated per environment.
- **Each environment is just a `*.tfvars` file** in [`environments/`](environments)
  (`dev`, `staging`, `prod`) plus a `backend-*.hcl` for its isolated remote state.
  Add a new environment by dropping in two files — no code changes.
- **Secrets are generated**, never committed: the Postgres password and JWT signing key
  come from `random_password` and are stored in Key Vault.
- Globally-unique names (ACR, Key Vault, Postgres) get a random suffix automatically.

```
terraform/
├── versions.tf  providers.tf  backend.tf
├── variables.tf  locals.tf  main.tf  outputs.tf
├── modules/{network,acr,aks,postgres,keyvault}/
└── environments/{dev,staging,prod}.tfvars  +  backend-*.hcl
```

## Prerequisites

- Terraform >= 1.5, Azure CLI (`az login`)
- A storage account for remote state (one-time bootstrap):

```bash
az group create -n rg-tfstate -l eastus
az storage account create -n postfstate -g rg-tfstate -l eastus --sku Standard_LRS
az storage container create -n tfstate --account-name postfstate
```

(Match these names to the `backend-*.hcl` files, or edit those files.)

## Usage

```bash
export TF_VAR_subscription_id="<your-subscription-id>"
cd terraform

# Pick the environment by combining its backend + tfvars. Example: dev
terraform init  -backend-config=environments/backend-dev.hcl
terraform plan  -var-file=environments/dev.tfvars
terraform apply -var-file=environments/dev.tfvars
```

Switch environments by re-initialising with another backend and var file:

```bash
terraform init -reconfigure -backend-config=environments/backend-prod.hcl
terraform apply -var-file=environments/prod.tfvars
```

> Prefer **Terraform workspaces** or separate state keys per env (already the case here —
> each `backend-*.hcl` uses a distinct `key`).

## After apply — deploy the app

```bash
# 1. Get kubeconfig (the exact command is also a Terraform output)
az aks get-credentials --resource-group $(terraform output -raw resource_group_name) \
                       --name $(terraform output -raw aks_cluster_name)

# 2. Build & push images to the provisioned registry
ACR=$(terraform output -raw acr_login_server)
az acr login --name "${ACR%%.*}"
docker build -t $ACR/pos-catalog:latest  ../services/catalog-service
docker build -t $ACR/pos-sales:latest    ../services/sales-service
docker build -t $ACR/pos-identity:latest ../services/identity-service
docker build -t $ACR/pos-frontend:latest ../frontend
docker push $ACR/pos-catalog:latest && docker push $ACR/pos-sales:latest \
  && docker push $ACR/pos-identity:latest && docker push $ACR/pos-frontend:latest

# 3. Apply the Kubernetes manifests (update image refs + DB connection strings to
#    point at $ACR and the Postgres FQDN / Key Vault from the Terraform outputs)
kubectl apply -k ../deploy/k8s
```

## Key outputs

| Output | Purpose |
|--------|---------|
| `acr_login_server` | Registry to push images to |
| `aks_cluster_name` / `kube_config_command` | Connect kubectl |
| `postgres_fqdn` / `postgres_databases` | DB connection (private, in-VNet) |
| `key_vault_name` | Holds `postgres-admin-password`, `postgres-host`, `jwt-signing-key` |

## Notes

- PostgreSQL is **private** (VNet-integrated, no public endpoint); it is reachable from AKS
  pods in the same VNet. Run `terraform apply` from a context that doesn't need direct DB
  access, or add a jumpbox/private endpoint if you need local connectivity.
- The shared symmetric JWT key is a demo simplification; production would use asymmetric keys.
