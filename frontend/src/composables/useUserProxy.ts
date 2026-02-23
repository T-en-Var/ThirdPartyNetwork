import api from '@/api/api';
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import type { UserProxy } from '@/types';
import { useCachedApi } from './useApi';

export default function useUserProxy() {
  const authStore = useAuthStore();

  const authError = ref<string | null>(null);

  const {
    data: userProxy,
    isFetching,
    loading,
    error: apiError,
    execute,
  } = useCachedApi<UserProxy, [loggedInUserId: string, targetProxyId: string]>(
    (loggedInUserId, targetProxyId) => `users_${loggedInUserId}_proxies_${targetProxyId}`,
    async (loggedInUserId, targetProxyId) =>
      await api.get<UserProxy>(`/users/${loggedInUserId}/proxies/${targetProxyId}`),
  );

  const _fetchUserProxy = async (userProxyId: string) => {
    authError.value = null;
    const loggedInUserProxy = await authStore.getUserProxy();

    if (!loggedInUserProxy) {
      authError.value = 'No logged in user.';
      return;
    }

    await execute(loggedInUserProxy.user.id, userProxyId);
  };
  const fetchUserProxy = {
    data: userProxy,
    isFetching,
    loading,
    error: apiError,
    execute: _fetchUserProxy,
  }

  return {
    fetchUserProxy,
  };
}
