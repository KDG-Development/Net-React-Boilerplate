namespace KDG.Boilerplate.Server.Models.Entities.Users;

public record UserActiveRecord
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public Guid? OrganizationId { get; init; }
    public string? OrganizationName { get; init; }
    public string[] PermissionGroups { get; init; } = [];
    public string[] Permissions { get; init; } = [];
}

