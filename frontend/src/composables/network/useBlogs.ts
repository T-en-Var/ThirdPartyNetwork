import api from '@/api/api';
import { networkKey } from '@/lib/cacheKeys';
import type { BlogDto, CreateBlogDto, UpdateBlogDto } from '@/types';
import { prependItem, removeById, useCachedApi, useMutation } from '../useApi';
import { useNetworkContentAccess } from '../useContentAccess';

export default function useBlogs() {
  const fetchBlogs = useCachedApi<BlogDto[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'blogs'),
    async (networkId) => await api.get<BlogDto[]>(`/networks/${networkId}/blogs`),
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
      listUpdater: (currentList, _, __, blogId) => removeById(currentList, blogId),
    },
  );

  const {
    publish: publishBlog,
    unpublish: unpublishBlog,
    setOwnership: setBlogOwnership,
    fetchAccessGrants: fetchBlogAccessGrants,
    createAccessGrant: createBlogAccessGrant,
    revokeAccessGrant: revokeBlogAccessGrant,
  } = useNetworkContentAccess<BlogDto>('blogs');

  return {
    fetchBlogs,
    fetchBlog,
    createBlog,
    updateBlog,
    deleteBlog,
    publishBlog,
    unpublishBlog,
    setBlogOwnership,
    fetchBlogAccessGrants,
    createBlogAccessGrant,
    revokeBlogAccessGrant,
  };
}
