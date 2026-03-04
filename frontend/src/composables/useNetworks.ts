import type { Network, NetworkUpdate } from '@/types';
import api from '@/api/api';
import { useCachedApi, useMutation } from './useApi';

export default function useNetworks() {
  const fetchMainNetwork = useCachedApi<Network, []>(
    () => `mainNetwork`,
    async () => await api.get<Network>(`/mainNetwork`),
  );

  const fetchNetworks = useCachedApi<Network[], []>(
    () => `networks`,
    async () => await api.get<Network[]>('/networks'),
  );
  
  const fetchNetwork = useCachedApi<Network, [networkId: string]>(
    (networkId) => `networks_${networkId}`,
    async (networkId) => {
      console.log("fetching")
      return await api.get<Network>(`/networks/${networkId}`)
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
    }
  )

  return { fetchMainNetwork, fetchNetworks, fetchNetwork, fetchNetworkDetails, updateNetwork };
}