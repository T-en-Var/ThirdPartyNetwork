<template>
  <div class="flex h-14 w-14 items-center justify-center rounded-md bg-white p-2 shadow">
    <div
      v-if="loading"
      class="h-7 w-7 animate-spin rounded-full border-4 border-gray-300 border-t-indigo-500"
      role="progressbar"
      aria-valuenow="0"
      aria-valuemin="0"
      aria-valuemax="100"
      aria-label="Loading logo"
    ></div>

    <CloudinaryFile
      v-else-if="imageFile"
      :display-only="true"
      :file="imageFile"
      class="h-full w-full object-contain"
      alt="Network Logo"
    />

    <div
      v-else
      class="flex h-full w-full items-center justify-center"
    >
      <img
        :src="`https://ui-avatars.com/api/?name=${networkName}&size=24&background=random`"
        :alt="`${networkName} logo`"
        class="max-h-full max-w-full rounded-md object-contain"
      />
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

const networkName = props.networkName || 'Network';
</script>
