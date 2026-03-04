<template>
  <div v-if="showLoader" class="text-center py-12">
    <LoadingSpinner size="lg" />
    <p class="mt-4 text-gray-600">Loading...</p>
  </div>

  <div v-else-if="error" class="bg-red-50 p-6 rounded-lg text-red-700 mb-6">
    <h2 class="text-xl font-semibold mb-2">Error</h2>
    <p>{{ error }}</p>
    <button v-if="buttonValue" @click="$emit('buttonAction')"
      class="mt-4 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700">
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
    hasValue: false
  }
);

defineEmits<{
  (e: 'buttonAction'): void;
}>();

const hasStarted = ref(false);

watch(() => props.loading, () => {
  hasStarted.value = true;
});

const showLoader = computed(() => !props.hasValue && !hasStarted.value || props.loading);
</script>
