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
}
