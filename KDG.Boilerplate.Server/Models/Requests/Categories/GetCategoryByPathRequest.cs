using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Models.Requests.Categories;

public class GetCategoryByPathRequest
{
    [FromQuery(Name = "path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Returns the path URL-decoded and trimmed of leading/trailing slashes.
    /// </summary>
    public string NormalizedPath => Uri.UnescapeDataString(Path).Trim('/');
}

