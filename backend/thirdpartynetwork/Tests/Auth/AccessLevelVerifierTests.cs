using System.Text.Json;
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

/// <summary>
/// PLAN.md §2.1/§3.3: AccessLevelVerifier is now derived from PublishedAt + ContentAccessGrant
/// rather than a stored AccessLevel enum — these tests were rewritten around that model.
/// </summary>
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

    private static File NewFile(Guid? authorUserProxyId = null, Guid networkId = default, DateTime? publishedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Bucket = "private",
        ObjectKey = Guid.NewGuid().ToString(),
        AuthorUserProxyId = authorUserProxyId,
        NetworkId = networkId,
        PublishedAt = publishedAt,
        Name = "",
        Format = "",
        MediaType = "",
    };

    private static Mock<IIdentityService> AnonymousIdentity()
    {
        var mock = new Mock<IIdentityService>();
        mock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
            { Id = Guid.Empty, IsDefault = true, User = null! });
        return mock;
    }

    private static Mock<IIdentityService> AuthenticatedIdentity(Guid userProxyId)
    {
        var mock = new Mock<IIdentityService>();
        mock.Setup(s => s.GetUserProxyAsync(true)).ReturnsAsync(new UserProxy
            { Id = userProxyId, IsDefault = true, User = new User() });
        return mock;
    }

    [Test]
    public async Task CheckAccess_Draft_ReturnsFalseForEveryoneButAuthor()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId); // PublishedAt null = Draft
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(Guid.NewGuid()).Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.False);

        var authorVerifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, identityVerifierMock.Object);
        Assert.That(await authorVerifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccess_Draft_NotOverriddenByAnActiveGrant()
    {
        // §2.1's Tier() derivation checks PublishedAt first and returns Draft immediately —
        // a grant on unpublished content is inert until the content is actually published.
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(); // Draft
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(new ContentAccessGrant
        {
            Id = Guid.NewGuid(),
            ContentKind = ContentKind.File,
            ContentId = file.Id,
            GranteeType = GranteeType.User,
            GranteeUserProxyId = granteeId,
            Capabilities = ContentCapabilities.View,
            GrantedByNetworkUserId = Guid.NewGuid(),
        });
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccess_Published_NoGrant_ReturnsAuthorOnly()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();

        var strangerVerifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(Guid.NewGuid()).Object, identityVerifierMock.Object);
        Assert.That(await strangerVerifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.False);

        var authorVerifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, identityVerifierMock.Object);
        Assert.That(await authorVerifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccess_UserGrant_AllowsOnlyNamedUser()
    {
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(new ContentAccessGrant
        {
            Id = Guid.NewGuid(),
            ContentKind = ContentKind.File,
            ContentId = file.Id,
            GranteeType = GranteeType.User,
            GranteeUserProxyId = granteeId,
            Capabilities = ContentCapabilities.View,
            GrantedByNetworkUserId = Guid.NewGuid(),
        });
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();

        var strangerVerifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(Guid.NewGuid()).Object, identityVerifierMock.Object);
        Assert.That(await strangerVerifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.False);

        var granteeVerifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, identityVerifierMock.Object);
        Assert.That(await granteeVerifier.CheckFileAccessAsync(file.Id.ToString(), Guid.NewGuid(), "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccess_NetworkGrant_RequiresMembership()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(new ContentAccessGrant
        {
            Id = Guid.NewGuid(),
            ContentKind = ContentKind.File,
            ContentId = file.Id,
            GranteeType = GranteeType.Network,
            GranteeNetworkId = networkId,
            Capabilities = ContentCapabilities.View,
            GrantedByNetworkUserId = Guid.NewGuid(),
        });
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(userId).Object, identityVerifierMock.Object);

        // Not a member of the granted network yet -> false.
        Assert.That(await verifier.CheckFileAccessAsync(file.Id.ToString(), networkId, "perm"), Is.False);

        // Join the granted network.
        ctx.NetworkUsers.Add(new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
        });
        await ctx.SaveChangesAsync();

        Assert.That(await verifier.CheckFileAccessAsync(file.Id.ToString(), networkId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccess_Anonymous_CoveredOnlyByGrantToAnIsPublicNetwork()
    {
        // §0.3: a Network-type grant naming an IsPublic network also covers that network's
        // anonymous visitors, reusing Network.IsPublic's existing meaning.
        await using var ctx = new DataContext(_options);

        var privateNetworkId = Guid.NewGuid();
        var publicNetworkId = Guid.NewGuid();
        ctx.Networks.Add(new Network
        {
            Id = publicNetworkId, Name = "pub", IsPublic = true, Description = "", RedirectURI = "#",
            Entitlement = new NetworkEntitlement { NetworkId = publicNetworkId },
        });
        ctx.Networks.Add(new Network
        {
            Id = privateNetworkId, Name = "priv", IsPublic = false, Description = "", RedirectURI = "#",
            Entitlement = new NetworkEntitlement { NetworkId = privateNetworkId },
        });

        var fileGrantedToPrivateNetwork = NewFile(publishedAt: DateTime.UtcNow);
        var fileGrantedToPublicNetwork = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(fileGrantedToPrivateNetwork, fileGrantedToPublicNetwork);

        ctx.ContentAccessGrants.AddRange(
            new ContentAccessGrant
            {
                Id = Guid.NewGuid(), ContentKind = ContentKind.File, ContentId = fileGrantedToPrivateNetwork.Id,
                GranteeType = GranteeType.Network, GranteeNetworkId = privateNetworkId,
                Capabilities = ContentCapabilities.View, GrantedByNetworkUserId = Guid.NewGuid(),
            },
            new ContentAccessGrant
            {
                Id = Guid.NewGuid(), ContentKind = ContentKind.File, ContentId = fileGrantedToPublicNetwork.Id,
                GranteeType = GranteeType.Network, GranteeNetworkId = publicNetworkId,
                Capabilities = ContentCapabilities.View, GrantedByNetworkUserId = Guid.NewGuid(),
            });
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckFileAccessAsync(fileGrantedToPrivateNetwork.Id.ToString(), Guid.NewGuid(), "perm"), Is.False);
        Assert.That(await verifier.CheckFileAccessAsync(fileGrantedToPublicNetwork.Id.ToString(), Guid.NewGuid(), "perm"), Is.True);
    }

    [Test]
    public async Task GetAccessPredicate_Unauthenticated_ExcludesEverythingWithoutAnIsPublicNetworkGrant()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();

        var publishedNoGrant = NewFile(publishedAt: DateTime.UtcNow);
        var draft = NewFile();
        ctx.Files.AddRange(publishedNoGrant, draft);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Does.Not.Contain(publishedNoGrant.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(draft.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Authenticated_Author_ReturnsOwnUnpublishedAndPublished()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var othersDraft = NewFile(authorUserProxyId: Guid.NewGuid());
        var myDraft = NewFile(authorUserProxyId: userId);
        var myPublished = NewFile(authorUserProxyId: userId, publishedAt: DateTime.UtcNow);

        ctx.Files.AddRange(othersDraft, myDraft, myPublished);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(userId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(myDraft.Id));
        Assert.That(results.Select(f => f.Id), Contains.Item(myPublished.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(othersDraft.Id));
    }

    [Test]
    public async Task GetAccessPredicate_NetworkMember_ReturnsNetworkGranted()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var otherNetworkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var grantedToMyNetwork = NewFile(publishedAt: DateTime.UtcNow);
        var grantedToOtherNetwork = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(grantedToMyNetwork, grantedToOtherNetwork);

        ctx.ContentAccessGrants.AddRange(
            new ContentAccessGrant
            {
                Id = Guid.NewGuid(), ContentKind = ContentKind.File, ContentId = grantedToMyNetwork.Id,
                GranteeType = GranteeType.Network, GranteeNetworkId = networkId,
                Capabilities = ContentCapabilities.View, GrantedByNetworkUserId = Guid.NewGuid(),
            },
            new ContentAccessGrant
            {
                Id = Guid.NewGuid(), ContentKind = ContentKind.File, ContentId = grantedToOtherNetwork.Id,
                GranteeType = GranteeType.Network, GranteeNetworkId = otherNetworkId,
                Capabilities = ContentCapabilities.View, GrantedByNetworkUserId = Guid.NewGuid(),
            });

        ctx.NetworkUsers.Add(new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
        });
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(userId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(grantedToMyNetwork.Id));
        Assert.That(results.Select(f => f.Id), Does.Not.Contain(grantedToOtherNetwork.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Admin_ReturnsAll()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var othersDraft = NewFile(authorUserProxyId: Guid.NewGuid());
        var grantedToNetwork = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(othersDraft, grantedToNetwork);
        ctx.ContentAccessGrants.Add(new ContentAccessGrant
        {
            Id = Guid.NewGuid(), ContentKind = ContentKind.File, ContentId = grantedToNetwork.Id,
            GranteeType = GranteeType.Network, GranteeNetworkId = networkId,
            Capabilities = ContentCapabilities.View, GrantedByNetworkUserId = Guid.NewGuid(),
        });
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, userId, "perm")).ReturnsAsync(true);

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(userId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var results = ctx.Files.Where(predicate).ToList();

        Assert.That(results.Select(f => f.Id), Contains.Item(othersDraft.Id));
        Assert.That(results.Select(f => f.Id), Contains.Item(grantedToNetwork.Id));
    }
}
