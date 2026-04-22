import api from '@/api/api';
import type { Configuration, CreateConfiguration } from '@/types';
import { useCachedApi, useMutation } from '../useApi';

const getKey = (userId: string, userProxyId: string, entryId?: string) => {
  const entity = 'configurations';
  const base = `users_${userId}_proxies_${userProxyId}_${entity}`;
  if (entryId !== undefined) return base + `_${entryId}`;
  return base;
};

export default function useConfigurations() {
  const fetchConfigurations = useCachedApi<Configuration[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => getKey(userId, userProxyId),
    async (userId, userProxyId) =>
      await api.get<Configuration[]>(`/users/${userId}/proxies/${userProxyId}/configurations`),
  );

  const fetchConfiguration = useCachedApi<
    Configuration,
    [networkId: string, userId: string, userProxyId: string, configId: string]
  >(
    (_, userId, userProxyId, cfgId) => getKey(userId, userProxyId, cfgId),
    async (networkId, configId) =>
      await api.get<Configuration>(`/networks/${networkId}/configurations/${configId}`),
  );

  const createConfiguration = useMutation<
    Configuration,
    [networkId: string, userId: string, userProxyId: string, payload: CreateConfiguration]
  >(
    async (networkId, _, __, payload) =>
      await api.post<Configuration, CreateConfiguration>(
        `/networks/${networkId}/configurations`,
        payload,
      ),
    {
      itemKeyFactory: (result, _, userId, userProxyId) => getKey(userId, userProxyId, result.id),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateConfiguration = useMutation<
    Configuration,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      configurationId: string,
      payload: Partial<Configuration>,
    ]
  >(
    async (networkId, _, __, configurationId, payload) =>
      await api.put<Configuration, Partial<Configuration>>(
        `/networks/${networkId}/configurations/${configurationId}`,
        payload,
      ),
    {
      itemKeyFactory: (_, __, userId, userProxyId, cfgId) => getKey(userId, userProxyId, cfgId),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, result, _, __, ___, configurationId) => {
        return currentList.map((item) => (item.id === configurationId ? result : item));
      },
    },
  );

  const deleteConfiguration = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, configurationId: string],
    Configuration
  >(
    async (networkId, _, __, configurationId) =>
      await api.delete<void>(`/networks/${networkId}/configurations/${configurationId}`),
    {
      itemKeyFactory: (_, __, userId, userProxyId, cfgId) => getKey(userId, userProxyId, cfgId),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, _, __, ___, ____, configurationId) =>
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
