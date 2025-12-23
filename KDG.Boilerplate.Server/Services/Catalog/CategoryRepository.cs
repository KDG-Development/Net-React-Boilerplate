using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByPathAsync(string fullPath);
    Task<List<Category>> GetChildrenAsync(Guid parentId);
    Task<List<Category>> GetAncestorsAsync(Guid categoryId);
}

public class CategoryRepository : ICategoryRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;

    public CategoryRepository(IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database)
    {
        _database = database;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT id, parent_id AS ParentId, name, full_path AS FullPath
                FROM category_slugs
                ORDER BY full_path";
            return (await connection.QueryAsync<Category>(sql)).ToList();
        });
    }

    public async Task<Category?> GetByPathAsync(string fullPath)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT id, parent_id AS ParentId, name, full_path AS FullPath
                FROM category_slugs
                WHERE full_path = @FullPath";
            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { FullPath = fullPath });
        });
    }

    public async Task<List<Category>> GetChildrenAsync(Guid parentId)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT id, parent_id AS ParentId, name, full_path AS FullPath
                FROM category_slugs
                WHERE parent_id = @ParentId
                ORDER BY name";
            return (await connection.QueryAsync<Category>(sql, new { ParentId = parentId })).ToList();
        });
    }

    public async Task<List<Category>> GetAncestorsAsync(Guid categoryId)
    {
        return await _database.WithConnection(async connection =>
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
            return (await connection.QueryAsync<Category>(sql, new { CategoryId = categoryId })).ToList();
        });
    }
}
