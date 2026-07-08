import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { CreateCustomPage, CreatePageBlock, CustomPage, PageBlock } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';

export default function useCustomPages() {
  const fetchCustomPages = useCachedApi<CustomPage[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => userProxyKey(userId, userProxyId, 'customPages'),
    async (userId, userProxyId) =>
      await api.get<CustomPage[]>(`/users/${userId}/proxies/${userProxyId}/customPages`),
  );

  const fetchCustomPage = useCachedApi<
    CustomPage,
    [networkId: string, userId: string, userProxyId: string, customPageId: string]
  >(
    (_networkId, userId, userProxyId, customPageId) =>
      userProxyKey(userId, userProxyId, 'customPages', customPageId),
    async (networkId, _userId, _userProxyId, customPageId) =>
      await api.get<CustomPage>(`/networks/${networkId}/customPages/${customPageId}`),
  );

  const createCustomPage = useMutation<
    CustomPage,
    [networkId: string, userId: string, userProxyId: string, payload: CreateCustomPage]
  >(
    async (networkId, _userId, _userProxyId, payload) =>
      await api.post(`/networks/${networkId}/customPages/`, payload),
    {
      itemKeyFactory: (result, _networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'customPages', result.id),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'customPages'),
      listUpdater: prependItem,
    },
  );

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
    async (networkId, _userId, _userProxyId, customPageId, payload) =>
      await api.put<CustomPage, CreateCustomPage>(
        `/networks/${networkId}/customPages/${customPageId}`,
        payload,
      ),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'customPages'),
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
    async (networkId, _userId, _userProxyId, customPageId) =>
      await api.delete(`/networks/${networkId}/customPages/${customPageId}/`),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'customPages'),
      listUpdater: (currentList, _result, _networkId, _userId, _userProxyId, customPageId) => {
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
    async (networkId, _userId, _userProxyId, customPageId, payload) =>
      await api.post(`/networks/${networkId}/customPages/${customPageId}/pageBlocks`, payload),
    {
      itemKeyFactory: (result, _networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId) + `_pageBlocks_${result.id}`,
      listKeyFactory: (_networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId),
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
    async (networkId, _userId, _userProxyId, customPageId, pageBlockId, payload) =>
      await api.put<PageBlock, PageBlock>(
        `/networks/${networkId}/customPages/${customPageId}/pageBlocks/${pageBlockId}`,
        payload,
      ),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, customPageId, pageBlockId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId) + `_pageBlocks_${pageBlockId}`,
      listKeyFactory: (_networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId),
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
    async (networkId, _userId, _userProxyId, customPageId, pageBlockId) =>
      await api.delete(
        `/networks/${networkId}/customPages/${customPageId}/pageBlocks/${pageBlockId}`,
      ),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, customPageId, pageBlockId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId) + `_pageBlocks_${pageBlockId}`,
      listKeyFactory: (_networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId),
      listUpdater: (
        currentList,
        _result,
        _networkId,
        _userId,
        _userProxyId,
        _customPageId,
        pageBlockId,
      ) => {
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
