import api from '@/api/api.ts';
import type { CreateUser, User, UpdateUser, UserMetrics } from '@/types';
import { globalCache, useCachedApi, useMutation } from './useApi';

export default function useUsers() {
  const fetchUsers = useCachedApi<User[], []>(
    () => `users`,
    async () => {
      const result = await api.get<User[]>(`/users/`);
      return result;
    },
  );
  const forceFetchUsers = useCachedApi<User[], []>(
    () => `users`,
    async () => {
      const result = await api.get<User[]>(`/users/`);
      return result;
    },
    undefined,
    true
  );

  const fetchUser = useCachedApi<User, [userId: string]>(
    (userId) => `users_${userId}`,
    async (userId) => await api.get<User>(`/users/${userId}/`),
    undefined,
    undefined,
    {
      initialData: (userId) => {
        const users = globalCache.get(`users`)?.data.value as User[];
        return users.find((u) => u.id == userId) ?? null;
      },
    },
  );

  const fetchUsersMetrics = useCachedApi<UserMetrics[], []>(
    () => `users_metrics`,
    async () => {
      const result = await api.get<UserMetrics[]>(`/metrics/users/`);
      return result;
    },
  );

  const createUser = useMutation<User, [payload: CreateUser]>(
    async (payload) => {
      const response = await api.post<User, CreateUser>(`/users/`, payload);
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

  const updateUser = useMutation<User, [userId: string, payload: UpdateUser]>(
    async (userId, payload) => {
      const response = await api.put<User, UpdateUser>(`/users/${userId}`, payload);
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

  const deleteUser = useMutation<void, [userId: string], User>(
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
  };
}
