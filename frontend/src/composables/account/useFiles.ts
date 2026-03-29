import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { NetworkFile, UpdateFile } from '@/types';
import { useCachedApi, useMutation } from '../useApi';
import type { AxiosProgressEvent } from 'axios';

const getKey = (userId: string, userProxyId: string, entryId?: string) => {
  const entity = 'files';
  const base = `users_${userId}_proxies_${userProxyId}_${entity}`;
  if (entryId !== undefined) return base + `_${entryId}`;
  return base;
};

export default function useFiles() {
  const progress: Ref<number> = ref(0);

  const fetchFiles = useCachedApi<NetworkFile[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => getKey(userId, userProxyId),
    async (userId, userProxyId) =>
      await api.get<NetworkFile[]>(`/users/${userId}/proxies/${userProxyId}/files/`),
  );

  const fetchFile = useCachedApi<
    NetworkFile,
    [userId: string, userProxyId: string, fileId: string]
  >(
    (userId, userProxyId, fileId) => getKey(userId, userProxyId, fileId),
    async (userId, userProxyId, fileId) =>
      await api.get<NetworkFile>(`/users/${userId}/proxies/${userProxyId}/files/${fileId}`),
  );

  const uploadFile = useMutation<
    NetworkFile,
    [
      networkId: string,
      userId: string,
      userProxyId: string,
      fileToUpload: File,
      accessLevel?: number,
    ]
  >(
    async (networkId, _, __, fileToUpload, accessLevel = 0) => {
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
      itemKeyFactory: (result, _, userId, userProxyId) => getKey(userId, userProxyId, result.id),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, result) => {
        return [result, ...currentList];
      },
    },
  );

  const updateFile = useMutation<
    NetworkFile,
    [networkId: string, userId: string, userProxyId: string, fileId: string, payload: UpdateFile]
  >(
    async (networkId, _, __, fileId, payload) =>
      await api.put<NetworkFile, UpdateFile>(`/networks/${networkId}/files/${fileId}`, payload),
    {
      itemKeyFactory: (_, __, userId, userProxyId, fileId) => getKey(userId, userProxyId, fileId),
      listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
      listUpdater: (currentList, result, _, __, ___, fileId) =>
        currentList.map((item) => (item.id === fileId ? result : item)),
    },
  );

  const deleteFile = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, fileId: string],
    NetworkFile
  >(async (networkId, fileId) => await api.delete<void>(`/networks/${networkId}/files/${fileId}`), {
    itemKeyFactory: (_, __, userId, userProxyId, fileId) => getKey(userId, userProxyId, fileId),
    listKeyFactory: (_, userId, userProxyId) => getKey(userId, userProxyId),
    listUpdater: (currentList, _, __, ___, ____, fileId) =>
      currentList.filter((item) => item.id !== fileId),
  });

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
