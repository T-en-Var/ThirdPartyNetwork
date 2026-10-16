using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class IdentityVerifierTests
{
    private DbContextOptions<DataContext> _options;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task IsSuperAdmin_Detected_FromMainNetwork_AdminPermission()
    {
        await using var ctx = new DataContext(_options);

        var main = new Network
        {
            Id = Guid.NewGuid(), Name = Constants.Networks.Main, IsPublic = false, Description = "Main network",
            RedirectURI = "",
            FeatureFlag = new()
            {
                AllowBlogs = true,
                AllowConfigurations = true,
                AllowFiles = true,
                AllowCustomPages = true,
            }
        };
        var user = new User();
        var proxy = new UserProxy { Id = Guid.NewGuid(), User = user, IsDefault = true };

        var permissionAdmin = new Permission
            { Id = Guid.NewGuid(), Name = Constants.Permissions.Administrator, Description = "admin" };
        var role = new Role
        {
            Id = Guid.NewGuid(), Name = "r", Description = "d", Network = main, NetworkId = main.Id, IsDefault = false,
            FeatureFlags = JsonDocument.Parse("{}")
        };
        var rolePerm = new RolePermission
            { Permission = permissionAdmin, Role = role, PermissionId = permissionAdmin.Id, RoleId = role.Id };
        role.RolePermissions.Add(rolePerm);

        var nu = new NetworkUser
        {
            Id = Guid.NewGuid(), NetworkId = main.Id, UserProxy = proxy, UserProxyId = proxy.Id,
            FeatureFlags = JsonDocument.Parse("{}")
        };
        var nur = new NetworkUserRole { NetworkUserId = nu.Id, RoleId = role.Id, Role = role, NetworkUser = nu };
        nu.NetworkUserRoles.Add(nur);
        proxy.NetworkUsers.Add(nu);

        ctx.Networks.Add(main);
        ctx.Permissions.Add(permissionAdmin);
        ctx.Roles.Add(role);
        ctx.RolePermissions.Add(rolePerm);
        ctx.UserProxies.Add(proxy);
        ctx.NetworkUsers.Add(nu);
        ctx.NetworkUserRoles.Add(nur);
        await ctx.SaveChangesAsync();

        var verifier = new IdentityVerifier(ctx);

        Assert.That(await verifier.IsSuperAdminAsync(proxy.Id), Is.True);
    }

    [Test]
    public async Task HasPermission_FindsPermission_InSpecificNetwork()
    {
        await using var ctx = new DataContext(_options);

        var net = new Network
        {
            Id = Guid.NewGuid(),
            Name = "other",
            IsPublic = false,
            Description = "",
            RedirectURI = "",
            FeatureFlag = new()
            {
                AllowBlogs = true,
                AllowConfigurations = true,
                AllowFiles = true,
                AllowCustomPages = true,
            }
        };
        var permission = new Permission { Id = Guid.NewGuid(), Name = "perm:x", Description = "x" };
        var role = new Role
        {
            Id = Guid.NewGuid(), Name = "r2", Description = "d2", Network = net, NetworkId = net.Id, IsDefault = false,
            FeatureFlags = JsonDocument.Parse("{}")
        };
        var rp = new RolePermission
            { Permission = permission, Role = role, PermissionId = permission.Id, RoleId = role.Id };
        role.RolePermissions.Add(rp);

        var user = new User();
        var proxy = new UserProxy { Id = Guid.NewGuid(), User = user, IsDefault = true };

        var nu = new NetworkUser { Id = Guid.NewGuid(), NetworkId = net.Id, UserProxy = proxy, UserProxyId = proxy.Id,
            FeatureFlags = JsonDocument.Parse("{}") };
        var nur = new NetworkUserRole { NetworkUserId = nu.Id, RoleId = role.Id, Role = role, NetworkUser = nu };
        nu.NetworkUserRoles.Add(nur);
        proxy.NetworkUsers.Add(nu);

        ctx.Networks.Add(net);
        ctx.Permissions.Add(permission);
        ctx.Roles.Add(role);
        ctx.RolePermissions.Add(rp);
        ctx.UserProxies.Add(proxy);
        ctx.NetworkUsers.Add(nu);
        ctx.NetworkUserRoles.Add(nur);
        await ctx.SaveChangesAsync();

        var verifier = new IdentityVerifier(ctx);

        Assert.That(await verifier.HasPermissionAsync(net.Id, proxy.Id, "perm:x"), Is.True);
    }
}