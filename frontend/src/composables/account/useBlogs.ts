import api from '@/api/api';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import { useCachedApi, useMutation } from '../useApi';

export default function useBlogs() {
  const fetchBlogs = useCachedApi<Blog[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_blogs`,
    async (userId, userProxyId) => await api.get<Blog[]>(`/users/${userId}/proxies/${userProxyId}/blogs`),
  );

  const fetchBlog = useCachedApi<Blog, [userId: string, userProxyId: string, blogId: string]>(
    (userId, userProxyId, blogId) => `users_${userId}_proxies_${userProxyId}_blogs_${blogId}`,
    async (userId, userProxyId, blogId) => await api.get<Blog>(`/users/${userId}/proxies/${userProxyId}/blogs/${blogId}`),
  );

  const createBlog = useMutation<Blog, [networkId: string, userId: string, userProxyId: string, payload: CreateBlog]>(
    async (networkId, _, __, payload) =>
      await api.post<Blog, CreateBlog>(`/networks/${networkId}/blogs/`, payload),
    {
      itemKeyFactory: (result, _, userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_blogs_${result.id}`,
      listKeyFactory: (_, userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_blogs`,
      listUpdater: (currentList, result) => {
        return currentList.map((item) => item.id == result.id ? result : item)
      }
    }
  );

  const deleteBlog = useMutation<void, [networkId: string, userId: string, userProxyId: string, blogId: string], Blog>(
    async (networkId, blogId) => await api.delete(`/networks/${networkId}/blogs/${blogId}/`),
    {
      itemKeyFactory: (_, __, userId, userProxyId, blogId) => `users_${userId}_proxies_${userProxyId}_blogs_${blogId}`,
      listKeyFactory: (_, userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_blogs`,
      listUpdater: (currentList, _, __, blogId) => currentList.filter((item) => item.id !== blogId),
    }
  )

  return { fetchBlogs, fetchBlog, createBlog, deleteBlog };
}
