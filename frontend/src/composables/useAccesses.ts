import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import type { Access, ErrorMessage } from '@/types'
import { useGlobalStore } from '@/stores/global'
import { useAccessesStore } from '@/stores/accesses'

export default function useAccesses() {
  const accesses: Ref<Access[]> = ref([]);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  
  const globalStore = useGlobalStore();
  const accessesStore = useAccessesStore();

  const fetchAccesses = async () => {
    loading.value = true;

    if (accessesStore.accesses.length > 0) {
      loading.value = false;
      accesses.value = accessesStore.accesses;
      return;
    }

    globalStore.startFetching();
    try {
      const response = await api.get<Access[]>(`/accesses/`)

      accesses.value = response.data;
      accessesStore.setAccesses(response.data)
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || null;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { accesses, loading, error, fetchAccesses };
}
