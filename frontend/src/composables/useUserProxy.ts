import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { NetworkPermissionCollection, UserProxy } from '@/types';
import { useCachedApi } from './useApi';
import { withUserProxyGuard } from './withUserProxyGuard';

export default function useUserProxy() {
  const fetchMe = useCachedApi<UserProxy, []>(
    () => `me`,
    async () => await api.get<UserProxy>(`/me`),
  );

  const fetchPermissionCollection = useCachedApi<NetworkPermissionCollection[], []>(
    () => `me_permissions`,
    async () => await api.get<NetworkPermissionCollection[]>(`/me/permissions`),
  );

  const cachedApi = useCachedApi<UserProxy, [loggedInUserId: string, targetProxyId: string]>(
    (loggedInUserId, targetProxyId) => userProxyKey(loggedInUserId, targetProxyId),
    async (loggedInUserId, targetProxyId) =>
      await api.get<UserProxy>(`/users/${loggedInUserId}/proxies/${targetProxyId}`),
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
