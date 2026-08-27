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
