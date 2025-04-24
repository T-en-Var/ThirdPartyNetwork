import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'
import type { Network } from '@/types'
import { useGlobalStore } from '@/stores/global';

export default function useUsersNetworks() {
  const networks: Ref<Network[]> = ref([]);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  
  const global = useGlobalStore();
  const authStore = useAuthStore();

  const fetchUsersNetworks = async () => {
    global.startFetching();
    loading.value = true;
    try {
      const user = await authStore.getUser();
      if (!user) {
        loading.value = false;
        error.value = "No logged in user."
        return;
      }

      const response = await api.get<Network[]>(`/users/${user.user.id}/proxies/${user.id}/networks`);

      if (response.status != 200) {
        loading.value = false;
        error.value = "Error fetching networks."
        return;
      }

      networks.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError).message;
    } finally {
      loading.value = false;
      global.stopFetching();
    }
  };

  return { networks, loading, error, fetchUsersNetworks };
}
