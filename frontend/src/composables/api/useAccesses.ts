import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import {
  type AccessDto,
  type NetworkAccessDto,
  type NetworkDto,
  type UpdateNetworkAccessDto,
} from '@/types';
import { replaceById, removeById, useCachedApi, useMutation } from './useApi';

export default function useAccesses() {
  const fetchAccesses = useCachedApi<AccessDto[], []>(
    () => 'accesses',
    async () => {
      const result = await api.get<AccessDto[]>('/accesses/');
      result.data.sort((a, b) => a.name.localeCompare(b.name));
      return result;
    },
  );

  // `listKeyFactory` below points at the SAME cache entry `useNetworks().fetchNetwork` uses
  // (`networks_${networkId}`), which holds the whole `NetworkDto`, not an access list. So
  // `currentList` here is actually that `NetworkDto`, and the array to update is its
  // `networkAccesses` field, keyed by `access.id` (there is no top-level `id`).
  const createNetworkAccess = useMutation<
    NetworkAccessDto,
    [networkId: string, accessId: string, isRequired: boolean],
    unknown
  >(
    async (networkId, accessId, isRequired) =>
      await api.post(`/networks/${networkId}/accesses/${accessId}`, { isRequired: isRequired }),
    {
      itemKeyFactory: (_, networkId, accessId) => networkKey(networkId, 'accesses', accessId),
      listKeyFactory: (networkId) => networkKey(networkId),
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as NetworkDto;
        network.networkAccesses.push({
          accessId: result.accessId,
          isRequired: result.isRequired,
          access: result.access,
        });
        network.networkAccesses.sort((a, b) => a.access.name.localeCompare(b.access.name));
        return network as unknown as unknown[];
      },
    },
  );

  const updateNetworkAccess = useMutation<
    NetworkAccessDto,
    [networkId: string, accessId: string, isRequired: boolean],
    unknown
  >(
    async (networkId, accessId, isRequired) =>
      await api.put<NetworkAccessDto, UpdateNetworkAccessDto>(
        `/networks/${networkId}/accesses/${accessId}`,
        { isRequired },
      ),
    {
      itemKeyFactory: (_, networkId, accessId) => networkKey(networkId, 'accesses', accessId),
      listKeyFactory: (networkId) => networkKey(networkId),
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as NetworkDto;
        network.networkAccesses = replaceById(
          network.networkAccesses,
          result,
          (item) => item.access.id,
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
      listUpdater: (currentList, _, __, accessId) => {
        const network = currentList as unknown as NetworkDto;
        network.networkAccesses = removeById(
          network.networkAccesses,
          accessId,
          (item) => item.access.id,
        );
        return network as unknown as unknown[];
      },
    },
  );

  return { fetchAccesses, createNetworkAccess, updateNetworkAccess, deleteNetworkAccess };
}
