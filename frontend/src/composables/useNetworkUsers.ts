import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { CreateNetworkUserDto, NetworkUserDto, UpdateNetworkUserDto } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useNetworkUsers() {
  const fetchNetworkUsers = useCachedApi<NetworkUserDto[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'users'),
    async (networkId) => {
      return await api.get<NetworkUserDto[]>(`networks/${networkId}/users/`);
    },
  );

  const createNetworkUser = useMutation<
    NetworkUserDto,
    [networkId: string, userProxyId: string, roleIds: string[]]
  >(
    async (networkId, userProxyId, roleIds) => {
      const created = await api.post<NetworkUserDto, CreateNetworkUserDto>(
        `/networks/${networkId}/users/${userProxyId}`,
        {},
      );

      if (roleIds.length > 0) {
        await Promise.all(
          roleIds.map((roleId) =>
            api.post(`/networks/${networkId}/users/${created.data.id}/roles/${roleId}/`, {}),
          ),
        );
      }

      return await api.get<NetworkUserDto>(`/networks/${networkId}/users/${created.data.id}/`);
    },
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'users', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'users'),
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateNetworkUser = useMutation<
    NetworkUserDto,
    [
      networkId: string,
      userId: string,
      payload: UpdateNetworkUserDto,
      addedRoles: string[],
      removedRoles: string[],
    ]
  >(
    async (networkId, userId, payload, addedRoles, removedRoles) => {
      await api.put<NetworkUserDto, UpdateNetworkUserDto>(
        `/networks/${networkId}/users/${userId}`,
        payload,
      );

      const hasChanges = addedRoles.length > 0 || removedRoles.length > 0;
      if (hasChanges) {
        await Promise.all([
          ...addedRoles.map((roleId) =>
            api.post(`/networks/${networkId}/users/${userId}/roles/${roleId}/`, {}),
          ),
          ...removedRoles.map((roleId) =>
            api.delete(`/networks/${networkId}/users/${userId}/roles/${roleId}/`),
          ),
        ]);
      }

      return await api.get<NetworkUserDto>(`/networks/${networkId}/users/${userId}/`);
    },
    {
      itemKeyFactory: (_, networkId, userId) => networkKey(networkId, 'users', userId),
      listKeyFactory: (networkId) => networkKey(networkId, 'users'),
      listUpdater: (currentList, result) => {
        return currentList.map((item) => (item.id === result.id ? result : item));
      },
    },
  );

  const deleteNetworkUser = useMutation<
    void,
    [networkId: string, networkUserId: string],
    NetworkUserDto
  >(
    async (networkId, networkUserId) =>
      await api.delete(`/networks/${networkId}/users/${networkUserId}/`),
    {
      itemKeyFactory: (_, networkId, networkUserId) =>
        networkKey(networkId, 'users', networkUserId),
      listKeyFactory: (networkId) => networkKey(networkId, 'users'),
      listUpdater: (currentList, _, __, networkUserId) => {
        return currentList.filter((item) => item.id !== networkUserId);
      },
    },
  );

  return { fetchNetworkUsers, createNetworkUser, updateNetworkUser, deleteNetworkUser };
}
