Real duplication, but there's a landmine buried in the authorization layer that I'd want you to see before picking the `{contentType}`-route version specifically.

**The duplication is as bad as it looks.** Diffing `ConfigurationController` against `BlogController`, the publish/unpublish/ownership/access-grant block (~65 lines) is structurally identical except for the type name and the DTO. Even the permission constants match: `IsContentOwner` and `ManageContentAccess` are literally the same string across all five content kinds already — only the moderation-flags endpoints use a kind-specific constant (`ModerateBlog` vs `ModerateConfiguration` vs...).

**Why I wouldn't reach for the `{contentType}`-route version first.** `PermissionHandler.ResolveContentRouteMatch` (`PermissionHandler.cs:216-229`) is what backs every `IsContentOwner` check and the owner-fallback half of `ManageContentAccess` — and it determines the `ContentKind` by *sniffing which route-parameter name is present*:

```csharp
if (routeValues.ContainsKey("fileId")) return (ContentKind.File, "fileId");
if (routeValues.ContainsKey("blogId")) return (ContentKind.Blog, "blogId");
if (routeValues.ContainsKey("configurationId")) return (ContentKind.Configuration, "configurationId");
...
```

If the base controller's route uses one shared parameter name (`{contentId}` or similar) instead of `{configurationId}`/`{blogId}`/etc., none of these match, `route` comes back `null`, and every owner gets silently denied — on publish, unpublish, ownership, and access grants, for all five content kinds simultaneously, the moment this ships. Today a routing mistake is scoped to one controller; centralizing the route makes this one lookup a single point of failure for content-owner auth across the whole app. Not a reason to abandon the idea, but it means "collapse the route" isn't a controller-layer-only change — it requires rewriting a security-critical piece of `PermissionHandler` (keying off the `{contentType}` route *value* via a new string→`ContentKind` map, instead of the parameter *name*) as a prerequisite, and that rewrite deserves its own careful review, not a side effect of a DRY pass.

There's also a structural wrinkle independent of auth: `PageBlockController` nests under CustomPage (`networks/{networkId}/customPages/{customPageId}/pageBlocks/{id}/publish`) — it doesn't fit the flat `{networkId}/{contentType}/{id}` shape the other four do. It'd need to stay its own thing either way.

**What I'd actually do:** get the DRY win without touching `PermissionHandler` at all — keep each controller's own `[Route]` prefix and its own route-parameter names exactly as they are (so `ResolveContentRouteMatch` keeps working unmodified), and move the *bodies* into a shared generic base class:

```csharp
public abstract class UserContentBaseController<TModel, TDto>(
    IUserContentBaseService<TModel> service,
    IContentAccessGrantService accessGrantService,
    IMapper mapper) : ControllerBase
    where TModel : UserContentBaseModel
    where TDto : UserContentBaseDto
{
    protected abstract ContentKind Kind { get; }
    protected abstract Task<TModel?> GetByIdAsync(Guid networkId, Guid id); // or add to IUserContentBaseService<T>

    [RequirePermission(Constants.Permissions.IsContentOwner)]
    public async Task<IActionResult> Publish(Guid networkId, Guid id)
    {
        var entity = await GetByIdAsync(networkId, id);
        if (entity == null) return NotFound();
        var published = await service.PublishAsync(entity);
        return Ok(mapper.Map<TDto>(published, opts => opts.Items["networkId"] = networkId));
    }
    // Unpublish, SetOwnershipMode, Get/Create/Revoke access-grant — same shape
}

[Route("networks/{networkId:guid}/configurations")]
public class ConfigurationController(IConfigurationService service, ...)
    : UserContentBaseController<Configuration, ConfigurationDto>(service, ...)
{
    protected override ContentKind Kind => ContentKind.Configuration;
    protected override Task<Configuration?> GetByIdAsync(Guid networkId, Guid id) => service.GetByIdAsync(networkId, id);
    // plus its own CRUD + moderation-flag actions, which stay here (kind-specific permission constants)
}
```

`PublishAsync`/`UnpublishAsync`/`SetOwnershipModeAsync` are already on the shared `IUserContentBaseService<T>` interface — that part's ready-made. `GetByIdAsync(networkId, id)` isn't on that interface yet but every concrete service already implements the identical signature, so promoting it there is a one-line, mechanical, zero-risk addition. Moderation-flags stay per-controller (only 3 of ~12 actions, and they're the ones with genuinely different permission constants per kind) — small enough that duplicating just those isn't worth forcing into the generic shape.

This gets you ~80% of the DRY benefit today with no changes to `PermissionHandler` or routing, and it's a natural stepping stone: if you later do want the single merged `{contentType}` route, `ResolveContentRouteMatch`'s rework becomes a separate, focused, reviewable change instead of being entangled with a big controller refactor.

Want me to implement the base-controller version?

---

## Follow-up: can't we just tell PermissionHandler which route value to grab?

Yes — and it's a smaller change than the rewrite I implied. You don't even need to touch the existing branches of `ResolveContentRouteMatch`; you can add a new one that reads the explicit route value instead of guessing.

**The core move:** add a `ContentKindMapper.FromRouteSegment(string)` (mirrors the existing `ReadPermissionFor(ContentKind)`), and make `ResolveContentRouteMatch` check for the new convention *first*, falling back to the old name-sniffing for every route that hasn't moved:

```csharp
// ContentKindMapper.cs — reverse of the plural segment each controller's [Route] already uses
public static ContentKind? FromRouteSegment(string segment) => segment switch
{
    "configurations" => ContentKind.Configuration,
    "blogs" => ContentKind.Blog,
    "files" => ContentKind.File,
    "custom-pages" => ContentKind.CustomPage,
    _ => null
};

// PermissionHandler.cs
private static (ContentKind Kind, string CompareString)? ResolveContentRouteMatch(HttpContext httpContext)
{
    var routeValues = httpContext.Request.RouteValues;

    // New generic-route convention: {networkId}/{contentType}/{id}/...
    if (routeValues.TryGetValue("contentType", out var typeValue) &&
        ContentKindMapper.FromRouteSegment(typeValue?.ToString() ?? "") is { } kind)
    {
        return (kind, "id");
    }

    // Existing per-kind controllers — unchanged
    if (routeValues.ContainsKey("fileId")) return (ContentKind.File, "fileId");
    if (routeValues.ContainsKey("blogId")) return (ContentKind.Blog, "blogId");
    if (routeValues.ContainsKey("configurationId")) return (ContentKind.Configuration, "configurationId");
    if (routeValues.ContainsKey("pageBlockId")) return (ContentKind.PageBlock, "pageBlockId");
    if (routeValues.ContainsKey("customPageId")) return (ContentKind.CustomPage, "customPageId");

    return null;
}
```

Because it's additive, nothing that works today can regress from this alone — every existing literal route still matches exactly as before; only the new `{contentType}/{id}` shape goes through the new branch. That de-risks it a lot more than the blanket "rewrite" framing I gave last message.

Two things worth doing alongside it, not strictly required but cheap and closes real gaps:

1. **Constrain `{contentType}` at the routing layer, not just in code:** `{contentType:regex(^(configurations|blogs|files|custom-pages)$)}` on the route template. That way a typo'd or made-up segment 404s before it ever reaches `PermissionHandler`, instead of relying on `FromRouteSegment` returning `null` and everything downstream correctly treating that as "deny." Belt-and-suspenders, and it keeps `PermissionHandler` from being the only thing standing between garbage input and a confusing 403.
2. **`HandleOwnerRequirementAsync`/`HandleContentManageRequirementAsync` currently don't take `RouteKey` at all** — they call `ResolveContentRouteMatch` with zero context from the attribute, purely inferring both kind and id-param-name from what's present in `RouteValues`. That's exactly why this trick works: the new branch is self-contained (checks for `contentType` + assumes `id`), no attribute plumbing needed. You do **not** need to add a `ContentTypeRouteKey` property to `RequirePermissionAttribute` for this to work, since `{contentType}` and `{id}` are just fixed, hardcoded names in the one new route template — only bother with a configurable attribute property if you anticipate more than one naming convention living side by side.

One more thing this unlocks, free: `HandleContentRequirementAsync` (the path for `Read*` permissions) currently hardcodes a switch from the fixed `permission` string to a specific `accessLevelVerifier.CheckXAccessAsync` call. If you ever merge the GET-by-id endpoints too, that switch collapses the same way — once you have `kind` resolved from `{contentType}`, you don't need five separate `CheckFileAccessAsync`/`CheckBlogAccessAsync`/etc. methods at all; `ContentKindMapper.ReadPermissionFor(kind)` plus the per-kind service's `GetByIdAsync` gets you straight to `CheckAccessForCallerAsync`. Not needed for today's ask, but it's the same shape of fix, so worth knowing it's not a dead end.

Net: this closes the blocker I raised. Want me to fold this into a concrete plan — `PermissionHandler` change + route constraint + the shared base controller from before — or keep it as a documented follow-up for now?
