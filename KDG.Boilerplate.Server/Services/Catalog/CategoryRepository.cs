using KDG.Boilerplate.Server.Models.Catalog;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
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
            var sql = "SELECT id, parent_id AS ParentId, name FROM categories";
            return (await connection.QueryAsync<Category>(sql)).ToList();
        });
    }
}
