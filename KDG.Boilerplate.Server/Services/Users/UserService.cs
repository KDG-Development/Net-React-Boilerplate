using KDG.Boilerplate.Models.DTO;
using KDG.Boilerplate.Server.Models.Organizations;
using KDG.Boilerplate.Server.Models.Users;
using KDG.Database.Interfaces;
using KDG.UserManagement.Models;
using Npgsql;

namespace KDG.Boilerplate.Services;

public interface IUserService
{
    Task<User?> UserLoginAsync(UserAuth authPayload);
}

public class UserService : IUserService
{
    private readonly IDatabase<NpgsqlConnection, NpgsqlTransaction> _database;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IDatabase<NpgsqlConnection, NpgsqlTransaction> database,
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _database = database;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> UserLoginAsync(UserAuth authPayload)
    {
        return await _database.WithConnection(async conn =>
        {
            var passwordHash = await _userRepository.GetPasswordHashAsync(conn, authPayload.Email);

            if (passwordHash == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", authPayload.Email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(authPayload.Password, passwordHash))
            {
                _logger.LogWarning("Invalid password attempt for user: {Email}", authPayload.Email);
                return null;
            }

            var result = await _userRepository.GetUserWithPermissionsAsync(conn, authPayload.Email);

            if (result == null) return null;

            return new User
            {
                Id = result.Id,
                Email = result.Email,
                Organization = result.OrganizationId.HasValue
                    ? new OrganizationMeta { Id = result.OrganizationId.Value, Name = result.OrganizationName ?? string.Empty }
                    : null,
                PermissionGroups =
                    new HashSet<PermissionGroupBase>(
                        (result.PermissionGroups ?? []).Select(group => new PermissionGroupBase(group))
                    ),
                Permissions =
                    new HashSet<PermissionBase>(
                        (result.Permissions ?? []).Select(permission => new PermissionBase(permission))
                    ),
            };
        });
    }
}

