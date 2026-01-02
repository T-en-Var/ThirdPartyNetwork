import { ref } from 'vue'
import api from '@/api/api'
import type { Ref } from 'vue'
import type { AxiosError } from 'axios'
import type { Blog } from '@/types/userContent/blog'
import { useGlobalStore } from '@/stores/global'

export default function useBlogs() {
  const blogs: Ref<Blog[]> = ref([])
  const blog: Ref<Blog | null> = ref(null)
  const loading = ref(false)
  const error: Ref<string | null> = ref(null)
  const globalStore = useGlobalStore()

  const fetchBlogs = async (networkId: string) => {
    globalStore.startFetching()
    loading.value = true
    try {
      const response = await api.get<Blog[]>(`/networks/${networkId}/blogs`)
      blogs.value = response.data
    } catch (err) {
      error.value = (err as AxiosError).message
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  const fetchBlog = async (networkId: string, blogId: string) => {
    globalStore.startFetching()
    loading.value = true
    try {
      const response = await api.get<Blog>(`/networks/${networkId}/blogs/${blogId}`)
      blog.value = response.data
    } catch (err) {
      error.value = (err as AxiosError).message
    } finally {
      loading.value = false
      globalStore.stopFetching()
    }
  }

  return { blogs, blog, loading, error, fetchBlogs, fetchBlog }
}
