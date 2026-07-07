import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { NetworkFile, UpdateFile } from '@/types';
import { prependItem, useCachedApi, useMutation } from '../useApi';
import { userProxyKey } from '@/lib/cacheKeys';
import type { AxiosProgressEvent } from 'axios';

export default function useFiles() {
  const progress: Ref<number> = ref(0);

  const fetchFiles = useCachedApi<NetworkFile[], [userId: string, userProxyId: string]>(
    (userId, userProxyId) => userProxyKey(userId, userProxyId, 'files'),
    async (userId, userProxyId) =>
      await api.get<NetworkFile[]>(`/users/${userId}/proxies/${userProxyId}/files/`),
  );

  const fetchFile = useCachedApi<
    NetworkFile,
    [networkId: string, userId: string, userProxyId: string, fileId: string]
  >(
    (_networkId, userId, userProxyId, fileId) => userProxyKey(userId, userProxyId, 'files', fileId),
    async (_networkId, userId, userProxyId, fileId) =>
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
    async (networkId, _userId, _userProxyId, fileToUpload, accessLevel = 0) => {
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
      itemKeyFactory: (result, _networkId, userId, userProxyId) =>
        userProxyKey(userId, userProxyId, 'files', result.id),
      listKeyFactory: (_networkId, userId, userProxyId) => userProxyKey(userId, userProxyId, 'files'),
      listUpdater: prependItem,
    },
  );

  const updateFile = useMutation<
    NetworkFile,
    [networkId: string, userId: string, userProxyId: string, fileId: string, payload: UpdateFile]
  >(
    async (networkId, _userId, _userProxyId, fileId, payload) =>
      await api.put<NetworkFile, UpdateFile>(`/networks/${networkId}/files/${fileId}`, payload),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, fileId) =>
        userProxyKey(userId, userProxyId, 'files', fileId),
      listKeyFactory: (_networkId, userId, userProxyId) => userProxyKey(userId, userProxyId, 'files'),
      listUpdater: (currentList, result, _networkId, _userId, _userProxyId, fileId) =>
        currentList.map((item) => (item.id === fileId ? result : item)),
    },
  );

  const deleteFile = useMutation<
    void,
    [networkId: string, userId: string, userProxyId: string, fileId: string],
    NetworkFile
  >(
    async (networkId, _userId, _userProxyId, fileId) =>
      await api.delete<void>(`/networks/${networkId}/files/${fileId}`),
    {
      itemKeyFactory: (_result, _networkId, userId, userProxyId, fileId) =>
        userProxyKey(userId, userProxyId, 'files', fileId),
      listKeyFactory: (_networkId, userId, userProxyId) => userProxyKey(userId, userProxyId, 'files'),
      listUpdater: (currentList, _result, _networkId, _userId, _userProxyId, fileId) =>
        currentList.filter((item) => item.id !== fileId),
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
