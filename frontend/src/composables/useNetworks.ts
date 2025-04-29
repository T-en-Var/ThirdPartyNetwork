// useNetworks.ts
import { ref } from 'vue';
import type { Network } from '@/types';
import api from '@/api/api';
import { useGlobalStore } from '@/stores/global';

export default function useNetworks() {
  const networks = ref<Network[]>([]);
  const loading = ref(true);
  const error = ref<string | null>(null);

  const global = useGlobalStore();

  const fetchNetworks = async () => {
    global.startFetching();
    loading.value = true;
    error.value = null;
    
    try {
      const response = await api.get<Network[]>('/networks');

      networks.value = response.data;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error occurred';
    } finally {
      global.stopFetching();
      loading.value = false;
    }
  };

  return {
    networks,
    loading,
    error,
    fetchNetworks,
  };
}