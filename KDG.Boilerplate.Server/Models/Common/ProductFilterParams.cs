using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Models.Common;

public class ProductFilterParams
{
    public const string MinPriceParam = "minPrice";
    public const string MaxPriceParam = "maxPrice";
    public const string SearchParam = "search";
    public const string FavoritesOnlyParam = "favoritesOnly";

    [FromQuery(Name = MinPriceParam)]
    public decimal? MinPrice { get; set; }

    [FromQuery(Name = MaxPriceParam)]
    public decimal? MaxPrice { get; set; }

    [FromQuery(Name = SearchParam)]
    public string? Search { get; set; }

    [FromQuery(Name = FavoritesOnlyParam)]
    public bool FavoritesOnly { get; set; }

    public bool HasPriceFilter => MinPrice.HasValue || MaxPrice.HasValue;
    public bool HasSearchFilter => !string.IsNullOrWhiteSpace(Search);

    /// <summary>
    /// Returns the search term formatted for PostgreSQL tsquery with prefix matching.
    /// Example: "laptop bag" becomes "laptop:* & bag:*"
    /// </summary>
    public string? GetSearchQuery()
    {
        if (!HasSearchFilter)
            return null;

        // Remove special characters that could break tsquery, keep only alphanumeric and spaces
        var cleaned = Regex.Replace(Search!, @"[^\w\s]", " ");
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length == 0)
            return null;

        // Add :* to each word for prefix matching, join with & for AND
        return string.Join(" & ", words.Select(w => $"{w}:*"));
    }
}

