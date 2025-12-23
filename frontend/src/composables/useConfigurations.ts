import { ref } from 'vue'
import api from '@/api/api'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import type { Configuration, CreateConfiguration } from '@/types'
import { useGlobalStore } from '@/stores/global'

export default function useConfigurations() {
  const configurations: Ref<Configuration[]> = ref([])
  const configuration: Ref<Configuration | null> = ref(null)
  const loading: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)

  const globalStore = useGlobalStore()

  const insertConfiguration = (cfg: Configuration) => {
    const index = configurations.value.findIndex(c => c.id === cfg.id)
    if (index !== -1) configurations.value.splice(index, 1, cfg)
    else configurations.value.unshift(cfg)
  }

  const fetchNetworkConfigurations = async (networkId: string) => {
    loading.value = true
    globalStore.startFetching()
    try {
      const response = await api.get<Configuration[]>(`/networks/${networkId}/configurations`)
      configurations.value = response.data
    } catch (err) {
      error.value = (err as AxiosError).message
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const fetchConfiguration = async (networkId: string, configId: string) => {
    loading.value = true
    globalStore.startFetching()
    try {
      const response = await api.get<Configuration>(`/networks/${networkId}/configurations/${configId}`)
      configuration.value = response.data
      insertConfiguration(response.data)
    } catch (err) {
      error.value = (err as AxiosError).message
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const createConfiguration = async (networkId: string, payload: CreateConfiguration) => {
    loading.value = true
    globalStore.startFetching()
    try {
      const response = await api.post<Configuration, CreateConfiguration>(`/networks/${networkId}/configurations`, payload)
      configurations.value.unshift(response.data)
      return response.data
    } catch (err) {
      error.value = (err as AxiosError).message
      throw err
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const updateConfiguration = async (networkId: string, configurationId: string, payload: Partial<Configuration>) => {
    loading.value = true
    globalStore.startFetching()
    try {
      const response = await api.put<Configuration, Partial<Configuration>>(`/networks/${networkId}/configurations/${configurationId}`, payload)
      insertConfiguration(response.data)
      return response.data
    } catch (err) {
      error.value = (err as AxiosError).message
      throw err
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const deleteConfiguration = async (networkId: string, configurationId: string) => {
    loading.value = true
    globalStore.startFetching()
    try {
      await api.delete(`/networks/${networkId}/configurations/${configurationId}`)
      configurations.value = configurations.value.filter(c => c.id !== configurationId)
    } catch (err) {
      error.value = (err as AxiosError).message
      throw err
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  return {
    configurations,
    configuration,
    loading,
    error,
    insertConfiguration,
    fetchNetworkConfigurations,
    fetchConfiguration,
    createConfiguration,
    updateConfiguration,
    deleteConfiguration,
  }
}
