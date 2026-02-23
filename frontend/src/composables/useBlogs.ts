import api from '@/api/api';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import { useCachedApi, useMutation } from './useApi';

export default function useBlogs() {
  const fetchBlogs = useCachedApi<Blog[], [networkId: string]>(
    (networkId) => `networks_${networkId}_blogs`,
    async (networkId) => await api.get<Blog[]>(`/networks/${networkId}/blogs`),
  );

  const fetchBlog = useCachedApi<Blog, [networkId: string, blogId: string]>(
    (networkId, blogId) => `networks_${networkId}_blogs_${blogId}`,
    async (networkId, blogId) => await api.get<Blog>(`/networks/${networkId}/blogs/${blogId}`),
  );

  const createBlog = useMutation<Blog, [networkId: string, payload: CreateBlog]>(
    async (networkId, payload) =>
      await api.post<Blog, CreateBlog>(`/networks/${networkId}/blogs/`, payload),
    {
      itemKeyFactory: (result, networkId) => `networks_${networkId}_blogs_${result.id}`,
      listKeyFactory: (networkId) => `networks_${networkId}_blogs`,
      listUpdater: (currentList, result) => {
        return currentList.map((item) => item.id == result.id ? result : item)
      }
    }
  );

  const deleteBlog = useMutation<void, [networkId: string, blogId: string], Blog>(
    async (networkId, blogId) => await api.delete(`/networks/${networkId}/blogs/${blogId}/`),
    {
      itemKeyFactory: (_, networkId, blogId) => `networks_${networkId}_blogs_${blogId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_blogs`,
      listUpdater: (currentList, _, __, blogId) => currentList.filter((item) => item.id !== blogId),
    }
  )

  return { fetchBlogs, fetchBlog, createBlog, deleteBlog };
}
