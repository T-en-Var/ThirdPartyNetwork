<template>
  <div class="auth-layout">
    <button
      class="auth-layout__back-btn"
      @click="goBack"
      aria-label="Go back to previous page or application"
    >
      <span
        class="auth-layout__back-icon"
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

    <div :class="['auth-layout__content', widthClasses[maxWidth]]">
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

const widthClasses = {
  sm: 'auth-layout__content--sm',
  md: 'auth-layout__content--md',
  lg: 'auth-layout__content--lg',
  xl: 'auth-layout__content--xl',
  '2xl': 'auth-layout__content--2xl',
} as const;

const goBack = () => {
  if (props.currentStep === 1) {
    window.location.href = safeAtob(props.backUrl) || '/';
  } else {
    emit('go-back-step');
  }
};
</script>
