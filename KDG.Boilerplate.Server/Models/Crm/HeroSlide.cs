namespace KDG.Boilerplate.Server.Models.Crm;

public class HeroSlide
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

