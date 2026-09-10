# PLAN.md — UGC Frontend Refactor: Backend Alignment + Shared Base Architecture

**Status**: draft, not started. Review this before any code changes land.

**Inputs**: `FRONTEND_UGC_REFACTOR_CONTEXT.md` (backend contract, source of truth for
*what* changed) + a full read of the current frontend UGC code (source of truth for
*what exists today*, summarized below). This document is the *how* — architecture and
sequencing — for both the backend-alignment work and the "every UGC page inherits from
one base" restructuring requested alongside it.

---

## 1. Goal

Two changes, done together because they touch the same files:

1. **Backend alignment**: the old `accessLevel: 0|1|2` model is gone from the API.
   Replace it everywhere in the frontend with `publishedAt` + `ownershipMode` +
   `ContentAccessGrant`s + `viewingUrl`, per `FRONTEND_UGC_REFACTOR_CONTEXT.md`.
2. **De-duplication via a shared base**: `File`, `Blog`, `Configuration`, `CustomPage`,
   `PageBlock` management UIs are currently five near-identical copies of the same list
   page, add-modal, and edit-tab, plus five (really ten — network- and account-scoped)
   near-identical CRUD composables. Collapse that into one base implementation that each
   content type *configures* rather than *reimplements*.

Doing #2 first (or as the same pass as #1) matters because the accessLevel→grants
change has to be made in five places today; with a shared base it's made once.


**Vue note on "inheritance"**: Vue 3 SFCs have no class-inheritance mechanism. The
equivalent, idiomatic patterns are:

--- userinput: i mean that the UGC vue components must reuse the same base components but 
add their own item display cards or something  

- **Composition**: a base composable that takes a small per-type config object and
  returns the full set of behavior (fetch/create/update/delete/publish/grants/...).
- **Base components driven by config + slots**: a base component that owns all shared
  markup/logic and exposes named slots + a typed config prop for the parts that
  genuinely differ per type (which fields to show, which columns to list). --- user: yes, this

This plan uses both. "Every management page inherits from the base UGC management page"
is implemented as: *every management page is the base component, instantiated with that
type's config and slot content* — not as five separate component files that copy the
base's template.

---

## 2. Current-state duplication inventory

Everything below currently hardcodes `accessLevel: number` and repeats structure
five (or ten) times.

| Concern | Files today | Duplication |
|---|---|---|
| Base DTO shape | `types/userContent/userContentBase.ts` | 1 file, but `accessLevel` field must go |
| Per-type DTO | `types/userContent/{blog,files,configuration,customPage,pageBlock}.ts` | 5 files, each `extends UserContentBase` |
| Network-scoped CRUD composable | `composables/network/use{Blogs,Files,Configurations,CustomPages}.ts` | 4 files (PageBlock CRUD lives inside `useCustomPages`), each hand-rolling `fetchX`/`fetchXs`/`createX`/`updateX`/`deleteX` over `useCachedApi`/`useMutation` |
| Account-scoped CRUD composable | `composables/account/use{Blogs,Files,Configurations,CustomPages}.ts` | 4 more files, same shape, different base path (`/users/{userId}/proxies/{userProxyId}/...` vs `/networks/{networkId}/...`) |
| List/manage tab | `components/tabs/usercontent/{Blogs,Files,Configurations,CustomPages}Tab.vue` | 4 files, each: wrap `UserContentViewer` + one `Add*Modal`, wire `add-new`/`edit`/`remove` |
| Create modal | `components/modals/usercontent/Add{Blog,File,Configuration,CustomPage,PageBlock}Modal.vue` | 5 files, each wraps `UserContentForm` (which renders `AccessLevelPicker`) + 1-3 type-specific fields |
| Edit tab | `components/tabs/usercontent/Edit{Blog,Configuration,CustomPage,PageBlock}Tab.vue` (File has no dedicated edit tab — `EditFileModal.vue` instead) | 4-5 files, each: `useEditableEntity` (already shared ✅) + hand-rolled form incl. `AccessLevelPicker` + type-specific fields |
| Action dispatch | `composables/useNetworkManageActions.ts` (`handle.blog`, `handle.configuration`, `handle.customPages`, `handle.files`, plus `handle.pageBlocks`) | One file, but internally repeats the same `fetch`/`fetchAll`/`edit`/`create`/`update`/`delete` shape 4-5 times via `genericFetch`/`genericFetchAll`/`genericMutation` (those three helpers are already the right idea — just not applied through one declarative table) |
| Visibility UI | `components/fields/AccessLevelPicker.vue`, `lib/accessLevels.ts`, the "Visibility" column in `UserContentViewer.vue` | Reads/writes `accessLevel`; must be replaced, not just retyped — the concept itself no longer exists |
| List page shared logic | `components/UserContentViewer.vue` | **Already** the right pattern (generic `<T extends UserContentBase>`, config via props) — extend this, don't replace it |
| Routes | `router/index.ts` | `manage-account-*` and `manage-network-*` trees repeat the same 5 route shapes (list, edit) per type |

Net: ~28 files currently encode "the same UGC CRUD/list/edit page" five times with small
variations. Target: 1 base composable factory + 2 base UI components + ~5 small
per-type config objects + thin per-type "unique fields" slot content.

---

## 3. Backend contract changes to adopt (summary — see context doc for full detail)

- Delete `accessLevel` from every UGC type. No replacement single field exists.
- Add to the shared base DTO: `publishedAt` (draft vs. live), `ownershipMode` (0
  `AuthorOwned` / 1 `NetworkOwned`), `viewingUrl` (unauthenticated, re-resolve-per-render,
  never cache/store), `authorUserProxy` (renamed+retyped from `author`, persona not
  membership object).
- `File` loses `url`/`isPublic`/`publicId`; gains `bucket`/`objectKey` (opaque) — display
  must go through `viewingUrl` (`GET /cdn/{token}`, 302 redirect) instead.
- New per-type endpoints, identical shape across all 5 controllers:
  `POST .../{id}/publish`, `POST .../{id}/unpublish`, `POST .../{id}/ownership`,
  `GET/POST .../{id}/access`, `DELETE .../{id}/access/{grantId}`,
  `GET/POST .../{id}/moderation-flags`, `PUT .../{id}/moderation-flags/{flagId}/resolve`.
- New payload shapes: `CreateContentAccessGrantDto`, `ContentAccessGrantDto`,
  `CreateContentModerationFlagDto`, `ResolveContentModerationFlagDto` (see context doc
  §5 for exact fields).
- Behavioral traps the UI must encode, not just the data: own network needs an explicit
  grant now (§7); unpublish overrides all grants; ownership mode changes who can manage,
  not who can view; revoked grants are soft-deleted and may still come back from `GET
  .../access`.

This plan does not restate the full contract — treat `FRONTEND_UGC_REFACTOR_CONTEXT.md`
as authoritative for exact field/endpoint names during implementation.

---

## 4. Target architecture

### 4.1 Layer 1 — Per-type config (the "what makes each type different")

One small, declarative object per content type. This is the only place a new content
type or a new per-type field touches.

```ts
// src/lib/userContent/types.ts
export interface UserContentTypeConfig<
  TEntity extends UserContentBase,
  TCreate extends UserContentCreateBase,
> {
  kind: 'File' | 'Blog' | 'Configuration' | 'CustomPage' | 'PageBlock';
  label: string;                     // "Blogs", "Files", ...
  networkBasePath: (networkId: string) => string;      // /networks/{id}/blogs
  accountBasePath: (userId: string, userProxyId: string) => string; // /users/.../proxies/.../blogs
  listColumns: ExtraColumn[];        // fed straight into the list component
  createDefaults: () => TCreate;
}
```

Five instances of this replace the "which fields to show" half of what's currently
copy-pasted per Tab/Modal file. (Type-specific *field editing UI* — the blog body
editor, the JSON editor for Configuration/PageBlock data — stays as slot content passed
into the base edit component; it is genuinely unique per type and shouldn't be forced
into a config object.)

### 4.2 Layer 2 — One CRUD composable factory (replaces 8 of the 10 composable files)

```ts
// src/composables/userContent/useUserContentApi.ts
export function useUserContentApi<TEntity extends UserContentBase, TCreate>(
  config: UserContentTypeConfig<TEntity, TCreate>,
) {
  // network-scoped: fetchAll/fetch/create/update/delete built once, generically,
  // over api.get/post/put/delete + config.networkBasePath, using the existing
  // useCachedApi/useMutation + networkKey() cache-key helpers unchanged.
  const network = { fetchAll, fetch, create, update, delete: remove };

  // account-scoped: same shape, config.accountBasePath, userProxyKey()
  const account = { fetchAll, fetch, create, delete: remove }; // account side has no update today — verify per type when implemented

  return { network, account };
}
```

`PageBlock`'s nested create/update/delete (scoped under a `CustomPage`, no list
endpoint per context doc §4) doesn't fit this shape 1:1 — it gets its own thin composable
that reuses the same `useCachedApi`/`useMutation` primitives but isn't forced through
`useUserContentApi`. Documented as a deliberate exception, not an oversight.

### 4.3 Layer 3 — Shared sharing/moderation composables (new, apply to all 5 types identically)

```ts
// src/composables/userContent/useContentAccess.ts
export function useContentAccess(basePath: (id: string) => string) {
  return { fetchGrants, createGrant, revokeGrant }; // GET/POST .../access, DELETE .../access/{grantId}
}

// src/composables/userContent/useContentModeration.ts
export function useContentModeration(basePath: (id: string) => string) {
  return { fetchFlags, createFlag, resolveFlag };
}

// src/composables/userContent/useContentPublishing.ts
export function useContentPublishing(basePath: (id: string) => string) {
  return { publish, unpublish, setOwnershipMode };
}
```

These take just a base-path function (`(id) => \`/networks/${networkId}/blogs/${id}\``)
so they work identically for all 5 types and both network/account scope — no per-type
config needed at all, since the sub-resource shape is byte-for-byte identical across
`File`/`Blog`/`Configuration`/`CustomPage`/`PageBlock` per context doc §4.

### 4.4 Layer 4 — Base UI components (the actual "inheritance")

**`BaseUserContentListPage.vue`** — evolves the existing `UserContentViewer.vue` (keep
its filter/search/table logic, it's already generic) into the full list *page*:
props = `{ config, entries, fetchEntries }`; emits `add-new` / `edit` / `remove` /
`manage-access` (new). Replaces the "Visibility" column (currently
`getAccessLevel(entry.accessLevel).label`) with a derived status badge:
`Draft` (no `publishedAt`) / `Published — N shared` (count of active, non-revoked
grants) — computed client-side per context doc §1 ("no single field summarizes
visibility"). Each `<X>Tab.vue` becomes:

```vue
<template>
  <BaseUserContentListPage :config="blogConfig" ... >
    <template #create-modal-fields="{ form }"> <!-- title input etc --> </template>
  </BaseUserContentListPage>
</template>
```

**`BaseUserContentEditPage.vue`** (new) — owns: `useEditableEntity` load/error state
(already shared, keep as-is), a `PublishToggle`, an `OwnershipModePicker` (tucked in an
"Advanced" disclosure per context doc §2 guidance — it's a rare, consequential setting),
an `AccessGrantsEditor` (list + add-row + revoke), and — gated on the viewer holding
`Moderate<Type>` — a `ModerationFlagsPanel`. Slot: `#fields` for the type-unique editor
(blog title/body, configuration key/value JSON, custom page name/slug, page block
text/position/data — page block also keeps its own tree/sibling navigation UI, which is
irreducibly type-specific and stays in a slot, not the base).

**`AccessGrantsEditor.vue`** (new, shared) — the "share with X" form from context doc §2:
grantee-type selector that conditionally shows the matching sub-field
(network / role / permission / user), capability checkboxes (View/Edit/Share only —
Comment/Like excluded per §5, not enforced yet), optional expiry date, existing-grants
list with revoke. Used by every type's edit page, and reusable later at Layer 5.

**`ModerationFlagsPanel.vue`** (new, shared) — flag list + resolve action, copy makes
`hiddenInNetwork` scope explicit per §5. Not part of the author's own edit flow;
rendered only when the viewer holds the type's `Moderate<Type>` permission (check via
existing `usePermissions`).

### 4.5 Layer 5 — Per-type entry points (thin)

Each of the 5 types ends up as:
- 1 config object (Layer 1)
- 1 usage of `BaseUserContentListPage` for its Tab
- 1 usage of `BaseUserContentEditPage` for its Edit tab, with a `#fields` slot for the
  handful of genuinely unique inputs

`PageBlock` has no top-level "list" page (no list endpoint, per context doc §4) — its
Tab-equivalent stays the existing embedded-in-`CustomPage` tree/nav UI in
`EditPageBlockTab.vue`, restructured to use `BaseUserContentEditPage` for the
publish/access/moderation portion but keeping its bespoke tree navigation as `#fields`
(or an additional named slot, e.g. `#navigation`) content.

---

## 5. File-level change map

**New files**
- `src/lib/userContent/types.ts` — `UserContentTypeConfig` + shared `ExtraColumn` type (hoisted out of `UserContentViewer.vue`)
- `src/lib/userContent/{blog,file,configuration,customPage,pageBlock}.config.ts` — one config object each
- `src/composables/userContent/useUserContentApi.ts`
- `src/composables/userContent/useContentAccess.ts`
- `src/composables/userContent/useContentModeration.ts`
- `src/composables/userContent/useContentPublishing.ts`
- `src/composables/userContent/useVisibilitySummary.ts` — client-derived Draft/Published+grant-count badge (context doc §1)
- `src/components/usercontent/BaseUserContentListPage.vue`
- `src/components/usercontent/BaseUserContentEditPage.vue`
- `src/components/usercontent/AccessGrantsEditor.vue`
- `src/components/usercontent/ModerationFlagsPanel.vue`
- `src/components/fields/PublishToggle.vue`
- `src/components/fields/OwnershipModePicker.vue`

**Modified**
- `src/types/userContent/userContentBase.ts` — remove `accessLevel`; add `publishedAt`, `ownershipMode`, `viewingUrl`, `authorUserProxy` (drop `author`/`authorId` as separate fields)
- `src/types/userContent/{blog,files,configuration,customPage,pageBlock}.ts` — drop `accessLevel` from Create* types; `files.ts` also drops `url`/`isPublic`/`publicId`, adds `bucket`/`objectKey`
- `src/types/index.ts` — add exports for new `ContentAccessGrant`/`ContentModerationFlag` types
- `src/components/UserContentViewer.vue` — either becomes `BaseUserContentListPage.vue` directly, or is kept as the internal table/filter primitive that the new base wraps (implementation detail to settle when coding; either way the Visibility column and `entry.author.userProxy` access need updating for the DTO rename)
- `src/composables/network/use{Blogs,Files,Configurations}.ts`, `src/composables/account/use{Blogs,Files,Configurations}.ts` — become thin `useUserContentApi(config).network` / `.account` re-exports (kept as separate files so call sites/imports don't all change at once — see Phase 2)
- `src/composables/network/useCustomPages.ts` — CustomPage part moves to the factory; PageBlock CRUD part stays bespoke but reuses `useContentPublishing`/`useContentAccess` for its new endpoints
- `src/composables/useNetworkManageActions.ts` — `handle.blog`/`handle.configuration`/`handle.customPages`/`handle.files` sub-objects become generated from the same per-type config table instead of hand-written per type; add `handle.<type>.publish/unpublish/setOwnership/grants.*` wiring
- `src/components/tabs/usercontent/{Blogs,Files,Configurations,CustomPages}Tab.vue` — shrink to a config + `BaseUserContentListPage` usage
- `src/components/tabs/usercontent/Edit{Blog,Configuration,CustomPage,PageBlock}Tab.vue` — shrink to `BaseUserContentEditPage` usage + `#fields` slot
- `src/components/modals/usercontent/Add{Blog,Configuration,CustomPage,PageBlock}Modal.vue`, `EditFileModal.vue` — drop `AccessLevelPicker`; `UserContentForm.vue` drops `hideAccessPicker`/`accessLevel` entirely (creation no longer sets visibility — that's a post-create action via the edit page, matching the backend's separate publish/grant endpoints)
- `src/components/cdn/CloudinaryFile.vue` (or its replacement) — must resolve `viewingUrl` instead of reading `file.url` directly; likely renamed since "Cloudinary" is gone per context doc §6
- Every place reading `file.url` directly and caching it (flagged explicitly in context doc §3/§6) — audit via search, e.g. `EditBlogTab.vue`'s `handleImageInserted` inserts `file.url` into the rich text editor today

**Deleted**
- `src/components/fields/AccessLevelPicker.vue`
- `src/lib/accessLevels.ts`

**Router** (`src/router/index.ts`) — no path changes required for existing list/edit
routes; consider whether "share/access" and "moderation" get their own child route (e.g.
`.../blogs/:blogId/access`, or a network-level `.../moderation` queue) vs. living inline
on the edit page — see Open Decisions §13.

---

## 6. How the "behavioral traps" from the context doc get addressed

| Trap (context doc §7) | Where it's handled in this plan |
|---|---|
| Own network needs an explicit grant now | `AccessGrantsEditor` / publish flow: on first `publish`, prompt or default-suggest a grant for the content's own `networkId` (needs a product decision — see §13) |
| Draft overrides all grants | `PublishToggle` copy says this explicitly; `AccessGrantsEditor` stays visible/editable even while unpublished but shows a "hidden while unpublished" banner |
| Ownership mode narrows who can manage | `OwnershipModePicker` copy explains the AuthorOwned/NetworkOwned distinction; any existing "editors manage all posts" help text gets removed/rewritten (grep for it during implementation) |
| Revoked grants aren't deleted | `AccessGrantsEditor`'s grant list filters `revokedAt == null` client-side before rendering "active grants," regardless of what `GET .../access` returns |
| File's `GET /files/{id}` needs `ManageFile` not `ReadFile` | Not worked around; `BaseUserContentEditPage`'s error state for File shows the real 403 rather than a bespoke fallback. Flag to backend as noted in context doc — tracked, not silently patched |
| No PageBlock list endpoint | `PageBlock`'s Tab-equivalent keeps deriving its list from the parent `CustomPage.pages` embed, as today |
| No "mint a share link" endpoint | `viewingUrl` surfaced as "preview link" wherever the content's own DTO is already being fetched (edit page); no standalone "Copy link" button from the list view is built, since nothing backs it yet — flagged, not faked |

---

## 7. Migration phases (each independently shippable/testable)

1. **Types** — update `userContentBase.ts` + per-type DTOs to match backend shape.
   Nothing else changes yet; this phase alone will break every current
   `accessLevel`/`file.url` reference, so it's done together with a mechanical
   find-all-usages pass (not a silent rename) rather than shipped alone.
2. **Composable factory** — introduce `useUserContentApi`, `useContentAccess`,
   `useContentModeration`, `useContentPublishing`; migrate Blog/Configuration first
   (simplest shape), keep File/CustomPage/PageBlock on old composables temporarily behind
   the same public interface so `useNetworkManageActions.ts` doesn't need a big-bang edit.
3. **List page** — build `BaseUserContentListPage`, cut over `BlogsTab`/`ConfigurationsTab`
   first, verify the derived Draft/Published+grants badge, then `FilesTab`/`CustomPagesTab`.
4. **Edit page + sharing UI** — build `BaseUserContentEditPage` + `AccessGrantsEditor`,
   cut over `EditBlogTab` first (has the richest form, good stress test), then
   Configuration/CustomPage/File(new edit tab)/PageBlock.
5. **Moderation** — `ModerationFlagsPanel`, gated by permission, added once the edit page
   shell exists for all 5 types.
6. **File-specific delivery** — swap every direct `file.url` consumer over to
   `viewingUrl` resolution (`CloudinaryFile.vue` and friends); this is its own phase
   because it's the change most likely to have silent runtime breakage (broken images)
   if rushed.
7. **Cleanup** — delete `AccessLevelPicker.vue`, `lib/accessLevels.ts`, dead
   `hideAccessPicker` prop, old per-type composable files once nothing imports them.

---

## 8. Assumptions / open decisions (flag or confirm before/while implementing)

- **Default "share with own network" on publish**: context doc §7 explicitly calls this
  out as an undecided product question. Plan assumes: prompt the user once on first
  publish ("Also share with your own network?" defaulted checked) rather than silently
  auto-creating the grant or silently not creating it. Confirm before building.
- **Where OwnershipMode lives**: assumed "Advanced" disclosure inside the edit page
  (per context doc §2's own suggestion), not the create flow. Confirm.
- **Where Moderation lives**: assumed a permission-gated panel on each edit page, not a
  separate network-level moderation queue route. A separate `.../manage/moderation`
  aggregate view across all flagged content in the network is arguably more useful for a
  moderator than checking each item individually — worth a product call before Phase 5.
- **Share-link gap**: no "Copy link" button anywhere until backend adds a mint-link
  endpoint (context doc §6). Confirmed as explicitly deferred, not silently dropped.
- **Account-scoped access/moderation**: context doc's endpoint list is written against
  the network-scoped controllers; assumed identical routes exist under the account/proxy
  scope for consistency — verify against actual backend routes before wiring
  `useContentAccess`/`useContentModeration` for the account side.

## 9. Out of scope for this refactor

- Any new backend work (the two backend gaps in §6/§13 get flagged, not built around).
- Comment/Like capability UI (explicitly not enforced server-side yet, per context doc §5).
- Redesigning visual styling beyond what's needed to fit the new controls in.
