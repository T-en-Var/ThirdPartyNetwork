<template>
  <div :class="customClass ? '' : 'rounded-xl border shadow-sm p-4 bg-white dark:bg-neutral-900 space-y-3'">
    <div v-if="!displayOnly" class="text-sm font-medium text-neutral-800 dark:text-neutral-200">
      {{ file.name }}
    </div>
    <div class="rounded overflow-hidden">
      <template v-if="isImage">
        <img :src="file.url" :alt="`${file.name}`" :class="customClass ?? 'w-full max-h-[400px] object-contain rounded'" />
      </template>
      <template v-else-if="isAudio">
        <audio controls :src="file.url" :class="customClass ?? 'w-full'" />
      </template>
      <template v-else-if="isVideo">
        <video controls :src="file.url" :class="customClass ?? 'w-full max-h-[400px] rounded'" />
      </template>
      <template v-else>
        <div class="flex items-center justify-between bg-neutral-100 dark:bg-neutral-800 p-3 rounded">
          <span class="truncate text-neutral-700 dark:text-neutral-300">
            {{ file.name }}
          </span>
          <a v-if="allowDownload" :href="file.url" download class="text-sm text-blue-600 hover:underline">
            Download
          </a>
        </div>
      </template>
    </div>
    <div v-if="!displayOnly" class="text-xs text-neutral-500 flex justify-between">
      <span>{{ file.format?.toUpperCase() || 'Unknown' }} • {{ readableSize(file.sizeBytes) }}</span>
      <span v-if="file.duration">Duration: {{ file.duration }}</span>
    </div>
    <div v-if="!displayOnly && allowDownload && (isImage || isVideo || isAudio)" class="pt-1">
      <span @click="downloadFile(file.url, file.name)" class="inline-block text-sm text-blue-600 hover:underline cursor-pointer">
        Download
      </span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { readableSize } from '@/lib/utils';
import type { NetworkFile } from '@/types';
import { computed } from 'vue'

const props = defineProps<{
  file: NetworkFile
  allowDownload?: boolean
  class?: string
  displayOnly?: boolean
}>()

const customClass = computed(() => props.class);

const isImage = computed(() => props.file.mediaType === 'image')
const isVideo = computed(() => props.file.mediaType === 'video')
const isAudio = computed(() => ['mp3', 'wav', 'ogg', 'aac', 'flac'].includes(props.file.format?.toLowerCase() ?? ''))

function downloadFile(fileUrl: string, filename: string) {
  fetch(fileUrl)
    .then(response => response.blob())
    .then(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      a.click();
      window.URL.revokeObjectURL(url);
    });
}
</script>

<style scoped>
/* optional custom styles */
</style>
