import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { Network } from '@/types';
import { useCachedApi } from './useApi';
import { withUserProxyGuard } from './withUserProxyGuard';

export default function useUsersNetworks() {
  const cachedApi = useCachedApi<Network[], [userId: string, proxyId: string]>(
    (userId, proxyId) => userProxyKey(userId, proxyId, 'networks'),
    async (userId, proxyId) =>
      await api.get<Network[]>(`/users/${userId}/proxies/${proxyId}/networks`),
  );

  const fetchUserNetworks = withUserProxyGuard(cachedApi, async (userProxy) => {
    await cachedApi.execute(userProxy.user.id, userProxy.id);
  });

  return { fetchUserNetworks };
}
