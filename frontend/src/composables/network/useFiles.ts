import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { AxiosProgressEvent } from 'axios';
import type { UpdateFile, NetworkFile } from '@/types';
import { networkKey } from '@/lib/cacheKeys';
import { prependItem, useCachedApi, useMutation } from '../useApi';

export default function useFiles() {
  const progress: Ref<number> = ref(0);

  const fetchFiles = useCachedApi<NetworkFile[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'files'),
    async (networkId) => await api.get<NetworkFile[]>(`/networks/${networkId}/files/`),
  );

  const fetchFile = useCachedApi<NetworkFile, [networkId: string, fileId: string]>(
    (networkId, fileId) => networkKey(networkId, 'files', fileId),
    async (networkId, fileId) =>
      await api.get<NetworkFile>(`/networks/${networkId}/files/${fileId}`),
  );

  const uploadFile = useMutation<
    NetworkFile,
    [networkId: string, fileToUpload: File, accessLevel?: number]
  >(
    async (networkId, fileToUpload, accessLevel = 0) => {
      progress.value = 0;

      const formData = new FormData();
      formData.append('file', fileToUpload);
      formData.append('accessLevel', accessLevel.toString());

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

      return await api.post<NetworkFile, FormData>(
        `/networks/${networkId}/files/`,
        formData,
        config,
      );
    },
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'files', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: prependItem,
    },
  );

  const updateFile = useMutation<
    NetworkFile,
    [networkId: string, fileId: string, payload: UpdateFile]
  >(
    async (networkId, fileId, payload) =>
      await api.put<NetworkFile, UpdateFile>(`/networks/${networkId}/files/${fileId}`, payload),
    {
      itemKeyFactory: (_, networkId, fileId) => networkKey(networkId, 'files', fileId),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: (currentList, result, __, fileId) =>
        currentList.map((item) => (item.id === fileId ? result : item)),
    },
  );

  const deleteFile = useMutation<void, [networkId: string, fileId: string], NetworkFile>(
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
