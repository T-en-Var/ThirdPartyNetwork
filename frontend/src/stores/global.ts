import type { NewToast, Toast, ToastList } from '@/types';
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useGlobalStore = defineStore('global', () => {
  const fetchCount = ref(0);
  const activeToasts = ref<ToastList>([]);

  const isFetching = computed(() => fetchCount.value > 0);

  function startFetching() {
    fetchCount.value += 1;
  }

  function stopFetching() {
    setTimeout(() => {
      fetchCount.value = Math.max(0, fetchCount.value - 1);
    }, 400);
  }

  function addToast(toast: NewToast) {
    const t: Toast = {
      id: Date.now().toString(),
      ...toast,
    };
    activeToasts.value.push(t);
    return t.id;
  }

  function removeToast(toastId: string) {
    activeToasts.value = activeToasts.value.filter((t) => t.id !== toastId);
  }

  return {
    isFetching,
    startFetching,
    stopFetching,

    addToast,
    removeToast,
    activeToasts,
  };
});
