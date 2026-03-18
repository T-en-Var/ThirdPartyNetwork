import type { Network, NetworkEntitlement, NetworkMetrics, NetworkUpdate, SettableEntitlement } from '@/types';
import api from '@/api/api';
import { globalCache, useCachedApi, useMutation } from './useApi';

export default function useNetworks() {
  const fetchMainNetwork = useCachedApi<Network, []>(
    () => `mainNetwork`,
    async () => await api.get<Network>(`/mainNetwork`),
  );

  const fetchMainNetworkDetails = useCachedApi<Network, []>(
    () => `mainNetwork_details`,
    async () => await api.get<Network>(`/mainNetwork/details`),
  );

  const fetchNetworks = useCachedApi<Network[], []>(
    () => `networks`,
    async () => await api.get<Network[]>('/networks'),
  );

  const fetchNetworksMetrics = useCachedApi<NetworkMetrics[], []>(
    () => `networks_metrics`,
    async () => await api.get<NetworkMetrics[]>('/metrics/networks'),
  );

  const forceFetchNetworks = useCachedApi<Network[], []>(
    () => `networks`,
    async () => await api.get<Network[]>('/networks'),
    undefined,
    true,
  );

  const fetchNetwork = useCachedApi<Network, [networkId: string]>(
    (networkId) => `networks_${networkId}`,
    async (networkId) => {
      return await api.get<Network>(`/networks/${networkId}`);
    },
    undefined,
    undefined,
    {
      initialData: (networkId) => {
        return globalCache.get(`networks_${networkId}`)?.data.value as Network | null;
      },
    },
  );

  const fetchNetworkDetails = useCachedApi<Network, [networkId: string]>(
    (networkId) => `networks_${networkId}_details`,
    async (networkId) => await api.get<Network>(`/networks/${networkId}/details`),
  );

  const updateNetwork = useMutation<Network, [networkId: string, payload: NetworkUpdate]>(
    async (networkId, payload) => await api.put(`/networks/${networkId}/`, payload),
    {
      itemKeyFactory: (_, networkId) => `networks_${networkId}`,
      listKeyFactory: () => `networks`,
      listUpdater: (currentList, result) => {
        return [...currentList, result];
      },
    },
  );

  const updateNetworkEntitlement = useMutation<
    NetworkEntitlement,
    [networkId: string, payload: SettableEntitlement],
    unknown
  >(async (networkId, payload) => await api.put(`/networks/${networkId}/entitlements`, payload), {
    itemKeyFactory: (_, networkId) => `networks_${networkId}_entitlement`,
    listKeyFactory: () => `networks`,
    listUpdater: (currentList, result, networkId) => {
      let networkList = currentList as unknown as Network[];
      networkList = networkList.map((item) => {
        if (item.id == result.networkId) {
          item.entitlement = result;
        }
        return item;
      });

      const mainNetwork = globalCache.get('mainNetwork_details');

      if (mainNetwork && networkId == (mainNetwork.data.value as Network).id) {
        (mainNetwork.data.value as Network).entitlement = result;
      }

      return networkList as unknown as unknown[];
    },
  });

  const deleteNetwork = useMutation<Network, [networkId: string], Network>(
    async (networkId) => await api.delete(`/networks/${networkId}/`),
    {
      itemKeyFactory: (_, networkId) => `networks_${networkId}`,
      listKeyFactory: () => `networks`,
      listUpdater: (currentList, _, networkId) => {
        return currentList.filter((item) => item.id !== networkId);
      },
    },
  );

  return {
    fetchMainNetwork,
    fetchMainNetworkDetails,
    forceFetchNetworks,
    fetchNetworks,
    fetchNetworksMetrics,
    fetchNetwork,
    fetchNetworkDetails,
    updateNetworkEntitlement,
    updateNetwork,
    deleteNetwork,
  };
}
