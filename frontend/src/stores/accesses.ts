import type { Access } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useAccessesStore = defineStore('accesses', () => {
  const accesses = ref<Access[]>([]);

  function setAccesses(newAccesses: Access[]) {
    accesses.value = newAccesses;
  }

  return {
    accesses,
    setAccesses,
  };
});
