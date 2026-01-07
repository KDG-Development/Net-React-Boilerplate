using KDG.Boilerplate.Services;
using KDG.IntegrationTests.Infrastructure;

namespace KDG.IntegrationTests.Tests.Catalog;

/// <summary>
/// Integration tests for category navigation that verify the catalog
/// browsing experience works correctly with real hierarchical data.
/// 
/// Business context: Category navigation is how customers discover products.
/// Broken navigation means customers can't find what they're looking for,
/// leading to abandoned sessions and lost sales.
/// </summary>
public class CategoryNavigationTests : IntegrationTestBase
{
    public CategoryNavigationTests(DatabaseTestFixture fixture) : base(fixture) { }

    /// <summary>
    /// Derives a slug from a category name (same logic the DB view uses).
    /// </summary>
    private static string ToSlug(string name) => 
        name.ToLowerInvariant().Replace(" ", "-");

    /// <summary>
    /// Verifies that GetCategoriesAsync builds a nested tree structure.
    /// 
    /// Business context: The category navigation UI displays categories as
    /// a hierarchical tree (e.g., collapsible sidebar). The service must
    /// transform flat database rows into a nested structure.
    /// 
    /// Real-world scenario: Customer sees "Electronics" with expandable
    /// children "Computers" and "Phones".
    /// </summary>
    [Fact]
    public async Task GetCategories_BuildsNestedTree()
    {
        await TestData.Catalog.CleanupTestCategories();
        
        var rootId = await TestData.Catalog.CreateCategory("Test Root Electronics");
        var childId = await TestData.Catalog.CreateCategory("Test Sub Computers", rootId);
        await TestData.Catalog.CreateCategory("Test Leaf Laptops", childId);

        try
        {
            var categoryService = GetService<ICategoryService>();
            var tree = await categoryService.GetCategoriesAsync();

            var rootNode = tree.Values.FirstOrDefault(n => n.Label == "Test Root Electronics");
            Assert.NotNull(rootNode);
            Assert.NotNull(rootNode.Children);
            
            var childNode = rootNode.Children.Values.FirstOrDefault(n => n.Label == "Test Sub Computers");
            Assert.NotNull(childNode);
            
            // Verify slugs are generated
            Assert.Equal("test-root-electronics", rootNode.Slug);
        }
        finally
        {
            await TestData.Catalog.CleanupTestCategories();
        }
    }

    /// <summary>
    /// Verifies that GetCategoryByPath returns breadcrumbs in correct order.
    /// 
    /// Business context: Breadcrumbs enable navigation back up the category
    /// tree, from most general to most specific. Missing or misordered
    /// breadcrumbs would strand users.
    /// 
    /// Real-world scenario: Customer viewing "Electronics > Computers > Laptops"
    /// clicks "Electronics" in breadcrumb to browse other electronics.
    /// </summary>
    [Fact]
    public async Task GetCategoryByPath_ReturnsBreadcrumbsInOrder()
    {
        await TestData.Catalog.CleanupTestCategories();
        
        var level1 = await TestData.Catalog.CreateCategory("Test Root Level1");
        var level2 = await TestData.Catalog.CreateCategory("Test Sub Level2", level1);
        await TestData.Catalog.CreateCategory("Test Leaf Level3", level2);

        try
        {
            var categoryService = GetService<ICategoryService>();
            
            var categoryDetail = await categoryService.GetCategoryByPathAsync(ToSlug("Test Leaf Level3"));

            Assert.NotNull(categoryDetail);
            Assert.Equal(3, categoryDetail.Breadcrumbs.Count);
            Assert.Equal("Test Root Level1", categoryDetail.Breadcrumbs[0].Name);
            Assert.Equal("Test Sub Level2", categoryDetail.Breadcrumbs[1].Name);
            Assert.Equal("Test Leaf Level3", categoryDetail.Breadcrumbs[2].Name);
        }
        finally
        {
            await TestData.Catalog.CleanupTestCategories();
        }
    }

    /// <summary>
    /// Verifies that GetCategoryByPath returns immediate child categories.
    /// 
    /// Business context: When viewing a category, users need to see what
    /// subcategories are available for further drilling down.
    /// 
    /// Real-world scenario: Customer viewing "Electronics" sees subcategories
    /// "Computers", "Phones", "Audio" to continue browsing.
    /// </summary>
    [Fact]
    public async Task GetCategoryByPath_ReturnsSubcategories()
    {
        await TestData.Catalog.CleanupTestCategories();
        
        var parentId = await TestData.Catalog.CreateCategory("Test Root Parent");
        await TestData.Catalog.CreateCategory("Test Sub Child1", parentId);
        await TestData.Catalog.CreateCategory("Test Sub Child2", parentId);

        try
        {
            var categoryService = GetService<ICategoryService>();
            
            var categoryDetail = await categoryService.GetCategoryByPathAsync(ToSlug("Test Root Parent"));

            Assert.NotNull(categoryDetail);
            Assert.Equal(2, categoryDetail.Subcategories.Count);
        }
        finally
        {
            await TestData.Catalog.CleanupTestCategories();
        }
    }

    /// <summary>
    /// Verifies that non-existent paths return null.
    /// 
    /// Business context: Invalid category URLs should return null so the
    /// controller can respond with 404, not an error.
    /// 
    /// Real-world scenario: Customer has bookmarked old category URL that
    /// no longer exists. They get a friendly "not found" page.
    /// </summary>
    [Fact]
    public async Task GetCategoryByPath_NonExistent_ReturnsNull()
    {
        var categoryService = GetService<ICategoryService>();
        
        var result = await categoryService.GetCategoryByPathAsync("non-existent-path");

        Assert.Null(result);
    }
}
