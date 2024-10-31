using System;
using KDG.UserManagement.Interfaces;

namespace KDG.Boilerplate.Server.Models.ActiveRecords;

public record UserActiveRecord
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string[] PermissionGroups { get; init; } = [];
    public string[] Permissions { get; init; } = [];
}

