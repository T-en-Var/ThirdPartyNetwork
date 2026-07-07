import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { Configuration, CreateConfiguration } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';

export default function useConfigurations() {
  const fetchConfigurations = useCachedApi<Configuration[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => userProxyKey(userId, userProxyId, 'configurations'),
    async (userId, userProxyId) =>
      await api.get<Configuration[]>(`/users/${userId}/proxies/${userProxyId}/configurations`),
  );

  const fetchConfiguration = useCachedApi<
    Configuration,
    [networkId: string, userId: string, userProxyId: string, configId: string]
  >(
    (_networkId, userId, userProxyId, configId) =>
      userProxyKey(userId, userProxyId, 'configurations', configId),
    async (networkId, _userId, _userProxyId, configId) =>
      await api.get<Configuration>(`/networks/${networkId}/configurations/${configId}`),
  );

  const createConfiguration = useMutation<
    Configuration,
    [networkId: string, userId: string, userProxyId: string, payload: CreateConfiguration]
  >(
    async (networkId, _userId, _userProxyId, payload) =>
      await api.post<Configuration, CreateConfiguration>(
        `/networks/${networkId}/configurations`,
        payload,
      ),
    {
      itemKeyFactory: (result, _networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'configurations', result.id),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'configurations'),
      listUpdater: prependItem,
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
    async (networkId, _userId, _userProxyId, configurationId, payload) =>
      await api.put<Configuration, Partial<Configuration>>(
        `/networks/${networkId}/configurations/${configurationId}`,
        payload,
      ),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, configId) =>
        userProxyKey(userId, userProxyId, 'configurations', configId),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'configurations'),
      listUpdater: (currentList, result, _networkId, _userId, _userProxyId, configurationId) => {
        return currentList.map((item) => (item.id === configurationId ? result : item));
      },
    },
  );

  const deleteConfiguration = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, configurationId: string],
    Configuration
  >(
    async (networkId, _userId, _userProxyId, configurationId) =>
      await api.delete<void>(`/networks/${networkId}/configurations/${configurationId}`),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, configId) =>
        userProxyKey(userId, userProxyId, 'configurations', configId),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'configurations'),
      listUpdater: (currentList, _result, _networkId, _userId, _userProxyId, configurationId) =>
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
