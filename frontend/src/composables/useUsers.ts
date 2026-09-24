import api from '@/api/api';
import { globalCache, useCachedApi, useMutation } from './useApi';
import type { CreateUserDto, CreateUserProxyDto, UpdateUserDto, UserDto, UserMetricsDto, UserProxyDto, UserWithNetworksDto } from '@/types';

export default function useUsers() {
  const fetchUsers = useCachedApi<UserWithNetworksDto[], []>(
    () => `users`,
    async () => {
      const result = await api.get<UserWithNetworksDto[]>(`/users/`);
      return result;
    },
  );
  const forceFetchUsers = useCachedApi<UserWithNetworksDto[], []>(
    () => `users`,
    async () => {
      const result = await api.get<UserWithNetworksDto[]>(`/users/`);
      return result;
    },
    undefined,
    true,
  );

  const fetchUser = useCachedApi<UserWithNetworksDto, [userId: string]>(
    (userId) => `users_${userId}`,
    async (userId) => await api.get<UserWithNetworksDto>(`/users/${userId}/`),
    undefined,
    undefined,
    {
      initialData: (userId) => {
        const users = globalCache.get(`users`)?.data.value as UserWithNetworksDto[] | undefined;
        return users?.find((u) => u.id == userId) ?? null;
      },
    },
  );

  const fetchUsersMetrics = useCachedApi<UserMetricsDto[], []>(
    () => `users_metrics`,
    async () => {
      const result = await api.get<UserMetricsDto[]>(`/metrics/users/`);
      return result;
    },
  );

  const createUser = useMutation<UserDto, [payload: CreateUserDto]>(
    async (payload) => {
      const response = await api.post<UserDto, CreateUserDto>(`/users/`, payload);
      return response;
    },
    {
      itemKeyFactory: (result) => `users_${result.id}`,
      listKeyFactory: () => `users`,
      listUpdater: (currentList, result) => {
        const newList = [...currentList, result];
        return newList;
      },
    },
  );

  const createUserProxy = useMutation<UserProxyDto, [userId: string, payload: CreateUserProxyDto], UserDto>(
    async (userId, payload) => {
      return await api.post(`/users/${userId}/proxies/`, payload);
    },
    {
      itemKeyFactory: (result, userId) => `users_${userId}_proxies_${result.id}`,
      listKeyFactory: (userId) => `users_${userId}`,
      listUpdater: (currentList, result) => {
        const newList = currentList as unknown as UserDto;
        newList.userProxies = [...newList.userProxies, result];
        return newList as unknown as UserDto[];
      },
    },
  );

  const updateUser = useMutation<UserDto, [userId: string, payload: UpdateUserDto]>(
    async (userId, payload) => {
      const response = await api.put<UserDto, UpdateUserDto>(`/users/${userId}`, payload);
      return response;
    },
    {
      itemKeyFactory: (_, userId) => `users_${userId}`,
      listKeyFactory: () => `users`,
      listUpdater: (currentList, result) => {
        const newList = currentList.map((item) => (item.id === result.id ? result : item));
        return newList;
      },
    },
  );

  const deleteUser = useMutation<void, [userId: string], UserDto>(
    async (userId) => {
      return await api.delete(`/users/${userId}/`);
    },
    {
      itemKeyFactory: (_, userId) => `users_${userId}`,
      listKeyFactory: () => `users`,
      listUpdater: (currentList, _, userId) => {
        return currentList.filter((item) => item.id !== userId);
      },
    },
  );

  return {
    fetchUsers,
    forceFetchUsers,
    fetchUser,
    fetchUsersMetrics,
    createUser,
    updateUser,
    deleteUser,
    createUserProxy,
  };
}
