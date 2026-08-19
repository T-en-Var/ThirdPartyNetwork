using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core.Helper;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class DynamicPermissionPolicyProviderTests
{
    private DynamicPermissionPolicyProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _provider = new DynamicPermissionPolicyProvider(Options.Create(new AuthorizationOptions()));
    }

    [Test]
    public async Task GetPolicyAsync_PlainPermission_BuildsNonListRequirement()
    {
        var policy = await _provider.GetPolicyAsync("Permission:a,b");
        var requirement = (PermissionRequirement)policy!.Requirements.Single();

        Assert.That(requirement.Permissions, Is.EqualTo(new[] { "a", "b" }));
        Assert.That(requirement.IsListEndpoint, Is.False);
    }

    [Test]
    public async Task GetPolicyAsync_ListMarker_BuildsListRequirement()
    {
        var policy = await _provider.GetPolicyAsync("Permission:List:a,b");
        var requirement = (PermissionRequirement)policy!.Requirements.Single();

        Assert.That(requirement.Permissions, Is.EqualTo(new[] { "a", "b" }));
        Assert.That(requirement.IsListEndpoint, Is.True);
    }

    [Test]
    public async Task GetPolicyAsync_RouteMarker_BuildsRequirementWithRouteKey()
    {
        var policy = await _provider.GetPolicyAsync("Permission:Route=fileId:a,b");
        var requirement = (PermissionRequirement)policy!.Requirements.Single();

        Assert.That(requirement.Permissions, Is.EqualTo(new[] { "a", "b" }));
        Assert.That(requirement.IsListEndpoint, Is.False);
        Assert.That(requirement.RouteKey, Is.EqualTo("fileId"));
    }

    [Test]
    public async Task GetPolicyAsync_ListAndRouteMarkers_BothParsed()
    {
        var policy = await _provider.GetPolicyAsync("Permission:List:Route=fileId:a");
        var requirement = (PermissionRequirement)policy!.Requirements.Single();

        Assert.That(requirement.Permissions, Is.EqualTo(new[] { "a" }));
        Assert.That(requirement.IsListEndpoint, Is.True);
        Assert.That(requirement.RouteKey, Is.EqualTo("fileId"));
    }
}
