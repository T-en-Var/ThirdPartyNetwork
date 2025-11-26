using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using thirdpartynetwork.Core.Data;

namespace thirdpartynetwork
{
    // Implement IDesignTimeDbContextFactory for your DataContext
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            
            // 1. Build Configuration (used primarily to find a connection string, if necessary)
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // 2. Set Database Provider and a dummy connection string
            // NOTE: Replace .UseNpgsql with your actual provider (.UseSqlServer, .UseSqlite, etc.)
            
            // This connection string is only used for EF design-time tools to reflect the model.
            // It will NOT connect to a database to generate the script.
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                                   ?? "Host=ci_server;Database=dummy_db;Username=ef_user;Password=dummy"; 

            optionsBuilder.UseNpgsql(connectionString);

            return new DataContext(optionsBuilder.Options);
        }
    }
}