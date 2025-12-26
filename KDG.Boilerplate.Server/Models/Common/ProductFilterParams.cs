using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Models.Common;

public class ProductFilterParams
{
    public const string MinPriceParam = "minPrice";
    public const string MaxPriceParam = "maxPrice";

    [FromQuery(Name = MinPriceParam)]
    public decimal? MinPrice { get; set; }

    [FromQuery(Name = MaxPriceParam)]
    public decimal? MaxPrice { get; set; }

    public bool HasPriceFilter => MinPrice.HasValue || MaxPrice.HasValue;
}

