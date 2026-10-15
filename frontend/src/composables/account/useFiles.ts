import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { AxiosProgressEvent } from 'axios';
import type { FileDto, UpdateFileDto } from '@/types';
import { networkKey } from '@/lib/cacheKeys';
import { prependItem, useCachedApi, useMutation } from '../useApi';

// Files always live under a network (there is no account-scoped storage), so every mutation
// here still needs a networkId and hits the same /networks/{networkId}/files routes as
// composables/network/useFiles.ts. This composable's own value-add is `fetchFiles`, which
// aggregates a user proxy's files across every network they belong to via the dedicated
// cross-network "authors" endpoint, something the network-scoped composable can't do.
export default function useFiles() {
  const progress: Ref<number> = ref(0);

  const fetchFiles = useCachedApi<FileDto[], [userProxyId: string]>(
    (userProxyId) => `authors_${userProxyId}_files`,
    async (userProxyId) => await api.get<FileDto[]>(`/authors/${userProxyId}/files`),
  );

  // Only this single-item fetch resolves a `viewingUrl` - the list endpoint above deliberately
  // leaves it null rather than minting a viewing link per row.
  const fetchFile = useCachedApi<FileDto, [networkId: string, fileId: string]>(
    (networkId, fileId) => networkKey(networkId, 'files', fileId),
    async (networkId, fileId) => await api.get<FileDto>(`/networks/${networkId}/files/${fileId}`),
  );

  const uploadFile = useMutation<FileDto, [networkId: string, fileToUpload: File]>(
    async (networkId, fileToUpload) => {
      progress.value = 0;

      const formData = new FormData();
      formData.append('file', fileToUpload);

      const config = {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (e: AxiosProgressEvent) => {
          if (e.total) {
            progress.value = Math.round((e.loaded * 100) / e.total);
          }
        },
      };

      return await api.post<FileDto, FormData>(`/networks/${networkId}/files/`, formData, config);
    },
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'files', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: prependItem,
    },
  );

  const updateFile = useMutation<FileDto, [networkId: string, fileId: string, payload: UpdateFileDto]>(
    async (networkId, fileId, payload) =>
      await api.put<FileDto, UpdateFileDto>(`/networks/${networkId}/files/${fileId}`, payload),
    {
      itemKeyFactory: (_, networkId, fileId) => networkKey(networkId, 'files', fileId),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: (currentList, result, __, fileId) =>
        currentList.map((item) => (item.id === fileId ? result : item)),
    },
  );

  const deleteFile = useMutation<void, [networkId: string, fileId: string], FileDto>(
    async (networkId, fileId) => await api.delete<void>(`/networks/${networkId}/files/${fileId}`),
    {
      itemKeyFactory: (_, networkId, fileId) => networkKey(networkId, 'files', fileId),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: (currentList, _, __, fileId) => currentList.filter((item) => item.id !== fileId),
    },
  );

  const resetProgress = () => {
    progress.value = 0;
  };

  return {
    progress,
    fetchFiles,
    fetchFile,
    uploadFile,
    updateFile,
    deleteFile,
    resetProgress,
  };
}
