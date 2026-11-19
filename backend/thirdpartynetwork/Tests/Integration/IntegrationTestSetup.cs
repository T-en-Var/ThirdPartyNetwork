using DotNetEnv;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// One-time setup/teardown for the entire thirdpartynetwork.Tests.Integration namespace:
/// starts a single throwaway Postgres container + one shared TestWebApplicationFactory for
/// the whole run (NUnit runs [SetUpFixture] OneTimeSetUp before any test in the namespace,
/// OneTimeTearDown after the last one).
/// </summary>
[SetUpFixture]
public class IntegrationTestSetup
{
    private static PostgreSqlContainer? _container;
    private static TestWebApplicationFactory? _factory;

    public static TestWebApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("Integration test factory not initialized yet.");

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        // Reuse the same local secrets the app already uses for `dotnet run` (TOKEN_SECRET,
        // LUCKY_PENNY_LICENSE, MINIO_*, ...) instead of hardcoding them in test source —
        // mirrors Program.cs's own Env.Load() call for local dev. DB_CONNECTION_STRING gets
        // overwritten below once the container is up.
        var envFile = FindDotEnvFile();
        if (envFile == null)
        {
            throw new InvalidOperationException(
                "Tests/Integration needs the repo's thirdpartynetwork/.env file (for TOKEN_SECRET, " +
                "LUCKY_PENNY_LICENSE, etc. — see the app's own local-dev setup) but none was found " +
                "walking up from the test binary's directory.");
        }
        Env.Load(envFile);

        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("thirdpartynetwork_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await _container.StartAsync();

        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", _container.GetConnectionString());
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        _factory = new TestWebApplicationFactory();
        // Force the host to actually boot now (runs migrations + DatabaseSeeder.SeedAsync
        // via Program.cs's own startup block) rather than lazily on the first test's request.
        using var warmupClient = _factory.CreateClient();
        await warmupClient.GetAsync("/");
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTestsAsync()
    {
        _factory?.Dispose();
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    private static string? FindDotEnvFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "thirdpartynetwork", ".env");
            if (System.IO.File.Exists(candidate)) return candidate;

            var siblingCandidate = Path.Combine(dir.FullName, ".env");
            if (System.IO.File.Exists(siblingCandidate) && dir.Name == "thirdpartynetwork") return siblingCandidate;

            dir = dir.Parent;
        }
        return null;
    }
}
