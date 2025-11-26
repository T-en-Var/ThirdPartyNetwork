import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import type { Network } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useMainNetwork() {
  const network: Ref<Network | null> = ref(null);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  const globalStore = useGlobalStore();

  const fetchMainNetwork = async () => {
    globalStore.startFetching();
    loading.value = true;
    try {
      const response = await api.get<Network>(`/mainNetwork/`)

      network.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError).message;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { network, loading, error, fetchMainNetwork };
}
