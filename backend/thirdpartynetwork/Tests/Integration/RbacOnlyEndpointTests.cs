using System.Net;
using System.Text.Json;
using NUnit.Framework;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// "RBAC-only" gating group (see the in-conversation gating-group breakdown): Create,
/// Update, Delete, and Moderation-flag List/Create/Resolve. Update/Delete are gated by
/// PermissionHandler.HandleContentManageRequirementAsync -> IdentityVerifier.
/// IsContentOwnerAsync (author-or-network-admin, PLAN.md §4's intended "narrowed Manage*"
/// behavior). Moderation-flag actions are gated by a *plain* network-scoped permission check
/// (Moderate&lt;Kind&gt;) via the PermissionHandler fallthrough branch — a caller holding
/// Moderate&lt;Kind&gt; in the content's network succeeds regardless of authorship.
/// </summary>
[TestFixture]
public class RbacOnlyEndpointTests : IntegrationTestBase
{
    private static IEnumerable<ContentKind> AllKinds() => ContentKindCatalog.All.Select(d => d.Kind);

    // ---------------------------------------------------------------------------------
    // BUG-CREATE-001 (NOT documented in PLAN.md — found while building this suite):
    // PermissionHandler.HandleContentManageRequirementAsync resolves the target content id
    // via ResolveContentRouteMatch, which only looks for {fileId}/{blogId}/{configurationId}/
    // {customPageId} route values. A POST-create request has none of these (the resource
    // doesn't exist yet) -> ResolveContentRouteMatch returns null -> the method returns
    // `false` UNCONDITIONALLY, before even the super-admin check. Since Create's only
    // [RequirePermission] entry is the single Manage<Kind> permission (which IS in
    // Constants.ContentManagePermissions, so it routes through this exact handler — not the
    // plain network-RBAC fallthrough), this means: as coded today, NOBODY — not even a
    // Main-network super-admin — can successfully call
    //   POST /networks/{networkId}/files
    //   POST /networks/{networkId}/blogs
    //   POST /networks/{networkId}/configurations
    //   POST /networks/{networkId}/customPages
    // via the live HTTP API. Every one of these always 403s. This is a distinct,
    // previously-unflagged bug — not one of PLAN.md's documented Wave-1 divergences.
    // The tests below assert the INTENDED behavior (an admin should be able to create
    // content) and are therefore expected to currently FAIL, correctly surfacing the bug
    // rather than being quietly worked around.
    // ---------------------------------------------------------------------------------
    [TestCaseSource(nameof(BrokenCreateKinds))]
    public async Task Create_AsSameNetworkAdmin_ShouldSucceed(ContentKind kind)
    {
        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        var admin = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkAdmin, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await admin.Client.PostAsync(
            descriptor.CollectionRoute(ContentNetwork.Id), ContentKindFixtures.BuildCreateRequestBody(kind));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.Created),
            $"BUG-CREATE-001: a network admin should be able to create a {kind}, but " +
            $"HandleContentManageRequirementAsync always denies POST-create (no content id " +
            $"exists yet to resolve ownership against). Got {response.StatusCode}.");
    }

    private static IEnumerable<ContentKind> BrokenCreateKinds() =>
        [ContentKind.File, ContentKind.Blog, ContentKind.Configuration, ContentKind.CustomPage];

    [TestCaseSource(nameof(BrokenCreateKinds))]
    public async Task Create_Anonymous_IsRejected(ContentKind kind)
    {
        // Sanity check that at least authentication gating still works ahead of the deeper
        // BUG-CREATE-001 above.
        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        var anon = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Anonymous, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await anon.Client.PostAsync(
            descriptor.CollectionRoute(ContentNetwork.Id), ContentKindFixtures.BuildCreateRequestBody(kind));

        AssertStatus(response, HttpStatusCode.Unauthorized, $"Create {kind} as Anonymous");
    }

    // PageBlock's create route nests under an existing {customPageId}, so
    // ResolveContentRouteMatch DOES find a route match there (falls through to matching
    // "customPageId", since no {pageBlockId} exists yet either) — meaning PageBlock create
    // is ownership-gated against its PARENT CustomPage, not broken the same way as the four
    // above. A parent with no author (AuthorOwned + null AuthorUserProxyId) falls back to
    // IsNetworkAdminAsync (admin-only) per IdentityVerifier.IsContentOwnerAsync.
    [Test]
    public async Task Create_PageBlock_AsSameNetworkAdmin_Succeeds_ViaParentCustomPageOwnership()
    {
        var parent = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.CustomPage, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        var admin = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkAdmin, ContentKind.PageBlock, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await admin.Client.PostAsync(
            $"networks/{ContentNetwork.Id}/customPages/{parent.ContentId}/pageBlocks",
            ContentKindFixtures.BuildCreateRequestBody(ContentKind.PageBlock));

        AssertStatus(response, HttpStatusCode.OK, "Create PageBlock as SameNetworkAdmin (parent has no author)");
    }

    [Test]
    public async Task Create_PageBlock_AsManageOnly_NonAdmin_NonOwner_IsForbidden()
    {
        var parent = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.CustomPage, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        var caller = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkManageOnly, ContentKind.PageBlock, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await caller.Client.PostAsync(
            $"networks/{ContentNetwork.Id}/customPages/{parent.ContentId}/pageBlocks",
            ContentKindFixtures.BuildCreateRequestBody(ContentKind.PageBlock));

        // Not the author of the parent page, not a network admin, and holding ManagePageBlock
        // (not ManageCustomPage) doesn't help here since ownership is checked against the
        // PARENT CustomPage's ContentKind, not PageBlock's.
        AssertStatus(response, HttpStatusCode.Forbidden, "Create PageBlock as SameNetworkManageOnly, non-owner parent");
    }

    // -----------------------------------------------------------------------------------
    // Update / Delete — ownership-gated (author-or-network-admin). Full 17-caller matrix.
    // -----------------------------------------------------------------------------------
    [TestCaseSource(nameof(UpdateDeleteCases))]
    public async Task Update_MatchesOwnershipGatedExpectation(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, publishedAt: DateTime.UtcNow);

        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        var route = kind == ContentKind.PageBlock
            ? $"networks/{ContentNetwork.Id}/customPages/{content.ParentCustomPageId}/pageBlocks/{content.ContentId}"
            : descriptor.ItemRoute(ContentNetwork.Id, content.ContentId);

        var response = await caller.Client.PutAsync(route, ContentKindFixtures.BuildUpdateRequestBody(kind));

        AssertStatus(response, ExpectedOwnershipGatedStatus(spec), $"Update {kind} as {spec}");
    }

    [TestCaseSource(nameof(UpdateDeleteCases))]
    public async Task Delete_MatchesOwnershipGatedExpectation(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, publishedAt: DateTime.UtcNow);

        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        var route = kind == ContentKind.PageBlock
            ? $"networks/{ContentNetwork.Id}/customPages/{content.ParentCustomPageId}/pageBlocks/{content.ContentId}"
            : descriptor.ItemRoute(ContentNetwork.Id, content.ContentId);

        var response = await caller.Client.DeleteAsync(route);
        var expected = ExpectedOwnershipGatedStatus(spec);

        // Delete succeeds with 204 NoContent, not 200 OK (Update's success status) — the one
        // place this group's shared expected-status helper needs a per-operation override.
        AssertStatus(response, expected == HttpStatusCode.OK ? HttpStatusCode.NoContent : expected, $"Delete {kind} as {spec}");
    }

    private static IEnumerable<TestCaseData> UpdateDeleteCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in CallerCatalog.AllSeventeen)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    private static HttpStatusCode ExpectedOwnershipGatedStatus(CallerSpec spec) => spec switch
    {
        CallerSpec.Anonymous => HttpStatusCode.Unauthorized,
        // UnauthorizedHttpException's default constructor always maps to 403, regardless of
        // its name — PreChecks' AccessIncomplete block is genuinely a 403, not a 401.
        CallerSpec.SameNetworkAccessIncomplete or CallerSpec.ExternalNetworkAccessIncomplete => HttpStatusCode.Forbidden,
        CallerSpec.Author => HttpStatusCode.OK,
        // NOT SameNetworkAdmin: IdentityVerifier.IsContentOwnerAsync's AuthorOwned-with-an-
        // author-set branch checks ONLY the author (or a sibling persona) — it does not fall
        // back to IsNetworkAdminAsync at all in that branch (that fallback only fires for
        // NetworkOwned content, or AuthorOwned content whose author persona was deleted).
        // A plain network Administrator — even of the content's own network — genuinely
        // cannot edit/delete another member's AuthorOwned content. This matches PLAN.md §4's
        // literal wording ("author-only for AuthorOwned content") once traced through the
        // actual code, even though it's a stricter reading than "admin" usually implies.
        // MainNetworkAdmin still succeeds via a SEPARATE, network-agnostic super-admin check
        // inside HandleContentManageRequirementAsync itself.
        CallerSpec.MainNetworkAdmin => HttpStatusCode.OK,
        _ => HttpStatusCode.Forbidden,
    };

    // -----------------------------------------------------------------------------------
    // Moderation-flag List/Create/Resolve — plain Moderate<Kind> network-permission gate,
    // NOT ownership. This is the one non-admin, non-author permission variant that should
    // actually succeed.
    // -----------------------------------------------------------------------------------
    [TestCaseSource(nameof(ModerationCases))]
    public async Task ModerationFlags_List_MatchesPlainPermissionExpectation(ContentKind kind, CallerSpec spec)
    {
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        var caller = await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await caller.Client.GetAsync(ModerationFlagsRoute(kind, content));

        AssertStatus(response, ExpectedPlainPermissionGatedStatus(spec), $"ModerationFlags list {kind} as {spec}");
    }

    [TestCaseSource(nameof(ModerationCases))]
    public async Task ModerationFlags_Create_MatchesPlainPermissionExpectation(ContentKind kind, CallerSpec spec)
    {
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        var caller = await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var body = new StringContent(
            JsonSerializer.Serialize(new { reason = "test flag", hiddenInNetwork = true }),
            System.Text.Encoding.UTF8, "application/json");
        var response = await caller.Client.PostAsync(ModerationFlagsRoute(kind, content), body);

        // MainNetworkAdmin passes authorization (confirmed separately: it's a genuine 404
        // from ContentModerationFlagService, not a 403) but then 404s at the service layer,
        // which looks up the caller's NetworkUser row IN THE CONTENT's network to stamp
        // FlaggedByNetworkUserId — a Main-network admin acting on a network they aren't
        // otherwise a member of has no such row. This is a data-modeling completeness gap
        // (a real cross-network super-admin action needs a NetworkUser row to attribute the
        // action to), not an authorization hole, so it's called out here rather than folded
        // into ExpectedPlainPermissionGatedStatus's general rule.
        var expected = spec == CallerSpec.MainNetworkAdmin ? HttpStatusCode.NotFound : ExpectedPlainPermissionGatedStatus(spec);
        AssertStatus(response, expected, $"ModerationFlags create {kind} as {spec}");
    }

    private static string ModerationFlagsRoute(ContentKind kind, SeededContent content)
    {
        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        var itemRoute = kind == ContentKind.PageBlock
            ? $"networks/{content.NetworkId}/customPages/{content.ParentCustomPageId}/pageBlocks/{content.ContentId}"
            : descriptor.ItemRoute(content.NetworkId, content.ContentId);
        return $"{itemRoute}/moderation-flags";
    }

    // Trimmed caller subset for the moderation matrix (the 17-caller matrix is already fully
    // exercised by Update/Delete above; here we only need enough to prove the DIFFERENT
    // gating rule: plain permission, not ownership).
    private static readonly CallerSpec[] ModerationCallers =
    [
        CallerSpec.Anonymous, CallerSpec.SameNetworkAccessIncomplete, CallerSpec.Author,
        CallerSpec.SameNetworkAdmin, CallerSpec.ExternalNetworkAdmin, CallerSpec.MainNetworkAdmin,
        CallerSpec.SameNetworkNone, CallerSpec.SameNetworkReadOnly, CallerSpec.SameNetworkManageOnly,
        CallerSpec.SameNetworkModerateOnly, CallerSpec.ExternalNetworkModerateOnly,
    ];

    private static IEnumerable<TestCaseData> ModerationCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in ModerationCallers)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    private static HttpStatusCode ExpectedPlainPermissionGatedStatus(CallerSpec spec) => spec switch
    {
        CallerSpec.Anonymous => HttpStatusCode.Unauthorized,
        CallerSpec.SameNetworkAccessIncomplete => HttpStatusCode.Forbidden,
        CallerSpec.SameNetworkAdmin => HttpStatusCode.OK,
        CallerSpec.MainNetworkAdmin => HttpStatusCode.OK,
        CallerSpec.SameNetworkModerateOnly => HttpStatusCode.OK, // the one non-admin case that succeeds
        // Author has no special moderation bypass (PLAN.md §2.5: "moderators flag/hide, they
        // never mutate... deliberately NOT an author/owner action") and only holds Read<Kind>.
        _ => HttpStatusCode.Forbidden,
    };
}
