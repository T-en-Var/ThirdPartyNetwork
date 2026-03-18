import api from '@/api/api.ts';
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
      itemKeyFactory: (_, networkId, accessId) => `networks_${networkId}_accesses_${accessId}`,
      listKeyFactory: (networkId) => `networks_${networkId}`,
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as Network;
        network.networkAccesses = network.networkAccesses.map((item) =>
          item.networkId === result.networkId && item.accessId === result.accessId ? result : item,
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
    unknown // we change a list in a single item of Network, not list, so type does not work
  >(
    async (networkId, accessId, isRequired) =>
      await api.put(`/networks/${networkId}/accesses/${accessId}?IsRequired=${!isRequired}`, {}),
    {
      itemKeyFactory: (_, networkId, accessId) => `networks_${networkId}_accesses_${accessId}`,
      // listKeyFactory: (networkId) => `networks_${networkId}_accesses`,
      listKeyFactory: (networkId) => `networks_${networkId}`,
      listUpdater: (currentList, result) => {
        const network = currentList as unknown as Network;
        network.networkAccesses = network.networkAccesses.map((item) =>
          item.networkId === result.networkId && item.accessId === result.accessId ? result : item,
        );
        return network as unknown as unknown[];
      },
    },
  );

  const deleteNetworkAccess = useMutation<
    void,
    [networkId: string, accessId: string],
    unknown // we change a list in a single item of Network, not list, so type does not work
  >(
    async (networkId, accessId) => await api.delete(`/networks/${networkId}/accesses/${accessId}/`),
    {
      itemKeyFactory: (_, networkId, accessId) => `networks_${networkId}_accesses_${accessId}`,
      listKeyFactory: (networkId) => `networks_${networkId}`,
      listUpdater: (currentList, _, networkId, accessId) => {
        const network = currentList as unknown as Network;
        network.networkAccesses = network.networkAccesses.filter(
          (item) => item.networkId !== networkId && item.accessId !== accessId,
        );
        return network as unknown as unknown[];
      },
    },
  );

  return { fetchAccesses, createAccess, updateNetworkAccess, deleteNetworkAccess };
}
