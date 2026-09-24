import api from '@/api/api';
import type { CreateRoleDto, NetworkDto, RoleDto, UpdateRoleDto } from '@/types';
import { globalCache, useCachedApi, useMutation } from './useApi';

export default function useRoles() {
  const fetchRoles = useCachedApi<RoleDto[], [networkId: string]>(
    (networkId) => `networks_${networkId}_roles`,
    async (networkId) => {
      const result = await api.get<RoleDto[]>(`/networks/${networkId}/roles/`);
      result.data.sort((a, b) => a.name.localeCompare(b.name));
      return result;
    },
    undefined,
    undefined,
    {
      initialData: (networkId) => {
        const network = globalCache.get(`networks_${networkId}`)?.data.value as NetworkDto | undefined;
        return network?.roles as RoleDto[] ?? [];
      },
    },
  );

  const fetchRole = useCachedApi<RoleDto, [networkId: string, roleId: string]>(
    (networkId, roleId) => `networks_${networkId}_roles_${roleId}`,
    async (networkId, roleId) => await api.get<RoleDto>(`/networks/${networkId}/roles/${roleId}/`),
  );

  const createRole = useMutation<
    RoleDto,
    [networkId: string, payload: CreateRoleDto, permissionIds: string[]]
  >(
    async (networkId, payload, permissionIds) => {
      const response = await api.post<RoleDto, CreateRoleDto>(`/networks/${networkId}/roles/`, payload);
      const roleId = response.data.id;

      if (permissionIds.length > 0) {
        await Promise.all(
          permissionIds.map((permId) =>
            api.post(`/networks/${networkId}/roles/${roleId}/permissions/${permId}/`, {}),
          ),
        );

        return await api.get<RoleDto>(`/networks/${networkId}/roles/${roleId}/`);
      }

      return response;
    },
    {
      itemKeyFactory: (result, networkId) => `networks_${networkId}_roles_${result.id}`,
      listKeyFactory: (networkId) => `networks_${networkId}_roles`,
      listUpdater: (currentList, result) => {
        const newList = [...currentList, result];
        return newList.sort((a, b) => a.name.localeCompare(b.name));
      },
    },
  );

  const updateRole = useMutation<
    RoleDto,
    [
      networkId: string,
      roleId: string,
      payload: UpdateRoleDto,
      addedPerms: string[],
      removedPerms: string[],
    ]
  >(
    async (networkId, roleId, payload, addedPerms, removedPerms) => {
      let response = await api.put<RoleDto, UpdateRoleDto>(
        `/networks/${networkId}/roles/${roleId}`,
        payload,
      );

      const hasChanges = addedPerms.length > 0 || removedPerms.length > 0;
      if (hasChanges) {
        await Promise.all([
          ...addedPerms.map((permId) =>
            api.post(`/networks/${networkId}/roles/${roleId}/permissions/${permId}/`, {}),
          ),
          ...removedPerms.map((permId) =>
            api.delete(`/networks/${networkId}/roles/${roleId}/permissions/${permId}/`),
          ),
        ]);
        response = await api.get<RoleDto>(`/networks/${networkId}/roles/${roleId}/`);
      }

      return response;
    },
    {
      itemKeyFactory: (_, networkId, roleId) => `networks_${networkId}_roles_${roleId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_roles`,
      listUpdater: (currentList, result) => {
        const newList = currentList.map((item) => (item.id === result.id ? result : item));
        return newList.sort((a, b) => a.name.localeCompare(b.name));
      },
    },
  );

  const deleteRole = useMutation<void, [networkId: string, roleId: string], RoleDto>(
    async (networkId, roleId) => {
      return await api.delete(`/networks/${networkId}/roles/${roleId}/`);
    },
    {
      itemKeyFactory: (_, networkId, roleId) => `networks_${networkId}_roles_${roleId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_roles`,
      listUpdater: (currentList, _, __, roleId) => {
        return currentList.filter((item) => item.id !== roleId);
      },
    },
  );

  return {
    fetchRoles,
    fetchRole,
    createRole,
    updateRole,
    deleteRole,
  };
}
