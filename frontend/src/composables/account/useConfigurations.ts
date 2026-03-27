import api from '@/api/api';
import type { Configuration, CreateConfiguration } from '@/types';
import { useCachedApi, useMutation } from '../useApi';

export default function useConfigurations() {
  const fetchNetworkConfigurations = useCachedApi<Configuration[], [networkId: string]>(
    (networkId) => `networks_${networkId}_configurations`,
    async (networkId) => await api.get<Configuration[]>(`/networks/${networkId}/configurations`),
  );

  const fetchUserConfigurations = useCachedApi<Configuration[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_configurations`,
    async (userId, userProxyId) => await api.get<Configuration[]>(`/users/${userId}/proxies/${userProxyId}/configurations`),
  );

  const fetchConfiguration = useCachedApi<Configuration, [networkId: string, configId: string]>(
    (networkId, configId) => `networks_${networkId}_configurations_${configId}`,
    async (networkId, configId) =>
      await api.get<Configuration>(`/networks/${networkId}/configurations/${configId}`),
  );

  const createConfiguration = useMutation<
    Configuration,
    [networkId: string, payload: CreateConfiguration]
  >(
    async (networkId, payload) =>
      await api.post<Configuration, CreateConfiguration>(
        `/networks/${networkId}/configurations`,
        payload,
      ),
    {
      itemKeyFactory: (result, networkId) => `networks_${networkId}_configurations_${result.id}`,
      listKeyFactory: (networkId) => `networks_${networkId}_configurations`,
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateConfiguration = useMutation<
    Configuration,
    [networkId: string, configurationId: string, payload: Partial<Configuration>]
  >(
    async (networkId, configurationId, payload) =>
      await api.put<Configuration, Partial<Configuration>>(
        `/networks/${networkId}/configurations/${configurationId}`,
        payload,
      ),
    {
      itemKeyFactory: (_, networkId, configurationId) =>
        `networks_${networkId}_configurations_${configurationId}`,

      listKeyFactory: (networkId) => `networks_${networkId}_configurations`,
      listUpdater: (currentList, result, networkId, configurationId) => {
        return currentList.map((item) => (item.id === configurationId ? result : item));
      },
    },
  );

  const deleteConfiguration = useMutation<
    void,
    [networkId: string, configurationId: string],
    Configuration
  >(
    async (networkId, configurationId) =>
      await api.delete<void>(`/networks/${networkId}/configurations/${configurationId}`),
    {
      itemKeyFactory: (_, networkId, configurationId) =>
        `networks_${networkId}_configurations_${configurationId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_configurations`,
      listUpdater: (currentList, _, __, configurationId) =>
        currentList.filter((item) => item.id !== configurationId),
    },
  );

  return {
    fetchNetworkConfigurations,
    fetchUserConfigurations,
    fetchConfiguration,
    createConfiguration,
    updateConfiguration,
    deleteConfiguration,
  };
}
