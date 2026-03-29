import api from '@/api/api.ts';
import type { CreateNetworkUser, NetworkUser, UpdateNetworkUser } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useNetworkUsers() {
  const fetchNetworkUsers = useCachedApi<NetworkUser[], [networkId: string]>(
    (networkId) => `networks_${networkId}_users`,
    async (networkId) => {
      return await api.get<NetworkUser[]>(`networks/${networkId}/users/`);
    },
  );

  const createNetworkUser = useMutation<
    NetworkUser,
    [networkId: string, payload: CreateNetworkUser]
  >(
    async (networkId, payload) =>
      await api.post<NetworkUser, CreateNetworkUser>(`/networks/${networkId}/users/`, payload),
    {
      itemKeyFactory: (result, networkId) => `networks_${networkId}_users_${result.id}`,
      listKeyFactory: (networkId) => `networks_${networkId}_users`,
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
      itemKeyFactory: (_, networkId, roleId) => `networks_${networkId}_roles_${roleId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_users`,
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
      itemKeyFactory: (_, networkId, networkUserId) =>
        `networks_${networkId}_users_${networkUserId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_users`,
      listUpdater: (currentList, _, __, networkUserId) => {
        return currentList.filter((item) => item.id !== networkUserId);
      },
    },
  );

  return { fetchNetworkUsers, createNetworkUser, updateNetworkUser, deleteNetworkUser };
}
