<template>
  <div class="w-full min-h-screen flex justify-center items-center bg-gray-100 relative p-5">
    <button
      class="absolute top-5 left-5 flex items-center gap-1 justify-center cursor-pointer text-sm text-slate-700 px-4 py-2 rounded bg-white shadow hover:bg-gray-100 hover:shadow-md transition"
      @click="goBack" aria-label="Go back to previous page or application">
      <span class="relative w-4 h-4" aria-hidden="true">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
          stroke-linecap="round" stroke-linejoin="round">
          <path d="M19 12H5" />
          <path d="M12 19l-7-7 7-7" />
        </svg>
      </span>
      <span>
        Back
      </span>
    </button>

    <div :class="`w-full max-w-${maxWidth}`">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  backUrl?: string;
  maxWidth?: string;
  currentStep: number;
}>();

const back = computed(() => props.backUrl ? atob(props.backUrl) : '/');
const emit = defineEmits(['go-back-step']);

const goBack = () => {
  if (props.currentStep === 1) {
    window.location.href = back.value;
  } else {
    emit('go-back-step');
  }
};
</script>