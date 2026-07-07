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

    <div :class="['w-full', widthClasses[maxWidth]]">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import { safeAtob } from '@/lib/utils';

const props = withDefaults(
  defineProps<{
    backUrl?: string;
    maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl';
    currentStep: number;
  }>(),
  {
    maxWidth: 'md',
  },
);

const emit = defineEmits(['go-back-step']);

// Tailwind only generates classes it can see verbatim at build time,
// so the max-width variants must be spelled out.
const widthClasses = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-lg',
  xl: 'max-w-xl',
  '2xl': 'max-w-2xl',
} as const;

const goBack = () => {
  if (props.currentStep === 1) {
    window.location.href = safeAtob(props.backUrl) || '/';
  } else {
    emit('go-back-step');
  }
};
</script>
