import { ref } from 'vue'
import api from '@/api/api.ts'
import type { Ref } from 'vue'
import type { AxiosError, AxiosProgressEvent } from 'axios'
import type { NetworkFile, ErrorMessage } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useFiles() {
  const files = ref<NetworkFile[]>([])
  const file: Ref<NetworkFile | null> = ref(null)
  const loading: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)
  const progress: Ref<number> = ref(0)

  const globalStore = useGlobalStore()

  const insertFile = (networkFile: NetworkFile) => {
    const index = files.value.findIndex(f => f.id === networkFile.id)

    if (index !== -1) files.value.splice(index, 1, networkFile)
    else files.value.unshift(networkFile)
  }

  const fetchNetworkFiles = async (networkId: string) => {
    loading.value = true

    if (files.value.length > 0) {
      loading.value = false
      return
    }

    globalStore.startFetching()
    try {
      const response = await api.get<NetworkFile[]>(`/networks/${networkId}/files/`)
      files.value = response.data
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to fetch files.'
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const fetchUserFiles = async (userId: string, userProxyId: string) => {
    loading.value = true

    if (files.value.length > 0) {
      loading.value = false
      return
    }

    globalStore.startFetching()
    try {
      const response = await api.get<NetworkFile[]>(`/users/${userId}/proxies/${userProxyId}/files/`)
      files.value = response.data
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to fetch files.'
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const fetchFile = async (networkId: string, fileId: string) => {
    loading.value = true
    globalStore.startFetching()

    try {
      const response = await api.get<NetworkFile>(`/networks/${networkId}/files/${fileId}`)
      file.value = response.data

      insertFile(response.data)
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to fetch file.'
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const uploadFile = async (networkId: string, fileToUpload: File, accessLevel: number = 0) => {
    loading.value = true
    progress.value = 0
    error.value = null
    file.value = null

    const formData = new FormData()
    formData.append('file', fileToUpload)
    formData.append('accessLevel', accessLevel.toString())

    const config = {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: (e: AxiosProgressEvent) => {
        if (e.total) {
          progress.value = Math.round((e.loaded * 100) / e.total)
        }
      }
    }

    globalStore.startFetching()
    try {
      const response = await api.post<NetworkFile, typeof formData>(`/networks/${networkId}/files/`, formData, config)
      file.value = response.data

      // Update list of files with new one
      files.value.unshift(response.data)
    } catch (err) {
      error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'File upload failed.'
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const reset = () => {
    file.value = null
    error.value = null
    progress.value = 0
  }

  return {
    files,
    file,
    loading,
    error,
    insertFile,
    progress,
    fetchNetworkFiles,
    fetchUserFiles,
    fetchFile,
    uploadFile,
    reset,
  }
}
