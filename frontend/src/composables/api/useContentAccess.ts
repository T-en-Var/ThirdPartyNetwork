import type { ContentAccessGrantDto, CreateContentAccessGrantDto, OwnershipMode } from '@/types';
import { prependItem, removeById, useCachedApi, useMutation } from './useApi';
import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';

/**
 * Every user-generated content type (files, blogs, configurations, custom pages, page blocks)
 * shares the exact same publish/unpublish/ownership/access-grant endpoints and payload shapes -
 * only the route segment (e.g. "files", "blogs") differs. This factory builds those six calls
 * once instead of hand-duplicating them per content type.
 *
 * `resource` is the content type's own cached-item factory (e.g. `(networkId, fileId) =>
 * networkKey(networkId, 'files', fileId)`) and `basePath` builds the base URL for a single item
 * (e.g. `(networkId, fileId) => \`/networks/${networkId}/files/${fileId}\``).
 */
export function useContentAccess<TDto extends { id: string }>(
  resourceKey: (networkId: string, contentId: string) => string,
  basePath: (networkId: string, contentId: string) => string,
  listKey: (networkId: string) => string,
) {
  const publish = useMutation<TDto, [networkId: string, contentId: string]>(
    async (networkId, contentId) =>
      await api.post<TDto, undefined>(`${basePath(networkId, contentId)}/publish`),
    {
      itemKeyFactory: (_, networkId, contentId) => resourceKey(networkId, contentId),
      listKeyFactory: (networkId) => listKey(networkId),
      listUpdater: (currentList, result) =>
        currentList.map((item) => (item.id === result.id ? result : item)),
    },
  );

  const unpublish = useMutation<TDto, [networkId: string, contentId: string]>(
    async (networkId, contentId) =>
      await api.post<TDto, undefined>(`${basePath(networkId, contentId)}/unpublish`),
    {
      itemKeyFactory: (_, networkId, contentId) => resourceKey(networkId, contentId),
      listKeyFactory: (networkId) => listKey(networkId),
      listUpdater: (currentList, result) =>
        currentList.map((item) => (item.id === result.id ? result : item)),
    },
  );

  const setOwnership = useMutation<TDto, [networkId: string, contentId: string, mode: OwnershipMode]>(
    async (networkId, contentId, mode) =>
      await api.post<TDto, { mode: OwnershipMode }>(`${basePath(networkId, contentId)}/ownership`, {
        mode,
      }),
    {
      itemKeyFactory: (_, networkId, contentId) => resourceKey(networkId, contentId),
      listKeyFactory: (networkId) => listKey(networkId),
      listUpdater: (currentList, result) =>
        currentList.map((item) => (item.id === result.id ? result : item)),
    },
  );

  const fetchAccessGrants = useCachedApi<ContentAccessGrantDto[], [networkId: string, contentId: string]>(
    (networkId, contentId) => `${resourceKey(networkId, contentId)}_access`,
    async (networkId, contentId) =>
      await api.get<ContentAccessGrantDto[]>(`${basePath(networkId, contentId)}/access`),
  );

  const createAccessGrant = useMutation<
    ContentAccessGrantDto,
    [networkId: string, contentId: string, payload: CreateContentAccessGrantDto]
  >(
    async (networkId, contentId, payload) =>
      await api.post<ContentAccessGrantDto, CreateContentAccessGrantDto>(
        `${basePath(networkId, contentId)}/access`,
        payload,
      ),
    {
      listKeyFactory: (networkId, contentId) => `${resourceKey(networkId, contentId)}_access`,
      listUpdater: prependItem,
    },
  );

  const revokeAccessGrant = useMutation<
    void,
    [networkId: string, contentId: string, grantId: string],
    ContentAccessGrantDto
  >(
    async (networkId, contentId, grantId) =>
      await api.delete<void>(`${basePath(networkId, contentId)}/access/${grantId}`),
    {
      listKeyFactory: (networkId, contentId) => `${resourceKey(networkId, contentId)}_access`,
      listUpdater: (currentList, _, __, ___, grantId) => removeById(currentList, grantId),
    },
  );

  return { publish, unpublish, setOwnership, fetchAccessGrants, createAccessGrant, revokeAccessGrant };
}

/** Convenience default for the common `/networks/{networkId}/{segment}/{id}` shape. */
export function useNetworkContentAccess<TDto extends { id: string }>(segment: string) {
  return useContentAccess<TDto>(
    (networkId, contentId) => networkKey(networkId, segment, contentId),
    (networkId, contentId) => `/networks/${networkId}/${segment}/${contentId}`,
    (networkId) => networkKey(networkId, segment),
  );
}
