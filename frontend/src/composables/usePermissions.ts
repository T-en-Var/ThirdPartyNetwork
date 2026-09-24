import api from '@/api/api';
import type { PermissionDto } from '@/types';
import { useCachedApi } from './useApi';

export default function usePermissions() {
  const fetchPermissions = useCachedApi<PermissionDto[], []>(
    () => 'permissions',
    async () => {
      const result = await api.get<PermissionDto[]>('/permissions/');
      result.data.sort((a, b) => a.name.localeCompare(b.name));
      return result;
    },
  );

  return { fetchPermissions };
}
