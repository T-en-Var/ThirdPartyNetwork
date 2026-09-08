using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using thirdpartynetwork.Core.Interfaces.Services;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// Real HTTP integration harness for the user-generated-content authorization matrix
/// (see the "Pragmatic" scope agreed in-conversation). Boots the actual app (Program.cs)
/// against a real, throwaway Postgres container via Testcontainers — deliberately NOT the
/// EF InMemory provider the rest of Tests/ uses, because AccessLevelVerifier.
/// GetAccessPredicateAsync is hand-translated LINQ that PLAN.md itself flags as a
/// duplication risk against CheckAccessForCallerAsync; InMemory's weaker LINQ translation
/// could mask exactly the class of bug these tests exist to catch.
///
/// One factory/container is shared for the whole Tests.Integration run (see
/// IntegrationTestSetup below) rather than per-fixture, since spinning up a fresh Postgres
/// container per test class is slow. Tests avoid interfering with each other by never
/// sharing mutable rows — every test creates its own Network(s)/Users/content with fresh
/// Guids, so no DB reset between tests is needed (the one shared, effectively-immutable
/// exception is the Main network + system Permission catalog, which DatabaseSeeder creates
/// once at app startup and which tests only ever ADD independent rows into).
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // picks up appsettings.Development.json's JwtSettings

        builder.ConfigureServices(services =>
        {
            // Swap real MinIO for a fake — only the authorization gating is under test here,
            // not object storage itself, and a live MinIO isn't part of this harness.
            services.RemoveAll<ICDNService>();
            services.AddSingleton<ICDNService, FakeCdnService>();
        });
    }
}
