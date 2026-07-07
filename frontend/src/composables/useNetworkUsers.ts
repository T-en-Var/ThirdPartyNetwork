import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { CreateNetworkUser, NetworkUser, UpdateNetworkUser } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useNetworkUsers() {
  const fetchNetworkUsers = useCachedApi<NetworkUser[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'users'),
    async (networkId) => {
      return await api.get<NetworkUser[]>(`networks/${networkId}/users/`);
    },
  );

  const createNetworkUser = useMutation<
    NetworkUser,
    [networkId: string, userProxyId: string, roleIds: string[]]
  >(
    async (networkId, userProxyId, roleIds) => {
      const created = await api.post<NetworkUser, CreateNetworkUser>(
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

      return await api.get<NetworkUser>(`/networks/${networkId}/users/${created.data.id}/`);
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
    NetworkUser,
    [
      networkId: string,
      userId: string,
      payload: UpdateNetworkUser,
      addedRoles: string[],
      removedRoles: string[],
    ]
  >(
    async (networkId, userId, payload, addedRoles, removedRoles) => {
      await api.put<NetworkUser, UpdateNetworkUser>(
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

      return await api.get<NetworkUser>(`/networks/${networkId}/users/${userId}/`);
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
    NetworkUser
  >(
    async (networkId, networkUserId) =>
      await api.delete(`/networks/${networkId}/users/${networkUserId}/`),
    {
      itemKeyFactory: (_, networkId, networkUserId) => networkKey(networkId, 'users', networkUserId),
      listKeyFactory: (networkId) => networkKey(networkId, 'users'),
      listUpdater: (currentList, _, __, networkUserId) => {
        return currentList.filter((item) => item.id !== networkUserId);
      },
    },
  );

  return { fetchNetworkUsers, createNetworkUser, updateNetworkUser, deleteNetworkUser };
}
