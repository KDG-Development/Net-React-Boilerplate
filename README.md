# Introduction

Follow the instructions under "Getting Started" to develop features directly applicable to the boilerplate project.

In order to build a new project using the boilerplate code, follow the instructions under "Creating a Project Based on Boilerplate".

# Prerequisites

1. Install Docker Desktop

# Getting Started

1. Fork or clone the boilerplate repository to your machine
2. If cloned, rename the remote with `git remote rename origin boilerplate`
3. Ensure you have an appropriate `appsettings.development.json`, which matches the default appsettings.json at the root of the project
4. Create a `.env` file at the repo root with:
```
S247_LICENSE_KEY=your_site24x7_license_key
SITE24X7_APP_NAME=YourAppName-Local
```
5. Start Docker Desktop and run the application:
```
docker compose --profile app up --build
```

> After startup (profile: app): Frontend https://localhost:5173, API https://localhost:5261

# Creating a Project Based on Boilerplate

1. Initialize project repository
2. Clone project repository to your machine
3. Add the boilerplate origin with `git remote add https://github.com/KDG-Development/Net-React-Boilerplate.git boilerplate`
4. Prevent accidental pushes to boilerplate origin with `git remote set-url boilerplate --push "do not push"`
5. Pull boilerplate code into project repository with `git pull boilerplate [boilerplate repository branch]`
6. Push boilerplate code to project origin with `git push`
7. Follow steps 3-5 under "Getting Started" in project repository.
8. Include a note in project documentation to follow steps 3-5 in this section while setting up the project on a new machine.

## Configuring the Azure Deployment Pipeline

This section guides you through setting up automated infrastructure provisioning and application deployment to Azure using Terraform and Azure DevOps Pipelines.

### Prerequisites

Before you begin, ensure you have:
- An active Azure subscription
- Azure DevOps organization and project
- Owner or User Access Administrator role on the Azure subscription (for one-time setup)
- Azure CLI installed locally (for manual setup steps)

### Architecture Overview

The infrastructure pipeline provisions:
- **Azure Key Vault** (with RBAC) for secure secret storage
- **Azure Container Registry** for Docker images
- **PostgreSQL Flexible Server** for database
- **Azure App Service** (Linux) for hosting the application
- **Managed Identities** for secure Key Vault access
- **Terraform State Storage** (auto-created on first run)

All secrets are stored in Key Vault and referenced by App Service via managed identity - no secrets in code or configuration files.

---

### Step 1: Create Azure Service Connection

1. In Azure DevOps, go to **Project Settings** → **Service connections**
2. Click **New service connection** → **Azure Resource Manager** → **Next**
3. Select **Workload Identity federation (automatic)**
4. Choose your subscription
5. Leave resource group empty (subscription-level access)
6. Name it: `sc-arm-<project>` (e.g., `sc-arm-kdg-boilerplate`)
7. Grant access permission to all pipelines (or configure per-pipeline)
8. Click **Save**

**Note**: The service connection uses Workload Identity Federation (OIDC), which is more secure than client secrets.

---

### Step 2: Grant Service Principal Permissions (One-Time Setup)

The service principal needs specific Azure RBAC roles to provision infrastructure and manage secrets. Run these commands once:

```bash
# Login to Azure
az login

# Get your service principal Object ID from Azure DevOps service connection details
# Replace with your actual Object ID and Subscription ID
SP_OBJECT_ID="<your-sp-object-id>"
SUBSCRIPTION_ID="<your-subscription-id>"

# Grant Contributor role (to create and manage resources)
az role assignment create \
  --assignee-object-id $SP_OBJECT_ID \
  --assignee-principal-type ServicePrincipal \
  --role "Contributor" \
  --scope "/subscriptions/$SUBSCRIPTION_ID"

# Grant Key Vault Secrets Officer role (to manage Key Vault secrets)
az role assignment create \
  --assignee-object-id $SP_OBJECT_ID \
  --assignee-principal-type ServicePrincipal \
  --role "Key Vault Secrets Officer" \
  --scope "/subscriptions/$SUBSCRIPTION_ID"
```

**Why these roles?**
- **Contributor**: Creates/manages resource groups, storage accounts, databases, app services, etc.
- **Key Vault Secrets Officer**: Creates and manages secrets in Key Vault (Terraform needs this)

---

### Step 3: Create Azure DevOps Variable Groups

Create two variable groups (one for QA, one for Prod) with environment-specific configuration:

1. In Azure DevOps, go to **Pipelines** → **Library** → **+ Variable group**
2. Create `qa-infra-vars` with these variables:

| Variable Name | Value | Secret? | Description |
|---------------|-------|---------|-------------|
| `TFSTATE_RG` | `rg-<project>-tfstate` | No | Resource group for Terraform state storage |
| `TFSTATE_ACCOUNT` | `<uniquename>` | No | Storage account name (3-24 lowercase chars/numbers, globally unique) |
| `TFSTATE_CONTAINER` | `tfstate` | No | Container name for state files |
| `PostgresAdminPassword` | `<strong-password>` | **Yes** | PostgreSQL admin password |
| `AzureServiceConnection` | `sc-arm-<project>` | No | Your service connection name |

3. Repeat for `prod-infra-vars` (use same variable names, different values for prod)

**Important Notes:**
- Storage account names must be **globally unique** across all of Azure
- Storage account names can only contain **lowercase letters and numbers** (no hyphens or underscores)
- Examples: `kdgboilerplatetfstate`, `mycompanytfstate123`
- Do NOT check "Link secrets from an Azure key vault" - we provision Key Vault via Terraform

---

### Step 4: Create Infrastructure Pipeline

1. In Azure DevOps, go to **Pipelines** → **New pipeline**
2. Select your repository
3. Choose **Existing Azure Pipelines YAML file**
4. Select `azure-pipelines-infra.yml`
5. Click **Save** (don't run yet)

On first run, you may be prompted to:
- Authorize the variable group usage
- Approve the pipeline to use the service connection

---

### Step 5: Run the Infrastructure Pipeline

The pipeline has the following parameters:

| Parameter | Default | Options | Description |
|-----------|---------|---------|-------------|
| `environment` | `qa` | `qa`, `prod` | Target environment |
| `projectName` | `kdg-boilerplate` | Any string | Project name (used in resource naming) |
| `location` | `eastus` | Azure region | Azure region for resources |

**First Run (QA Environment):**
1. Click **Run pipeline**
2. Set `environment` = `qa`
3. Verify other parameters
4. Click **Run**

**What happens on first run:**
- Creates Terraform state storage (resource group, storage account, container)
- Grants service principal access to state storage
- Provisions all infrastructure via Terraform:
  - Resource group: `rg-kdg-boilerplate-qa`
  - Key Vault: `kv-kdg-boilerplate-qa-xxxxx`
  - Container Registry: `acrkdgboilerplateqaxxxxx`
  - PostgreSQL Server: `pg-kdg-boilerplate-qa-xxxxx`
  - PostgreSQL Database: `kdg_boilerplate_db`
  - App Service Plan: `asp-kdg-boilerplate-qa`
  - App Service: `app-kdg-boilerplate-qa`
  - 6 Key Vault secrets (JWT keys, connection strings, etc.)
- Grants webapp managed identity access to Key Vault
- Configures App Service with Key Vault references

**Expected duration:** 5-10 minutes

**Second Run (Prod Environment):**
1. Click **Run pipeline** again
2. Set `environment` = `prod`
3. Click **Run**

This creates isolated production infrastructure with separate resources and secrets.

---

### Step 6: Verify Infrastructure

After successful pipeline runs, verify in Azure Portal:

**QA Environment:**
- Resource Group: `rg-kdg-boilerplate-qa` exists
- Key Vault: Contains secrets named `*-qa` (e.g., `postgres-connection-string-qa`, `jwt-key-qa`)
- App Service: Shows "Your web app is running and waiting for your content"
- PostgreSQL: Server is running and has database `kdg_boilerplate_db`

**Prod Environment:**
- Resource Group: `rg-kdg-boilerplate-prod` exists
- Similar resources with `-prod` naming

**Configuration in App Service:**
- Go to App Service → **Configuration** → **Application settings**
- Verify settings reference Key Vault (shown as green with key icon):
  - `ConnectionString` = `@Microsoft.KeyVault(SecretUri=...)`
  - `Jwt__Key` = `@Microsoft.KeyVault(SecretUri=...)`
  - `Jwt__Issuer` = actual value (not in Key Vault)
  - `Jwt__Audience` = actual value (not in Key Vault)
  - `BaseUrl` = App Service URL

---

### Pipeline Behavior on Subsequent Runs

The infrastructure pipeline is **idempotent** - safe to run multiple times:

| Step | First Run | Subsequent Runs |
|------|-----------|-----------------|
| Bootstrap State Storage | Creates storage infrastructure | Skips (already exists) |
| Terraform Init | Downloads providers | Uses existing state |
| Terraform Plan | Plans 15+ resource creations | Shows "No changes" if code unchanged |
| Terraform Apply | Creates all resources | Only applies changes if code modified |
| Grant Key Vault Access | Grants webapp access | Skips (already exists) |
| App Service Config | Applies settings | Reapplies (causes brief restart) |

**When to re-run:**
- After modifying Terraform code (e.g., changing SKUs, adding resources)
- After Key Vault secrets expire or need rotation
- To ensure infrastructure matches code (drift detection)

---

### Application Pipeline and Local Usage

1. **Application Deployment**: Use `azure-pipelines-example.yml` as a base for building and deploying your application
   - Update Docker registry connection
   - Update web app names
   - Configure environment-specific variables

2. **Local Development**:
   - Uses `appsettings.development.json` and client `.env` files
   - Never pull QA/Prod secrets locally
   - Run database migrations against local PostgreSQL in Docker

3. **QA/Prod Environments**:
   - App Service reads secrets from Key Vault via managed identity
   - No secrets stored in application settings or environment variables directly
   - Secrets automatically updated when Key Vault values change

---

### Troubleshooting

**Storage account name invalid:**
- Ensure name is 3-24 characters
- Only lowercase letters and numbers (no hyphens, underscores, or uppercase)

**Authorization errors:**
- Verify service principal has Contributor + Key Vault Secrets Officer roles
- Check role assignments in Azure Portal → Subscriptions → Access control (IAM)

**Terraform backend initialization fails:**
- Ensure TFSTATE_ACCOUNT variable matches actual storage account name
- Verify service principal has "Storage Blob Data Contributor" role on storage account

**PostgreSQL internal errors:**
- May be transient Azure issue - re-run pipeline
- Check Azure region capacity and quotas

**Key Vault access denied:**
- Verify webapp managed identity has "Key Vault Secrets User" role
- Wait 2-3 minutes for role assignments to propagate

**Pipeline parameter not showing:**
- Edit pipeline → Click "..." → **Triggers** → **Variables** tab to verify parameters

---

### Security Best Practices

✅ **Secrets in Key Vault only** - Never commit secrets to git  
✅ **Managed identities** - No service principal keys in application code  
✅ **RBAC authorization** - Key Vault uses Azure RBAC, not legacy access policies  
✅ **Separate environments** - QA and Prod are completely isolated  
✅ **Terraform state** - Stored securely in Azure Storage with RBAC  
✅ **OIDC authentication** - Pipeline uses Workload Identity Federation (no long-lived secrets)

## Site24x7 APM Integration

This boilerplate includes Site24x7 APM for the backend. The agent is installed in a dedicated Docker stage and enabled by providing environment variables at runtime. No license keys are baked into images.

- **Dockerfile**: The `final-apm` stage installs and configures the Site24x7 .NET Core agent.
- **docker-compose**: A `webapp-apm` service is available for local testing with APM enabled.
- **Azure Pipelines**: The example pipeline builds using the `final-apm` stage so deployment images include the agent, and applies environment variables from a secure `.env.[environment]` file to the Azure Web App at deploy time.

### Local workflow (docker-compose)
1. Define `.env` as shown above.
2. Start the APM-enabled profile:
```
docker compose --profile apm up --build
```
3. App (profile: apm) serves HTTPS at `https://localhost:5173` (static app) and `https://localhost:5261` (API). Your browser may warn about the self-signed certificate.

### CI/CD deployment (Azure Pipelines + Azure App Service for Containers)
- The sample `azure-pipelines-example.yml` builds the image using the `final-apm` Docker target and pushes to your container registry.
- The pipeline downloads a secure `.env.[environment]` file and applies its key/values as Application settings on your Azure Web App for Containers.

#### Environment variables via secure .env
1. In Azure DevOps → Pipelines → Library → Secure files, upload one file named:
   - `.env.[environment]`
2. Each file must contain at minimum:
```
S247_LICENSE_KEY=your_site24x7_license_key
SITE24X7_APP_NAME=YourAppName
```
3. In `azure-pipelines-[environment].yml` set:
   - `environment` to `production` (controls which `.env.[environment]` is used)
   - `webAppName` to your Azure Web App for Containers name
4. The pipeline will:
   - Build and push the APM-enabled image
   - Download `.env.[environment]`
   - Apply all entries as App Settings to the Web App

#### Notes and troubleshooting
- The APM test service is only for local validation. It uses the same ports as the standard profile; run one profile at a time.
- HTTPS: the APM profile uses a dev certificate generated at build; expect a browser warning.
- Agent config precedence: the image entrypoint removes any generated `apminsight.conf` on container start so `S247_LICENSE_KEY` and `SITE24X7_APP_NAME` from environment are always used.
- License capacity: each running instance consumes an APM license. If logs show `responseObj is null` or `response-code 702 (LicenseInstancesExceeded)`, free inactive monitors in Site24x7 or increase capacity, then restart and generate traffic.
- Many inactive instances in the portal can come from repeated local runs. This is expected for testing and can be cleaned up in the Site24x7 UI.
- Confirm license/app envs are present in the container:
```
docker compose exec webapp-apm sh -lc 'printenv | grep -E "S247|SITE24X7" || echo "No S247/SITE24X7 envs found"'
```
- Tail all agent logs if any exist:
```
docker compose exec webapp-apm sh -lc 'files=$(find "$CORECLR_SITE24X7_HOME" -type f -iname "*.log" 2>/dev/null); if [ -n "$files" ]; then echo "$files" && tail -n +1 -F $files; else echo "No agent log files found under $CORECLR_SITE24X7_HOME yet"; fi'
```

References:
- Site24x7 .NET Core agent via NuGet: https://www.site24x7.com/help/apm/dotnet-agent/install-dot-net-core-agent-via-nuget.html

## Additional DevTools

Additional [DevTools](https://github.com/KDG-Development/KDG-Net-DevTools) are available to assist with development when using this boilerplate

1. At the root of the project, run `mkdir DevTools` if the folder doesnt exist
1. Install with `git clone https://github.com/KDG-Development/KDG-Net-DevTools ./DevTools`
2. See the [README](https://github.com/KDG-Development/KDG-Net-DevTools/blob/main/README.md) for more information


# Staying up to date

This boilerplate is designed to be both a starting point and an ever-evolving foundation for your .NET/React applications. This section is applicable both for developing features directly for the boilerplate, as well as project repositories.

In order to extract the most value from this boilerplate, it is important to keep your application up to date frequently reintegrating into your application with one of the applicable workflows:

## Cloned Repositories
- Run `git pull --rebase [branch] boilerplate`
## Forked Repositories
- Create a pull request from this repository into your fork

# Contributing

Pull requests are welcome and very much appreciated!

## Testing library changes locally

When working on common libraries that you want to integrate into this boilerplate, you should test them prior to releasing them on nuget.

1. Update your local packages version to a semantic development version for local consumption via .csproj or related
```
<PropertyGroup>
    <Version>0.0.1-some-feature-development-1</Version>
</PropertyGroup>
```

The first part of this temporary version is the version which you aspire to release. The latter half is your local iteration for testing.


2. Add the path to your local consuming project build via .csproj or similar
```
  <PropertyGroup>
    <RestoreSources>
    $(RestoreSources);
    <!-- Add your paths to local nuget directories here -->
    <!-- e.g., [...\folder\some-folder-with-a-nupkg-file]; without the brackets -->
    https://api.nuget.org/v3/index.json;
  </RestoreSources>
  </PropertyGroup>
```

3. Ensure the package reference inside the csproj references the most recent development build
```
<PackageReference Include="[your-package-name]" Version="0.0.1-some-feature-development-1" />
```
4. Restore with `dotnet restore`