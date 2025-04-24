<template>
  <div class="fixed bottom-4 left-4 z-50">
    <transition
      enter-active-class="transition duration-500 ease-out"
      enter-from-class="transform translate-y-8 opacity-0"
      enter-to-class="transform translate-y-0 opacity-100"
      leave-active-class="transition duration-300 ease-in-out" 
      leave-from-class="transform translate-y-0 opacity-100"
      leave-to-class="transform translate-y-8 opacity-0"
    >
      <div 
        v-if="global.isFetching"
        class="rounded-lg bg-blue-700 px-5 py-3 text-sm font-medium text-white shadow-lg flex items-center space-x-2"
        :class="{ 'animate-bounce': isExiting }"
      >
        <svg class="animate-spin h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <span>Fetching data...</span>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { useGlobalStore } from '@/stores/global';
import { ref, watch } from 'vue';

const global = useGlobalStore();
const isExiting = ref(false);

// Add the jump animation before exiting
watch(() => global.isFetching, (newValue, oldValue) => {
  if (oldValue === true && newValue === false) {
    isExiting.value = true;
    setTimeout(() => {
      isExiting.value = false;
    }, 300); // Duration of bounce animation
  }
});
</script>