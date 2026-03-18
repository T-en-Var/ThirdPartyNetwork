import api from '@/api/api';
import { ref, computed } from 'vue';
import { useAuthStore } from '@/stores/auth';
import type { Network } from '@/types';
import { useCachedApi } from './useApi';

export default function useUsersNetworks() {
  const authStore = useAuthStore();

  const authError = ref<string | null>(null);

  const {
    data: networks,
    isFetching,
    loading,
    error: apiError,
    execute,
  } = useCachedApi<Network[], [userId: string, proxyId: string]>(
    (userId, proxyId) => `users_${userId}_proxies_${proxyId}_networks`,
    async (userId, proxyId) =>
      await api.get<Network[]>(`/users/${userId}/proxies/${proxyId}/networks`),
  );

  const _fetchUserNetworks = async () => {
    authError.value = null;
    const userProxy = await authStore.getUserProxy();

    if (!userProxy) {
      authError.value = 'No logged in user.';
      return;
    }

    await execute(userProxy.user.id, userProxy.id);
  };

  const fetchUserNetworks = {
    data: networks,
    isFetching,
    loading,
    error: computed(() => authError.value || apiError.value),
    execute: _fetchUserNetworks,
  };

  return { fetchUserNetworks }
}
