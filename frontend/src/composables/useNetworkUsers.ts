import { ref } from 'vue'
import api from '@/api/api.ts'
import type { AxiosError } from 'axios'
import type { NetworkUser, ErrorMessage } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function usenetworkUsers() {
  const networkUsers = ref<NetworkUser[]>([]);
  const loading = ref<boolean>(false);
  const error = ref<string | null>(null);
  
  const globalStore = useGlobalStore();

  const fetchNetworkUsers = async (networkId: string) => {
    loading.value = true;

    globalStore.startFetching();
    try {
      const response = await api.get<NetworkUser[]>(`networks/${networkId}/users/`)

      networkUsers.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || null;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { networkUsers, loading, error, fetchNetworkUsers };
}
