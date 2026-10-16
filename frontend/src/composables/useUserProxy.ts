import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { NetworkPermissionsCollectionDto, UserProxyDto } from '@/types';
import { useCachedApi } from './useApi';
import { withUserProxyGuard } from './withUserProxyGuard';

export default function useUserProxy() {
  const fetchMe = useCachedApi<UserProxyDto, []>(
    () => `me`,
    async () => await api.get<UserProxyDto>(`/me`),
  );

  const fetchPermissionCollection = useCachedApi<NetworkPermissionsCollectionDto[], []>(
    () => `me_permissions`,
    async () => await api.get<NetworkPermissionsCollectionDto[]>(`/me/permissions`),
  );

  const cachedApi = useCachedApi<UserProxyDto, [loggedInUserId: string, targetProxyId: string]>(
    (loggedInUserId, targetProxyId) => userProxyKey(loggedInUserId, targetProxyId),
    async (loggedInUserId, targetProxyId) =>
      await api.get<UserProxyDto>(`/users/${loggedInUserId}/proxies/${targetProxyId}`),
  );

  const fetchUserProxy = withUserProxyGuard(
    cachedApi,
    async (loggedInUserProxy, userProxyId: string) => {
      await cachedApi.execute(loggedInUserProxy.user.id, userProxyId);
    },
  );

  return {
    fetchUserProxy,
    fetchMe,
    fetchPermissionCollection,
  };
}
