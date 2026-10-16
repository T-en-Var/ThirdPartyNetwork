import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import { type NetworkAccessDto, type AccessDto, type NetworkDto } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useAccesses() {
  const fetchAccesses = useCachedApi<AccessDto[], []>(
    () => 'accesses',
    async () => {
      const result = await api.get<AccessDto[]>('/accesses/');
      result.data.sort((a, b) => a.name.localeCompare(b.name));
      return result;
    },
  );

  const createAccess = useMutation<
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
        network.networkAccesses = network.networkAccesses.map((item) =>
          item.access.id === result.access.id
            ? { accessId: result.accessId, isRequired: result.isRequired, access: result.access }
            : item,
        );
        network.networkAccesses = network.networkAccesses.sort((a, b) =>
          a.access.name.localeCompare(b.access.name),
        );

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
      await api.put(`/networks/${networkId}/accesses/${accessId}?IsRequired=${!isRequired}`, {}),
    {
      itemKeyFactory: (_, networkId, accessId) => networkKey(networkId, 'accesses', accessId),
      listKeyFactory: (networkId) => networkKey(networkId),
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as NetworkDto;
        network.networkAccesses = network.networkAccesses.map((item) =>
          item.access.id === result.access.id
            ? { accessId: result.accessId, isRequired: result.isRequired, access: result.access }
            : item,
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
        network.networkAccesses = network.networkAccesses.filter(
          (item) => item.access.id !== accessId,
        );
        return network as unknown as unknown[];
      },
    },
  );

  return { fetchAccesses, createAccess, updateNetworkAccess, deleteNetworkAccess };
}
