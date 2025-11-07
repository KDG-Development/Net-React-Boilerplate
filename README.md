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

**Configuration Approach:**
- ✅ Infrastructure provisioned via Terraform (fully automated)
- ✅ Azure service connections created manually (one-time setup)
- ✅ Deployment variable groups created manually (one-time setup)
- ✅ Automatic permission granting (Key Vault, ACR) with fallback to manual commands
- ❌ No Personal Access Tokens (PAT) required

### Prerequisites

Before you begin, ensure you have:
- An active Azure subscription
- Azure DevOps organization and project
- Owner or User Access Administrator role on the Azure subscription (for one-time setup)
- Azure CLI installed locally (for manual setup steps)
- Ability to create service connections and variable groups in Azure DevOps

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

The service principal needs specific Azure RBAC roles to provision infrastructure and manage secrets.

**Option A: Using the Setup Script (Recommended)**

Run the provided PowerShell script:

```powershell
.\setup-service-principal.ps1
```

The script will:
- Check your Azure login
- Prompt for the service principal Object ID (from Azure DevOps service connection)
- Grant all three required roles automatically
- Handle cases where roles already exist

**Option B: Manual Commands**

If you prefer to run commands manually:

```bash
# Login to Azure
az login

# Get your service principal Object ID from Azure DevOps service connection details
# Go to Azure DevOps → Project Settings → Service connections → Your connection → Details
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

# Grant User Access Administrator role (to assign roles to managed identities)
az role assignment create \
  --assignee-object-id $SP_OBJECT_ID \
  --assignee-principal-type ServicePrincipal \
  --role "User Access Administrator" \
  --scope "/subscriptions/$SUBSCRIPTION_ID"
```

**Why these roles?**
- **Contributor**: Creates/manages resource groups, storage accounts, databases, app services, etc.
- **Key Vault Secrets Officer**: Creates and manages secrets in Key Vault (Terraform needs this)
- **User Access Administrator**: Allows the pipeline to automatically grant roles (Storage Blob Data Contributor, Key Vault Secrets User, AcrPull) without manual commands

**Important:** All three roles are required for fully automated deployment. Without User Access Administrator, you'll need to manually run permission grant commands displayed in the pipeline output.

---

### Step 3: Create Infrastructure Variable Groups

Create two variable groups (one for QA, one for Prod) with infrastructure configuration:

1. In Azure DevOps, go to **Pipelines** → **Library** → **+ Variable group**
2. Create `qa-infra-vars` with these variables:

| Variable Name | Value | Secret? | Description |
|---------------|-------|---------|-------------|
| `TFSTATE_RG` | `rg-<project>-tfstate` | No | Resource group for Terraform state storage |
| `TFSTATE_ACCOUNT` | `<uniquename>` | No | Storage account name (3-24 lowercase chars/numbers, globally unique) |
| `TFSTATE_CONTAINER` | `tfstate` | No | Container name for state files |
| `PostgresAdminPassword` | `<strong-password>` | **Yes** | PostgreSQL admin password |
| `AzureServiceConnection` | `sc-arm-<project>` | No | Your service connection name |
| `ProjectName` | `kdg-boilerplate` | No | Project name (used in resource naming) |

3. Repeat for `prod-infra-vars` (use same variable names, different values for prod)

**Important Notes:**
- Storage account names must be **globally unique** across all of Azure
- Storage account names can only contain **lowercase letters and numbers** (no hyphens or underscores)
- Examples: `kdgboilerplatetfstate`, `mycompanytfstate123`
- Do NOT check "Link secrets from an Azure key vault" - we provision Key Vault via Terraform
- Grant access permission to all pipelines (or specific pipelines as needed)

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

**Infrastructure Provisioning Stage:**
- Creates Terraform state storage (resource group, storage account, container)
- Grants service principal access to state storage  
- Provisions all infrastructure via Terraform:
  - Resource group: `rg-kdg-boilerplate-qa`
  - Key Vault: `kv-kdg-boilerplate-qa-xxxxx`
  - Container Registry: `acrkdgboilerplateqaxxxxx`
  - PostgreSQL Server: `pg-kdg-boilerplate-qa-xxxxx`
  - PostgreSQL Database: `kdg_boilerplate_db`
  - App Service Plan: `asp-kdg-boilerplate-qa`
  - App Service: `app-kdg-boilerplate-qa` (configured to listen on port 8080)
  - 6 Key Vault secrets (JWT keys, connection strings, etc.)
  - PostgreSQL extensions configuration (UUID-OSSP, PGCRYPTO, CITEXT, BTREE_GIST)
- **Automatically attempts to grant permissions** (if service principal has sufficient rights):
  - Webapp managed identity → Key Vault Secrets User role
  - Webapp managed identity → AcrPull role on Container Registry
- Configures App Service with Key Vault references and container settings

**Expected duration:** 6-10 minutes

**Important Notes:**
- If automatic permission granting fails, the pipeline displays manual commands to run
- The webapp uses managed identity to pull images from ACR (no passwords needed)
- All secrets are in Key Vault and referenced by App Service via managed identity

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

### Step 7: Create ACR Service Connection (Manual)

After the infrastructure pipeline completes, manually create a service connection for Azure Container Registry:

1. **Go to Azure DevOps** → **Project Settings** → **Service connections**
2. Click **New service connection** → **Docker Registry** → **Azure Container Registry**
3. **Configure the connection:**
   - **Authentication Type**: Select **Workload Identity federation (automatic)** or **Service Principal**
   - **Subscription**: Select your Azure subscription
   - **Azure Container Registry**: Select your ACR (e.g., `acrkdgboilerplateqaxxxxx`)
   - **Service connection name**: `acr-<project>-<env>` (e.g., `acr-kdg-boilerplate-qa`)
   - **Description**: `Container Registry for <env> environment`
   - Check **Grant access permission to all pipelines**
4. Click **Save**

Repeat this process for production environment with the production ACR.

---

### Step 8: Create Deployment Variable Groups (Manual)

Create variable groups for deployment pipelines with resource information:

1. **Go to Azure DevOps** → **Pipelines** → **Library** → **+ Variable group**
2. Create `qa-deployment-vars` with these variables:

| Variable Name | Value | Description |
|---------------|-------|-------------|
| `dockerRegistryServiceConnection` | `acr-<project>-qa` | ACR service connection name from Step 7 |
| `azureResourceServiceConnection` | `sc-arm-<project>` | ARM service connection name from Step 1 |
| `resourceGroupName` | `rg-<project>-qa` | Resource group name (from Azure Portal) |
| `webAppName` | `app-<project>-qa` | App Service name (from Azure Portal) |
| `postgresServerName` | `pg-<project>-qa-xxxxx` | PostgreSQL server name (from Azure Portal) |
| `imageRepository` | `<project-name>` | Docker image repository name (e.g., `kdg-boilerplate`) |
| `environment` | `qa` | Environment name |

3. Check **Grant access permission to all pipelines**
4. Click **Save**
5. Repeat for `prod-deployment-vars` with production values

**Finding Resource Names:**
```bash
# Get resource names from Azure
$RG = "rg-<project>-qa"
az webapp list --resource-group $RG --query "[0].name" -o tsv
az postgres flexible-server list --resource-group $RG --query "[0].name" -o tsv
az acr list --resource-group $RG --query "[0].name" -o tsv
```

---

### Step 9: Prepare Secure Files for Deployment

Upload environment-specific configuration files to Azure DevOps Secure Files:

1. **Create `appsettings.qa.json`**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

2. **Create `.env.qa`**:
```bash
S247_LICENSE_KEY=your_site24x7_license_key_for_qa
SITE24X7_APP_NAME=YourAppName-QA
ASPNETCORE_ENVIRONMENT=Production
```

3. **Upload to Azure DevOps**:
   - Go to **Pipelines** → **Library** → **Secure files** tab
   - Click **+ Secure file**
   - Upload `appsettings.qa.json` and `.env.qa`
   - Click **Authorize for use in all pipelines** for each file

4. **Repeat for production**: Create and upload `appsettings.prod.json` and `.env.prod`

**Important**: Do NOT put database connection strings or JWT keys in these files - they're managed by Azure Key Vault!

---

### Step 10: Running the Deployment Pipeline

1. In Azure DevOps, go to **Pipelines** → **New pipeline**
2. Select your repository → **Existing Azure Pipelines YAML file**
3. Select `/azure-pipelines-example.yml`
4. Click **Save** (don't run yet)
5. Grant permissions when prompted:
   - Variable group usage
   - Service connection usage
   - Secure file usage

**First Deployment:**
1. Click **Run pipeline**
2. Select environment: `qa`
3. Verify parameters match your configuration
4. Click **Run**

**Pipeline Steps:**
- Downloads secure files (`appsettings.qa.json`, `.env.qa`)
- Builds Docker image (port 8080)
- Creates temporary PostgreSQL firewall rule
- Runs database migrations
- Removes firewall rule
- Pushes image to Azure Container Registry
- Deploys to App Service

**Expected duration:** 3-5 minutes

**Important Notes:**
- All variables come from the `<env>-deployment-vars` variable group
- Database credentials come from Key Vault (webapp's managed identity)
- Container runs on port 8080 (configured in Dockerfile)
- Migrations require temporary firewall access to PostgreSQL

---

### Pipeline Behavior on Subsequent Runs

Both pipelines are **idempotent** - safe to run multiple times:

**Infrastructure Pipeline:**

| Step | First Run | Subsequent Runs |
|------|-----------|-----------------|
| Bootstrap State Storage | Creates storage infrastructure | Skips (already exists) |
| Terraform Init | Downloads providers | Uses existing state |
| Terraform Plan | Plans 15+ resource creations | Shows "No changes" if code unchanged |
| Terraform Apply | Creates all resources | Only applies changes if code modified |
| Grant Permissions | Attempts to grant Key Vault & ACR access | Shows success or "already exists" |
| App Service Config | Applies settings | Reapplies (causes brief restart) |

**When to re-run infrastructure pipeline:**
- After modifying Terraform code (e.g., changing SKUs, adding resources)
- After Key Vault secrets expire or need rotation
- To ensure infrastructure matches code (drift detection)
- When database extensions need updating

**Deployment Pipeline:**
- Safe to run multiple times
- Always builds fresh Docker image
- Runs migrations (skips if already applied)
- Deploys new container to App Service
- Brief downtime (~30 seconds) during deployment

---

### Application Pipeline and Local Usage

1. **Application Deployment**: The `azure-pipelines-example.yml` pipeline uses manual variable groups
   - Variables populated from `<env>-deployment-vars` variable groups (created in Step 8)
   - Select environment (`qa` or `prod`) when running
   - Secure files (`appsettings.<env>.json`, `.env.<env>`) must be uploaded (Step 9)
   - Builds container, runs migrations, deploys to App Service

2. **Local Development**:
   - Uses `appsettings.development.json` and client `.env` files
   - Never pull QA/Prod secrets locally
   - Run database migrations against local PostgreSQL in Docker

3. **QA/Prod Environments**:
   - App Service reads secrets from Key Vault via managed identity
   - Container images pulled from ACR using managed identity
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
- Check the infrastructure pipeline output for manual permission grant commands
- Run the displayed commands to grant "Key Vault Secrets User" role to webapp
- Wait 2-3 minutes for role assignments to propagate
- Restart the App Service after granting permissions

**ACR pull authentication error:**
- Check the infrastructure pipeline output for manual permission grant commands
- Run the displayed commands to grant "AcrPull" role to webapp managed identity
- Verify ACR service connection is configured correctly in Azure DevOps

**Container timeout on port 8080:**
- Ensure Dockerfile exposes and listens on port 8080 (not 5261)
- Check `ASPNETCORE_URLS=http://+:8080` in Dockerfile
- Enable container logging: `az webapp log config --docker-container-logging filesystem`
- View logs: `az webapp log tail --name <app-name> --resource-group <rg>`

**Database migration fails with extension errors:**
- Verify PostgreSQL extensions are configured in Terraform (`UUID-OSSP`, `PGCRYPTO`, `CITEXT`, `BTREE_GIST`)
- Re-run infrastructure pipeline to apply extension configuration
- Check PostgreSQL server configuration in Azure Portal

**Variable group not found:**
- Ensure you created `<env>-deployment-vars` variable groups (Step 8)
- Grant pipeline permission to access variable groups
- Variable group names must match exactly (case-sensitive)

**Secure file not found:**
- Upload `appsettings.<env>.json` and `.env.<env>` to Azure DevOps Secure Files (Step 9)
- Click "Authorize for use in all pipelines" for each file
- File names must match exactly (including the dot prefix for `.env.*`)

---

### Security Best Practices

✅ **Secrets in Key Vault only** - Never commit secrets to git  
✅ **Managed identities** - No service principal keys in application code  
✅ **RBAC authorization** - Key Vault uses Azure RBAC, not legacy access policies  
✅ **Separate environments** - QA and Prod are completely isolated  
✅ **Terraform state** - Stored securely in Azure Storage with RBAC  
✅ **OIDC authentication** - Pipeline uses Workload Identity Federation (no long-lived secrets)  
✅ **Infrastructure as Code** - All infrastructure managed via Terraform  
✅ **Container security** - Images pulled from private ACR using managed identity  
✅ **Secure files** - Environment-specific configs stored in Azure DevOps Secure Files  
✅ **Automatic permission granting** - Pipeline attempts to grant required roles automatically  
✅ **Database extensions** - Explicitly allow-listed PostgreSQL extensions for security  
✅ **Port configuration** - Standardized on port 8080 for containerized deployments

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