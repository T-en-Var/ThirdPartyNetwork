<template>
  <div
    v-if="showLoader"
    class="loading-state"
  >
    <LoadingSpinner size="lg" />
    <p class="loading-state__label">Loading...</p>
  </div>

  <div
    v-else-if="error"
    class="alert alert--error"
  >
    <h2 class="alert__title">Error</h2>
    <p class="alert__message">{{ error }}</p>
    <button
      v-if="buttonValue"
      @click="$emit('buttonAction')"
      class="btn btn--secondary"
    >
      {{ buttonValue }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import LoadingSpinner from './LoadingSpinner.vue';

const props = withDefaults(
  defineProps<{
    loading: boolean;
    error?: string | null;
    hasValue?: boolean;
    buttonValue?: string;
  }>(),
  {
    hasValue: false,
  },
);

defineEmits<{
  (e: 'buttonAction'): void;
}>();

const hasStarted = ref(false);

watch(
  () => props.loading,
  () => {
    hasStarted.value = true;
  },
);

const showLoader = computed(() => (!props.hasValue && !hasStarted.value) || props.loading);
</script>
