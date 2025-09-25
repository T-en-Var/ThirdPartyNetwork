import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'
import type { UserProxy } from '@/types'
import { useGlobalStore } from '@/stores/global';

export default function useUserProxy() {
  const userProxy: Ref<UserProxy | null> = ref(null);
  const loading: Ref<boolean> = ref(false);
  const error: Ref<string | null> = ref(null);
  
  const global = useGlobalStore();
  const authStore = useAuthStore();

  const fetchUserProxy = async (userProxyId: string) => {
    global.startFetching();
    loading.value = true;
    try {
      const loggedInUserProxy = await authStore.getUserProxy();
      if (!loggedInUserProxy) {
        loading.value = false;
        error.value = "No logged in user."
        return;
      }

      const response = await api.get<UserProxy>(`/users/${loggedInUserProxy.user.id}/proxies/${userProxyId}`);

      if (response.status != 200) {
        loading.value = false;
        error.value = "Error fetching networks."
        return;
      }

      userProxy.value = response.data;
    } catch (err) {
      error.value = (err as AxiosError).message;
    } finally {
      loading.value = false;
      global.stopFetching();
    }
  };

  return { userProxy, loading, error, fetchUserProxy };
}
