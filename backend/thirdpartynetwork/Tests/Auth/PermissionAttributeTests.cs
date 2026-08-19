using NUnit.Framework;
using thirdpartynetwork.Application.Auth.Requirements;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class PermissionAttributeTests
{
    [Test]
    public void PermissionRequirement_StoresPermissions()
    {
        var req = new PermissionRequirement("a", "b");
        Assert.That(req.Permissions, Has.Count.EqualTo(2));
    }

    [Test]
    public void RequirePermissionAttribute_SetsPolicy()
    {
        var attr = new RequirePermissionAttribute("x", "y");
        Assert.That(attr.Policy, Is.EqualTo("Permission:x,y"));
    }

    [Test]
    public void PermissionRequirement_IsListEndpoint_DefaultsFalse()
    {
        var req = new PermissionRequirement("a", "b");
        Assert.That(req.IsListEndpoint, Is.False);
    }

    [Test]
    public void PermissionRequirement_IsListEndpoint_CanBeSetTrue()
    {
        var req = new PermissionRequirement(["a", "b"], isListEndpoint: true);
        Assert.That(req.IsListEndpoint, Is.True);
        Assert.That(req.Permissions, Has.Count.EqualTo(2));
    }

    [Test]
    public void PermissionRequirement_RouteKey_DefaultsNull()
    {
        var req = new PermissionRequirement("a", "b");
        Assert.That(req.RouteKey, Is.Null);
    }

    [Test]
    public void PermissionRequirement_RouteKey_CanBeSet()
    {
        var req = new PermissionRequirement(["a"], routeKey: "fileId");
        Assert.That(req.RouteKey, Is.EqualTo("fileId"));
    }

    [Test]
    public void RequirePermissionAttribute_IsListEndpoint_MarksPolicyAsList()
    {
        var attr = new RequirePermissionAttribute("x", "y") { IsListEndpoint = true };
        Assert.That(attr.Policy, Is.EqualTo("Permission:List:x,y"));
    }

    [Test]
    public void RequirePermissionAttribute_RouteKey_MarksPolicyWithRoute()
    {
        var attr = new RequirePermissionAttribute("x", "y") { RouteKey = "fileId" };
        Assert.That(attr.Policy, Is.EqualTo("Permission:Route=fileId:x,y"));
    }

    [Test]
    public void RequirePermissionAttribute_IsListEndpointAndRouteKey_Combine()
    {
        var attr = new RequirePermissionAttribute("x") { IsListEndpoint = true, RouteKey = "fileId" };
        Assert.That(attr.Policy, Is.EqualTo("Permission:List:Route=fileId:x"));
    }
}
