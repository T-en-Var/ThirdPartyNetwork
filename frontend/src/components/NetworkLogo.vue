<template>
  <div class="w-14 h-14 bg-white rounded-md shadow flex items-center justify-center p-2">
    <div v-if="loading"
      class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin"
      role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" aria-label="Loading logo">
    </div>

    <CloudinaryFile v-else-if="imageFile" :display-only="true" :file="imageFile"
      class="w-full h-full object-contain" alt="Network Logo" />

    <div v-else class="w-full h-full flex items-center justify-center">
      <img
        :src="`https://ui-avatars.com/api/?name=${networkName}&size=24&background=random`"
        :alt="`${networkName} logo`"
        class="max-w-full max-h-full rounded-md object-contain" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NetworkFile } from '@/types';
import CloudinaryFile from './cdn/CloudinaryFile.vue';

const props = defineProps<{
  loading: boolean;
  imageFile?: NetworkFile | null;
  networkName: string;
}>();

// Fallback logic for name if it's empty
const networkName = props.networkName || 'Network';
</script>
