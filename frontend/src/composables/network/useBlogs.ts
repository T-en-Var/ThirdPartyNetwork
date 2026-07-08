import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import { removeById, replaceById, useCachedApi, useMutation } from '../useApi';

export default function useBlogs() {
  const fetchBlogs = useCachedApi<Blog[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'blogs'),
    async (networkId) => await api.get<Blog[]>(`/networks/${networkId}/blogs`),
  );

  const fetchBlog = useCachedApi<Blog, [networkId: string, blogId: string]>(
    (networkId, blogId) => networkKey(networkId, 'blogs', blogId),
    async (networkId, blogId) => await api.get<Blog>(`/networks/${networkId}/blogs/${blogId}`),
  );

  const createBlog = useMutation<Blog, [networkId: string, payload: CreateBlog]>(
    async (networkId, payload) =>
      await api.post<Blog, CreateBlog>(`/networks/${networkId}/blogs/`, payload),
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'blogs', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'blogs'),
      listUpdater: replaceById,
    },
  );

  const deleteBlog = useMutation<void, [networkId: string, blogId: string], Blog>(
    async (networkId, blogId) => await api.delete(`/networks/${networkId}/blogs/${blogId}/`),
    {
      itemKeyFactory: (_, networkId, blogId) => networkKey(networkId, 'blogs', blogId),
      listKeyFactory: (networkId) => networkKey(networkId, 'blogs'),
      listUpdater: (currentList, _, __, blogId) => removeById(currentList, blogId),
    },
  );

  return { fetchBlogs, fetchBlog, createBlog, deleteBlog };
}
