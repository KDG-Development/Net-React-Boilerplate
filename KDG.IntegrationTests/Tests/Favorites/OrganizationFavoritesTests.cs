using Dapper;
using KDG.Boilerplate.Server.Models.Requests.Common;
using KDG.Boilerplate.Server.Models.Requests.Products;
using KDG.Boilerplate.Services;
using KDG.IntegrationTests.Infrastructure;

namespace KDG.IntegrationTests.Tests.Favorites;

/// <summary>
/// Integration tests for organization-scoped favorites that verify the B2B
/// business constraint: favorites belong to organizations, not individuals.
/// 
/// Business context: This platform serves B2B customers where purchasing
/// decisions are collaborative. Favorites are shared across an organization.
/// Users without an organization cannot use this feature.
/// </summary>
public class OrganizationFavoritesTests : IntegrationTestBase
{
    public OrganizationFavoritesTests(DatabaseTestFixture fixture) : base(fixture) { }

    #region Test Data Setup

    /// <summary>
    /// Checks if a product is in the user's favorites using the catalog service.
    /// </summary>
    private async Task<bool> IsFavorite(Guid userId, Guid productId)
    {
        var productService = GetService<IProductService>();
        var favorites = await productService.GetCatalogProductsAsync(
            userId,
            new PaginationParams(),
            filters: new ProductFilters { FavoritesOnly = true });
        
        return favorites.Items.Any(p => p.Id == productId);
    }

    private async Task CleanupTestData(Guid organizationId)
    {
        using var connection = await GetDatabaseConnection();
        await connection.ExecuteAsync("DELETE FROM organization_favorites WHERE organization_id = @OrgId", new { OrgId = organizationId });
        await connection.ExecuteAsync("DELETE FROM users WHERE organization_id = @OrgId", new { OrgId = organizationId });
        await connection.ExecuteAsync("DELETE FROM organizations WHERE id = @OrgId", new { OrgId = organizationId });
    }

    #endregion

    /// <summary>
    /// Verifies that users without an organization cannot add favorites.
    /// 
    /// Business context: Favorites are shared across an organization. Users
    /// not assigned to a company cannot use this feature - it would create
    /// orphaned data with no organization to share it.
    /// 
    /// Real-world scenario: New employee signs up but HR hasn't assigned
    /// them yet. They receive a clear error explaining they need org membership.
    /// </summary>
    [Fact]
    public async Task Favorites_UserWithoutOrganization_IsRejected()
    {
        var userId = await TestData.Users.Create(organizationId: null);
        var productId = await TestData.Catalog.CreateProduct("Orphan Test");

        var favoritesService = GetService<IFavoritesService>();

        var addResult = await favoritesService.AddFavoriteAsync(userId, productId);
        Assert.False(addResult.Success);
        Assert.Equal("User does not belong to an organization", addResult.Error);

        var removeResult = await favoritesService.RemoveFavoriteAsync(userId, Guid.NewGuid());
        Assert.False(removeResult.Success);
    }

    /// <summary>
    /// Verifies that users with an organization can add and remove favorites.
    /// 
    /// Business context: Core feature - team members can mark products as
    /// favorites to build a shared list for their organization.
    /// 
    /// Real-world scenario: Procurement manager favorites products they want
    /// to order regularly, then removes discontinued ones.
    /// </summary>
    [Fact]
    public async Task Favorites_UserWithOrganization_CanAddAndRemove()
    {
        var orgId = await TestData.Organizations.Create("Test Org Favorites");
        var userId = await TestData.Users.Create(organizationId: orgId);
        var productId = await TestData.Catalog.CreateProduct("Org Favorite Test");

        try
        {
            var favoritesService = GetService<IFavoritesService>();

            // Add favorite
            var addResult = await favoritesService.AddFavoriteAsync(userId, productId);
            Assert.True(addResult.Success);
            Assert.True(await IsFavorite(userId, productId));

            // Remove favorite
            var removeResult = await favoritesService.RemoveFavoriteAsync(userId, productId);
            Assert.True(removeResult.Success);
            Assert.False(await IsFavorite(userId, productId));
        }
        finally
        {
            await CleanupTestData(orgId);
        }
    }

    /// <summary>
    /// Verifies that adding the same favorite twice is idempotent.
    /// 
    /// Business context: Multiple team members might favorite the same product,
    /// or a user might double-click. This should not create duplicates.
    /// 
    /// Real-world scenario: User clicks favorite button rapidly. Second request
    /// succeeds without creating duplicate entries.
    /// </summary>
    [Fact]
    public async Task AddFavorite_Duplicate_IsSafe()
    {
        var orgId = await TestData.Organizations.Create("Test Org Duplicate");
        var userId = await TestData.Users.Create(organizationId: orgId);
        var productId = await TestData.Catalog.CreateProduct("Duplicate Test");

        try
        {
            var favoritesService = GetService<IFavoritesService>();
            var productService = GetService<IProductService>();

            await favoritesService.AddFavoriteAsync(userId, productId);
            await favoritesService.AddFavoriteAsync(userId, productId);

            var favorites = await productService.GetCatalogProductsAsync(
                userId,
                new PaginationParams(),
                filters: new ProductFilters { FavoritesOnly = true });
            
            Assert.Single(favorites.Items);
        }
        finally
        {
            await CleanupTestData(orgId);
        }
    }

    /// <summary>
    /// Verifies that removing a non-existent favorite is safe.
    /// 
    /// Business context: User might click un-favorite on a product that's
    /// already removed (stale UI state). This should succeed without error.
    /// </summary>
    [Fact]
    public async Task RemoveFavorite_NonExistent_IsSafe()
    {
        var orgId = await TestData.Organizations.Create("Test Org Remove NonExistent");
        var userId = await TestData.Users.Create(organizationId: orgId);

        try
        {
            var favoritesService = GetService<IFavoritesService>();
            var result = await favoritesService.RemoveFavoriteAsync(userId, Guid.NewGuid());

            Assert.True(result.Success);
        }
        finally
        {
            await CleanupTestData(orgId);
        }
    }
}
