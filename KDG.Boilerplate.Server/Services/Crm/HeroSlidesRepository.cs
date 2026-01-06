using KDG.Boilerplate.Server.Models.Crm;
using Npgsql;
using Dapper;

namespace KDG.Boilerplate.Services.Crm;

public interface IHeroSlidesRepository
{
    Task<List<HeroSlide>> GetAllAsync(NpgsqlConnection conn, HeroSlideFilters? filters = null);
    Task<HeroSlide?> GetByIdAsync(NpgsqlConnection conn, Guid id);
    Task<HeroSlide> CreateAsync(NpgsqlTransaction t, HeroSlide slide);
    Task<HeroSlide?> UpdateAsync(NpgsqlTransaction t, Guid id, UpdateHeroSlideDto dto);
    Task<HeroSlide?> UpdateImageUrlAsync(NpgsqlTransaction t, Guid id, string imageUrl);
    Task<bool> DeleteAsync(NpgsqlTransaction t, Guid id);
    Task ReorderAsync(NpgsqlTransaction t, List<Guid> slideIds);
    Task<int> GetNextSortOrderAsync(NpgsqlConnection conn);
}

public class HeroSlidesRepository : IHeroSlidesRepository
{
    public async Task<List<HeroSlide>> GetAllAsync(NpgsqlConnection conn, HeroSlideFilters? filters = null)
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
        return (await conn.QueryAsync<HeroSlide>(sql, parameters)).ToList();
    }

    public async Task<HeroSlide?> GetByIdAsync(NpgsqlConnection conn, Guid id)
    {
        var sql = @"
            SELECT 
                id, image_url AS ImageUrl,
                button_text AS ButtonText, button_url AS ButtonUrl,
                sort_order AS SortOrder, is_active AS IsActive
            FROM hero_slides
            WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<HeroSlide>(sql, new { Id = id });
    }

    public async Task<HeroSlide> CreateAsync(NpgsqlTransaction t, HeroSlide slide)
    {
        var sql = @"
            INSERT INTO hero_slides (id, image_url, button_text, button_url, sort_order, is_active)
            VALUES (@Id, @ImageUrl, @ButtonText, @ButtonUrl, @SortOrder, @IsActive)
            RETURNING 
                id, image_url AS ImageUrl,
                button_text AS ButtonText, button_url AS ButtonUrl,
                sort_order AS SortOrder, is_active AS IsActive";
        return await t.Connection!.QuerySingleAsync<HeroSlide>(sql, slide, t);
    }

    public async Task<HeroSlide?> UpdateAsync(NpgsqlTransaction t, Guid id, UpdateHeroSlideDto dto)
    {
        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.ButtonText != null) { updates.Add("button_text = @ButtonText"); parameters.Add("ButtonText", dto.ButtonText); }
        if (dto.ButtonUrl != null) { updates.Add("button_url = @ButtonUrl"); parameters.Add("ButtonUrl", dto.ButtonUrl); }
        if (dto.SortOrder.HasValue) { updates.Add("sort_order = @SortOrder"); parameters.Add("SortOrder", dto.SortOrder.Value); }
        if (dto.IsActive.HasValue) { updates.Add("is_active = @IsActive"); parameters.Add("IsActive", dto.IsActive.Value); }

        if (updates.Count == 0) return await GetByIdAsync(t.Connection!, id);

        var sql = $@"
            UPDATE hero_slides
            SET {string.Join(", ", updates)}
            WHERE id = @Id
            RETURNING 
                id, image_url AS ImageUrl,
                button_text AS ButtonText, button_url AS ButtonUrl,
                sort_order AS SortOrder, is_active AS IsActive";
        return await t.Connection!.QueryFirstOrDefaultAsync<HeroSlide>(sql, parameters, t);
    }

    public async Task<bool> DeleteAsync(NpgsqlTransaction t, Guid id)
    {
        var sql = "DELETE FROM hero_slides WHERE id = @Id";
        var affected = await t.Connection!.ExecuteAsync(sql, new { Id = id }, t);
        return affected > 0;
    }

    public async Task ReorderAsync(NpgsqlTransaction t, List<Guid> slideIds)
    {
        for (var i = 0; i < slideIds.Count; i++)
        {
            var sql = "UPDATE hero_slides SET sort_order = @SortOrder WHERE id = @Id";
            await t.Connection!.ExecuteAsync(sql, new { Id = slideIds[i], SortOrder = i }, t);
        }
    }

    public async Task<HeroSlide?> UpdateImageUrlAsync(NpgsqlTransaction t, Guid id, string imageUrl)
    {
        var sql = @"
            UPDATE hero_slides
            SET image_url = @ImageUrl
            WHERE id = @Id
            RETURNING 
                id, image_url AS ImageUrl,
                button_text AS ButtonText, button_url AS ButtonUrl,
                sort_order AS SortOrder, is_active AS IsActive";
        return await t.Connection!.QueryFirstOrDefaultAsync<HeroSlide>(sql, new { Id = id, ImageUrl = imageUrl }, t);
    }

    public async Task<int> GetNextSortOrderAsync(NpgsqlConnection conn)
    {
        var sql = "SELECT COALESCE(MAX(sort_order), -1) + 1 FROM hero_slides";
        return await conn.QuerySingleAsync<int>(sql);
    }
}
