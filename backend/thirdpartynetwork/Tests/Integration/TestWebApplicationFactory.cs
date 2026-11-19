using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using thirdpartynetwork.Core.Interfaces.Services;

namespace thirdpartynetwork.Tests.Integration;

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
