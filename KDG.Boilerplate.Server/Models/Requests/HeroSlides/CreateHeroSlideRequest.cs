namespace KDG.Boilerplate.Server.Models.Requests.HeroSlides;

public class CreateHeroSlideRequest
{
    public IFormFile? Image { get; set; }
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

