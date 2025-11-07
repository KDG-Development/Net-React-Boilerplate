# Azure DevOps Service Principal Setup Script
# This script grants the necessary permissions to your service principal for automated deployments

Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "Azure DevOps Service Principal Setup" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if logged in to Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Not logged in to Azure. Logging in..." -ForegroundColor Yellow
    az login
    $account = az account show | ConvertFrom-Json
}

Write-Host "✓ Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host "✓ Subscription: $($account.name) ($($account.id))" -ForegroundColor Green
Write-Host ""

# Get service principal Object ID
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "To find your Service Principal Object ID:" -ForegroundColor Yellow
Write-Host "1. Go to Azure DevOps → Project Settings → Service connections" -ForegroundColor Yellow
Write-Host "2. Click on your service connection (e.g., sc-arm-kdg-boilerplate)" -ForegroundColor Yellow
Write-Host "3. Look for 'Object ID' in the details" -ForegroundColor Yellow
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

$SP_OBJECT_ID = Read-Host "Enter Service Principal Object ID"

if ([string]::IsNullOrWhiteSpace($SP_OBJECT_ID)) {
    Write-Host "Error: Service Principal Object ID is required" -ForegroundColor Red
    exit 1
}

$SUBSCRIPTION_ID = $account.id

Write-Host ""
Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  Service Principal Object ID: $SP_OBJECT_ID" -ForegroundColor White
Write-Host "  Subscription ID: $SUBSCRIPTION_ID" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continue with these settings? (y/n)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Cancelled by user" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "Granting Permissions..." -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

# Grant Contributor role
Write-Host "1/3 Granting Contributor role..." -ForegroundColor Yellow
$contributorResult = az role assignment create `
    --assignee-object-id $SP_OBJECT_ID `
    --assignee-principal-type ServicePrincipal `
    --role "Contributor" `
    --scope "/subscriptions/$SUBSCRIPTION_ID" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "    ✓ Contributor role granted successfully" -ForegroundColor Green
} else {
    if ($contributorResult -like "*already exists*") {
        Write-Host "    ✓ Contributor role already exists" -ForegroundColor Green
    } else {
        Write-Host "    ✗ Failed to grant Contributor role" -ForegroundColor Red
        Write-Host "    Error: $contributorResult" -ForegroundColor Red
    }
}

# Grant Key Vault Secrets Officer role
Write-Host "2/3 Granting Key Vault Secrets Officer role..." -ForegroundColor Yellow
$kvResult = az role assignment create `
    --assignee-object-id $SP_OBJECT_ID `
    --assignee-principal-type ServicePrincipal `
    --role "Key Vault Secrets Officer" `
    --scope "/subscriptions/$SUBSCRIPTION_ID" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "    ✓ Key Vault Secrets Officer role granted successfully" -ForegroundColor Green
} else {
    if ($kvResult -like "*already exists*") {
        Write-Host "    ✓ Key Vault Secrets Officer role already exists" -ForegroundColor Green
    } else {
        Write-Host "    ✗ Failed to grant Key Vault Secrets Officer role" -ForegroundColor Red
        Write-Host "    Error: $kvResult" -ForegroundColor Red
    }
}

# Grant User Access Administrator role
Write-Host "3/3 Granting User Access Administrator role..." -ForegroundColor Yellow
$uaaResult = az role assignment create `
    --assignee-object-id $SP_OBJECT_ID `
    --assignee-principal-type ServicePrincipal `
    --role "User Access Administrator" `
    --scope "/subscriptions/$SUBSCRIPTION_ID" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "    ✓ User Access Administrator role granted successfully" -ForegroundColor Green
} else {
    if ($uaaResult -like "*already exists*") {
        Write-Host "    ✓ User Access Administrator role already exists" -ForegroundColor Green
    } else {
        Write-Host "    ✗ Failed to grant User Access Administrator role" -ForegroundColor Red
        Write-Host "    Error: $uaaResult" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Granted Roles:" -ForegroundColor Cyan
Write-Host "  ✓ Contributor - Creates and manages resources" -ForegroundColor Green
Write-Host "  ✓ Key Vault Secrets Officer - Manages Key Vault secrets" -ForegroundColor Green
Write-Host "  ✓ User Access Administrator - Assigns roles to managed identities" -ForegroundColor Green
Write-Host ""
Write-Host "Your service principal is now ready for automated deployments!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Create infrastructure variable groups in Azure DevOps (Step 3)" -ForegroundColor White
Write-Host "2. Run the infrastructure pipeline (azure-pipelines-infra.yml)" -ForegroundColor White
Write-Host "3. The pipeline will automatically handle all remaining permissions" -ForegroundColor White
Write-Host ""

