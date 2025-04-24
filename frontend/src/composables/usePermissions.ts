import { ref } from 'vue'
import api from '@/api/api.ts'
import type { AxiosError } from 'axios'
import type { Permission, ErrorMessage } from '@/types'
import { useGlobalStore } from '@/stores/global'
import { usePermissionsStore } from '@/stores/permissions'

export default function usePermissions() {
  const permissions = ref<Permission[]>([]);
  const loading = ref<boolean>(false);
  const error = ref<string | null>(null);
  
  const globalStore = useGlobalStore();
  const permissionsStore = usePermissionsStore();

  const fetchPermissions = async () => {
    loading.value = true;

    if (permissionsStore.permissions.length > 0) {
      loading.value = false;
      permissions.value = permissionsStore.permissions;
      return;
    }

    globalStore.startFetching();
    try {
      const response = await api.get<Permission[]>(`/permissions/`)

      permissions.value = response.data;
      permissionsStore.setPermissions(response.data)
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || null;
    } finally {
      loading.value = false;
      globalStore.stopFetching();
    }
  };

  return { permissions, loading, error, fetchPermissions };
}
