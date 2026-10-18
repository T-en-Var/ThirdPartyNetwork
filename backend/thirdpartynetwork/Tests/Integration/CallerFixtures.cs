using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>An authenticated (or anonymous) HTTP client standing in for one of the caller
/// configurations in the axis map (see the in-conversation combination-matrix breakdown).</summary>
public record TestCaller(string Name, HttpClient Client, Guid UserProxyId, bool IsAnonymous);

public static class CallerFixtures
{
    public static async Task<Network> CreateNetworkAsync(DataContext ctx, string name, bool isPublic = true)
    {
        var network = new Network
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsPublic = isPublic,
            Description = "test network",
            RedirectURI = "#",
            // Generous limits throughout — these tests are about authorization, not
            // featureFlag/quota business rules, so the limits just need to be high enough
            // to never spuriously trip (NetworkFeatureFlag defaults everything to 0/disabled).
            FeatureFlag = new NetworkFeatureFlag
            {
                NetworkId = Guid.Empty, // set by EF via the nav below
                AllowBlogs = true, BlogCountLimit = 100_000,
                AllowConfigurations = true, ConfigurationCountLimit = 100_000,
                AllowFiles = true, FileCountLimit = 100_000, FileSizeLimit = 100_000_000, FileStorageLimit = 100_000_000,
                AllowCustomPages = true, CustomPageCountLimit = 100_000,
                CustomPageBlockCountLimit = 100_000, CustomPageBlockSizeLimit = 100_000,
            },
        };
        ctx.Networks.Add(network);
        await ctx.SaveChangesAsync();
        return network;
    }

    public static async Task<Network> GetMainNetworkAsync(DataContext ctx) =>
        await ctx.Networks.FirstAsync(n => n.Name == Constants.Networks.Main);

    private static async Task<Permission> GetOrCreatePermissionAsync(DataContext ctx, string name)
    {
        var existing = await ctx.Permissions.FirstOrDefaultAsync(p => p.Name == name);
        if (existing != null) return existing;

        var permission = new Permission { Id = Guid.NewGuid(), Name = name, Description = "test" };
        ctx.Permissions.Add(permission);
        await ctx.SaveChangesAsync();
        return permission;
    }

    /// <summary>Creates a role in `network` holding exactly the given permissions (empty ==
    /// no permissions at all, distinct from the default seeded read-only "User" role).</summary>
    public static async Task<Role> CreateRoleAsync(DataContext ctx, Network network, string roleName, params string[] permissionNames)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(), Name = roleName, Description = "test role", IsDefault = false,
            Network = network, NetworkId = network.Id, FeatureFlags = new RoleFeatureFlag(),
        };
        ctx.Roles.Add(role);

        foreach (var permName in permissionNames)
        {
            var perm = await GetOrCreatePermissionAsync(ctx, permName);
            ctx.RolePermissions.Add(new RolePermission { Role = role, RoleId = role.Id, Permission = perm, PermissionId = perm.Id });
        }

        await ctx.SaveChangesAsync();
        return role;
    }

    public static async Task<UserProxy> CreateUserAsync(DataContext ctx, string label)
    {
        var user = new User();
        var proxy = new UserProxy
        {
            Id = Guid.NewGuid(), IsDefault = true, User = user,
            Username = $"{label}-{Guid.NewGuid():N}"[..24], FirstName = label, LastName = "Test",
        };
        user.UserProxies.Add(proxy);
        ctx.UserProxies.Add(proxy);
        await ctx.SaveChangesAsync();
        return proxy;
    }

    public static async Task<NetworkUser> JoinNetworkAsync(
        DataContext ctx, Network network, UserProxy proxy, Role[] roles, bool accessIncomplete = false)
    {
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), NetworkId = network.Id, UserProxyId = proxy.Id, UserProxy = proxy,
            AccessIncomplete = accessIncomplete, FeatureFlags = new NetworkUserFeatureFlag(),
        };
        ctx.NetworkUsers.Add(networkUser);
        foreach (var role in roles)
        {
            ctx.NetworkUserRoles.Add(new NetworkUserRole { NetworkUser = networkUser, NetworkUserId = networkUser.Id, Role = role, RoleId = role.Id });
        }
        await ctx.SaveChangesAsync();
        return networkUser;
    }

    /// <summary>
    /// Mints an access token with the exact same shape as TokenGenerator.GenerateAccessToken
    /// (jti/name/uid claims, same issuer/audience/signing scheme) — deliberately NOT via the
    /// app's own ITokenGenerator. TokenGenerator.TokenSecret is a `private static readonly`
    /// field, initialized once per process on first touch of the class; the pre-existing
    /// Tests/Auth/TokenGeneratorTests.cs sets its OWN dummy TOKEN_SECRET in [SetUp] and (since
    /// NUnit doesn't order unrelated fixtures across namespaces, and "Tests.Auth" sorts before
    /// "Tests.Integration") reliably runs first when the whole suite executes together —
    /// permanently baking that unrelated secret into the static field before any of these
    /// integration tests get a chance to run. Reading the env var fresh here, at mint time,
    /// makes this suite's outcome independent of unrelated tests' execution order.
    /// </summary>
    public static string MintToken(NetworkUser networkUser)
    {
        using var scope = IntegrationTestSetup.Factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var userProxy = networkUser.UserProxy;
        userProxy.ApplyDefaults();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Name, userProxy.Username!),
            new("uid", userProxy.Id.ToString()),
        };
        if (networkUser.AccessIncomplete)
        {
            claims.Add(new Claim("AccessIncomplete", "true"));
        }

        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = configuration["JwtSettings:Issuer"],
            Audience = configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    public static TestCaller BuildAnonymous() =>
        new("Anonymous", IntegrationTestSetup.Factory.CreateClient(), Guid.Empty, true);

    public static Task<TestCaller> BuildAuthenticatedAsync(string name, NetworkUser networkUser)
    {
        var token = MintToken(networkUser);
        var client = IntegrationTestSetup.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return Task.FromResult(new TestCaller(name, client, networkUser.UserProxyId, false));
    }
}
