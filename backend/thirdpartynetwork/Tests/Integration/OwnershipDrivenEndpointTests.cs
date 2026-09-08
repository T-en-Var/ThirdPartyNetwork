using System.Net;
using System.Text.Json;
using NUnit.Framework;
using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// "Ownership-driven" gating group: Publish, Unpublish, SetOwnership, and Access-grant
/// List/Create/Delete — all gated by IdentityVerifier.IsContentOwnerAsync (Publish/Unpublish/
/// SetOwnership) or that OR'd with the plain ManageContentAccess permission (access-grant
/// endpoints). Only the OWNERSHIP slice of content state matters here — publish state,
/// existing grants, and moderation flags are irrelevant to these checks.
/// </summary>
[TestFixture]
public class OwnershipDrivenEndpointTests : IntegrationTestBase
{
    private static IEnumerable<ContentKind> AllKinds() => ContentKindCatalog.All.Select(d => d.Kind);

    // -----------------------------------------------------------------------------------
    // Publish — full 17-caller matrix, AuthorOwned-with-author-set state (mirrors the
    // RBAC-only group's Update/Delete matrix, since it's the exact same IsContentOwnerAsync
    // check).
    // -----------------------------------------------------------------------------------
    [TestCaseSource(nameof(PublishCases))]
    public async Task Publish_AuthorOwned_MatchesOwnershipExpectation(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId);
        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await caller.Client.PostAsync($"{ItemRoute(kind, content)}/publish", null);

        AssertStatus(response, ExpectedOwnershipStatusCore(spec), $"Publish {kind} (AuthorOwned) as {spec}");
    }

    private static IEnumerable<TestCaseData> PublishCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in CallerCatalog.AllSeventeen)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    // See RbacOnlyEndpointTests.ExpectedOwnershipGatedStatus for why SameNetworkAdmin is
    // Forbidden here (IsContentOwnerAsync's AuthorOwned-with-set-author branch never
    // consults network-admin status) and why AccessIncomplete is 403, not 401.
    //
    // BUG-003 (NOT documented in PLAN.md — found while building this suite): unlike every
    // other permission branch in PermissionHandler (HandleRequirementAsync,
    // HandleGeneralRequirementAsync, HandleContentRequirementAsync, and
    // HandleContentManageRequirementAsync all explicitly call IsSuperAdminOrHasSuper
    // PermissionAsync first), HandleOwnerRequirementAsync's IsContentOwner branch calls
    // IdentityVerifier.IsContentOwnerAsync directly with NO super-admin check at all. Since
    // Publish/Unpublish/SetOwnershipMode are gated by IsContentOwner alone (not OR'd with
    // any plain permission the way the access-grant endpoints are), a Main-network
    // super-admin — who bypasses literally everything else in this app, including Update/
    // Delete on this exact same content — cannot publish, unpublish, or reassign ownership
    // of content in a network they don't specifically administer. See
    // MainNetworkAdmin_CannotPublish_UnlikeEveryOtherOperation_BUG003 below for a direct,
    // load-bearing demonstration of this gap.
    private static HttpStatusCode ExpectedOwnershipStatusCore(CallerSpec spec) => spec switch
    {
        CallerSpec.Anonymous => HttpStatusCode.Unauthorized,
        CallerSpec.SameNetworkAccessIncomplete or CallerSpec.ExternalNetworkAccessIncomplete => HttpStatusCode.Forbidden,
        CallerSpec.Author => HttpStatusCode.OK,
        _ => HttpStatusCode.Forbidden, // includes MainNetworkAdmin — see BUG-003 above
    };

    [TestCaseSource(nameof(AllKindsSource))]
    public async Task MainNetworkAdmin_CannotPublish_UnlikeEveryOtherOperation_BUG003(ContentKind kind)
    {
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, publishedAt: null);
        var mainAdmin = await CallerCatalog.BuildAsync(Ctx, CallerSpec.MainNetworkAdmin, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await mainAdmin.Client.PostAsync($"{ItemRoute(kind, content)}/publish", null);

        AssertStatus(response, HttpStatusCode.Forbidden,
            "BUG-003: a Main-network super-admin — who bypasses Update/Delete/moderation/access-grant " +
            "checks on this exact content — is still Forbidden from Publish, because HandleOwnerRequirementAsync's " +
            "IsContentOwner branch never calls IsSuperAdminOrHasSuperPermissionAsync at all.");
    }

    private static IEnumerable<ContentKind> AllKindsSource() => AllKinds();

    // -----------------------------------------------------------------------------------
    // NetworkOwned flips the author's own bypass off — IdentityVerifier.IsContentOwnerAsync
    // falls straight to IsNetworkAdminAsync (admin-only) once OwnershipMode is NetworkOwned,
    // regardless of who AuthorUserProxyId still points at. This is the single most important
    // ownership-mode assertion in this suite.
    // -----------------------------------------------------------------------------------
    [TestCaseSource(nameof(SomeKinds))]
    public async Task Publish_NetworkOwned_AuthorNoLongerBypasses(ContentKind kind)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(
            Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, ownershipMode: OwnershipMode.NetworkOwned);

        var authorResponse = await author.Client.PostAsync($"{ItemRoute(kind, content)}/publish", null);
        AssertStatus(authorResponse, HttpStatusCode.Forbidden, $"Publish {kind} (NetworkOwned) as its own author — should NO LONGER bypass");

        var admin = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkAdmin, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var adminResponse = await admin.Client.PostAsync($"{ItemRoute(kind, content)}/publish", null);
        AssertStatus(adminResponse, HttpStatusCode.OK, $"Publish {kind} (NetworkOwned) as network admin — should still succeed");
    }

    [TestCaseSource(nameof(SomeKinds))]
    public async Task Publish_AuthorOwned_WithDeletedAuthorPersona_OnlyAdminBypasses(ContentKind kind)
    {
        // AuthorUserProxyId == null (e.g. the author's persona was deleted): the AuthorOwned
        // branch requires authorUserProxyId.HasValue, so this also falls to admin-only.
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: null);

        var manageOnly = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkManageOnly, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var manageResponse = await manageOnly.Client.PostAsync($"{ItemRoute(kind, content)}/publish", null);
        AssertStatus(manageResponse, HttpStatusCode.Forbidden, $"Publish {kind} (AuthorOwned, null author) as Manage-only, non-admin");

        var admin = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkAdmin, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var adminResponse = await admin.Client.PostAsync($"{ItemRoute(kind, content)}/publish", null);
        AssertStatus(adminResponse, HttpStatusCode.OK, $"Publish {kind} (AuthorOwned, null author) as network admin — should still succeed");
    }

    private static IEnumerable<ContentKind> SomeKinds() => [ContentKind.File, ContentKind.Blog, ContentKind.CustomPage];

    // -----------------------------------------------------------------------------------
    // Unpublish / SetOwnership — same gate as Publish; representative caller subset only
    // (the mechanism is already fully proven above and in the RBAC-only Update/Delete matrix).
    // -----------------------------------------------------------------------------------
    private static readonly CallerSpec[] RepresentativeCallers =
    [
        CallerSpec.Anonymous, CallerSpec.Author, CallerSpec.SameNetworkAdmin,
        CallerSpec.ExternalNetworkAdmin, CallerSpec.MainNetworkAdmin, CallerSpec.SameNetworkManageOnly,
    ];

    [TestCaseSource(nameof(UnpublishSetOwnershipCases))]
    public async Task Unpublish_MatchesOwnershipExpectation(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, publishedAt: DateTime.UtcNow);
        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await caller.Client.PostAsync($"{ItemRoute(kind, content)}/unpublish", null);

        AssertStatus(response, ExpectedOwnershipStatusCore(spec), $"Unpublish {kind} as {spec}");
    }

    [TestCaseSource(nameof(UnpublishSetOwnershipCases))]
    public async Task SetOwnershipMode_MatchesOwnershipExpectation(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId);
        var caller = spec == CallerSpec.Author ? author : await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork);

        // The app's default JSON options serialize enums numerically, not as strings —
        // OwnershipMode.NetworkOwned == 1.
        var body = new StringContent(JsonSerializer.Serialize(new { mode = 1 }), System.Text.Encoding.UTF8, "application/json");
        var response = await caller.Client.PostAsync($"{ItemRoute(kind, content)}/ownership", body);

        AssertStatus(response, ExpectedOwnershipStatusCore(spec), $"SetOwnershipMode {kind} as {spec}");
    }

    private static IEnumerable<TestCaseData> UnpublishSetOwnershipCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in RepresentativeCallers)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    // -----------------------------------------------------------------------------------
    // Access-grant List/Create/Delete — gated by ManageContentAccess OR IsContentOwner.
    // Dedicated proof that each side of the OR is independently sufficient, plus a
    // representative caller matrix.
    // -----------------------------------------------------------------------------------
    [Test]
    public async Task AccessGrantList_ManageContentAccessPermission_IsSufficientAlone_EvenForNonOwner()
    {
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.File, ContentNetwork.Id, publishedAt: DateTime.UtcNow);
        var caller = await CallerCatalog.BuildAsync(Ctx, CallerSpec.SameNetworkManageContentAccessOnly, ContentKind.File, ContentNetwork, ExternalNetwork, MainNetwork);

        var response = await caller.Client.GetAsync($"{ItemRoute(ContentKind.File, content)}/access");

        AssertStatus(response, HttpStatusCode.OK, "AccessGrant list: ManageContentAccess permission alone, not the owner");
    }

    [Test]
    public async Task AccessGrantList_IsContentOwner_IsSufficientAlone_WithoutManageContentAccessPermission()
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, ContentKind.File, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, ContentKind.File, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, publishedAt: DateTime.UtcNow);

        // The Author caller only holds ReadFile (see CallerCatalog.BuildAsync's Author case) —
        // explicitly NOT ManageContentAccess.
        var response = await author.Client.GetAsync($"{ItemRoute(ContentKind.File, content)}/access");

        AssertStatus(response, HttpStatusCode.OK, "AccessGrant list: content owner alone, without ManageContentAccess permission");
    }

    [TestCaseSource(nameof(AccessGrantCases))]
    public async Task AccessGrantList_MatchesOwnershipOrPermissionExpectation(ContentKind kind, CallerSpec spec)
    {
        var author = await CallerCatalog.BuildAsync(Ctx, CallerSpec.Author, kind, ContentNetwork, ExternalNetwork, MainNetwork);
        var content = await ContentKindFixtures.CreateAsync(Ctx, kind, ContentNetwork.Id, authorUserProxyId: author.UserProxyId, publishedAt: DateTime.UtcNow);
        var caller = spec switch
        {
            CallerSpec.Author => author,
            _ => await CallerCatalog.BuildAsync(Ctx, spec, kind, ContentNetwork, ExternalNetwork, MainNetwork),
        };

        var response = await caller.Client.GetAsync($"{ItemRoute(kind, content)}/access");

        AssertStatus(response, ExpectedOwnershipOrManageContentAccessStatus(spec), $"AccessGrant list {kind} as {spec}");
    }

    private static readonly CallerSpec[] AccessGrantCallers =
    [
        CallerSpec.Anonymous, CallerSpec.Author, CallerSpec.SameNetworkAdmin, CallerSpec.ExternalNetworkAdmin,
        CallerSpec.MainNetworkAdmin, CallerSpec.SameNetworkManageOnly, CallerSpec.SameNetworkManageContentAccessOnly,
        CallerSpec.ExternalNetworkManageContentAccessOnly,
    ];

    private static IEnumerable<TestCaseData> AccessGrantCases()
    {
        foreach (var kind in AllKinds())
        foreach (var spec in AccessGrantCallers)
            yield return new TestCaseData(kind, spec).SetName($"{{m}}({kind},{spec})");
    }

    private static HttpStatusCode ExpectedOwnershipOrManageContentAccessStatus(CallerSpec spec) => spec switch
    {
        CallerSpec.Anonymous => HttpStatusCode.Unauthorized,
        CallerSpec.Author => HttpStatusCode.OK,
        CallerSpec.SameNetworkAdmin => HttpStatusCode.OK,
        CallerSpec.MainNetworkAdmin => HttpStatusCode.OK,
        CallerSpec.SameNetworkManageContentAccessOnly => HttpStatusCode.OK,
        // Holding the permission in the WRONG (external) network doesn't help — the plain
        // fallthrough RBAC check is scoped to the content's own network.
        CallerSpec.ExternalNetworkManageContentAccessOnly => HttpStatusCode.Forbidden,
        CallerSpec.ExternalNetworkAdmin => HttpStatusCode.Forbidden,
        _ => HttpStatusCode.Forbidden,
    };

    private static string ItemRoute(ContentKind kind, SeededContent content)
    {
        var descriptor = ContentKindCatalog.All.First(d => d.Kind == kind);
        return kind == ContentKind.PageBlock
            ? $"networks/{content.NetworkId}/customPages/{content.ParentCustomPageId}/pageBlocks/{content.ContentId}"
            : descriptor.ItemRoute(content.NetworkId, content.ContentId);
    }
}
