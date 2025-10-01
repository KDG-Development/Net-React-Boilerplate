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

Infra setup (required)
1. Create service connections in Azure DevOps:
   - Docker Registry: connection to your Azure Container Registry
   - Azure Resource Manager (ARM): name it like `sc-arm-<project>` (e.g., `sc-arm-kdg-boilerplate`)
2. Create two variable groups (Pipelines → Library → Variable groups):
   - `qa-infra-vars`
   - `prod-infra-vars`
   Add these variables to each (mark the password as secret):
     - `TFSTATE_RG` = `rg-<project>-tfstate` (e.g., `rg-kdg-boilerplate-tfstate`)
     - `TFSTATE_ACCOUNT` = globally unique SA name, lowercase 3–24 chars (e.g., `stkdgboilertf38219`)
     - `TFSTATE_CONTAINER` = `tfstate`
     - `PostgresAdminPassword` = a strong password (secret)
     - `AzureServiceConnection` = your ARM service connection name (e.g., `sc-arm-kdg-boilerplate`)
   - Do NOT check “Link secrets from an Azure key vault as variables”. Key Vault is provisioned by Terraform and used at runtime in QA/Prod.
3. Create a pipeline from `azure-pipelines-infra.yml`. The YAML already links the groups conditionally by environment and exposes parameters for project and location. Ensure your group names match or update the YAML:
   ```yaml
   variables:
     - ${{ if eq(parameters.environment, 'qa') }}:
       - group: qa-infra-vars
     - ${{ if eq(parameters.environment, 'prod') }}:
       - group: prod-infra-vars
     - name: ProjectName
       value: ${{ parameters.projectName }}
     - name: Location
       value: ${{ parameters.location }}
   ```
   Run-time parameters (with defaults): `environment=qa|prod`, `projectName=kdg-boilerplate`, `location=eastus`. On first run you may be prompted to Authorize the variable group usage.
4. Run the infra pipeline twice, once per environment:
   - `environment=qa`
   - `environment=prod`
   This provisions isolated infra per env and configures the Web App to read secrets from Key Vault via app settings (no secrets in files or repo).
5. Verify infra:
   - Web Apps are running
   - Key Vault contains env-specific secrets (e.g., `postgres-connection-string-qa`, `jwt-key-qa`)

Application pipeline and local usage
1. Use `azure-pipelines-example.yml` as a base for your application build/deploy. Update variables and service connection names accordingly.
2. Local development uses only local files: `appsettings.development.json` and client `.env`. Do not pull QA/Prod secrets into local files.
3. QA/Prod rely on App Service application settings with Key Vault references applied by the infra pipeline.

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