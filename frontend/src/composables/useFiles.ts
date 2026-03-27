import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { AxiosProgressEvent } from 'axios';
import type { UpdateFile, NetworkFile } from '@/types';
import { useCachedApi, useMutation } from './useApi';

export default function useFiles() {
  // We keep progress here because it's specific to this module's upload functionality.
  const progress: Ref<number> = ref(0);

  const fetchNetworkFiles = useCachedApi<NetworkFile[], [networkId: string]>(
    (networkId) => `networks_${networkId}_files`,
    async (networkId) => await api.get<NetworkFile[]>(`/networks/${networkId}/files/`),
  );

  const fetchUserFiles = useCachedApi<NetworkFile[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_files`,
    async (userId, userProxyId) =>
      await api.get<NetworkFile[]>(`/users/${userId}/proxies/${userProxyId}/files/`),
  );

  const fetchFile = useCachedApi<NetworkFile, [networkId: string, fileId: string]>(
    (networkId, fileId) => `networks_${networkId}_files_${fileId}`,
    async (networkId, fileId) =>
      await api.get<NetworkFile>(`/networks/${networkId}/files/${fileId}`),
  );

  const uploadFile = useMutation<
    NetworkFile,
    [networkId: string, fileToUpload: File, accessLevel?: number]
  >(
    async (networkId, fileToUpload, accessLevel = 0) => {
      progress.value = 0; // Reset progress on start

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
      itemKeyFactory: (result, networkId) => `networks_${networkId}_files_${result.id}`,

      listKeyFactory: (networkId) => `networks_${networkId}_files`,
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateFile = useMutation<
    NetworkFile,
    [networkId: string, fileId: string, payload: UpdateFile]
  >(
    async (networkId, fileId, payload) =>
      await api.put<NetworkFile, UpdateFile>(`/networks/${networkId}/files/${fileId}`, payload),
    {
      itemKeyFactory: (_, networkId, fileId) => `networks_${networkId}_files_${fileId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_files`,
      listUpdater: (currentList, result, __, fileId) =>
        currentList.map((item) => (item.id === fileId ? result : item)),
    },
  );

  const deleteFile = useMutation<void, [networkId: string, fileId: string], NetworkFile>(
    async (networkId, fileId) =>
      await api.delete<void>(`/networks/${networkId}/configurations/${fileId}`),
    {
      itemKeyFactory: (_, networkId, fileId) => `networks_${networkId}_files_${fileId}`,
      listKeyFactory: (networkId) => `networks_${networkId}_files`,
      listUpdater: (currentList, _, __, fileId) => currentList.filter((item) => item.id !== fileId),
    },
  );

  const resetProgress = () => {
    progress.value = 0;
  };

  return {
    progress,
    fetchNetworkFiles,
    fetchUserFiles,
    fetchFile,
    uploadFile,
    updateFile,
    deleteFile,
    resetProgress,
  };
}
