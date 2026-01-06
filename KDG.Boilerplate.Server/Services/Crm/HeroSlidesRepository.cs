using KDG.Boilerplate.Server.Models.Crm;
using KDG.Database.Interfaces;
using Dapper;

namespace KDG.Boilerplate.Services.Crm;

public interface IHeroSlidesRepository
{
    Task<List<HeroSlide>> GetAllAsync(HeroSlideFilters? filters = null);
    Task<HeroSlide?> GetByIdAsync(Guid id);
    Task<HeroSlide> CreateAsync(HeroSlide slide);
    Task<HeroSlide?> UpdateAsync(Guid id, UpdateHeroSlideDto dto);
    Task<HeroSlide?> UpdateImageUrlAsync(Guid id, string imageUrl);
    Task<bool> DeleteAsync(Guid id);
    Task ReorderAsync(List<Guid> slideIds);
    Task<int> GetNextSortOrderAsync();
}

public class HeroSlidesRepository : IHeroSlidesRepository
{
    private readonly IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> _database;

    public HeroSlidesRepository(IDatabase<Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction> database)
    {
        _database = database;
    }

    public async Task<List<HeroSlide>> GetAllAsync(HeroSlideFilters? filters = null)
    {
        return await _database.WithConnection(async connection =>
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (filters?.IsActive.HasValue == true)
            {
                conditions.Add("is_active = @IsActive");
                parameters.Add("IsActive", filters.IsActive.Value);
            }

            var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            var sql = $@"
                SELECT 
                    id, image_url AS ImageUrl,
                    button_text AS ButtonText, button_url AS ButtonUrl,
                    sort_order AS SortOrder, is_active AS IsActive
                FROM hero_slides
                {whereClause}
                ORDER BY sort_order";
            return (await connection.QueryAsync<HeroSlide>(sql, parameters)).ToList();
        });
    }

    public async Task<HeroSlide?> GetByIdAsync(Guid id)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                SELECT 
                    id, image_url AS ImageUrl,
                    button_text AS ButtonText, button_url AS ButtonUrl,
                    sort_order AS SortOrder, is_active AS IsActive
                FROM hero_slides
                WHERE id = @Id";
            return await connection.QueryFirstOrDefaultAsync<HeroSlide>(sql, new { Id = id });
        });
    }

    public async Task<HeroSlide> CreateAsync(HeroSlide slide)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                INSERT INTO hero_slides (id, image_url, button_text, button_url, sort_order, is_active)
                VALUES (@Id, @ImageUrl, @ButtonText, @ButtonUrl, @SortOrder, @IsActive)
                RETURNING 
                    id, image_url AS ImageUrl,
                    button_text AS ButtonText, button_url AS ButtonUrl,
                    sort_order AS SortOrder, is_active AS IsActive";
            return await connection.QuerySingleAsync<HeroSlide>(sql, slide);
        });
    }

    public async Task<HeroSlide?> UpdateAsync(Guid id, UpdateHeroSlideDto dto)
    {
        return await _database.WithConnection(async connection =>
        {
            var updates = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            if (dto.ButtonText != null) { updates.Add("button_text = @ButtonText"); parameters.Add("ButtonText", dto.ButtonText); }
            if (dto.ButtonUrl != null) { updates.Add("button_url = @ButtonUrl"); parameters.Add("ButtonUrl", dto.ButtonUrl); }
            if (dto.SortOrder.HasValue) { updates.Add("sort_order = @SortOrder"); parameters.Add("SortOrder", dto.SortOrder.Value); }
            if (dto.IsActive.HasValue) { updates.Add("is_active = @IsActive"); parameters.Add("IsActive", dto.IsActive.Value); }

            if (updates.Count == 0) return await GetByIdAsync(id);

            var sql = $@"
                UPDATE hero_slides
                SET {string.Join(", ", updates)}
                WHERE id = @Id
                RETURNING 
                    id, image_url AS ImageUrl,
                    button_text AS ButtonText, button_url AS ButtonUrl,
                    sort_order AS SortOrder, is_active AS IsActive";
            return await connection.QueryFirstOrDefaultAsync<HeroSlide>(sql, parameters);
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = "DELETE FROM hero_slides WHERE id = @Id";
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        });
    }

    public async Task ReorderAsync(List<Guid> slideIds)
    {
        await _database.WithConnection(async connection =>
        {
            for (var i = 0; i < slideIds.Count; i++)
            {
                var sql = "UPDATE hero_slides SET sort_order = @SortOrder WHERE id = @Id";
                await connection.ExecuteAsync(sql, new { Id = slideIds[i], SortOrder = i });
            }
            return true;
        });
    }

    public async Task<int> GetNextSortOrderAsync()
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = "SELECT COALESCE(MAX(sort_order), -1) + 1 FROM hero_slides";
            return await connection.QuerySingleAsync<int>(sql);
        });
    }

    public async Task<HeroSlide?> UpdateImageUrlAsync(Guid id, string imageUrl)
    {
        return await _database.WithConnection(async connection =>
        {
            var sql = @"
                UPDATE hero_slides
                SET image_url = @ImageUrl
                WHERE id = @Id
                RETURNING 
                    id, image_url AS ImageUrl,
                    button_text AS ButtonText, button_url AS ButtonUrl,
                    sort_order AS SortOrder, is_active AS IsActive";
            return await connection.QueryFirstOrDefaultAsync<HeroSlide>(sql, new { Id = id, ImageUrl = imageUrl });
        });
    }
}

