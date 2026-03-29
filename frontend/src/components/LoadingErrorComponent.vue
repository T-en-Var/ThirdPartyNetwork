<template>
  <div
    v-if="showLoader"
    class="py-12 text-center"
  >
    <LoadingSpinner size="lg" />
    <p class="mt-4 text-gray-600">Loading...</p>
  </div>

  <div
    v-else-if="error"
    class="mb-6 rounded-lg bg-red-50 p-6 text-red-700"
  >
    <h2 class="mb-2 text-xl font-semibold">Error</h2>
    <p>{{ error }}</p>
    <button
      v-if="buttonValue"
      @click="$emit('buttonAction')"
      class="mt-4 rounded-md bg-gray-600 px-4 py-2 text-white hover:bg-gray-700"
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
