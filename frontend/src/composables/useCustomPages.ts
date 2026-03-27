import api from '@/api/api.ts';
import type { CreateCustomPage, CreatePageBlock, CustomPage, PageBlock } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useCustomPages() {
  const fetchCustomPages = useCachedApi<CustomPage[], [networkId: string]>(
    (networkId) => `networks_${networkId}_customPages`,
    async (networkId) => await api.get<CustomPage[]>(`/networks/${networkId}/custompages`),
  );

  const fetchUserCustomPages = useCachedApi<CustomPage[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_customPages`,
    async (userId, userProxyId) => await api.get<CustomPage[]>(`/users/${userId}/proxies/${userProxyId}/customPages`),
  );

  const fetchCustomPage = useCachedApi<CustomPage, [networkId: string, customPageId: string]>(
    (networkId, customPageId) => `networks_${networkId}_customPages_${customPageId}`,
    async (networkId, customPageId) =>
      await api.get<CustomPage>(`/networks/${networkId}/custompages/${customPageId}`),
  );

  const createCustomPage = useMutation<CustomPage, [networkId: string, payload: CreateCustomPage]>(
    async (networkId, payload) => await api.post(`/networks/${networkId}/customPages/`, payload),
    {
      itemKeyFactory: (result, networkId) => `networks_${networkId}_customPages_${result.id}`,
      listKeyFactory: (networkId) => `networks_${networkId}_customPages`,
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateCustomPage = useMutation<
    CustomPage,
    [networkId: string, customPageId: string, payload: CreateCustomPage]
  >(
    async (networkId, customPageId, payload) =>
      await api.put<CustomPage, CreateCustomPage>(
        `/networks/${networkId}/customPages/${customPageId}`,
        payload,
      ),
    {
      itemKeyFactory: (_, networkId, customPageId) =>
        `networks_${networkId}_customPages_${customPageId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_customPages`,
      listUpdater: (currentList, result) => {
        return currentList.map((item) => (item.id == result.id ? result : item));
      },
    },
  );

  const deleteCustomPage = useMutation<void, [networkId: string, customPageId: string], CustomPage>(
    async (networkId, customPageId) =>
      await api.delete(`/networks/${networkId}/customPages/${customPageId}/`),
    {
      itemKeyFactory: (networkId, customPageId) =>
        `networks_${networkId}_customPages_${customPageId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_customPages`,
      listUpdater: (currentList, _, __, customPageId) => {
        return currentList.filter((item) => item.id !== customPageId);
      },
    },
  );

  const createPageBlock = useMutation<
    PageBlock,
    [networkId: string, customPageId: string, payload: CreatePageBlock],
    unknown
  >(
    async (networkId, customPageId, payload) =>
      await api.post(`/networks/${networkId}/customPages/${customPageId}/pageBlocks`, payload),
    {
      itemKeyFactory: (result, networkId, customPageId) =>
        `networks_${networkId}_customPages_${customPageId}_pageBlocks_${result.id}`,
      listKeyFactory: (networkId, customPageId) =>
        `networks_${networkId}_customPages_${customPageId}`,
      listUpdater: (currentList, result) => {
        const customPage = currentList as unknown as CustomPage;
        customPage.pages = [result, ...customPage.pages];
        return customPage as unknown as PageBlock[];
      },
    },
  );

  const updatePageBlock = useMutation<
    PageBlock,
    [networkId: string, customPageId: string, pageBlockId: string, payload: PageBlock],
    unknown
  >(
    async (networkId, customPageId, pageBlockId, payload) =>
      await api.put<PageBlock, PageBlock>(
        `/networks/${networkId}/customPages/${customPageId}/pageBlocks/${pageBlockId}`,
        payload,
      ),
      {
      itemKeyFactory: (_, networkId, customPageId, pageBlockId) =>
        `networks_${networkId}_customPages_${customPageId}_pageBlocks_${pageBlockId}`,
      listKeyFactory: (networkId, customPageId) =>
        `networks_${networkId}_customPages_${customPageId}`,
      listUpdater: (currentList, result) => {
        const customPage = currentList as unknown as CustomPage;
        customPage.pages = customPage.pages.map((item) => item.id === result.id ? result : item);
        return customPage as unknown as unknown[];
      },
      }
  );

  return {
    fetchCustomPages,
    fetchUserCustomPages,
    fetchCustomPage,
    createCustomPage,
    updateCustomPage,
    deleteCustomPage,
    createPageBlock,
    updatePageBlock,
  };
}
