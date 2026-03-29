<template>
  <div class="relative flex min-h-screen w-full items-center justify-center bg-gray-100 p-5">
    <button
      class="absolute left-5 top-5 flex cursor-pointer items-center justify-center gap-1 rounded bg-white px-4 py-2 text-sm text-slate-700 shadow transition hover:bg-gray-100 hover:shadow-md"
      @click="goBack"
      aria-label="Go back to previous page or application"
    >
      <span
        class="relative h-4 w-4"
        aria-hidden="true"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <path d="M19 12H5" />
          <path d="M12 19l-7-7 7-7" />
        </svg>
      </span>
      <span> Back </span>
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

const back = computed(() => (props.backUrl ? atob(props.backUrl) : '/'));
const emit = defineEmits(['go-back-step']);

const goBack = () => {
  if (props.currentStep === 1) {
    window.location.href = back.value;
  } else {
    emit('go-back-step');
  }
};
</script>
