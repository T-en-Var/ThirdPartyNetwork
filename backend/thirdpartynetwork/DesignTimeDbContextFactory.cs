using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using thirdpartynetwork.Core.Data;

namespace thirdpartynetwork
{
    // Implement IDesignTimeDbContextFactory for your DataContext
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            Env.Load();
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            // This connection string is only used for EF design-time tools to reflect the model.
            // It will NOT connect to a database to generate the script.
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                   ?? "Host=ci_server;Database=dummy_db;Username=ef_user;Password=dummy";

            optionsBuilder.UseNpgsql(connectionString);

            return new DataContext(optionsBuilder.Options);
        }
    }
}