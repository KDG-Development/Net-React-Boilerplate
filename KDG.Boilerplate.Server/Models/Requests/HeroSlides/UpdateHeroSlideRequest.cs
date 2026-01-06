namespace KDG.Boilerplate.Server.Models.Requests.HeroSlides;

public class UpdateHeroSlideRequest
{
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

