import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useGlobalStore = defineStore('global', () => {
  const fetchCount = ref(0);

  const isFetching = computed(() => fetchCount.value > 0);

  function startFetching() {
    // console.trace("start");
    fetchCount.value += 1;
  }

  function stopFetching() {
    // console.trace("stop");
    setTimeout(() => {
      fetchCount.value = Math.max(0, fetchCount.value - 1);
    }, 250);
  }

  return {
    isFetching,
    startFetching,
    stopFetching,
  };
});
