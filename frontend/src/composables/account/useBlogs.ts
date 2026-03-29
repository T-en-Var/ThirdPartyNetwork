import api from '@/api/api';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import { useCachedApi, useMutation } from '../useApi';

const getKey = (userId: string, userProxyId: string, entryId?: string) => {
  const entity = 'blogs';
  const base = `users_${userId}_proxies_${userProxyId}_${entity}`;
  if (entryId !== undefined) return base + `_${entryId}`;
  return base;
};

export default function useBlogs() {
  const fetchBlogs = useCachedApi<Blog[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => getKey(userId, userProxyId),
    async (userId, userProxyId) =>
      await api.get<Blog[]>(`/users/${userId}/proxies/${userProxyId}/blogs`),
  );

  const fetchBlog = useCachedApi<Blog, [userId: string, userProxyId: string, blogId: string]>(
    (userId, userProxyId, blogId) => getKey(userId, userProxyId, blogId),
    async (userId, userProxyId, blogId) =>
      await api.get<Blog>(`/users/${userId}/proxies/${userProxyId}/blogs/${blogId}`),
  );

  const createBlog = useMutation<
    Blog,
    [networkId: string, userId: string, userProxyId: string, payload: CreateBlog]
  >(
    async (networkId, _, __, payload) =>
      await api.post<Blog, CreateBlog>(`/networks/${networkId}/blogs/`, payload),
    {
      itemKeyFactory: (result, _, userId, userProxyId) => getKey(userId, userProxyId, result.id),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, result) => {
        return currentList.map((item) => (item.id == result.id ? result : item));
      },
    },
  );

  const deleteBlog = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, blogId: string],
    Blog
  >(async (networkId, blogId) => await api.delete(`/networks/${networkId}/blogs/${blogId}/`), {
    itemKeyFactory: (_, __, userId, userProxyId, blogId) => getKey(userId, userProxyId, blogId),
    listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
    listUpdater: (currentList, _, __, blogId) => currentList.filter((item) => item.id !== blogId),
  });

  return { fetchBlogs, fetchBlog, createBlog, deleteBlog };
}
