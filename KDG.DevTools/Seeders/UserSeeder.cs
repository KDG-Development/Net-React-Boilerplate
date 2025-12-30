using System.Reflection;
using Dapper;
using KDG.Boilerplate.Models.Auth;
using KDG.Database;

namespace KDG.DevTools.Seeders;

public class UserSeeder
{
    private readonly PostgreSQL _database;
    private readonly List<(string Name, string Value)> _availableGroups;

    public UserSeeder(PostgreSQL database)
    {
        _database = database;
        _availableGroups = DiscoverPermissionGroups();
    }

    private static List<(string Name, string Value)> DiscoverPermissionGroups()
    {
        return typeof(PermissionGroups)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (Name: f.Name, Value: (string)f.GetRawConstantValue()!))
            .ToList();
    }

    public async Task SeedAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=== User Seeder ===");
        Console.WriteLine();

        // Prompt for email
        Console.Write("Email: ");
        var email = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("Email is required.");
            return;
        }

        // Prompt for password
        Console.Write("Password: ");
        var password = ReadPassword();
        Console.WriteLine();
        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("Password is required.");
            return;
        }

        // Display available permission groups
        Console.WriteLine();
        Console.WriteLine("Available permission groups:");
        for (var i = 0; i < _availableGroups.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {_availableGroups[i].Name}");
        }
        Console.Write("Select group (number, or empty for none): ");
        var groupInput = Console.ReadLine()?.Trim();

        string? selectedGroup = null;
        if (!string.IsNullOrWhiteSpace(groupInput) && int.TryParse(groupInput, out var groupIndex))
        {
            if (groupIndex >= 1 && groupIndex <= _availableGroups.Count)
            {
                selectedGroup = _availableGroups[groupIndex - 1].Value;
            }
            else
            {
                Console.Error.WriteLine($"Invalid selection. Must be 1-{_availableGroups.Count}.");
                return;
            }
        }

        // Prompt for organization name
        Console.WriteLine();
        Console.Write("Organization name (or empty to skip): ");
        var organizationName = Console.ReadLine()?.Trim();

        Console.WriteLine();
        Console.WriteLine($"Creating user {email}...");

        try
        {
            await CreateUser(email, password, selectedGroup, organizationName);

            if (selectedGroup != null)
            {
                var groupName = _availableGroups.First(g => g.Value == selectedGroup).Name;
                Console.WriteLine($"Assigned group: {groupName}");
            }
            if (!string.IsNullOrWhiteSpace(organizationName))
            {
                Console.WriteLine($"Created organization: {organizationName}");
            }
            Console.WriteLine("User created successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating user: {ex.Message}");
        }
    }

    private async Task CreateUser(string email, string password, string? permissionGroup, string? organizationName)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var salt = passwordHash.Substring(0, 29); // BCrypt salt is first 29 chars

        await _database.WithConnection(async conn =>
        {
            // Create organization if provided
            Guid? organizationId = null;
            if (!string.IsNullOrWhiteSpace(organizationName))
            {
                organizationId = await conn.QuerySingleAsync<Guid>(
                    @"INSERT INTO organizations (name) VALUES (@Name) RETURNING id",
                    new { Name = organizationName }
                );
            }

            // Upsert user with organization
            var userId = await conn.QuerySingleAsync<Guid>(
                @"INSERT INTO users (email, organization_id) VALUES (@Email, @OrganizationId)
                  ON CONFLICT (email) DO UPDATE SET email = excluded.email, organization_id = @OrganizationId
                  RETURNING id",
                new { Email = email, OrganizationId = organizationId }
            );

            // Deactivate existing passwords and insert new one
            await conn.ExecuteAsync(
                @"UPDATE user_passwords SET deactivated = now() 
                  WHERE user_id = @UserId AND deactivated IS NULL",
                new { UserId = userId }
            );

            await conn.ExecuteAsync(
                @"INSERT INTO user_passwords (user_id, password_hash, salt) 
                  VALUES (@UserId, @PasswordHash, @Salt)",
                new { UserId = userId, PasswordHash = passwordHash, Salt = salt }
            );

            // Handle permission group assignment
            if (permissionGroup != null)
            {
                // Remove existing permission groups and assign new one
                await conn.ExecuteAsync(
                    "DELETE FROM user_permission_groups WHERE user_id = @UserId",
                    new { UserId = userId }
                );

                await conn.ExecuteAsync(
                    @"INSERT INTO user_permission_groups (user_id, permission_group) 
                      VALUES (@UserId, @PermissionGroup)",
                    new { UserId = userId, PermissionGroup = permissionGroup }
                );
            }

            return true;
        });
    }

    private static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        return password.ToString();
    }
}

