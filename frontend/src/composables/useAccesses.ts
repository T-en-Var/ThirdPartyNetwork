import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import { type NetworkAccess, type Access, type Network } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useAccesses() {
  const fetchAccesses = useCachedApi<Access[], []>(
    () => 'accesses',
    async () => {
      const result = await api.get<Access[]>('/accesses/');
      result.data.sort((a, b) => a.name.localeCompare(b.name));
      return result;
    },
  );

  const createAccess = useMutation<
    NetworkAccess,
    [networkId: string, accessId: string, isRequired: boolean],
    unknown
  >(
    async (networkId, accessId, isRequired) =>
      await api.post(`/networks/${networkId}/accesses/${accessId}`, { isRequired: isRequired }),
    {
      itemKeyFactory: (_, networkId, accessId) => networkKey(networkId, 'accesses', accessId),
      listKeyFactory: (networkId) => networkKey(networkId),
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as Network;
        network.networkAccesses = network.networkAccesses.map((item) =>
          item.network.id === result.network.id && item.access.id === result.access.id ? result : item,
        );
        network.networkAccesses = network.networkAccesses.sort((a, b) =>
          a.access.name.localeCompare(b.access.name),
        );

        return network as unknown as unknown[];
      },
    },
  );

  const updateNetworkAccess = useMutation<
    NetworkAccess,
    [networkId: string, accessId: string, isRequired: boolean],
    unknown
  >(
    async (networkId, accessId, isRequired) =>
      await api.put(`/networks/${networkId}/accesses/${accessId}?IsRequired=${!isRequired}`, {}),
    {
      itemKeyFactory: (_, networkId, accessId) => networkKey(networkId, 'accesses', accessId),
      listKeyFactory: (networkId) => networkKey(networkId),
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as Network;
        network.networkAccesses = network.networkAccesses.map((item) =>
          item.network.id === result.network.id && item.access.id === result.access.id ? result : item,
        );
        return network as unknown as unknown[];
      },
    },
  );

  const deleteNetworkAccess = useMutation<void, [networkId: string, accessId: string], unknown>(
    async (networkId, accessId) => await api.delete(`/networks/${networkId}/accesses/${accessId}/`),
    {
      itemKeyFactory: (_, networkId, accessId) => networkKey(networkId, 'accesses', accessId),
      listKeyFactory: (networkId) => networkKey(networkId),
      listUpdater: (currentList, _, networkId, accessId) => {
        const network = currentList as unknown as Network;
        network.networkAccesses = network.networkAccesses.filter(
          (item) => item.network.id !== networkId || item.access.id !== accessId,
        );
        return network as unknown as unknown[];
      },
    },
  );

  return { fetchAccesses, createAccess, updateNetworkAccess, deleteNetworkAccess };
}
