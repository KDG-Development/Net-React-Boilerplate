using KDG.Boilerplate.Server.Models.Entities.Catalog;
using KDG.Database.Interfaces;
using Npgsql;

namespace KDG.Boilerplate.Services;

public interface ICategoryService
{
    Task<Dictionary<string, CategoryNode>> GetCategoriesAsync();
    Task<CategoryDetailResponse?> GetCategoryByPathAsync(string path);
}

public class CategoryService : ICategoryService
{
    private readonly IDatabase<NpgsqlConnection, NpgsqlTransaction> _database;
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(
        IDatabase<NpgsqlConnection, NpgsqlTransaction> database,
        ICategoryRepository categoryRepository)
    {
        _database = database;
        _categoryRepository = categoryRepository;
    }

    public async Task<Dictionary<string, CategoryNode>> GetCategoriesAsync()
    {
        var categories = await _database.WithConnection(async conn =>
            await _categoryRepository.GetAllAsync(conn));
        return BuildCategoryTree(categories);
    }

    public async Task<CategoryDetailResponse?> GetCategoryByPathAsync(string path)
    {
        return await _database.WithConnection(async conn =>
        {
            var category = await _categoryRepository.GetByPathAsync(conn, path);
            if (category == null) return null;

            var ancestors = await _categoryRepository.GetAncestorsAsync(conn, category.Id);
            var children = await _categoryRepository.GetChildrenAsync(conn, category.Id);

            return new CategoryDetailResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = GetSlug(category.FullPath),
                Breadcrumbs = ancestors.Select(a => new CategoryBreadcrumb
                {
                    Id = a.Id,
                    Name = a.Name,
                    Slug = GetSlug(a.FullPath)
                }).ToList(),
                Subcategories = children.Select(c => new SubcategoryInfo
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = GetSlug(c.FullPath)
                }).ToList()
            };
        });
    }

    private static string GetSlug(string fullPath)
    {
        var lastSlash = fullPath.LastIndexOf('/');
        return lastSlash >= 0 ? fullPath[(lastSlash + 1)..] : fullPath;
    }

    private static Dictionary<string, CategoryNode> BuildCategoryTree(List<Category> categories)
    {
        var childrenByParent = categories
            .Where(c => c.ParentId.HasValue)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new Dictionary<string, CategoryNode>();
        var roots = categories.Where(c => !c.ParentId.HasValue);

        foreach (var root in roots)
        {
            result[root.Id.ToString()] = BuildNode(root, childrenByParent);
        }

        return result;
    }

    private static CategoryNode BuildNode(Category category, Dictionary<Guid, List<Category>> childrenByParent)
    {
        var node = new CategoryNode 
        { 
            Label = category.Name,
            Slug = GetSlug(category.FullPath)
        };

        if (childrenByParent.TryGetValue(category.Id, out var children) && children.Count > 0)
        {
            node.Children = new Dictionary<string, CategoryNode>();
            foreach (var child in children)
            {
                node.Children[child.Id.ToString()] = BuildNode(child, childrenByParent);
            }
        }

        return node;
    }
}
