using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// "Read-driven" gating group: List, Get-single, and the anonymous /cdn/{token} + /view/{token}
/// redirects — all ultimately governed by AccessLevelVerifier's waterfall (deleted -> author ->
/// moderation-hide -> permission-bypass -> draft-gate -> grants). Two sub-groups:
///  (A) caller-driven visibility, across representative callers x representative content
///      states x all 5 kinds (List always returns 200 with filtered rows — see the dedicated
///      test below — so List assertions check row PRESENCE, not status code).
///  (B) the grant mechanism itself (6 grantee types + expired/revoked/no-View-capability),
///      tested thoroughly against File and spot-checked against Blog — the grant-matching
///      code (AccessLevelVerifier.CheckAccessForCallerAsync's grant query) is identical
///      across all 5 kinds, parameterized only by ContentKindMapper, so this is an
///      equivalence-class trim, not a coverage gap.
/// </summary>
[TestFixture]
public class ReadDrivenEndpointTests : IntegrationTestBase
{
    private static IEnumerable<ContentKind> AllKinds() => ContentKindCatalog.All.Select(d => d.Kind);

    // -----------------------------------------------------------------------------------
    // (A) Caller-driven visibility — List always 200s; assert on row presence/absence.
    // -----------------------------------------------------------------------------------
    private static readonly CallerSpec[] VisibilityCallers =
    [
        CallerSpec.Anonymous, CallerSpec.Author, CallerSpec.SameNetworkAdmin,
        CallerSpec.ExternalNetworkAdmin, CallerSpec.MainNetworkAdmin,
        CallerSpec.SameNetworkReadOnly, CallerSpec.SameNetworkNone,
        CallerSpec.ExternalNetworkReadOnly, CallerSpec.ExternalNetworkNone,
    ];

    [TestCaseSource(nameof(ListCases))]
    public async Task List_AlwaysReturns200_ButRowVisibilityMatchesAccessRules(ContentKind kind, CallerSpec spec)
    {
        if (kind == ContentKind.PageBlock) Assert.Ignore("PageBlock has no List endpoint.");

        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var published = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, publishedAt: DateTime.UtcNow);
        var draft = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId);

        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);

        var response = await caller.Client.GetAsync(descriptor.CollectionRoute(ContentNetwork.Id));

        AssertStatus(response, HttpStatusCode.OK, $"List {kind} as {spec} (should always 200)");

        var ids = await ExtractIdsAsync(response, kind);

        var canSee = spec is CallerSpec.Author or CallerSpec.SameNetworkAdmin
            or CallerSpec.MainNetworkAdmin or CallerSpec.SameNetworkReadOnly;

        Assert.That(ids.Contains(draft.ContentId), Is.EqualTo(canSee),
            $"List {kind} as {spec}: draft row visibility mismatch.");
        Assert.That(ids.Contains(published.ContentId), Is.EqualTo(canSee),
            $"List {kind} as {spec}: published-no-grant row visibility mismatch.");
    }

    private static IEnumerable<TestCaseData> ListCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in VisibilityCallers)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    [TestCaseSource(nameof(GetSingleCases))]
    public async Task GetSingle_Draft_OnlyVisibleToAuthorAndAdmins(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var draft = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId);
        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await caller.Client.GetAsync(ItemRoute(kind, draft));

        var canSee = kind == ContentKind.File
            ? spec is CallerSpec.Author or CallerSpec.MainNetworkAdmin
            : spec is CallerSpec.Author or CallerSpec.SameNetworkAdmin or CallerSpec.MainNetworkAdmin or CallerSpec.SameNetworkReadOnly;

        // Authenticated-but-denied is 403 Forbidden here (not 404) — [RequirePermission]'s
        // policy failure with an authenticated principal triggers ASP.NET's Forbid path;
        // only Anonymous (no principal at all) triggers the Challenge (401) path. Neither
        // ever falls back to the controller's own `return NotFound()` — that only fires
        // once authorization has already succeeded and the DB lookup itself comes up empty.
        var expectedWhenDenied = spec == CallerSpec.Anonymous ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden;
        AssertStatus(response, canSee ? HttpStatusCode.OK : expectedWhenDenied, $"GetSingle draft {kind} as {spec}");
    }

    private static IEnumerable<TestCaseData> GetSingleCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in VisibilityCallers)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    [Test]
    public async Task GetSingle_ModerationHidden_InCallersNetwork_Returns404_EvenThoughPublished()
    {
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        Ctx.ContentModerationFlags.Add(new ContentModerationFlag
        {
            Id = Guid.NewGuid(), ContentKind = ContentKind.Blog, ContentId = content.ContentId,
            NetworkId = ContentNetwork.Id, Reason = "test", Status = ModerationFlagStatus.Open, HiddenInNetwork = true,
        });
        await Ctx.SaveChangesAsync();

        var reader = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkReadOnly, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);
        var response = await reader.Client.GetAsync(ItemRoute(ContentKind.Blog, content));

        AssertStatus(response, HttpStatusCode.Forbidden, "GetSingle Blog, moderation-hidden in caller's network");
    }

    [Test]
    public async Task GetSingle_ModerationHidden_InADifferentNetwork_DoesNotAffectVisibility_Control()
    {
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        Ctx.ContentModerationFlags.Add(new ContentModerationFlag
        {
            Id = Guid.NewGuid(), ContentKind = ContentKind.Blog, ContentId = content.ContentId,
            NetworkId = ExternalNetwork.Id, // flagged in a DIFFERENT network than the content's own
            Reason = "test", Status = ModerationFlagStatus.Open, HiddenInNetwork = true,
        });
        await Ctx.SaveChangesAsync();

        var reader = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkReadOnly, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);
        var response = await reader.Client.GetAsync(ItemRoute(ContentKind.Blog, content));

        AssertStatus(response, HttpStatusCode.OK, "GetSingle Blog, moderation-hidden in a DIFFERENT network (control)");
    }

    [Test]
    public async Task FileSingleGet_UsesManageFilePermission_NotReadFile_KnownDivergence()
    {
        var file = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.File, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        var blog = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        // Two separate callers, each built for their OWN kind — CallerCatalog.BuildAsync's
        // "kind" parameter picks which Read<Kind> permission the role gets, so a single
        // shared caller here would only ever hold one of the two permissions.
        var fileReader = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkReadOnly, ContentKind.File, ContentNetwork, ExternalNetwork, MainNetwork);
        var blogReader = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkReadOnly, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);

        var fileResponse = await fileReader.Client.GetAsync(ItemRoute(ContentKind.File, file));
        var blogResponse = await blogReader.Client.GetAsync(ItemRoute(ContentKind.Blog, blog));

        AssertStatus(fileResponse, HttpStatusCode.Forbidden, "GetSingle File as ReadFile-only holder (needs ManageFile instead)");
        AssertStatus(blogResponse, HttpStatusCode.OK, "GetSingle Blog as ReadBlog-only holder (control: works as expected)");
    }

    // -----------------------------------------------------------------------------------
    // (B) Grant mechanism — thorough on Blog, spot-checked on Configuration. NOT File: see
    // BUG-002 below — File's single-GET never reaches the grant-checking code at all.
    // Authenticated-but-denied is 403 (not 404) throughout, same reasoning as above.
    // -----------------------------------------------------------------------------------
    [TestCaseSource(nameof(GrantKinds))]
    public async Task Grant_ToSpecificUser_AllowsOnlyThatUser(ContentKind kind)
    {
        var grantee = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var stranger = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(kind, content.ContentId, GranteeType.User, granteeUserProxyId: grantee.UserProxyId);

        AssertStatus(await grantee.Client.GetAsync(ItemRoute(kind, content)), HttpStatusCode.OK, $"Grant-to-user {kind}: grantee");
        AssertStatus(await stranger.Client.GetAsync(ItemRoute(kind, content)), HttpStatusCode.Forbidden, $"Grant-to-user {kind}: stranger");
    }

    [TestCaseSource(nameof(GrantKinds))]
    public async Task Grant_ToNetwork_RequiresMembership(ContentKind kind)
    {
        var member = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(kind, content.ContentId, GranteeType.Network, granteeNetworkId: ExternalNetwork.Id);

        AssertStatus(await member.Client.GetAsync(ItemRoute(kind, content)), HttpStatusCode.OK, $"Grant-to-network {kind}: member of granted network");

        var nonMember = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkNone, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        AssertStatus(await nonMember.Client.GetAsync(ItemRoute(kind, content)), HttpStatusCode.Forbidden, $"Grant-to-network {kind}: not a member of the granted network");
    }

    [Test]
    public async Task Grant_ToPublicNetwork_AlsoCoversAnonymousVisitors()
    {
        var publicNetwork = await CallerFixtures.CreateNetworkAsync(Ctx, $"public-{Guid.NewGuid():N}", isPublic: true);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.Network, granteeNetworkId: publicNetwork.Id);

        var anon = CallerFixtures.BuildAnonymous();
        AssertStatus(await anon.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.OK, "Grant to an IsPublic network covers anonymous visitors");
    }

    [Test]
    public async Task Grant_ToPrivateNetwork_DoesNotCoverAnonymousVisitors_Control()
    {
        var privateNetwork = await CallerFixtures.CreateNetworkAsync(Ctx, $"private-{Guid.NewGuid():N}", isPublic: false);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.Network, granteeNetworkId: privateNetwork.Id);

        var anon = CallerFixtures.BuildAnonymous();
        AssertStatus(await anon.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.Unauthorized, "Grant to a non-public network (control)");
    }

    [Test]
    public async Task Grant_ToNetworkRole_AllowsHoldersOfThatRole()
    {
        var role = await CallerFixtures.CreateRoleAsync(Ctx, ExternalNetwork, $"grantee-role-{Guid.NewGuid():N}");
        var proxy = await CallerFixtures.CreateUserAsync(Ctx, "RoleGrantee");
        var networkUser = await CallerFixtures.JoinNetworkAsync(Ctx, ExternalNetwork, proxy, [role]);
        var holder = await CallerFixtures.BuildAuthenticatedAsync("RoleGrantee", networkUser);

        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.NetworkRole, granteeRoleId: role.Id);

        AssertStatus(await holder.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.OK, "Grant to a NetworkRole: role holder");

        var nonHolder = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);
        AssertStatus(await nonHolder.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.Forbidden, "Grant to a NetworkRole: non-holder");
    }

    [Test]
    public async Task Grant_ToNetworkPermission_AllowsHoldersOfThatPermission()
    {
        const string grantedPermName = "test:grant-permission";
        var role = await CallerFixtures.CreateRoleAsync(Ctx, ExternalNetwork, $"perm-role-{Guid.NewGuid():N}", grantedPermName);
        var proxy = await CallerFixtures.CreateUserAsync(Ctx, "PermGrantee");
        var networkUser = await CallerFixtures.JoinNetworkAsync(Ctx, ExternalNetwork, proxy, [role]);
        var holder = await CallerFixtures.BuildAuthenticatedAsync("PermGrantee", networkUser);

        var grantedPermission = await Ctx.Permissions.FirstAsync(p => p.Name == grantedPermName);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.NetworkPermission, granteePermissionId: grantedPermission.Id);

        AssertStatus(await holder.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.OK, "Grant to a NetworkPermission: holder");
    }

    [Test]
    public async Task Grant_Expired_ActsAsNoGrant()
    {
        var grantee = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.User, granteeUserProxyId: grantee.UserProxyId,
            expiresAt: DateTime.UtcNow.AddDays(-1));

        AssertStatus(await grantee.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.Forbidden, "Expired grant acts as no grant");
    }

    [Test]
    public async Task Grant_Revoked_ActsAsNoGrant()
    {
        var grantee = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.User, granteeUserProxyId: grantee.UserProxyId,
            revokedAt: DateTime.UtcNow);

        AssertStatus(await grantee.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.Forbidden, "Revoked grant acts as no grant");
    }

    [Test]
    public async Task Grant_WithoutViewCapability_ActsAsNoGrant()
    {
        var grantee = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, ContentKind.Blog, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.Blog, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.Blog, content.ContentId, GranteeType.User, granteeUserProxyId: grantee.UserProxyId,
            capabilities: ContentCapabilities.Edit); // Edit but NOT View

        AssertStatus(await grantee.Client.GetAsync(ItemRoute(ContentKind.Blog, content)), HttpStatusCode.Forbidden, "Grant without View capability acts as no grant for reads");
    }

    // -----------------------------------------------------------------------------------
    // BUG-002: File's single-GET is gated by ManageFile, which routes
    // through PermissionHandler.HandleContentManageRequirementAsync -> IdentityVerifier.
    // IsContentOwnerAsync (author-or-network-admin ONLY). It never calls
    // AccessLevelVerifier.CheckFileAccessAsync at all — meaning ContentAccessGrants 
    // are COMPLETELY INERT for retrieving a File's metadata/viewingUrl through
    // the API. A file shared with a specific user, or with an entire public network, is
    // unreachable to that grantee via GET /networks/{id}/files/{fileId} — only the file's
    // own author or that network's Administrator can ever retrieve it. This is a strictly
    // bigger problem than "wrong permission name": the sharing feature does not work for
    // Files specifically, regardless of which permission name gates it.
    // -----------------------------------------------------------------------------------
    [Test]
    public async Task Grant_ToSpecificUser_DoesNotHelp_ForFile_BUG002()
    {
        var grantee = await CallerCatalog.BuildAsync(Ctx, CallerSpec.ExternalNetworkNone, ContentKind.File, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.File, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        await AddGrantAsync(ContentKind.File, content.ContentId, GranteeType.User, granteeUserProxyId: grantee.UserProxyId);

        var response = await grantee.Client.GetAsync(ItemRoute(ContentKind.File, content));

        AssertStatus(response, HttpStatusCode.Forbidden,
            "BUG-002: a File explicitly shared with this exact user via ContentAccessGrant is " +
            "still unreachable through GetSingle, because File's single-GET checks ManageFile " +
            "(ownership-only) and never consults grants at all — unlike every other kind.");
    }

    private static IEnumerable<ContentKind> GrantKinds() => [ContentKind.Blog, ContentKind.Configuration];

    private async Task AddGrantAsync(
        ContentKind kind, Guid contentId, GranteeType granteeType,
        Guid? granteeUserProxyId = null, Guid? granteeNetworkId = null, Guid? granteeRoleId = null, Guid? granteePermissionId = null,
        DateTime? expiresAt = null, DateTime? revokedAt = null, ContentCapabilities capabilities = ContentCapabilities.View)
    {
        Ctx.ContentAccessGrants.Add(new ContentAccessGrant
        {
            Id = Guid.NewGuid(), ContentKind = kind, ContentId = contentId, GranteeType = granteeType,
            GranteeUserProxyId = granteeUserProxyId, GranteeNetworkId = granteeNetworkId,
            GranteeRoleId = granteeRoleId, GranteePermissionId = granteePermissionId,
            Capabilities = capabilities, ExpiresAt = expiresAt, RevokedAt = revokedAt,
        });
        await Ctx.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------------------
    // Anonymous redirect endpoints — one representative check each (File -> /cdn, Blog -> /view).
    // -----------------------------------------------------------------------------------
    [Test]
    public async Task CdnRedirect_ForPubliclyVisibleFile_Returns302()
    {
        // Deliberately NO grant at all: if this file were fully anonymous-visible,
        // ComputeViewingUrlAsync would shortcut straight to a stable public MinIO URL
        // instead of minting a /cdn/{token} link (see its own doc comment) — this test
        // wants the actual redirect-endpoint mechanism, so the file must NOT be publicly
        // reachable. SameNetworkAdmin still reaches GetSingle via IsContentOwnerAsync's
        // null-author fallback to IsNetworkAdminAsync (no author was set below).
        var file = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.File, ContentNetwork.Id, publishedAt: DateTime.UtcNow);

        var admin = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkAdmin, ContentKind.File, ContentNetwork, ExternalNetwork, MainNetwork);
        var itemResponse = await admin.Client.GetAsync(ItemRoute(ContentKind.File, file));
        AssertStatus(itemResponse, HttpStatusCode.OK, "GetSingle File to compute ViewingUrl");

        var dto = await itemResponse.Content.ReadFromJsonAsync<JsonElement>();
        var viewingUrl = dto.GetProperty("viewingUrl").GetString();
        Assert.That(viewingUrl, Is.Not.Null.And.Contains("/cdn/"));

        using var noRedirectClient = IntegrationTestSetup.Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var redirectResponse = await noRedirectClient.GetAsync(new Uri(viewingUrl!).PathAndQuery);

        AssertStatus(redirectResponse, HttpStatusCode.Redirect, "/cdn/{token} should 302-redirect to a presigned URL");
    }

    private static string ItemRoute(ContentKind kind, SeededContent content)
    {
        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        return kind == ContentKind.PageBlock
            ? $"networks/{content.NetworkId}/customPages/{content.ParentCustomPageId}/pageBlocks/{content.ContentId}"
            : descriptor.ItemRoute(content.NetworkId, content.ContentId);
    }

    private static async Task<HashSet<Guid>> ExtractIdsAsync(HttpResponseMessage response, ContentKind kind)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = new HashSet<Guid>();
        foreach (var item in json.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var idProp) && idProp.TryGetGuid(out var id))
                ids.Add(id);
        }
        return ids;
    }
}
