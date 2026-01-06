namespace KDG.Boilerplate.Server.Models.Crm;

public class HeroSlideFilters
{
    public bool? IsActive { get; set; }
}

public class CreateHeroSlideDto
{
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateHeroSlideDto
{
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class ReorderHeroSlidesDto
{
    public List<Guid> SlideIds { get; set; } = [];
}

