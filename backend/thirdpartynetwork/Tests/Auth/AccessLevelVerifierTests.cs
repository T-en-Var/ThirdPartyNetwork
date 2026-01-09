using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class AccessLevelVerifierTests
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
    public async Task CheckAccess_Public_ReturnsTrue_ForAnyone()
    {
        await using var ctx = new DataContext(_options);
        var file = new File
        {
            Id = Guid.NewGuid(),
            AccessLevel = AccessLevel.Public,
            PublicId = Guid.NewGuid(),
            Format = "",
            Name = "",
            MediaType = "",
            Url = "",
        };
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        var identityVerifierMock = new Mock<IIdentityVerifier>();

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        var ok = await verifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm");
        Assert.That(ok, Is.True);
    }

    [Test]
    public async Task CheckAccess_Private_ReturnsAuthorOnly()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var file = new File
        {
            Id = Guid.NewGuid(),
            AccessLevel = AccessLevel.Private,
            PublicId = Guid.NewGuid(),
            AuthorId = authorId,
            Format = "",
            Name = "",
            MediaType = "",
            Url = "",
        };
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
            { Id = Guid.NewGuid(), IsDefault = true, User = new User { } });

        var identityVerifierMock = new Mock<IIdentityVerifier>();

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        // Not the author -> false
        var ok1 = await verifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm");
        Assert.That(ok1, Is.False);

        // As author
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
            { Id = authorId, IsDefault = true, User = new User { } });
        var ok2 = await verifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm");
        Assert.That(ok2, Is.True);
    }

    [Test]
    public async Task CheckAccess_PublicToNetwork_RequiresNetworkMembership()
    {
        using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var file = new File
        {
            Id = Guid.NewGuid(), 
            AccessLevel = AccessLevel.PublicToNetwork, 
            NetworkId = networkId,
            Format = "",
            Name = "",
            MediaType = "",
            Url = "",
            PublicId = Guid.NewGuid()
        };
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
        {
            Id = userProxyId, NetworkUsers = new System.Collections.Generic.List<NetworkUser>(), IsDefault = true,
            User = new User { }
        });

        var identityVerifierMock = new Mock<IIdentityVerifier>();

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        // Not member -> false
        var ok1 = await verifier.CheckFileAccessAsync(file.Id.ToString(), networkId, "perm");
        Assert.That(ok1, Is.False);

        // Member
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
        {
            Id = userProxyId, IsDefault = true, User = new User { },
            NetworkUsers = new System.Collections.Generic.List<NetworkUser>
                { new NetworkUser { NetworkId = networkId } }
        });
        var ok2 = await verifier.CheckFileAccessAsync(file.Id.ToString(), networkId, "perm");
        Assert.That(ok2, Is.True);
    }

    [Test]
    public async Task GetAccessPredicate_Unauthenticated_ReturnsOnlyPublic()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();

        var publicFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.Public, Name = "", Format = "", MediaType = "", Url = "" };
        var privateFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.Private, AuthorId = Guid.NewGuid(), Name = "", Format = "", MediaType = "", Url = "" };
        var networkFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.PublicToNetwork, NetworkId = networkId, Name = "", Format = "", MediaType = "", Url = "" };

        ctx.Files.AddRange(publicFile, privateFile, networkFile);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy { User = null, IsDefault = true });

        var identityVerifierMock = new Mock<IIdentityVerifier>();

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(publicFile.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(privateFile.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(networkFile.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Authenticated_Author_ReturnsPrivateOwn()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var otherPrivate = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.Private, AuthorId = Guid.NewGuid(), Name = "", Format = "", MediaType = "", Url = "" };
        var myPrivate = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.Private, AuthorId = userId, Name = "", Format = "", MediaType = "", Url = "" };

        ctx.Files.AddRange(otherPrivate, myPrivate);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
        {
            Id = userId,
            User = new User(),
            NetworkUsers = new List<NetworkUser>(),
            IsDefault = true,
        });

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(false);

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(myPrivate.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(otherPrivate.Id));
    }

    [Test]
    public async Task GetAccessPredicate_NetworkMember_ReturnsPublicToNetwork()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var otherNetworkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var netFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.PublicToNetwork, NetworkId = networkId, Name = "", Format = "", MediaType = "", Url = "" };
        var otherNetFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.PublicToNetwork, NetworkId = otherNetworkId, Name = "", Format = "", MediaType = "", Url = "" };

        ctx.Files.AddRange(netFile, otherNetFile);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
        {
            Id = userId,
            User = new User(),
            NetworkUsers = new List<NetworkUser>
            {
                new NetworkUser { NetworkId = networkId }
            },
            IsDefault = true
        });

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(false);

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(netFile.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(otherNetFile.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Admin_ReturnsAll()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var privateFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.Private, AuthorId = Guid.NewGuid(), Name = "", Format = "", MediaType = "", Url = "" };
        var netFile = new File { Id = Guid.NewGuid(), PublicId = Guid.NewGuid(), AccessLevel = AccessLevel.PublicToNetwork, NetworkId = networkId, Name = "", Format = "", MediaType = "", Url = "" };

        ctx.Files.AddRange(privateFile, netFile);
        await ctx.SaveChangesAsync();

        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
        {
            Id = userId,
            User = new User(),
            NetworkUsers = new List<NetworkUser>(),
            IsDefault = true
        });

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, userId, "perm")).ReturnsAsync(true);

        var verifier = new AccessLevelVerifier(ctx, identityServiceMock.Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(privateFile.Id));
        Assert.That(results.Select(f => f.Id), Contains.Item(netFile.Id));
    }
}