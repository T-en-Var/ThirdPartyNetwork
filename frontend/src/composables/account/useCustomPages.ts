import api from '@/api/api.ts';
import type { CreateCustomPage, CreatePageBlock, CustomPage, PageBlock } from '@/types';
import { useCachedApi, useMutation } from '../useApi';

const getKey = (userId: string, userProxyId: string, entryId?: string) => {
  const entity = 'customPages';
  const base = `users_${userId}_proxies_${userProxyId}_${entity}`;
  if (entryId !== undefined) return base + `_${entryId}`;
  return base;
};

export default function useCustomPages() {
  const fetchCustomPages = useCachedApi<CustomPage[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => getKey(userId, userProxyId),
    async (userId, userProxyId) =>
      await api.get<CustomPage[]>(`/users/${userId}/proxies/${userProxyId}/customPages`),
  );

  const fetchCustomPage = useCachedApi<
    CustomPage,
    [networkId: string, userId: string, userProxyId: string, customPageId: string]
  >(
    (_, userId, userProxyId, customPageId) => getKey(userId, userProxyId, customPageId),
    async (networkId, _, __, customPageId) =>
      await api.get<CustomPage>(`/networks/${networkId}/custompages/${customPageId}`),
  );

  const createCustomPage = useMutation<
    CustomPage,
    [networkId: string, userId: string, userProxyId: string, payload: CreateCustomPage]
  >(async (networkId, payload) => await api.post(`/networks/${networkId}/customPages/`, payload), {
    itemKeyFactory: (result, _, userId, userProxyId) => getKey(userId, userProxyId, result.id),
    listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
    listUpdater: (currentList, result) => {
      return [result, ...currentList];
    },
  });

  const updateCustomPage = useMutation<
    CustomPage,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      payload: CreateCustomPage,
    ]
  >(
    async (networkId, _, __, customPageId, payload) =>
      await api.put<CustomPage, CreateCustomPage>(
        `/networks/${networkId}/customPages/${customPageId}`,
        payload,
      ),
    {
      itemKeyFactory: (_, __, userId, userProxyId, cpId) => getKey(userId, userProxyId, cpId),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, result) => {
        return currentList.map((item) => (item.id == result.id ? result : item));
      },
    },
  );

  const deleteCustomPage = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, customPageId: string],
    CustomPage
  >(
    async (networkId, _, __, customPageId) =>
      await api.delete(`/networks/${networkId}/customPages/${customPageId}/`),
    {
      itemKeyFactory: (_, userId, userProxyId, cpId) => getKey(userId, userProxyId, cpId),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, _, __, customPageId) => {
        return currentList.filter((item) => item.id !== customPageId);
      },
    },
  );

  const createPageBlock = useMutation<
    PageBlock,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      payload: CreatePageBlock,
    ],
    unknown
  >(
    async (networkId, _, __, customPageId, payload) =>
      await api.post(`/networks/${networkId}/customPages/${customPageId}/pageBlocks`, payload),
    {
      itemKeyFactory: (result, _, userId, userProxyId, customPageId) =>
        getKey(userId, userProxyId, customPageId) + `_pageBlocks_${result.id}`,
      listKeyFactory: (_, userId, userProxyId, customPageId) =>
        getKey(userId, userProxyId, customPageId),
      listUpdater: (currentList, result) => {
        const customPage = currentList as unknown as CustomPage;
        customPage.pages = [result, ...customPage.pages];
        return customPage as unknown as PageBlock[];
      },
    },
  );

  const updatePageBlock = useMutation<
    PageBlock,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      pageBlockId: string,
      payload: PageBlock,
    ],
    unknown
  >(
    async (networkId, _, __, customPageId, pageBlockId, payload) =>
      await api.put<PageBlock, PageBlock>(
        `/networks/${networkId}/customPages/${customPageId}/pageBlocks/${pageBlockId}`,
        payload,
      ),
    {
      itemKeyFactory: (_, __, userId, userProxyId, customPageId, pageBlockId) =>
        getKey(userId, userProxyId, customPageId) + `_pageBlocks_${pageBlockId}`,
      listKeyFactory: (_, userId, userProxyId, customPageId) =>
        getKey(userId, userProxyId, customPageId),
      listUpdater: (currentList, result) => {
        const customPage = currentList as unknown as CustomPage;
        customPage.pages = customPage.pages.map((item) => (item.id === result.id ? result : item));
        return customPage as unknown as unknown[];
      },
    },
  );

  const deletePageBlock = useMutation<
    void,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      pageBlockId: string,
    ],
    unknown
  >(
    async (networkId, _, __, customPageId, pageBlockId) =>
      await api.delete(
        `/networks/${networkId}/customPages/${customPageId}/pageBlocks/${pageBlockId}`,
      ),
    {
      itemKeyFactory: (_, __, userId, userProxyId, customPageId, pageBlockId) =>
        getKey(userId, userProxyId, customPageId) + `_pageBlocks_${pageBlockId}`,
      listKeyFactory: (_, userId, userProxyId, customPageId) =>
        getKey(userId, userProxyId, customPageId),
      listUpdater: (currentList, _, __, ___, ____, _____, pageBlockId) => {
        const customPage = currentList as unknown as CustomPage;
        customPage.pages = customPage.pages.filter((item) => item.id !== pageBlockId);
        return customPage as unknown as unknown[];
      },
    },
  );

  return {
    fetchCustomPages,
    fetchCustomPage,
    createCustomPage,
    updateCustomPage,
    deleteCustomPage,
    createPageBlock,
    updatePageBlock,
    deletePageBlock,
  };
}
