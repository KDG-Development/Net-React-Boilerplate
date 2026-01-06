using KDG.Boilerplate.Server.Models.Entities.Catalog;
using Npgsql;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(NpgsqlConnection conn);
    Task<Category?> GetByPathAsync(NpgsqlConnection conn, string fullPath);
    Task<List<Category>> GetChildrenAsync(NpgsqlConnection conn, Guid parentId);
    Task<List<Category>> GetAncestorsAsync(NpgsqlConnection conn, Guid categoryId);
}

public class CategoryRepository : ICategoryRepository
{
    public async Task<List<Category>> GetAllAsync(NpgsqlConnection conn)
    {
        var sql = @"
            SELECT id, parent_id AS ParentId, name, full_path AS FullPath
            FROM category_slugs
            ORDER BY full_path";
        return (await conn.QueryAsync<Category>(sql)).ToList();
    }

    public async Task<Category?> GetByPathAsync(NpgsqlConnection conn, string slug)
    {
        var sql = @"
            SELECT id, parent_id AS ParentId, name, full_path AS FullPath
            FROM category_slugs
            WHERE full_path = @Slug OR full_path LIKE '%/' || @Slug";
        return await conn.QueryFirstOrDefaultAsync<Category>(sql, new { Slug = slug });
    }

    public async Task<List<Category>> GetChildrenAsync(NpgsqlConnection conn, Guid parentId)
    {
        var sql = @"
            SELECT id, parent_id AS ParentId, name, full_path AS FullPath
            FROM category_slugs
            WHERE parent_id = @ParentId
            ORDER BY name";
        return (await conn.QueryAsync<Category>(sql, new { ParentId = parentId })).ToList();
    }

    public async Task<List<Category>> GetAncestorsAsync(NpgsqlConnection conn, Guid categoryId)
    {
        var sql = @"
            WITH RECURSIVE ancestors AS (
                SELECT id, parent_id, name FROM categories WHERE id = @CategoryId
                UNION ALL
                SELECT c.id, c.parent_id, c.name FROM categories c
                JOIN ancestors a ON c.id = a.parent_id
            )
            SELECT 
                cs.id, 
                cs.parent_id AS ParentId, 
                cs.name, 
                cs.full_path AS FullPath
            FROM ancestors a
            JOIN category_slugs cs ON cs.id = a.id
            ORDER BY cs.full_path";
        return (await conn.QueryAsync<Category>(sql, new { CategoryId = categoryId })).ToList();
    }
}
