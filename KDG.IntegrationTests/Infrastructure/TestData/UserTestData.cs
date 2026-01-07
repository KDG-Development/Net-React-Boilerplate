using Dapper;
using Npgsql;

namespace KDG.IntegrationTests.Infrastructure.TestData;

/// <summary>
/// Test data helpers for users and permissions.
/// 
/// NOTE: Direct SQL is temporary. Users will be synced from NetSuite.
/// Update to use the service layer once implemented.
/// </summary>
public class UserTestData
{
    private readonly Func<Task<NpgsqlConnection>> _getConnection;

    public UserTestData(Func<Task<NpgsqlConnection>> getConnection)
    {
        _getConnection = getConnection;
    }

    /// <summary>
    /// Creates a test user. For auth tests, provide email and password.
    /// For other tests, these will be auto-generated.
    /// </summary>
    public async Task<Guid> Create(
        string? email = null,
        string? password = null,
        Guid? organizationId = null)
    {
        using var connection = await _getConnection();
        
        var userId = Guid.NewGuid();
        email ??= $"test-{userId}@test.example.com";

        await connection.ExecuteAsync(@"
            INSERT INTO users (id, email, organization_id)
            VALUES (@Id, @Email, @OrganizationId)",
            new { Id = userId, Email = email, OrganizationId = organizationId });

        if (password != null)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            await connection.ExecuteAsync(@"
                INSERT INTO user_passwords (user_id, password_hash, salt)
                VALUES (@UserId, @PasswordHash, @Salt)",
                new { UserId = userId, PasswordHash = passwordHash, Salt = salt });
        }

        return userId;
    }

    public async Task AssignPermissionGroup(Guid userId, string permissionGroup)
    {
        using var connection = await _getConnection();
        
        await connection.ExecuteAsync(@"
            INSERT INTO permission_groups (permission_group)
            VALUES (@PermissionGroup)
            ON CONFLICT DO NOTHING",
            new { PermissionGroup = permissionGroup });

        await connection.ExecuteAsync(@"
            INSERT INTO user_permission_groups (user_id, permission_group)
            VALUES (@UserId, @PermissionGroup)",
            new { UserId = userId, PermissionGroup = permissionGroup });
    }
}

