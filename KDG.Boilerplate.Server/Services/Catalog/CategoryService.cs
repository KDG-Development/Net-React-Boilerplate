using KDG.Boilerplate.Server.Models.Catalog;

namespace KDG.Boilerplate.Services;

public interface ICategoryService
{
    Task<Dictionary<string, CategoryNode>> GetCategoriesAsync();
    Task<CategoryDetailResponse?> GetCategoryByPathAsync(string path);
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Dictionary<string, CategoryNode>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return BuildCategoryTree(categories);
    }

    public async Task<CategoryDetailResponse?> GetCategoryByPathAsync(string path)
    {
        var category = await _categoryRepository.GetByPathAsync(path);
        if (category == null) return null;

        var ancestors = await _categoryRepository.GetAncestorsAsync(category.Id);
        var children = await _categoryRepository.GetChildrenAsync(category.Id);

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

