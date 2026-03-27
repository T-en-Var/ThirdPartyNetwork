import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { NetworkFile, UpdateFile } from '@/types';
import { useCachedApi, useMutation } from '../useApi';
import type { AxiosProgressEvent } from 'axios';

export default function useFiles() {
  // We keep progress here because it's specific to this module's upload functionality.
  const progress: Ref<number> = ref(0);

  const fetchFiles = useCachedApi<NetworkFile[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_files`,
    async (userId, userProxyId) =>
      await api.get<NetworkFile[]>(`/users/${userId}/proxies/${userProxyId}/files/`),
  );

  const fetchFile = useCachedApi<NetworkFile, [userId: string, userProxyId: string, fileId: string]>(
    (userId, userProxyId, fileId) => `users_${userId}_proxies_${userProxyId}_files_${fileId}`,
    async (userId, userProxyId, fileId) =>
      await api.get<NetworkFile>(`/users/${userId}/proxies/${userProxyId}/files/${fileId}`),
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
      itemKeyFactory: (result, networkId) => `networks_${networkId}_files_${result.id}`,

      listKeyFactory: (networkId) => `networks_${networkId}_files`,
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateFile = useMutation<NetworkFile, [networkId: string, userId: string, userProxyId: string, fileId: string, payload: UpdateFile]>(
    async (networkId, _, __, fileId, payload) =>
      await api.put<NetworkFile, UpdateFile>(`/networks/${networkId}/files/${fileId}`, payload),
    {
      itemKeyFactory: (_, __, userId, userProxyId, fileId) => `users_${userId}_proxies_${userProxyId}_files_${fileId}`,
      listKeyFactory: (_, userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_files`,
      listUpdater: (currentList, result, _, __, ___, fileId) =>
        currentList.map((item) => (item.id === fileId ? result : item)),
    },
  );

  const deleteFile = useMutation<void, [networkId: string, userId: string, userProxyId: string, fileId: string], NetworkFile>(
    async (networkId, fileId) =>
      await api.delete<void>(`/networks/${networkId}/files/${fileId}`),
    {
      itemKeyFactory: (_, __, userId, userProxyId, fileId) => `users_${userId}_proxies_${userProxyId}_files_${fileId}`,
      listKeyFactory: (_, userId, userProxyId) => `users_${userId}_proxies_${userProxyId}_files`,
      listUpdater: (currentList, _, __, ___, ____, fileId) => currentList.filter((item) => item.id !== fileId),
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
