import api from '@/api/api.ts'
import type { Permission } from '@/types'
import { useCachedApi } from './useApi';

export default function usePermissions() {
  const fetchPermissions = useCachedApi<Permission[], []>(
    () => 'permissions',
    async () => {
      const result = await api.get<Permission[]>('/permissions/');
      result.data.sort((a, b) => a.name.localeCompare(b.name));
      return result;
    },
  );

  return { fetchPermissions };
}
