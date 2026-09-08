using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// The 17 effective caller configurations from the combination-matrix breakdown: 5 fixed
/// identities + 2 "low-level" slots (SameNetwork / ExternalNetwork) x 6 held-permission
/// variants. `SameNetwork*`/`ExternalNetwork*` mean relative to whatever network the content
/// under test lives in for that particular test — the test passes `contentNetwork` and
/// `externalNetwork` (a distinct network) explicitly.
/// </summary>
public enum CallerSpec
{
    Anonymous,
    Author,
    SameNetworkAdmin,
    ExternalNetworkAdmin,
    MainNetworkAdmin,

    SameNetworkNone,
    SameNetworkReadOnly,
    SameNetworkManageOnly,
    SameNetworkModerateOnly,
    SameNetworkManageContentAccessOnly,
    SameNetworkAccessIncomplete,

    ExternalNetworkNone,
    ExternalNetworkReadOnly,
    ExternalNetworkManageOnly,
    ExternalNetworkModerateOnly,
    ExternalNetworkManageContentAccessOnly,
    ExternalNetworkAccessIncomplete,
}

public static class CallerCatalog

{
    public static readonly CallerSpec[] AllSeventeen =
    [
        CallerSpec.Anonymous, CallerSpec.Author, CallerSpec.SameNetworkAdmin,
        CallerSpec.ExternalNetworkAdmin, CallerSpec.MainNetworkAdmin,
        CallerSpec.SameNetworkNone, CallerSpec.SameNetworkReadOnly, CallerSpec.SameNetworkManageOnly,
        CallerSpec.SameNetworkModerateOnly, CallerSpec.SameNetworkManageContentAccessOnly, CallerSpec.SameNetworkAccessIncomplete,
        CallerSpec.ExternalNetworkNone, CallerSpec.ExternalNetworkReadOnly, CallerSpec.ExternalNetworkManageOnly,
        CallerSpec.ExternalNetworkModerateOnly, CallerSpec.ExternalNetworkManageContentAccessOnly, CallerSpec.ExternalNetworkAccessIncomplete,
    ];

    /// <summary>The user's 4 originally-required callers, by their CallerSpec equivalent:
    /// same-network low-level (default Read-only role) / external-network low-level (same) /
    /// external-network MainNetwork admin / same-network admin.</summary>
    public static readonly CallerSpec[] RequiredFour =
    [
        CallerSpec.SameNetworkReadOnly, CallerSpec.ExternalNetworkReadOnly,
        CallerSpec.MainNetworkAdmin, CallerSpec.SameNetworkAdmin,
    ];

    public static async Task<TestCaller> BuildAsync(
        DataContext ctx, CallerSpec spec, ContentKind kind,
        Network contentNetwork, Network externalNetwork, Network mainNetwork)
    {
        switch (spec)
        {
            case CallerSpec.Anonymous:
                return CallerFixtures.BuildAnonymous();

            case CallerSpec.Author:
                // ReadNetwork alongside Read<Kind>: NetworkService.GetByIdAsync's own
                // visibility filter (FilterNetworksAsync) requires ReadNetwork independently
                // of anything under test here — the real seeded default "User" role always
                // includes it too, this just mirrors that baseline.
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "Author", [PermissionNames.Read(kind), Constants.Permissions.ReadNetwork]);

            case CallerSpec.SameNetworkAdmin:
                return await BuildAdminAsync(ctx, contentNetwork, "SameNetworkAdmin");

            case CallerSpec.ExternalNetworkAdmin:
                return await BuildAdminAsync(ctx, externalNetwork, "ExternalNetworkAdmin");

            case CallerSpec.MainNetworkAdmin:
                return await BuildAdminAsync(ctx, mainNetwork, "MainNetworkAdmin(external)");

            case CallerSpec.SameNetworkNone:
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "SameNetworkNone", []);
            case CallerSpec.SameNetworkReadOnly:
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "SameNetworkReadOnly", [PermissionNames.Read(kind)]);
            case CallerSpec.SameNetworkManageOnly:
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "SameNetworkManageOnly", [PermissionNames.Manage(kind)]);
            case CallerSpec.SameNetworkModerateOnly:
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "SameNetworkModerateOnly", [PermissionNames.Moderate(kind)]);
            case CallerSpec.SameNetworkManageContentAccessOnly:
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "SameNetworkManageContentAccessOnly", [Constants.Permissions.ManageContentAccess]);
            case CallerSpec.SameNetworkAccessIncomplete:
                return await BuildLowLevelAsync(ctx, contentNetwork, kind, "SameNetworkAccessIncomplete", [Constants.Permissions.Administrator], accessIncomplete: true);

            case CallerSpec.ExternalNetworkNone:
                return await BuildLowLevelAsync(ctx, externalNetwork, kind, "ExternalNetworkNone", []);
            case CallerSpec.ExternalNetworkReadOnly:
                return await BuildLowLevelAsync(ctx, externalNetwork, kind, "ExternalNetworkReadOnly", [PermissionNames.Read(kind)]);
            case CallerSpec.ExternalNetworkManageOnly:
                return await BuildLowLevelAsync(ctx, externalNetwork, kind, "ExternalNetworkManageOnly", [PermissionNames.Manage(kind)]);
            case CallerSpec.ExternalNetworkModerateOnly:
                return await BuildLowLevelAsync(ctx, externalNetwork, kind, "ExternalNetworkModerateOnly", [PermissionNames.Moderate(kind)]);
            case CallerSpec.ExternalNetworkManageContentAccessOnly:
                return await BuildLowLevelAsync(ctx, externalNetwork, kind, "ExternalNetworkManageContentAccessOnly", [Constants.Permissions.ManageContentAccess]);
            case CallerSpec.ExternalNetworkAccessIncomplete:
                return await BuildLowLevelAsync(ctx, externalNetwork, kind, "ExternalNetworkAccessIncomplete", [Constants.Permissions.Administrator], accessIncomplete: true);

            default:
                throw new NotSupportedException(spec.ToString());
        }
    }

    private static async Task<TestCaller> BuildAdminAsync(DataContext ctx, Network network, string label)
    {
        var role = await CallerFixtures.CreateRoleAsync(ctx, network, $"{label}-{Guid.NewGuid():N}", Constants.Permissions.Administrator);
        var proxy = await CallerFixtures.CreateUserAsync(ctx, label);
        var networkUser = await CallerFixtures.JoinNetworkAsync(ctx, network, proxy, [role]);
        return await CallerFixtures.BuildAuthenticatedAsync(label, networkUser);
    }

    private static async Task<TestCaller> BuildLowLevelAsync(
        DataContext ctx, Network network, ContentKind kind, string label, string[] permissions, bool accessIncomplete = false)
    {
        var role = await CallerFixtures.CreateRoleAsync(ctx, network, $"{label}-{kind}-{Guid.NewGuid():N}", permissions);
        var proxy = await CallerFixtures.CreateUserAsync(ctx, label);
        var networkUser = await CallerFixtures.JoinNetworkAsync(ctx, network, proxy, [role], accessIncomplete);
        return await CallerFixtures.BuildAuthenticatedAsync(label, networkUser);
    }
}
