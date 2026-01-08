# Blackhawk Ecom

Ecommerce website for Blackhawk, handling storefront operations and customer-facing functionality.

## System Context

This service is part of a multi-repository product data pipeline:

```
3rd Party Vendors --> [Vendor Sync] --> NetSuite --> [Ecom Website]
```

| Repository | Responsibility |
|------------|----------------|
| [Blackhawk.VendorSync](https://dev.azure.com/KDG1/Blackhawk/_git/Blackhawk.VendorSync) | Vendor data to NetSuite |
| **ecom** (this repo) | Ecommerce operations and NetSuite sync |

## Local Development

Requires Docker Desktop.

```bash
docker compose --profile app up --build    # Start app (Frontend: https://localhost:5173, API: https://localhost:5261)
```

### Configuration

1. Create `appsettings.development.json` matching the default `appsettings.json`
2. Create `.env` at repo root:
```
S247_LICENSE_KEY=your_site24x7_license_key
SITE24X7_APP_NAME=YourAppName-Local
```

### Data Seeding

```bash
docker compose run --rm devtools                              # Default: 10 categories, 50 products
docker compose run --rm devtools seed --categories 20 --products 100
docker compose run --rm devtools seed --user                  # Interactive user creation
```

## Testing

```bash
cd KDG.UnitTests && dotnet test         # Unit tests
cd KDG.IntegrationTests && dotnet test  # Integration tests (requires Docker)
```

## Azure Deployment

Infrastructure and deployment are managed via Azure DevOps pipelines with Terraform.

### Prerequisites

| Requirement | Description |
|-------------|-------------|
| Service Connection | `sc-arm-<project>` with Contributor + Key Vault Secrets Officer + User Access Administrator roles |
| Variable Groups | `qa-infra-vars`, `prod-infra-vars`, `qa-deployment-vars`, `prod-deployment-vars` |
| Secure Files | `appsettings.qa.json`, `.env.qa`, `appsettings.prod.json`, `.env.prod` |
| ACR Connection | `acr-<project>-<env>` Docker Registry service connection |

### Pipelines

| Pipeline | Purpose |
|----------|---------|
| `azure-pipelines-infra.yml` | Provisions Key Vault, ACR, PostgreSQL, App Service via Terraform |
| `azure-pipelines-example.yml` | Builds container, runs migrations, deploys to App Service |
| `azure-pipelines-unit-tests.yml` | Unit tests with coverage |
| `azure-pipelines-integration-tests.yml` | Integration tests with Testcontainers |

### Troubleshooting

```bash
# View container logs
az webapp log config --docker-container-logging filesystem --name <app> --resource-group <rg>
az webapp log tail --name <app> --resource-group <rg>

# Get resource names
az webapp list --resource-group <rg> --query "[0].name" -o tsv
az postgres flexible-server list --resource-group <rg> --query "[0].name" -o tsv
```

## Site24x7 APM

APM is enabled via environment variables at runtime.

```bash
docker compose --profile apm up --build   # Local APM testing
```

Required environment variables:
```
S247_LICENSE_KEY=your_license_key
SITE24X7_APP_NAME=YourAppName
```

Verify APM config in container:
```bash
docker compose exec webapp-apm sh -lc 'printenv | grep -E "S247|SITE24X7"'
```
