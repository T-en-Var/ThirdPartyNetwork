import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { BlogDto, CreateBlogDto, UpdateBlogDto } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';

// Blogs always live under a network (there is no account-scoped storage), so every mutation here
// still needs a networkId and hits the same /networks/{networkId}/blogs routes as
// composables/network/useBlogs.ts. This composable's own value-add is `fetchBlogs`, which
// aggregates a user proxy's blogs across every network they belong to via the dedicated
// cross-network "authors" endpoint, something the network-scoped composable can't do.
export default function useBlogs() {
  const fetchBlogs = useCachedApi<BlogDto[], [userProxyId: string]>(
    (userProxyId) => `authors_${userProxyId}_blogs`,
    async (userProxyId) => await api.get<BlogDto[]>(`/authors/${userProxyId}/blogs`),
  );

  const fetchBlog = useCachedApi<BlogDto, [networkId: string, blogId: string]>(
    (networkId, blogId) => networkKey(networkId, 'blogs', blogId),
    async (networkId, blogId) => await api.get<BlogDto>(`/networks/${networkId}/blogs/${blogId}`),
  );

  const createBlog = useMutation<BlogDto, [networkId: string, payload: CreateBlogDto]>(
    async (networkId, payload) =>
      await api.post<BlogDto, CreateBlogDto>(`/networks/${networkId}/blogs/`, payload),
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'blogs', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'blogs'),
      listUpdater: prependItem,
    },
  );

  const updateBlog = useMutation<BlogDto, [networkId: string, blogId: string, payload: UpdateBlogDto]>(
    async (networkId, blogId, payload) =>
      await api.put<BlogDto, UpdateBlogDto>(`/networks/${networkId}/blogs/${blogId}`, payload),
    {
      itemKeyFactory: (_, networkId, blogId) => networkKey(networkId, 'blogs', blogId),
      listKeyFactory: (networkId) => networkKey(networkId, 'blogs'),
      listUpdater: (currentList, result, __, blogId) =>
        currentList.map((item) => (item.id === blogId ? result : item)),
    },
  );

  const deleteBlog = useMutation<void, [networkId: string, blogId: string], BlogDto>(
    async (networkId, blogId) => await api.delete(`/networks/${networkId}/blogs/${blogId}/`),
    {
      itemKeyFactory: (_, networkId, blogId) => networkKey(networkId, 'blogs', blogId),
      listKeyFactory: (networkId) => networkKey(networkId, 'blogs'),
      listUpdater: (currentList, _, __, blogId) => currentList.filter((item) => item.id !== blogId),
    },
  );

  return { fetchBlogs, fetchBlog, createBlog, updateBlog, deleteBlog };
}
