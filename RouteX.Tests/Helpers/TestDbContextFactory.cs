using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RouteX.Data;

namespace RouteX.Tests.Helpers
{
    /// <summary>
    /// Creates an in-memory ApplicationDbContext for testing.
    /// Each test should use a unique database name to ensure isolation.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            // Provide an empty configuration — tests don't need real connection strings
            var configuration = new ConfigurationBuilder().Build();

            var context = new ApplicationDbContext(options, configuration);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
