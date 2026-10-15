import { ref } from 'vue';
import api from '@/api/api';
import type { Ref } from 'vue';
import type { AxiosProgressEvent } from 'axios';
import type {
  FileDto,
  UpdateFileDto,
  OwnershipMode,
  ContentAccessGrantDto,
  CreateContentAccessGrantDto,
} from '@/types';
import { networkKey } from '@/lib/cacheKeys';
import { prependItem, removeById, replaceById, useCachedApi, useMutation } from '../useApi';

export default function useFiles() {
  const progress: Ref<number> = ref(0);

  const fetchFiles = useCachedApi<FileDto[], [networkId: string]>(
    (networkId) => networkKey(networkId, 'files'),
    async (networkId) => await api.get<FileDto[]>(`/networks/${networkId}/files/`),
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

  const publishFile = useMutation<FileDto, [networkId: string, fileId: string]>(
    async (networkId, fileId) =>
      await api.post<FileDto, undefined>(`/networks/${networkId}/files/${fileId}/publish`),
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'files', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: replaceById,
    },
  );

  const unpublishFile = useMutation<FileDto, [networkId: string, fileId: string]>(
    async (networkId, fileId) =>
      await api.post<FileDto, undefined>(`/networks/${networkId}/files/${fileId}/unpublish`),
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'files', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: replaceById,
    },
  );

  const setFileOwnership = useMutation<FileDto, [networkId: string, fileId: string, mode: OwnershipMode]>(
    async (networkId, fileId, mode) =>
      await api.post<FileDto, { mode: OwnershipMode }>(`/networks/${networkId}/files/${fileId}/ownership`, {
        mode,
      }),
    {
      itemKeyFactory: (result, networkId) => networkKey(networkId, 'files', result.id),
      listKeyFactory: (networkId) => networkKey(networkId, 'files'),
      listUpdater: replaceById,
    },
  );

  const fetchFileAccessGrants = useCachedApi<ContentAccessGrantDto[], [networkId: string, fileId: string]>(
    (networkId, fileId) => networkKey(networkId, 'files', fileId, 'access'),
    async (networkId, fileId) =>
      await api.get<ContentAccessGrantDto[]>(`/networks/${networkId}/files/${fileId}/access`),
  );

  const createFileAccessGrant = useMutation<
    ContentAccessGrantDto,
    [networkId: string, fileId: string, payload: CreateContentAccessGrantDto]
  >(
    async (networkId, fileId, payload) =>
      await api.post<ContentAccessGrantDto, CreateContentAccessGrantDto>(
        `/networks/${networkId}/files/${fileId}/access`,
        payload,
      ),
    {
      listKeyFactory: (networkId, fileId) => networkKey(networkId, 'files', fileId, 'access'),
      listUpdater: prependItem,
    },
  );

  const revokeFileAccessGrant = useMutation<
    void,
    [networkId: string, fileId: string, grantId: string],
    ContentAccessGrantDto
  >(
    async (networkId, fileId, grantId) =>
      await api.delete<void>(`/networks/${networkId}/files/${fileId}/access/${grantId}`),
    {
      listKeyFactory: (networkId, fileId) => networkKey(networkId, 'files', fileId, 'access'),
      listUpdater: (currentList, _, __, ___, grantId) => removeById(currentList, grantId),
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
    publishFile,
    unpublishFile,
    setFileOwnership,
    fetchFileAccessGrants,
    createFileAccessGrant,
    revokeFileAccessGrant,
    resetProgress,
  };
}
