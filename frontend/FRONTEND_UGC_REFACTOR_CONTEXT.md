# Frontend context: User-Generated-Content UI refactor

**Purpose of this document**: full context for whoever (person or agent) refactors the
frontend UGC UI (files, blogs, configurations, custom pages, page blocks) to match the
backend changes shipped on the `alpha` branch. This is a handoff doc, not a spec for new
backend work — the backend described here is already implemented and merged.

**Source material**: this file summarizes `PLAN.md`, `DATABASE_AND_AUTH_OVERVIEW.md`, and
a direct read of the current controller/DTO/enum code as of branch `alpha`
(commit `67e6357`, "Fix content-post/create endpoints auth issues"). Those three docs have
more historical/design rationale if something here is ambiguous; this file only carries
what the frontend needs to act on.

---

## 1. The core change, in one sentence

The old single `AccessLevel` enum (`Private` / `PublicToNetwork` / `Public`) on every
content type was **removed entirely** and replaced with a real, per-resource,
per-recipient sharing system (grants + share links + publish state), plus generic
moderation and a MinIO-backed file store. This touches **all five UGC types**: `File`,
`Blog`, `Configuration`, `CustomPage`, `PageBlock` — the API surface is identical in
shape across all five (see §4).

**Old mental model the current UI (probably) has**: "pick a visibility level from a
dropdown: Private / Network / Public."

**New mental model the UI needs**: visibility is now the *combination* of three
independent, separately-actioned facts:

1. **Is it published at all?** (`PublishedAt` — draft vs. live)
2. **Who has it been explicitly shared with?** (a list of `ContentAccessGrant` rows —
   network, network-role, network-permission, or a specific person)
3. **Is there an active share link out there?** (`ContentViewingLink` — surfaced today
   only as the `ViewingUrl` field, see §6)

There is **no single field anywhere in the API that summarizes "the visibility level"**
as one label (no `Draft`/`Private`/`Unlisted`/`PublicToNetwork` string comes back from any
endpoint). If the UI needs to *display* a single-word tier, it must be derived
client-side from `PublishedAt` + the grants list, or a request should go back to backend
to add a computed field — it does not exist today.

---

## 2. What a "share this content" UI screen now needs to do

Replace any existing "visibility" dropdown/toggle with a screen that manages, per content
item:

- **Publish / unpublish** — one button/toggle, backed by `POST .../{id}/publish` and
  `POST .../{id}/unpublish`. Unpublished (`PublishedAt == null`) = only the author can see
  it, full stop, regardless of any grant below.
- **Ownership mode** — `AuthorOwned` (default) vs `NetworkOwned`, via
  `POST .../{id}/ownership` with `{ mode: 0 | 1 }`. This decides who can *manage* the
  content (edit/delete/reshare) going forward — `NetworkOwned` means network admins have
  that power too, not just the original author. This is a fairly consequential, rarely-
  changed setting — probably belongs in an "advanced"/settings area of the content editor,
  not the primary share flow.
- **A grants list/editor** — add/remove rows of "share with: [Network | a specific role in
  a network | a specific permission-holder in a network | one specific person], with
  capabilities: [View, Edit, Share], optionally expiring on [date]". Backed by
  `GET/POST .../{id}/access` and `DELETE .../{id}/access/{grantId}`. This is the actual
  replacement for the old "Network" and "Public" visibility levels — see the important
  caveat in §7 about your own network needing an explicit grant now too.
- **A share-link / copy-link action**, if the product still wants one — see the gap noted
  in §6/§7 (no dedicated "mint a link" endpoint exists yet; only a passively-computed
  `ViewingUrl` is returned).
- **A moderation view** (separate screen/permission, not part of the author's own share
  UI) — for users with a `Moderate<Type>` permission: list/flag/resolve
  `ContentModerationFlag` rows. This is explicitly flag-and-hide only; a moderator can
  never edit or delete someone else's content through this system, even if they hold
  `Moderate<Type>`.

---

## 3. DTO shape — identical base fields across every content type

`UserContentBaseDto`, inherited/extended by `FileDto`, `BlogDto`, `ConfigurationDto`,
`PageDto` (CustomPage), `PageBlockDto`:

```ts
{
  id: Guid
  networkId: Guid
  authorUserProxyId: Guid | null
  authorUserProxy: UserProxyLightShortProfileDto | null   // persona object, consent-gated fields
  publishedAt: DateTime | null      // null = Draft
  ownershipMode: 0 /* AuthorOwned */ | 1 /* NetworkOwned */
  viewingUrl: string | null         // see §6
  createdOn: DateTime
  updatedAt: DateTime
  // ...plus type-specific fields (Blog: title/slug/summary/body/coverImage; File: bucket/
  // objectKey/format/sizeBytes/width/height/duration/mediaType/name/isSystemProtected; etc.)
}
```

Things that **changed shape** and will break any code still assuming the old model:

- **No `accessLevel` field anywhere.** If the frontend has a type/interface with
  `accessLevel: 'Private' | 'PublicToNetwork' | 'Public'`, delete it — nothing in the API
  returns this anymore.
- **`author` is a persona (`authorUserProxy`), not a network-membership object.** The old
  `NetworkUserLightDto`-shaped author (which used to leak the author's raw entitlement/
  quota JSON) is gone. Field renamed `authorUserProxy` too, not just re-typed.
- **`File` has no `url`/`publicId` field at all anymore.** Replaced by `bucket` +
  `objectKey` (opaque storage coordinates, not directly usable by a client) plus the
  computed `viewingUrl` (see §6). **Any frontend code that reads a file's URL directly off
  its DTO and caches/stores it must stop** — there is no stable URL to cache; it must
  always re-resolve `viewingUrl` per render/request.

---

## 4. New endpoints — identical shape on every content controller

Applies to `File`, `Blog`, `Configuration`, `CustomPage`, `PageBlock` controllers
(`networks/{networkId}/{files|blogs|configurations|customPages/{customPageId}/pageBlocks}`),
in addition to each type's pre-existing CRUD:

```
POST   .../{id}/publish                                  -> <TypeDto>
POST   .../{id}/unpublish                                -> <TypeDto>
POST   .../{id}/ownership          { mode: 0|1 }          -> <TypeDto>

GET    .../{id}/access                                    -> ContentAccessGrantDto[]
POST   .../{id}/access             <- CreateContentAccessGrantDto  -> ContentAccessGrantDto
DELETE .../{id}/access/{grantId}                          -> 204

GET    .../{id}/moderation-flags                          -> ContentModerationFlagDto[]
POST   .../{id}/moderation-flags   <- CreateContentModerationFlagDto -> ContentModerationFlagDto
PUT    .../{id}/moderation-flags/{flagId}/resolve
        <- ResolveContentModerationFlagDto                -> ContentModerationFlagDto
```

**Exception**: `PageBlockController` has no list (`GET` collection) endpoint at all — this
predates this change and wasn't fixed by it. If the page-block editor UI needs to list
blocks independent of fetching the parent `CustomPage`, that endpoint doesn't exist; either
work through the parent page's embedded blocks or flag this back to backend.

**Permission note for File specifically**: `GET .../files/{fileId}` (single-item) is gated
by `ManageFile`, not `ReadFile` — inconsistent with every sibling type, where the
single-item `GET` uses the matching `Read<Type>` permission. A plain viewer who can list
files may get a 403 fetching one file's detail page directly. This is a known,
not-yet-fixed backend inconsistency, not something to work around cleverly in the
frontend — worth flagging back rather than silently special-casing File's error handling.

---

## 5. Access grant & moderation-flag payload shapes

### `CreateContentAccessGrantDto` (the "share with X" form)

```ts
{
  granteeType: 0 /* Network */ | 1 /* NetworkRole */ | 2 /* NetworkPermission */ | 3 /* User */
  granteeNetworkId?: Guid       // set for Network / NetworkRole / NetworkPermission
  granteeRoleId?: Guid         // set for NetworkRole (with granteeNetworkId)
  granteePermissionId?: Guid   // set for NetworkPermission (with granteeNetworkId)
  granteeUserProxyId?: Guid    // set for User — share with one specific person's persona
  capabilities: number         // flags: View=1, Edit=2, Share=4, Comment=8 (reserved), Like=16 (reserved)
  expiresAt?: DateTime | null  // self-expiring share; omit/null = doesn't expire on its own
}
```

The form only needs to show/require the grantee sub-field(s) matching the chosen
`granteeType` — the other grantee-id fields should stay unset. `Comment` and `Like`
capability bits exist in the enum but are **not enforced anywhere in the backend yet** —
don't build UI copy that implies checking those boxes does anything today (fine to leave
them out of the capability picker entirely until backend wires them up).

`ContentAccessGrantDto` (what comes back in the list) additionally carries `id`,
`grantedByNetworkUserId` (audit — who granted it), `revokedAt` (soft-revoke marker — a
grant with a non-null `revokedAt` is inactive; there's no "un-revoke" endpoint, only
delete-and-recreate), and `createdOn`.

### `CreateContentModerationFlagDto` / `ResolveContentModerationFlagDto`

```ts
CreateContentModerationFlagDto { reason: string, hiddenInNetwork: bool = true }
ResolveContentModerationFlagDto {
  status: 0 /* Open */ | 1 /* Resolved */ | 2 /* Dismissed */
  hiddenInNetwork?: bool   // optionally flip the hide state while resolving
}
```

`hiddenInNetwork` only suppresses the content within the *flagging* network's own
views/lists — it never touches `publishedAt` or any grant, so it's invisible to every
other network the content might also be shared with. Good to make explicit in any
moderation UI copy ("hidden in this network only").

---

## 6. File delivery & the `viewingUrl` field

- Cloudinary is gone; files are stored in MinIO. `FileDto` has no stable, directly-usable
  URL field — only storage coordinates (`bucket`, `objectKey`) that aren't meaningful to a
  client, plus the computed `viewingUrl`.
- `viewingUrl` (present on every content type's DTO, not just `File`) points at one of two
  new, deliberately **unauthenticated** endpoints — no bearer token is sent or checked,
  the token embedded in the URL is itself the credential, freshly re-validated against
  current access rules every time it's resolved:
  - `GET /cdn/{token}` — `File` only. 302-redirects to a freshly presigned, short-lived
    MinIO URL. Treat this like any redirecting image/media `src` — don't try to cache or
    persist the URL it redirects *to*.
  - `GET /view/{token}` — `Blog` / `Configuration` / `CustomPage` / `PageBlock`. Returns
    the exact same DTO shape as the authenticated `GET .../{id}` for that type. Useful for
    an "open in a new tab, no login required" preview-share flow.
- **Always re-fetch `viewingUrl` fresh** (re-request the parent DTO) rather than storing it
  — it can expire, and a stale one may 404 even though the content itself is still fully
  accessible.

**Gap to be aware of**: there is currently no endpoint to *mint* a `ContentViewingLink` on
demand for an explicit "get a shareable link" UI action — `viewingUrl` is only produced as
a side effect of fetching the content's own detail DTO. If product wants a standalone
"Copy share link" button that works independent of viewing the item (e.g. from a list
view, without opening the detail page), that's not built yet and should be raised with
backend rather than assumed to exist.

---

## 7. Behavioral traps worth flagging explicitly (things that will surprise QA/users if not called out in the new UI)

- **Your own network is no longer automatically included.** Under the old model, the
  moment content was set to `PublicToNetwork`, every member of the content's *own* home
  network could see it — no explicit action needed. Under the new model that's gone: your
  own network needs an actual grant row too, exactly like any other network. If the new UI
  doesn't make "share with my own network" an obvious, maybe-default first step in the
  share flow, users who expect the old default behavior will find their content
  unexpectedly invisible to their own teammates after publishing. Worth a deliberate
  product decision (default-create that grant on first publish? require an explicit
  step?) rather than silently inheriting old assumptions into new copy.
- **Draft is stronger than any grant.** `publishedAt == null` unconditionally hides content
  from everyone but the author, even if grants already exist on it (e.g. content was
  shared, then unpublished again). The UI should make clear that "unpublish" is a hard
  override, not just "remove from the current visibility tier."
- **Manage permissions no longer mean "any editor can fix any post."** Previously, holding
  e.g. `ManageBlog` let a user edit/delete *any* member's content in the network, including
  private drafts. That's now narrowed to author-only (`AuthorOwned`) or author-or-admin
  (`NetworkOwned`) — see `ownershipMode` in §2. Any existing UI/help copy that says "editors
  can manage all posts" needs updating, and any network relying on that behavior needs to
  be told to set affected content to `NetworkOwned` to keep it.
- **Revoked grants aren't deleted, just deactivated.** `DELETE .../{id}/access/{grantId}`
  soft-revokes (sets `revokedAt`) rather than removing the row — if the grants list ever
  shows history/audit, expect revoked rows to still come back from `GET .../{id}/access`
  unless the backend filters them (verify this before assuming the list is "active grants
  only").
- **File's single-item permission inconsistency** — see §4. Don't build error-handling
  logic around it; flag it.

---

## 8. Reference: relevant enums

```csharp
ContentKind          File=0, Blog=1, Configuration=2, CustomPage=3, PageBlock=4
GranteeType          Network=0, NetworkRole=1, NetworkPermission=2, User=3
ContentCapabilities  [Flags] None=0, View=1, Edit=2, Share=4, Comment=8, Like=16
ModerationFlagStatus Open=0, Resolved=1, Dismissed=2
OwnershipMode        AuthorOwned=0, NetworkOwned=1
ContentViewingLinkMode  OneTimeView=0, TimeLimited=1, Permanent=2   // not exposed in any DTO today, backend-internal
```

---

## 9. Where to look in the backend if something here is unclear

- `PLAN.md` (repo root) — the full design doc, including the ERD, the exact
  `Tier()` derivation logic (why nothing computes a visibility label yet), and a "Wave 1
  Implementation Review" section listing every known divergence between design and shipped
  code (includes the File permission inconsistency and PageBlock list-endpoint gap noted
  above, plus others that don't affect the frontend).
- `DATABASE_AND_AUTH_OVERVIEW.md` (repo root) — the pre-existing system overview, now
  annotated with ⚠️ notes pointing at what this change superseded.
- Controllers: `thirdpartynetwork/Presentation/Controllers/UserContent/*.cs` (one per
  content type, identical shape) and `CdnController.cs` / `ViewController.cs` (the two
  token-resolution endpoints from §6).
- DTOs: `thirdpartynetwork/Application/Dtos/UserContent/**`,
  `Application/Dtos/ContentAccessGrant/**`, `Application/Dtos/ContentModerationFlag/**`.
