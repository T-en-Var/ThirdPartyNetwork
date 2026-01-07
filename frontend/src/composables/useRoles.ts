import { ref } from 'vue'
import api from '@/api/api.ts'
import type { AxiosError } from 'axios'
import type { Role, ErrorMessage } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useRoles() {
  const roles = ref<Role[]>([]);
  const loading = ref<boolean>(false);
  const error = ref<string | null>(null);
  
  const globalStore = useGlobalStore();

  const fetchRoles = async (networkId: string) => {
    loading.value = true;

    globalStore.startFetching();
    try {
      const response = await api.get<Role[]>(`networks/${networkId}/roles/`)
      const data = response.data.sort((a, b) => a.name > b.name ? 1 : -1);

      roles.value = data;
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || null;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { roles, loading, error, fetchRoles };
}
