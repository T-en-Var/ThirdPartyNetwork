import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { Configuration, CreateConfiguration } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';

export default function useConfigurations() {
  const fetchConfigurations = useCachedApi<Configuration[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'configurations'),
    async (networkId) => await api.get<Configuration[]>(`/networks/${networkId}/configurations`),
  );

  const fetchConfiguration = useCachedApi<Configuration, [networkId: string, configId: string]>(
    (networkId, configId) => networkKey(networkId, 'configurations', configId),
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
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'configurations', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'configurations'),
      listUpdater: prependItem,
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
        networkKey(networkId, 'configurations', configurationId),
      listKeyFactory: (networkId) => networkKey(networkId, 'configurations'),
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
        networkKey(networkId, 'configurations', configurationId),
      listKeyFactory: (networkId) => networkKey(networkId, 'configurations'),
      listUpdater: (currentList, _, __, configurationId) =>
        currentList.filter((item) => item.id !== configurationId),
    },
  );

  return {
    fetchConfigurations,
    fetchConfiguration,
    createConfiguration,
    updateConfiguration,
    deleteConfiguration,
  };
}
