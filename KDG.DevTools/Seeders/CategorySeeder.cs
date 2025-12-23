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
        var categoryNames = new[]
        {
            "Electronics", "Clothing", "Home & Garden", "Sports & Outdoors",
            "Books", "Toys & Games", "Health & Beauty", "Automotive",
            "Office Supplies", "Pet Supplies", "Food & Grocery", "Jewelry",
            "Music & Instruments", "Art & Crafts", "Baby & Kids"
        };

        // Calculate how many root categories and subcategories to create
        var rootCount = Math.Max(1, count / 3);
        var subCount = count - rootCount;

        var insertedCount = 0;
        var rootCategoryIds = new List<Guid>();

        // Insert root categories
        var selectedRoots = faker.PickRandom(categoryNames, Math.Min(rootCount, categoryNames.Length)).ToList();
        
        foreach (var name in selectedRoots)
        {
            var id = Guid.NewGuid();
            var description = faker.Commerce.Department() + " - " + faker.Lorem.Sentence();

            await _database.WithConnection(async conn =>
            {
                await conn.ExecuteAsync(
                    "INSERT INTO categories (id, name, description) VALUES (@Id, @Name, @Description)",
                    new { Id = id, Name = name, Description = description }
                );
                return true;
            });

            rootCategoryIds.Add(id);
            insertedCount++;
            Console.WriteLine($"  Created root category: {name}");
        }

        // Insert subcategories
        for (var i = 0; i < subCount && rootCategoryIds.Count > 0; i++)
        {
            var parentId = faker.PickRandom(rootCategoryIds);
            var subName = faker.Commerce.Categories(1)[0] + " " + faker.Commerce.ProductAdjective();
            var description = faker.Lorem.Sentence();

            await _database.WithConnection(async conn =>
            {
                await conn.ExecuteAsync(
                    "INSERT INTO categories (id, parent_id, name, description) VALUES (@Id, @ParentId, @Name, @Description)",
                    new { Id = Guid.NewGuid(), ParentId = parentId, Name = subName, Description = description }
                );
                return true;
            });

            insertedCount++;
            Console.WriteLine($"  Created subcategory: {subName}");
        }

        return insertedCount;
    }
}
