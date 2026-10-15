using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core;
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

    private static Blog NewBlog(Guid? authorUserProxyId = null, Guid networkId = default, DateTime? publishedAt = null, string? slug = null) => new()
    {
        Id = Guid.NewGuid(),
        Title = "t",
        Slug = slug ?? Guid.NewGuid().ToString(),
        Summary = "s",
        Body = JsonDocument.Parse("{}"),
        AuthorUserProxyId = authorUserProxyId,
        NetworkId = networkId,
        PublishedAt = publishedAt,
    };

    private static Configuration NewConfiguration(Guid? authorUserProxyId = null, Guid networkId = default, DateTime? publishedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Key = "k",
        Value = JsonDocument.Parse("{}"),
        AuthorUserProxyId = authorUserProxyId,
        NetworkId = networkId,
        PublishedAt = publishedAt,
    };

    private static CustomPage NewCustomPage(Guid? authorUserProxyId = null, Guid networkId = default, DateTime? publishedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "n",
        Slug = Guid.NewGuid().ToString(),
        AuthorUserProxyId = authorUserProxyId,
        NetworkId = networkId,
        PublishedAt = publishedAt,
    };

    private static PageBlock NewPageBlock(Guid customPageId, Guid? authorUserProxyId = null, Guid networkId = default, DateTime? publishedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Text = "t",
        Position = 0,
        Data = JsonDocument.Parse("{}"),
        CustomPageId = customPageId,
        AuthorUserProxyId = authorUserProxyId,
        NetworkId = networkId,
        PublishedAt = publishedAt,
    };

    private static Network NewNetwork(Guid id, bool isPublic = false, ContentKindFlags? externalContentKinds = null) => new()
    {
        Id = id,
        Name = id.ToString(),
        IsPublic = isPublic,
        Description = "",
        RedirectURI = "#",
        Entitlement = new NetworkEntitlement { NetworkId = id },
        ExternalContentKinds = externalContentKinds ?? ContentKindFlags.File,
    };

    private static ContentModerationFlag NewFlag(ContentKind kind, Guid contentId, Guid networkId, bool hidden = true) => new()
    {
        Id = Guid.NewGuid(),
        ContentKind = kind,
        ContentId = contentId,
        NetworkId = networkId,
        HiddenInNetwork = hidden,
    };

    private static ContentAccessGrant NewGrant(
        ContentKind kind, Guid contentId, GranteeType type,
        Guid? userProxyId = null, Guid? networkId = null, Guid? roleId = null, Guid? permissionId = null,
        ContentCapabilities capabilities = ContentCapabilities.View,
        DateTime? expiresAt = null, DateTime? revokedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        ContentKind = kind,
        ContentId = contentId,
        GranteeType = type,
        GranteeUserProxyId = userProxyId,
        GranteeNetworkId = networkId,
        GranteeRoleId = roleId,
        GranteePermissionId = permissionId,
        Capabilities = capabilities,
        ExpiresAt = expiresAt,
        RevokedAt = revokedAt,
        GrantedByNetworkUserId = Guid.NewGuid(),
    };

    // Creates a NetworkUser + Role + Permission + RolePermission + NetworkUserRole chain so
    // `userProxyId` holds `permissionName` in `networkId`. Returns (roleId, permissionId) so
    // callers can also build GranteeType.NetworkRole/NetworkPermission grants against them.
    private static async Task<(Guid RoleId, Guid PermissionId)> GrantNetworkPermissionAsync(
        DataContext ctx, Guid userProxyId, Guid networkId, string permissionName)
    {
        var permission = new Permission { Id = Guid.NewGuid(), Name = permissionName, Description = "" };
        var role = new Role
        {
            Id = Guid.NewGuid(), Name = permissionName, Description = "", IsDefault = false, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
        };

        ctx.Permissions.Add(permission);
        ctx.Roles.Add(role);
        ctx.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id, Permission = permission });
        ctx.NetworkUsers.Add(networkUser);
        ctx.NetworkUserRoles.Add(new NetworkUserRole { NetworkUserId = networkUser.Id, RoleId = role.Id });

        await ctx.SaveChangesAsync();
        return (role.Id, permission.Id);
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

    // ----------------------------------------------------------------------
    // CheckAccessForCallerAsync - precedence and grant-matrix edge cases
    // ----------------------------------------------------------------------

    [Test]
    public async Task CheckAccessForCaller_NullModel_ReturnsFalse()
    {
        await using var ctx = new DataContext(_options);
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(null, Guid.NewGuid(), Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_Deleted_ReturnsFalseEvenForAuthor()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId, publishedAt: DateTime.UtcNow);
        file.DeletedAt = DateTime.UtcNow;
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), authorId, "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_Draft_PermissionHolder_ReturnsTrue()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: Guid.NewGuid()); // draft, not caller's own
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, networkId, callerId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_Flagged_OverridesPermission_ForNonAuthor()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: Guid.NewGuid(), publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, networkId));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, networkId, callerId, "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_FlaggedInDifferentNetwork_DoesNotHide()
    {
        await using var ctx = new DataContext(_options);
        var flaggedNetworkId = Guid.NewGuid();
        var requestedNetworkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: Guid.NewGuid(), publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, flaggedNetworkId));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(requestedNetworkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, requestedNetworkId, callerId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_FlagNotHiddenInNetwork_DoesNotHide()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: Guid.NewGuid(), publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, networkId, hidden: false));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, networkId, callerId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_Author_SeesContentDespiteFlag()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, networkId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, networkId, authorId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_RevokedGrant_IsInvisible()
    {
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, revokedAt: DateTime.UtcNow));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), granteeId, "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_ExpiredGrant_IsInvisible()
    {
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, expiresAt: DateTime.UtcNow.AddMinutes(-1)));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), granteeId, "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_FutureExpiringGrant_IsVisible()
    {
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, expiresAt: DateTime.UtcNow.AddMinutes(1)));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), granteeId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_GrantWithoutViewCapability_IsInvisible()
    {
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, capabilities: ContentCapabilities.Edit));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), granteeId, "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_GrantWithViewPlusOtherCapabilities_IsVisible()
    {
        await using var ctx = new DataContext(_options);
        var granteeId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, capabilities: ContentCapabilities.View | ContentCapabilities.Comment));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), granteeId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_NetworkRoleGrant_MatchesCallerRole()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var (roleId, _) = await GrantNetworkPermissionAsync(ctx, callerId, networkId, "SomeOtherPermission");
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.NetworkRole, roleId: roleId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), callerId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_NetworkRoleGrant_NoMatch_IsInvisible()
    {
        await using var ctx = new DataContext(_options);
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.NetworkRole, roleId: Guid.NewGuid()));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(Guid.NewGuid()).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_NetworkPermissionGrant_MatchesCallerPermission()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var (_, permissionId) = await GrantNetworkPermissionAsync(ctx, callerId, networkId, "SomePermission");
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.NetworkPermission, permissionId: permissionId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), callerId, "perm"), Is.True);
    }

    [Test]
    public async Task CheckAccessForCaller_NetworkPermissionGrant_NoMatch_IsInvisible()
    {
        await using var ctx = new DataContext(_options);
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.NetworkPermission, permissionId: Guid.NewGuid()));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(Guid.NewGuid()).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckAccessForCaller_Anonymous_NetworkGrant_NonPublicNetwork_IsInvisible()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        ctx.Networks.Add(NewNetwork(networkId, isPublic: false));
        var file = NewFile(publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.Network, networkId: networkId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckAccessForCallerAsync(file, Guid.NewGuid(), Guid.Empty, "perm"), Is.False);
    }

    // ----------------------------------------------------------------------
    // GetAccessPredicateAsync - precedence and grant-matrix edge cases
    // (complements the four pre-existing GetAccessPredicate_* tests above)
    // ----------------------------------------------------------------------

    [Test]
    public async Task GetAccessPredicate_Deleted_ExcludedEvenForAuthor()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId, networkId: networkId, publishedAt: DateTime.UtcNow);
        file.DeletedAt = DateTime.UtcNow;
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Draft_PermissionHolder_Included()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var draft = NewFile(authorUserProxyId: Guid.NewGuid(), networkId: networkId);
        ctx.Files.Add(draft);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(draft.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Draft_GrantOnly_Excluded()
    {
        // Grants only kick in for published content, even at the predicate level.
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var draft = NewFile(authorUserProxyId: Guid.NewGuid(), networkId: networkId);
        ctx.Files.Add(draft);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, draft.Id, GranteeType.User, userProxyId: callerId));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(draft.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Flagged_OverridesPermission()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: Guid.NewGuid(), networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, networkId));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(networkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_FlaggedInDifferentNetwork_NotHidden()
    {
        await using var ctx = new DataContext(_options);
        var requestedNetworkId = Guid.NewGuid();
        var otherNetworkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: Guid.NewGuid(), networkId: requestedNetworkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, otherNetworkId));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsAllowedAsync(requestedNetworkId, callerId, "perm")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(requestedNetworkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Author_SeesContentDespiteFlag()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId, networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, networkId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_RevokedGrant_Excluded()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();
        var file = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, revokedAt: DateTime.UtcNow));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_ExpiredGrant_Excluded()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();
        var file = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, expiresAt: DateTime.UtcNow.AddMinutes(-1)));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_GrantWithoutViewCapability_Excluded()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();
        var file = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: granteeId, capabilities: ContentCapabilities.Edit));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_NetworkRoleGrant_Included()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var (roleId, _) = await GrantNetworkPermissionAsync(ctx, callerId, networkId, "SomeOtherPermission");
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.NetworkRole, roleId: roleId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_NetworkPermissionGrant_Included()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var file = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var (_, permissionId) = await GrantNetworkPermissionAsync(ctx, callerId, networkId, "SomePermission");
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.NetworkPermission, permissionId: permissionId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(file.Id));
    }

    [Test]
    public async Task GetAccessPredicate_Anonymous_RoleOrPermissionGrant_NeverMatches()
    {
        // Anonymous callers have no roles/permissions at all, so these grantee types can never
        // match them - only a Network+IsPublic grant can.
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var file1 = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        var file2 = NewFile(networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(file1, file2);
        ctx.ContentAccessGrants.AddRange(
            NewGrant(ContentKind.File, file1.Id, GranteeType.NetworkRole, roleId: Guid.NewGuid()),
            NewGrant(ContentKind.File, file2.Id, GranteeType.NetworkPermission, permissionId: Guid.NewGuid()));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetAccessPredicateAsync<File>(networkId, "perm");
        var ids = ctx.Files.Where(predicate).Select(f => f.Id).ToList();
        Assert.That(ids, Does.Not.Contain(file1.Id));
        Assert.That(ids, Does.Not.Contain(file2.Id));
    }

    // ----------------------------------------------------------------------
    // GetCrossNetworkAccessPredicateAsync - new, previously untested
    // ----------------------------------------------------------------------

    [Test]
    public async Task GetCrossNetworkAccessPredicate_Anonymous_OnlyPublicGrantedVisible()
    {
        await using var ctx = new DataContext(_options);
        var publicNetworkId = Guid.NewGuid();
        var privateNetworkId = Guid.NewGuid();
        ctx.Networks.Add(NewNetwork(publicNetworkId, isPublic: true));
        ctx.Networks.Add(NewNetwork(privateNetworkId, isPublic: false));

        var publicFile = NewFile(networkId: publicNetworkId, publishedAt: DateTime.UtcNow);
        var privateFile = NewFile(networkId: privateNetworkId, publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(publicFile, privateFile);
        ctx.ContentAccessGrants.AddRange(
            NewGrant(ContentKind.File, publicFile.Id, GranteeType.Network, networkId: publicNetworkId),
            NewGrant(ContentKind.File, privateFile.Id, GranteeType.Network, networkId: privateNetworkId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("perm");
        var ids = ctx.Files.Where(predicate).Select(f => f.Id).ToList();
        Assert.That(ids, Contains.Item(publicFile.Id));
        Assert.That(ids, Does.Not.Contain(privateFile.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_Author_SeesOwnAcrossNetworksIncludingDrafts()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var networkA = Guid.NewGuid();
        var networkB = Guid.NewGuid();

        var draftInA = NewFile(authorUserProxyId: authorId, networkId: networkA);
        var publishedInB = NewFile(authorUserProxyId: authorId, networkId: networkB, publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(draftInA, publishedInB);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("perm");
        var ids = ctx.Files.Where(predicate).Select(f => f.Id).ToList();
        Assert.That(ids, Contains.Item(draftInA.Id));
        Assert.That(ids, Contains.Item(publishedInB.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_Deleted_ExcludedEvenForAuthor()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var file = NewFile(authorUserProxyId: authorId, publishedAt: DateTime.UtcNow);
        file.DeletedAt = DateTime.UtcNow;
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("perm");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_PermissionInOneNetworkOnly_IsolatesOtherNetwork()
    {
        // The core new behavior vs GetAccessPredicateAsync: permission is evaluated per-row
        // against that row's own NetworkId, not one fixed network passed in by the caller.
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var networkA = Guid.NewGuid();
        var networkB = Guid.NewGuid();

        var contentInA = NewFile(authorUserProxyId: authorId, networkId: networkA, publishedAt: DateTime.UtcNow);
        var contentInB = NewFile(authorUserProxyId: authorId, networkId: networkB, publishedAt: DateTime.UtcNow);
        ctx.Files.AddRange(contentInA, contentInB);
        await ctx.SaveChangesAsync();

        await GrantNetworkPermissionAsync(ctx, callerId, networkA, "ReadFile");

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        var ids = ctx.Files.Where(predicate).Select(f => f.Id).ToList();
        Assert.That(ids, Contains.Item(contentInA.Id));
        Assert.That(ids, Does.Not.Contain(contentInB.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_PermissionHolder_SeesDraftsInThatNetwork()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var networkId = Guid.NewGuid();

        var draft = NewFile(authorUserProxyId: authorId, networkId: networkId);
        ctx.Files.Add(draft);
        await ctx.SaveChangesAsync();

        await GrantNetworkPermissionAsync(ctx, callerId, networkId, "ReadFile");

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(draft.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_AdministratorPermission_ActsAsOverride()
    {
        // A caller holding "Administrator" (not the specific read permission) in a network
        // should still see that network's content.
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var networkId = Guid.NewGuid();

        var draft = NewFile(authorUserProxyId: authorId, networkId: networkId);
        ctx.Files.Add(draft);
        await ctx.SaveChangesAsync();

        await GrantNetworkPermissionAsync(ctx, callerId, networkId, Constants.Permissions.Administrator);

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(draft.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_SuperAdmin_SeesEverythingAcrossNetworks_ExceptDeleted()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var networkA = Guid.NewGuid();
        var networkB = Guid.NewGuid();

        var draftInA = NewFile(authorUserProxyId: authorId, networkId: networkA);
        var publishedInB = NewFile(authorUserProxyId: authorId, networkId: networkB, publishedAt: DateTime.UtcNow);
        var deletedInB = NewFile(authorUserProxyId: authorId, networkId: networkB, publishedAt: DateTime.UtcNow);
        deletedInB.DeletedAt = DateTime.UtcNow;
        ctx.Files.AddRange(draftInA, publishedInB, deletedInB);
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsSuperAdminOrHasSuperPermissionAsync(callerId, "ReadFile")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        var ids = ctx.Files.Where(predicate).Select(f => f.Id).ToList();
        Assert.That(ids, Contains.Item(draftInA.Id));
        Assert.That(ids, Contains.Item(publishedInB.Id));
        Assert.That(ids, Does.Not.Contain(deletedInB.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_SuperAdmin_StillHiddenByModerationFlag()
    {
        // Moderation overrides even unlimited (super-admin) access - only the author bypass
        // escapes the flag check.
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var networkId = Guid.NewGuid();

        var file = NewFile(authorUserProxyId: authorId, networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, networkId));
        await ctx.SaveChangesAsync();

        var identityVerifierMock = new Mock<IIdentityVerifier>();
        identityVerifierMock.Setup(v => v.IsSuperAdminOrHasSuperPermissionAsync(callerId, "ReadFile")).ReturnsAsync(true);
        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, identityVerifierMock.Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(file.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_Flagged_KeysOffContentsOwnNetwork()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var contentNetworkId = Guid.NewGuid();
        var unrelatedNetworkId = Guid.NewGuid();

        var file = NewFile(authorUserProxyId: authorId, networkId: contentNetworkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        // Flag scoped to a network this content doesn't belong to - must not hide it.
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.File, file.Id, unrelatedNetworkId));
        await ctx.SaveChangesAsync();

        await GrantNetworkPermissionAsync(ctx, callerId, contentNetworkId, "ReadFile");

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(file.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_UserGrant_VisibleAcrossNetworks()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();
        var networkId = Guid.NewGuid();

        var file = NewFile(authorUserProxyId: authorId, networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User, userProxyId: granteeId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Contains.Item(file.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_GrantOnDraft_Excluded()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();

        var draft = NewFile(authorUserProxyId: authorId);
        ctx.Files.Add(draft);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, draft.Id, GranteeType.User, userProxyId: granteeId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(draft.Id));
    }

    [Test]
    public async Task GetCrossNetworkAccessPredicate_MembershipWithoutPermission_DoesNotGrantAccess()
    {
        // Being a NetworkUser in the content's network is not enough on its own - the caller
        // needs the actual read permission (or Administrator) via a role.
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var networkId = Guid.NewGuid();

        var draft = NewFile(authorUserProxyId: authorId, networkId: networkId);
        ctx.Files.Add(draft);
        ctx.NetworkUsers.Add(new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = callerId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
        });
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(callerId).Object, new Mock<IIdentityVerifier>().Object);

        var predicate = await verifier.GetCrossNetworkAccessPredicateAsync<File>("ReadFile");
        Assert.That(ctx.Files.Where(predicate).Select(f => f.Id), Does.Not.Contain(draft.Id));
    }

    // ----------------------------------------------------------------------
    // CanNetworkExternallyAdoptAsync - previously untested
    // ----------------------------------------------------------------------

    [Test]
    public async Task CanNetworkExternallyAdopt_SameNetwork_TrueEvenIfDeleted()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var file = NewFile(networkId: networkId);
        file.DeletedAt = DateTime.UtcNow;
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, networkId), Is.True);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_DifferentNetwork_Deleted_False()
    {
        await using var ctx = new DataContext(_options);
        var file = NewFile(networkId: Guid.NewGuid());
        file.DeletedAt = DateTime.UtcNow;
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_NamedInvitation_TrueEvenForDraft()
    {
        await using var ctx = new DataContext(_options);
        var consumingNetworkId = Guid.NewGuid();
        var file = NewFile(networkId: Guid.NewGuid()); // draft
        ctx.Files.Add(file);
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.Network, networkId: consumingNetworkId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, consumingNetworkId), Is.True);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_Draft_NoInvitation_False()
    {
        await using var ctx = new DataContext(_options);
        var file = NewFile(networkId: Guid.NewGuid()); // draft
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_Published_FlagNotAllowed_False()
    {
        await using var ctx = new DataContext(_options);
        var ownerNetworkId = Guid.NewGuid();
        var consumingNetworkId = Guid.NewGuid();
        ctx.Networks.Add(NewNetwork(ownerNetworkId, externalContentKinds: ContentKindFlags.None));

        var file = NewFile(networkId: ownerNetworkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        // Even an unrelated live grant exists - the per-kind opt-in flag still wins.
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User, userProxyId: Guid.NewGuid()));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, consumingNetworkId), Is.False);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_Published_FlagAllowed_AnyLiveGrantExists_True()
    {
        await using var ctx = new DataContext(_options);
        var ownerNetworkId = Guid.NewGuid();
        var consumingNetworkId = Guid.NewGuid();
        ctx.Networks.Add(NewNetwork(ownerNetworkId, externalContentKinds: ContentKindFlags.File));

        var file = NewFile(networkId: ownerNetworkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        // Note the current implementation doesn't require this grant to name consumingNetworkId
        // or carry View - any live grant at all is treated as proof the content is being shared.
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.File, file.Id, GranteeType.User,
            userProxyId: Guid.NewGuid(), capabilities: ContentCapabilities.Comment));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, consumingNetworkId), Is.True);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_Published_FlagAllowed_NoGrantsAtAll_False()
    {
        await using var ctx = new DataContext(_options);
        var ownerNetworkId = Guid.NewGuid();
        var consumingNetworkId = Guid.NewGuid();
        ctx.Networks.Add(NewNetwork(ownerNetworkId, externalContentKinds: ContentKindFlags.File));

        var file = NewFile(networkId: ownerNetworkId, publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, consumingNetworkId), Is.False);
    }

    [Test]
    public async Task CanNetworkExternallyAdopt_OwningNetworkMissing_False()
    {
        await using var ctx = new DataContext(_options);
        // Owning network row deliberately not added to the context.
        var file = NewFile(networkId: Guid.NewGuid(), publishedAt: DateTime.UtcNow);
        ctx.Files.Add(file);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CanNetworkExternallyAdoptAsync(file, ContentKind.File, Guid.NewGuid()), Is.False);
    }

    // ----------------------------------------------------------------------
    // Content-lookup wrappers (CheckBlogAccessAsync etc.) - resolution smoke tests
    // ----------------------------------------------------------------------

    [Test]
    public async Task CheckBlogAccessAsync_NotFound_ReturnsFalse()
    {
        await using var ctx = new DataContext(_options);
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckBlogAccessAsync(Guid.NewGuid().ToString(), Guid.NewGuid(), "perm"), Is.False);
        Assert.That(await verifier.CheckBlogAccessAsync("not-a-guid-or-known-slug", Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckBlogAccessAsync_ResolvesByIdOrBySlug()
    {
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var blog = NewBlog(authorUserProxyId: authorId, slug: "my-post");
        ctx.Blogs.Add(blog);
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(authorId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckBlogAccessAsync(blog.Id.ToString(), Guid.NewGuid(), "perm"), Is.True);
        Assert.That(await verifier.CheckBlogAccessAsync("my-post", Guid.NewGuid(), "perm"), Is.True);
    }

    [Test]
    public async Task CheckConfigurationAccessAsync_And_CheckCustomPageAccessAsync_NotFound_ReturnsFalse()
    {
        await using var ctx = new DataContext(_options);
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckConfigurationAccessAsync(Guid.NewGuid().ToString(), Guid.NewGuid(), "perm"), Is.False);
        Assert.That(await verifier.CheckCustomPageAccessAsync(Guid.NewGuid().ToString(), Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckPageBlockAccessAsync_NotFound_ReturnsFalse()
    {
        await using var ctx = new DataContext(_options);
        var verifier = new AccessLevelVerifier(ctx, AnonymousIdentity().Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckPageBlockAccessAsync(Guid.NewGuid().ToString(), Guid.NewGuid(), "perm"), Is.False);
    }

    [Test]
    public async Task CheckPageBlockAccessAsync_DependsOnParentCustomPageAccess_NotItsOwnGrants()
    {
        // The implementation explicitly checks the parent CustomPage's access, ignoring any
        // grant/flag placed directly on the PageBlock row itself.
        await using var ctx = new DataContext(_options);
        var authorId = Guid.NewGuid();
        var customPage = NewCustomPage(authorUserProxyId: authorId, publishedAt: DateTime.UtcNow);
        var block = NewPageBlock(customPage.Id, publishedAt: DateTime.UtcNow); // block itself has no author
        ctx.CustomPages.Add(customPage);
        ctx.PageBlocks.Add(block);
        // Grant placed on the PARENT custom page, not on the block.
        var granteeId = Guid.NewGuid();
        ctx.ContentAccessGrants.Add(NewGrant(ContentKind.CustomPage, customPage.Id, GranteeType.User, userProxyId: granteeId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(granteeId).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckPageBlockAccessAsync(block.Id.ToString(), Guid.NewGuid(), "perm"), Is.True);
    }

    [Test]
    public async Task CheckPageBlockAccessAsync_ParentCustomPageHidden_HidesBlockToo()
    {
        await using var ctx = new DataContext(_options);
        var networkId = Guid.NewGuid();
        var customPage = NewCustomPage(authorUserProxyId: Guid.NewGuid(), networkId: networkId, publishedAt: DateTime.UtcNow);
        var block = NewPageBlock(customPage.Id, networkId: networkId, publishedAt: DateTime.UtcNow);
        ctx.CustomPages.Add(customPage);
        ctx.PageBlocks.Add(block);
        ctx.ContentModerationFlags.Add(NewFlag(ContentKind.CustomPage, customPage.Id, networkId));
        await ctx.SaveChangesAsync();

        var verifier = new AccessLevelVerifier(ctx, AuthenticatedIdentity(Guid.NewGuid()).Object, new Mock<IIdentityVerifier>().Object);

        Assert.That(await verifier.CheckPageBlockAccessAsync(block.Id.ToString(), networkId, "perm"), Is.False);
    }
}
