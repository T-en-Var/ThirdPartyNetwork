using System.Text.Json;
using Moq;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class TokenGeneratorTests
{
    private const string Secret = "token-secret-for-tests-which-is-long";

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("TOKEN_SECRET", Secret);
    }

    [Test]
    public void GenerateTokenPair_CreatesTokens_And_StoresJti()
    {
        var jtiServiceMock = new Mock<IJtiCacheService>();

        var inMemory = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("JwtSettings:Issuer", "test-issuer"),
                new KeyValuePair<string, string?>("JwtSettings:Audience", "test-audience")
            })
            .Build();

        var tokenGenerator = new TokenGenerator(jtiServiceMock.Object, inMemory);

        // Build small network/user graph
        var permission = new Permission { Id = Guid.NewGuid(), Name = "perm:x", Description = "x" };
        var role = new Role
        {
            Id = Guid.NewGuid(), Name = "r", Description = "d", Network = new Network
            {
                Name = "null",
                IsPublic = false,
                Description = "",
                RedirectURI = "",
                Entitlement = new()
                {
                    AllowBlogs = true,
                    AllowConfigurations = true,
                    AllowFiles = true,
                    AllowCustomPages = true
                }
            },
            IsDefault = false,
            Entitlements = JsonDocument.Parse("{}")
        };
        role.RolePermissions.Add(new RolePermission { Permission = permission, Role = role });

        var user = new User();
        var userProxy = new UserProxy { Id = Guid.NewGuid(), Username = "u", IsDefault = true, User = user };

        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxy = userProxy, UserProxyId = userProxy.Id,
            Entitlements = JsonDocument.Parse("{}")
        };
        var nur = new NetworkUserRole
        {
            NetworkUserId = networkUser.Id,
            RoleId = role.Id,
            Role = role,
            NetworkUser = networkUser
        };
        networkUser.NetworkUserRoles.Add(nur);
        userProxy.NetworkUsers.Add(networkUser);

        var tokens = tokenGenerator.GenerateTokenPair(networkUser);

        Assert.That(tokens, Is.Not.Null);
        Assert.That(tokens.AccessToken, Is.Not.Empty);
        Assert.That(tokens.RefreshToken, Is.Not.Empty);

        jtiServiceMock.Verify(s => s.StoreJti(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<TimeSpan>()),
            Times.AtLeastOnce);
    }
}