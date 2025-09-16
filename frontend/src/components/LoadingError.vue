<template>
  <div v-if="loading" class="text-center py-12">
    <div class="spinner-border text-blue-500 mx-auto"></div>
    <p class="mt-4 text-gray-600">Loading...</p>
  </div>
  
  <div v-else-if="error" class="bg-red-50 p-6 rounded-lg text-red-700 mb-6">
    <h2 class="text-xl font-semibold mb-2">Error</h2>
    <p>{{ error }}</p>
    <button @click="$emit('buttonAction')" class="mt-4 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700">
      {{ buttonValue }}
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  loading: boolean;
  error: string | null;
  buttonValue: string;
}>();

defineEmits<{
  (e: 'buttonAction'): void;
}>();
</script>