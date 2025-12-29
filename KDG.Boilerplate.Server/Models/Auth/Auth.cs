namespace KDG.Boilerplate.Models.Auth
{
  public static class Permissions
  {
    public const string CreateUser = "add-user";
    public const string EditUser = "edit-user";
  }

  public static class PermissionGroups
  {
    public const string SystemAdmin = "system-admin";
    public const string CustomerAdmin = "customer-admin";
    public const string CustomerUser = "customer-user";
  }
}
