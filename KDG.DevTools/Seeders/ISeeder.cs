namespace KDG.DevTools.Seeders;

public interface ISeeder
{
    string Name { get; }
    Task<int> SeedAsync(int count);
}

