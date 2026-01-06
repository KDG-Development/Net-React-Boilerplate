using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Models.Requests.Common;

public class PaginationParams
{
    public const string PageParam = "page";
    public const string PageSizeParam = "pageSize";
    
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    [FromQuery(Name = PageParam)]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    [FromQuery(Name = PageSizeParam)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? DefaultPageSize : value);
    }

    public int Offset => (Page - 1) * PageSize;
}

