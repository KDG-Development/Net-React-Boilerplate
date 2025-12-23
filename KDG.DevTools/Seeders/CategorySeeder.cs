using Bogus;
using Dapper;
using KDG.Database;

namespace KDG.DevTools.Seeders;

public class CategorySeeder : ISeeder
{
    private readonly PostgreSQL _database;

    public string Name => "Categories";

    public CategorySeeder(PostgreSQL database)
    {
        _database = database;
    }

    public async Task<int> SeedAsync(int count)
    {
        var faker = new Faker();
        var insertedCount = 0;

        // Track categories at each level for parent selection
        var level1Ids = new List<Guid>();
        var level2Ids = new List<Guid>();
        var level3Ids = new List<Guid>();

        // Distribution: ~20% root, ~30% level 2, ~30% level 3, ~20% level 4
        var rootCount = Math.Max(3, count / 5);
        var level2Count = count * 3 / 10;
        var level3Count = count * 3 / 10;
        var level4Count = count - rootCount - level2Count - level3Count;

        // Create root categories (level 1)
        for (var i = 0; i < rootCount; i++)
        {
            var id = Guid.NewGuid();
            var name = faker.Commerce.Department();

            await InsertCategory(id, null, name);
            level1Ids.Add(id);
            insertedCount++;
            Console.WriteLine($"  L1: {name}");
        }

        // Create level 2 categories
        for (var i = 0; i < level2Count && level1Ids.Count > 0; i++)
        {
            var id = Guid.NewGuid();
            var parentId = faker.PickRandom(level1Ids);
            var name = faker.Commerce.Categories(1)[0];

            await InsertCategory(id, parentId, name);
            level2Ids.Add(id);
            insertedCount++;
            Console.WriteLine($"    L2: {name}");
        }

        // Create level 3 categories
        for (var i = 0; i < level3Count && level2Ids.Count > 0; i++)
        {
            var id = Guid.NewGuid();
            var parentId = faker.PickRandom(level2Ids);
            var name = faker.Commerce.ProductAdjective() + " " + faker.Commerce.ProductMaterial();

            await InsertCategory(id, parentId, name);
            level3Ids.Add(id);
            insertedCount++;
            Console.WriteLine($"      L3: {name}");
        }

        // Create level 4 categories
        for (var i = 0; i < level4Count && level3Ids.Count > 0; i++)
        {
            var id = Guid.NewGuid();
            var parentId = faker.PickRandom(level3Ids);
            var name = faker.Commerce.ProductName();

            await InsertCategory(id, parentId, name);
            insertedCount++;
            Console.WriteLine($"        L4: {name}");
        }

        return insertedCount;
    }

    private async Task InsertCategory(Guid id, Guid? parentId, string name)
    {
        await _database.WithConnection(async conn =>
        {
            await conn.ExecuteAsync(
                "INSERT INTO categories (id, parent_id, name) VALUES (@Id, @ParentId, @Name)",
                new { Id = id, ParentId = parentId, Name = name }
            );
            return true;
        });
    }
}
