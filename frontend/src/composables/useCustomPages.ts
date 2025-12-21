import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import type { CustomPage } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useCustomPage() {
  const customPages: Ref<CustomPage[]> = ref([]);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  const globalStore = useGlobalStore();

  const fetchCustomPages = async (networkId: string) => {
    globalStore.startFetching();
    loading.value = true;

    try {
      const response = await api.get<CustomPage[]>(`/networks/${networkId}/custompages`)

      customPages.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError).message;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  }

  return { customPages, loading, error, fetchCustomPages };
}
