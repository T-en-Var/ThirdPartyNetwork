<template>
  <div v-if="!hasStarted || loading" class="text-center py-12">
    <LoadingSpinner />
    <p class="mt-4 text-gray-600">Loading...</p>
  </div>

  <div v-else-if="error" class="bg-red-50 p-6 rounded-lg text-red-700 mb-6">
    <h2 class="text-xl font-semibold mb-2">Error</h2>
    <p>{{ error }}</p>
    <button v-if="buttonValue" @click="$emit('buttonAction')" class="mt-4 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700">
      {{ buttonValue }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import LoadingSpinner from './LoadingSpinner.vue';

const props = defineProps<{
  loading: boolean;
  error?: string;
  buttonValue?: string;
}>();

defineEmits<{
  (e: 'buttonAction'): void;
}>();

const hasStarted = ref(false);

watch(() => props.loading, () => {
  hasStarted.value = true
})
</script>

<style scoped>
.spinner-border {
  width: 3rem;
  height: 3rem;
  border: 3px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}
@keyframes spinner-border {
  100% {
    transform: rotate(360deg);
  }
}
</style>
