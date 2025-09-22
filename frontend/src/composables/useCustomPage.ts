import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import type { CustomPage } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useCustomPage() {
  const customPage: Ref<CustomPage | null> = ref(null);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  const globalStore = useGlobalStore();

  const fetchCustomPage = async (networkId: string, custompageId: string) => {
    globalStore.startFetching();
    loading.value = true;

    try {
      const response = await api.get<CustomPage>(`/networks/${networkId}/custompages/${custompageId}`)

      customPage.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError).message;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  }

  return { customPage, loading, error, fetchCustomPage };
}
