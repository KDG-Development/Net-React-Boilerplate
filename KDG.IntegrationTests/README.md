# Integration Tests - KDG Boilerplate

This project contains integration tests for applications based on the KDG Boilerplate using real PostgreSQL databases.

## Quick Start - Local Development

The easiest way to run integration tests locally is with `dotnet test`:

```bash
cd KDG.IntegrationTests
dotnet test
```

**That's it!** Testcontainers will automatically:
- Spin up a PostgreSQL 16 container
- Run all database migrations
- Execute your tests
- Clean up the container when done

### Requirements

- Docker Desktop running
- .NET 8.0 SDK

## Three Ways to Run Integration Tests

### 1. Direct `dotnet test` (Recommended for Local Development)

**Uses**: Testcontainers  
**Best for**: Quick local testing, debugging individual tests

```bash
cd KDG.IntegrationTests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~ShouldConnectToDatabase"

# With verbose output
dotnet test --verbosity normal
```

**How it works**:
- Testcontainers automatically detects Docker is available
- Spins up a PostgreSQL 16 container with a dynamic port
- Runs migrations from `Migrations/scripts/`
- Executes tests against the temporary database
- Cleans up the container automatically

**Advantages**:
✅ No manual setup required  
✅ Works with standard .NET testing tools  
✅ Integrates with IDE test runners  
✅ Fast iteration during development  

### 2. Docker Compose (Full Environment)

**Uses**: Testcontainers (via Docker socket mount)  
**Best for**: Testing the full stack, consistent with production environment

```bash
# From project root
docker compose --profile integration-test up --build

# Clean up
docker compose --profile integration-test down -v
```

**How it works**:
- Tests run in a container with Docker socket mounted (`/var/run/docker.sock`)
- Testcontainers automatically spins up a PostgreSQL 16 container
- Runs migrations from `Migrations/scripts/`
- Executes tests against the temporary database
- Cleans up containers automatically

**Requirements**:
- Docker socket must be accessible (mounted in docker-compose.yml)
- Docker Desktop or Docker Engine running
- Resource Reaper (ryuk) is disabled via `TESTCONTAINERS_RYUK_DISABLED=true` environment variable for Docker-in-Docker compatibility

### 3. Azure Pipelines CI/CD (Automated)

**Uses**: Testcontainers  
**Best for**: CI/CD, automated testing on push

**How it works**:
- Uses `azure-pipelines-integration-tests.yml` pipeline file
- Testcontainers automatically spins up a PostgreSQL container
- Runs migrations automatically
- Executes tests against the temporary database
- Cleans up containers automatically

**Advantages**:
✅ Consistent approach across all environments  
✅ Automatic cleanup via Testcontainers  
✅ No service containers needed  
✅ No configuration files needed - uses inline credentials

**Pipeline Configuration**:
- Pipeline file: `azure-pipelines-integration-tests.yml`
- Set to `trigger: none` and `pr: none` (manual/triggered runs only)
- Docker is available by default in Azure Pipelines agents

## Consistent Testcontainers Usage

All execution modes use Testcontainers for database management:

- **Local `dotnet test`**: Testcontainers spins up PostgreSQL automatically
- **Docker Compose**: Testcontainers works via Docker socket mount
- **Azure Pipelines**: Testcontainers spins up PostgreSQL in CI environment

This ensures consistent behavior across all environments without any configuration changes or setup files.

## No Configuration Required

Integration tests use inline credentials directly in the code. Since test databases are temporary and cleaned up after tests, hardcoded test credentials are acceptable and significantly reduce setup complexity:

- **Connection strings**: Hardcoded for Testcontainers (all environments use the same approach)
- **JWT settings**: Default test values built into the test infrastructure
- **Zero setup**: Just run `dotnet test` - no config files needed!

## Further Reading

- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [xUnit Documentation](https://xunit.net/)
- [Azure Pipelines Documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/)

