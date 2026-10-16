using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Tests.Integration;

[TestFixture]
public abstract class IntegrationTestBase
{
    private IServiceScope _scope = null!;
    protected DataContext Ctx = null!;
    protected Network ContentNetwork = null!;
    protected Network ExternalNetwork = null!;
    protected Network MainNetwork = null!;

    [SetUp]
    public async Task BaseSetUpAsync()
    {
        _scope = IntegrationTestSetup.Factory.Services.CreateScope();
        Ctx = _scope.ServiceProvider.GetRequiredService<DataContext>();

        // Fresh, uniquely-named networks per test — see TestWebApplicationFactory's doc
        // comment on why no DB reset is needed between tests. Deliberately NOT IsPublic:
        // AccessLevelVerifier treats a Network-type grant naming an IsPublic network as
        // visible to EVERY authenticated caller, not just that network's members (the same
        // rule that covers anonymous visitors) — a public ExternalNetwork would silently
        // break any "requires actual membership" assertion.
        ContentNetwork = await CallerFixtures.CreateNetworkAsync(Ctx, $"content-{Guid.NewGuid():N}", isPublic: false);
        ExternalNetwork = await CallerFixtures.CreateNetworkAsync(Ctx, $"external-{Guid.NewGuid():N}", isPublic: false);
        MainNetwork = await CallerFixtures.GetMainNetworkAsync(Ctx);
    }

    [TearDown]
    public void BaseTearDown() => _scope.Dispose();

    protected static void AssertStatus(System.Net.Http.HttpResponseMessage response, System.Net.HttpStatusCode expected, string context)
    {
        Assert.That(response.StatusCode, Is.EqualTo(expected),
            $"{context}: expected {expected} but got {response.StatusCode}. Body: {response.Content.ReadAsStringAsync().Result}");
    }
}
