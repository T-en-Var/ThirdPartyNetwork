import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { ConfigurationDto, CreateConfigurationDto, UpdateConfigurationDto } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';

// Configurations always live under a network (there is no account-scoped storage), so every
// mutation here still needs a networkId and hits the same /networks/{networkId}/configurations
// routes as composables/network/useConfigurations.ts. This composable's own value-add is
// `fetchConfigurations`, which aggregates a user proxy's configurations across every network
// they belong to via the dedicated cross-network "authors" endpoint, something the
// network-scoped composable can't do.
export default function useConfigurations() {
  const fetchConfigurations = useCachedApi<ConfigurationDto[], [userProxyId: string]>(
    (userProxyId) => `authors_${userProxyId}_configurations`,
    async (userProxyId) => await api.get<ConfigurationDto[]>(`/authors/${userProxyId}/configurations`),
  );

  const fetchConfiguration = useCachedApi<ConfigurationDto, [networkId: string, configId: string]>(
    (networkId, configId) => networkKey(networkId, 'configurations', configId),
    async (networkId, configId) =>
      await api.get<ConfigurationDto>(`/networks/${networkId}/configurations/${configId}`),
  );

  const createConfiguration = useMutation<
    ConfigurationDto,
    [networkId: string, payload: CreateConfigurationDto]
  >(
    async (networkId, payload) =>
      await api.post<ConfigurationDto, CreateConfigurationDto>(
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
    ConfigurationDto,
    [networkId: string, configurationId: string, payload: UpdateConfigurationDto]
  >(
    async (networkId, configurationId, payload) =>
      await api.put<ConfigurationDto, UpdateConfigurationDto>(
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
    ConfigurationDto
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
