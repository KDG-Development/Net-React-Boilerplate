using KDG.IntegrationTests.Infrastructure.TestData;
using Npgsql;

namespace KDG.IntegrationTests.Infrastructure;

/// <summary>
/// Factory for creating test data in integration tests.
/// Organized by domain for scalability.
/// 
/// NOTE: Most test data is created via direct SQL as a temporary measure.
/// These entities will be synced from NetSuite in production. Once the
/// service layer supports these operations, update these helpers accordingly.
/// </summary>
public class TestDataFactory
{
    public UserTestData Users { get; }
    public OrganizationTestData Organizations { get; }
    public CatalogTestData Catalog { get; }

    public TestDataFactory(Func<Task<NpgsqlConnection>> getConnection)
    {
        Users = new UserTestData(getConnection);
        Organizations = new OrganizationTestData(getConnection);
        Catalog = new CatalogTestData(getConnection);
    }
}
