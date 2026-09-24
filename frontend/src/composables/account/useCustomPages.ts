import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { CreateCustomPageDto, CreatePageBlockDto, CustomPageDto, PageBlockDto, UpdatePageBlockDto } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';

export default function useCustomPages() {
  const fetchCustomPages = useCachedApi<CustomPageDto[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => userProxyKey(userId, userProxyId, 'customPages'),
    async (userId, userProxyId) =>
      await api.get<CustomPageDto[]>(`/users/${userId}/proxies/${userProxyId}/customPages`),
  );

  const fetchCustomPage = useCachedApi<
    CustomPageDto,
    [networkId: string, userId: string, userProxyId: string, customPageId: string]
  >(
    (_networkId, userId, userProxyId, customPageId) =>
      userProxyKey(userId, userProxyId, 'customPages', customPageId),
    async (networkId, _userId, _userProxyId, customPageId) =>
      await api.get<CustomPageDto>(`/networks/${networkId}/customPages/${customPageId}`),
  );

  const createCustomPage = useMutation<
    CustomPageDto,
    [networkId: string, userId: string, userProxyId: string, payload: CreateCustomPageDto]
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
    CustomPageDto,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      payload: CreateCustomPageDto,
    ]
  >(
    async (networkId, _userId, _userProxyId, customPageId, payload) =>
      await api.put<CustomPageDto, CreateCustomPageDto>(
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
    CustomPageDto
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
    PageBlockDto,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      payload: CreatePageBlockDto,
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
        const customPage = currentList as unknown as CustomPageDto;
        customPage.blocks = [result, ...customPage.blocks];
        return customPage as unknown as PageBlockDto[];
      },
    },
  );

  const updatePageBlock = useMutation<
    PageBlockDto,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      customPageId: string,
      pageBlockId: string,
      payload: UpdatePageBlockDto,
    ],
    unknown
  >(
    async (networkId, _userId, _userProxyId, customPageId, pageBlockId, payload) =>
      await api.put<PageBlockDto, UpdatePageBlockDto>(
        `/networks/${networkId}/customPages/${customPageId}/pageBlocks/${pageBlockId}`,
        payload,
      ),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, customPageId, pageBlockId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId) +
        `_pageBlocks_${pageBlockId}`,
      listKeyFactory: (_networkId, userId, userProxyId, customPageId) =>
        userProxyKey(userId, userProxyId, 'customPages', customPageId),
      listUpdater: (currentList, result) => {
        const customPage = currentList as unknown as CustomPageDto;
        customPage.blocks = customPage.blocks.map((item) => (item.id === result.id ? result : item));
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
        userProxyKey(userId, userProxyId, 'customPages', customPageId) +
        `_pageBlocks_${pageBlockId}`,
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
        const customPage = currentList as unknown as CustomPageDto;
        customPage.blocks = customPage.blocks.filter((item) => item.id !== pageBlockId);
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
