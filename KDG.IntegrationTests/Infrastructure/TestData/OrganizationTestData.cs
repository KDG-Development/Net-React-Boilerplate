using Dapper;
using Npgsql;

namespace KDG.IntegrationTests.Infrastructure.TestData;

/// <summary>
/// Test data helpers for organizations.
/// 
/// NOTE: Direct SQL is temporary. Organizations will be synced from NetSuite.
/// Update to use the service layer once implemented.
/// </summary>
public class OrganizationTestData
{
    private readonly Func<Task<NpgsqlConnection>> _getConnection;

    public OrganizationTestData(Func<Task<NpgsqlConnection>> getConnection)
    {
        _getConnection = getConnection;
    }

    public async Task<Guid> Create(string name)
    {
        using var connection = await _getConnection();
        
        var orgId = Guid.NewGuid();
        await connection.ExecuteAsync(@"
            INSERT INTO organizations (id, name)
            VALUES (@Id, @Name)",
            new { Id = orgId, Name = name });

        return orgId;
    }
}

