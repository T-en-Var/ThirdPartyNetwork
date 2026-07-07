import api from '@/api/api';
import { userProxyKey } from '@/lib/cacheKeys';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import { removeById, replaceById, useCachedApi, useMutation } from '../useApi';

export default function useBlogs() {
  const fetchBlogs = useCachedApi<Blog[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => userProxyKey(userId, userProxyId, 'blogs'),
    async (userId, userProxyId) =>
      await api.get<Blog[]>(`/users/${userId}/proxies/${userProxyId}/blogs`),
  );

  const fetchBlog = useCachedApi<
    Blog,
    [networkId: string, userId: string, userProxyId: string, blogId: string]
  >(
    (_networkId, userId, userProxyId, blogId) => userProxyKey(userId, userProxyId, 'blogs', blogId),
    async (networkId, _userId, _userProxyId, blogId) =>
      await api.get<Blog>(`/networks/${networkId}/blogs/${blogId}`),
  );

  const createBlog = useMutation<
    Blog,
    [networkId: string, userId: string, userProxyId: string, payload: CreateBlog]
  >(
    async (networkId, _, __, payload) =>
      await api.post<Blog, CreateBlog>(`/networks/${networkId}/blogs/`, payload),
    {
      itemKeyFactory: (result, _, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'blogs', result.id),
      listKeyFactory: (_, userId, userProxyId) => userProxyKey(userId, userProxyId, 'blogs'),
      listUpdater: replaceById,
    },
  );

  const deleteBlog = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, blogId: string],
    Blog
  >(
    async (networkId, _userId, _userProxyId, blogId) =>
      await api.delete(`/networks/${networkId}/blogs/${blogId}/`),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, blogId) =>
        userProxyKey(userId, userProxyId, 'blogs', blogId),
      listKeyFactory: (_networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'blogs'),
      listUpdater: (currentList, _result, _networkId, _userId, _userProxyId, blogId) =>
        removeById(currentList, blogId),
    },
  );

  return { fetchBlogs, fetchBlog, createBlog, deleteBlog };
}
