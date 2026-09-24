import type {
  NetworkDto,
  NetworkEntitlementDto,
  NetworkMetricsDto,
  UpdateNetworkDto,
  SettableEntitlement,
} from '@/types';
import api from '@/api/api';
import { globalCache, useCachedApi, useMutation } from './useApi';

export default function useNetworks() {
  const fetchMainNetwork = useCachedApi<NetworkDto, []>(
    () => `mainNetwork`,
    async () => await api.get<NetworkDto>(`/mainNetwork`),
  );

  const fetchMainNetworkDetails = useCachedApi<NetworkDto, []>(
    () => `mainNetwork_details`,
    async () => await api.get<NetworkDto>(`/mainNetwork/details`),
  );

  const fetchNetworks = useCachedApi<NetworkDto[], []>(
    () => `networks`,
    async () => await api.get<NetworkDto[]>('/networks'),
  );

  const fetchNetworksMetrics = useCachedApi<NetworkMetricsDto[], []>(
    () => `networks_metrics`,
    async () => await api.get<NetworkMetricsDto[]>('/metrics/networks'),
  );

  const forceFetchNetworks = useCachedApi<NetworkDto[], []>(
    () => `networks`,
    async () => await api.get<NetworkDto[]>('/networks'),
    undefined,
    true,
  );

  const fetchNetwork = useCachedApi<NetworkDto, [networkId: string]>(
    (networkId) => `networks_${networkId}`,
    async (networkId) => await api.get<NetworkDto>(`/networks/${networkId}`),
    undefined,
    undefined,
    {
      initialData: (networkId) => {
        return globalCache.get(`networks_${networkId}`)?.data.value as NetworkDto | null;
      },
    },
  );

  const fetchNetworkDetails = useCachedApi<NetworkDto, [networkId: string]>(
    (networkId) => `networks_${networkId}_details`,
    async (networkId) => await api.get<NetworkDto>(`/networks/${networkId}/details`),
  );

  const updateNetwork = useMutation<NetworkDto, [networkId: string, payload: UpdateNetworkDto]>(
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
    NetworkEntitlementDto,
    [networkId: string, payload: SettableEntitlement],
    unknown
  >(async (networkId, payload) => await api.put(`/networks/${networkId}/entitlements`, payload), {
    itemKeyFactory: (_, networkId) => `networks_${networkId}_entitlement`,
    listKeyFactory: () => `networks`,
    listUpdater: (currentList, result, networkId) => {
      let networkList = currentList as unknown as NetworkDto[];
      networkList = networkList.map((item) => {
        if (item.id == result.networkId) {
          item.entitlement = result;
        }
        return item;
      });

      const mainNetwork = globalCache.get('mainNetwork_details');

      if (mainNetwork && networkId == (mainNetwork.data.value as NetworkDto).id) {
        (mainNetwork.data.value as NetworkDto).entitlement = result;
      }

      return networkList as unknown as unknown[];
    },
  });

  const deleteNetwork = useMutation<NetworkDto, [networkId: string], NetworkDto>(
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
