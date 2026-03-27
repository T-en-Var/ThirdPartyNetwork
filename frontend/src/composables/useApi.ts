import { useGlobalStore } from '@/stores/global';
import type { ErrorMessage } from '@/types';
import type { AxiosError, AxiosResponse } from 'axios';
import { computed, ref, shallowRef, type Ref } from 'vue';

interface CacheEntry<T> {
  data: Ref<T | null>;
  isFetching: Ref<boolean>;
  error: Ref<string | null>;
  lastFetch: number;
}

// Global "Source of Truth" (Primitive values, not refs, to keep it simple)
export const globalCache = new Map<string, CacheEntry<unknown>>();

function getOrCreateEntry<T>(key: string): CacheEntry<T> {
  if (!globalCache.has(key)) {
    globalCache.set(key, {
      data: ref(null),
      isFetching: ref(false),
      error: ref(null),
      lastFetch: 0,
    });
  }
  return globalCache.get(key) as CacheEntry<T>;
}

export interface UseCachedApiReturn<T, P extends unknown[]> {
  execute: (...args: P) => Promise<void>;
  isFetching: Ref<boolean>;
  loading: Ref<boolean>;
  error: Ref<string | null>;
  data: Ref<T | null>;
}

export function useCachedApi<T, P extends unknown[]>(
  keyFactory: (...args: P) => string,
  fetcher: (...args: P) => Promise<AxiosResponse<T>>,
  staleTime = 60000,
  forceUpdate: boolean = false,
  options?: { initialData?: (...args: P) => T | null },
) {
  const globalStore = useGlobalStore();

  const currentEntry = shallowRef<CacheEntry<T> | null>(null);

  const execute = async (...args: P) => {
    const key = keyFactory(...args);
    const entry = getOrCreateEntry<T>(key);
    currentEntry.value = entry; // Keys point to same objects

    const now = Date.now();

    if (entry.data.value === null && options?.initialData) {
      const seededData = options.initialData(...args);
      if (seededData !== undefined) {
        entry.data.value = seededData;
        entry.lastFetch = 0;
      }
    }

    if (!forceUpdate && entry.data.value !== null && now - entry.lastFetch < staleTime) {
      return;
    }

    entry.isFetching.value = true;
    entry.error.value = null;
    globalStore.startFetching();

    try {
      const result = await fetcher(...args);
      entry.data.value = result.data;
      entry.lastFetch = Date.now();
    } catch (err) {
      entry.error.value =
        (err as AxiosError<ErrorMessage>)?.response?.data?.message ||
        (err as AxiosError).message ||
        'API Error';
    } finally {
      entry.isFetching.value = false;
      globalStore.stopFetching();
    }
  };

  return {
    data: computed(() => currentEntry.value?.data.value ?? null),
    isFetching: computed(() => currentEntry.value?.isFetching.value ?? false),
    /** Is fetching and has no data */
    loading: computed(() => {
      const isNetworkActive = currentEntry.value?.isFetching.value ?? false;
      const hasNoData = currentEntry.value?.data.value === null;
      return isNetworkActive && hasNoData;
    }),
    error: computed(() => currentEntry.value?.error.value ?? null),
    execute,
  };
}

export interface UseMutationReturn<T, P extends unknown[]> {
  execute: (...args: P) => Promise<T>;
  loading: Ref<boolean>;
  error: Ref<string | null>;
}

export function useMutation<T, P extends unknown[], TListItem = T>(
  action: (...args: P) => Promise<AxiosResponse<T>>,
  options?: {
    itemKeyFactory?: (result: T, ...args: P) => string,
    listKeyFactory?: (...args: P) => string,
    listUpdater?: (currentList: TListItem[], result: T, ...args: P) => TListItem[],
    onSuccess?: (data: T) => void,
  },
) {
  const loading = ref(false);
  const error = ref<string | null>(null);
  const globalStore = useGlobalStore();

  const execute = async (...args: P): Promise<T> => {
    loading.value = true;
    error.value = null;
    globalStore.startFetching();

    try {
      const result = await action(...args);

      if (options?.listKeyFactory && options?.listUpdater) {
        const listKey = options.listKeyFactory(...args);
        const listEntry = getOrCreateEntry<TListItem[]>(listKey);

        if (listEntry.data.value != null) {
          listEntry.data.value = options.listUpdater(listEntry.data.value, result.data, ...args);
          listEntry.lastFetch = Date.now();
        }
      }

      if (options?.itemKeyFactory) {
        const key = options.itemKeyFactory(result.data, ...args);
        const itemEntry = getOrCreateEntry<T>(key);
        
        itemEntry.data.value = result.data;
        itemEntry.lastFetch = Date.now();
      }

      if (options?.onSuccess) {
        options.onSuccess(result.data);
      }

      return result.data;
    } catch (err) {
      const msg =
        (err as AxiosError<ErrorMessage>)?.response?.data?.message ||
        (err as AxiosError).message ||
        'API Error';
      error.value = msg;

      // Re-throw so the component can handle specific logic
      throw err;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { execute, loading, error };
}
