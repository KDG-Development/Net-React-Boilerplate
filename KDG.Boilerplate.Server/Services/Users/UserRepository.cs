using KDG.Boilerplate.Server.Models.Entities.Users;
using Npgsql;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface IUserRepository
{
    Task<string?> GetPasswordHashAsync(NpgsqlConnection conn, string email);
    Task<UserActiveRecord?> GetUserWithPermissionsAsync(NpgsqlConnection conn, string email);
}

public class UserRepository : IUserRepository
{
    public async Task<string?> GetPasswordHashAsync(NpgsqlConnection conn, string email)
    {
        var sql = @"
            select up.password_hash
            from users u
            join user_passwords up on up.user_id = u.id and up.deactivated is null
            where u.email = @Email";

        return await conn.QueryFirstOrDefaultAsync<string>(sql, new { Email = email });
    }

    public async Task<UserActiveRecord?> GetUserWithPermissionsAsync(NpgsqlConnection conn, string email)
    {
        var sql = @"
            select
                u.id,
                u.email,
                u.organization_id AS OrganizationId,
                o.name AS OrganizationName,
                coalesce(array_agg(distinct upg.permission_group) filter (where upg.permission_group is not null), ARRAY[]::text[]) as PermissionGroups,
                coalesce(array_agg(distinct p.permission) filter (where p.permission is not null), ARRAY[]::text[]) as Permissions
            from users u
            left join organizations o on o.id = u.organization_id
            left join user_permission_groups upg on upg.user_id = u.id
            left join permission_group_permissions pgp on pgp.permission_group = upg.permission_group
            left join user_permissions up on up.user_id = u.id
            left join permissions p on
                p.permission = up.permission
                or p.permission = pgp.permission
            where u.email = @Email
            group by u.id, u.organization_id, o.name";

        return await conn.QueryFirstOrDefaultAsync<UserActiveRecord>(sql, new { Email = email });
    }
}
