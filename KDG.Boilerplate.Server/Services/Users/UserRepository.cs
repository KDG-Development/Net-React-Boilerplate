using KDG.Boilerplate.Models.DTO;
using KDG.Database.Interfaces;
using KDG.UserManagement.Interfaces;
using KDG.UserManagement.Models;
using Dapper;
using KDG.Boilerplate.Server.Models.Users;
using KDG.Boilerplate.Server.Models.ActiveRecords;

namespace KDG.Boilerplate.Services;

public interface IUserRepository {
    public Task<User?> UserLogin(UserAuth authPayload);
}

public class UserRepository : IUserRepository {
    private IDatabase<Npgsql.NpgsqlConnection,Npgsql.NpgsqlTransaction> _database;

    public UserRepository(
        IDatabase<Npgsql.NpgsqlConnection,Npgsql.NpgsqlTransaction> database
    ) {
        _database = database;
    }
    
    public async Task<User?> UserLogin(UserAuth authPayload) {
    return await _database.WithConnection(async connection => {
        var sql = @"
            select
                u.id,
                u.email,
                array_agg(distinct upg.permission_group) filter (where upg.permission_group is not null) as permission_groups,
                array_agg(distinct p.permission) filter (where p.permission is not null) as permissions
            from users u
            left join user_permission_groups upg on upg.user_id = u.id
            left join permission_group_permissions pgp on pgp.permission_group = upg.permission_group
            left join user_permissions up on up.user_id = u.id
            left join permissions p on
                p.permission = up.permission
                or p.permission = pgp.permission
            where u.email = @Email
            group by u.id";

        var result = await connection.QueryFirstOrDefaultAsync<UserActiveRecord>(sql, authPayload);
        
        if (result == null) return null;
        
        return new User {
            Id = result.Id,
            Email = result.Email,
            PermissionGroups =
                new HashSet<PermissionGroupBase>(
                    result.PermissionGroups.Select(group => new PermissionGroupBase(group))
                ),
            Permissions =
                new HashSet<PermissionBase>(
                    result.Permissions.Select(permission => new PermissionBase(permission))
                ),
        };
    });
}
}