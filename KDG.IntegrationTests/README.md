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

**Uses**: Docker Compose with explicit PostgreSQL service  
**Best for**: Testing the full stack, consistent with production environment

```bash
# From project root
docker compose --profile integration-test up --build

# Clean up
docker compose --profile integration-test down -v
```

**How it works**:
- Explicit PostgreSQL service on port 5454
- Migrations run as a separate service with inline connection string
- Tests run in a container
- Database uses tmpfs (in-memory, destroyed after tests)

### 3. Azure Pipelines CI/CD (Automated)

**Uses**: Service containers  
**Best for**: CI/CD, automated testing on push

**How it works**:
- Azure Pipelines starts a PostgreSQL service container (localhost:5432)
- Tests automatically detect CI environment (`AGENT_ID` env var)
- Uses existing service container instead of spinning up Testcontainers
- Migrations run directly against service container
- Fastest option for CI/CD

**Advantages**:
✅ No Testcontainers overhead in CI  
✅ Reuses existing PostgreSQL service  
✅ Integrated with Azure Pipelines workflow  
✅ No configuration files needed - uses inline credentials

## Environment Detection

The test infrastructure automatically detects which environment it's running in:

### Priority Order:
1. **Azure Pipelines Service Container**: If `AGENT_ID` or `TF_BUILD` env var exists and PostgreSQL available on localhost:5432
2. **Docker Compose**: If PostgreSQL is accessible at `integration-test-db:5432`
3. **Testcontainers**: Fallback - spin up automatic PostgreSQL container

This ensures optimal performance in each scenario without any configuration changes or setup files.

## No Configuration Required

Integration tests use inline credentials directly in the code. Since test databases are temporary and cleaned up after tests, hardcoded test credentials are acceptable and significantly reduce setup complexity:

- **Connection strings**: Hardcoded for each environment (Testcontainers, Docker Compose, Azure Pipelines)
- **JWT settings**: Default test values built into the test infrastructure
- **Zero setup**: Just run `dotnet test` - no config files needed!

## Further Reading

- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [xUnit Documentation](https://xunit.net/)
- [Azure Pipelines Service Containers](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/service-containers)

