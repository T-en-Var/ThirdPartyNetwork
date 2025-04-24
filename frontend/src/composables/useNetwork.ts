import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'
import type { Network } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useNetwork() {
  const network: Ref<Network | null> = ref(null);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  const globalStore = useGlobalStore();

  const authStore = useAuthStore();

  const fetchNetwork = async (networkId: string) => {
    globalStore.startFetching();
    loading.value = true;
    try {
      const user = await authStore.getUser();
      if (!user) {
        loading.value = false;
        error.value = "No logged in user."
        return;
      }

      const response = await api.get<Network>(`/networks/${networkId}`)

      network.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError).message;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { network, loading, error, fetchNetwork };
}
