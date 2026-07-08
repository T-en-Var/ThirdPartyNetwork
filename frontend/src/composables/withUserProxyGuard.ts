import { computed, ref, type ComputedRef, type Ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import type { UserProxy } from '@/types';

interface GuardableCachedApi<TData> {
  data: Ref<TData | null> | ComputedRef<TData | null>;
  isFetching: Ref<boolean> | ComputedRef<boolean>;
  loading: Ref<boolean> | ComputedRef<boolean>;
  error: Ref<string | null> | ComputedRef<string | null>;
}

/**
 * Wraps a useCachedApi result so it only executes once a logged-in user proxy is available,
 * surfacing 'No logged in user.' as the error otherwise instead of hitting the API.
 */
export function withUserProxyGuard<TData, TArgs extends unknown[]>(
  cachedApi: GuardableCachedApi<TData>,
  execute: (userProxy: UserProxy, ...args: TArgs) => Promise<void>,
) {
  const authStore = useAuthStore();
  const authError = ref<string | null>(null);

  const run = async (...args: TArgs) => {
    authError.value = null;
    const userProxy = await authStore.getUserProxy();

    if (!userProxy) {
      authError.value = 'No logged in user.';
      return;
    }

    await execute(userProxy, ...args);
  };

  return {
    data: cachedApi.data,
    isFetching: cachedApi.isFetching,
    loading: cachedApi.loading,
    error: computed(() => authError.value || cachedApi.error.value),
    execute: run,
  };
}
