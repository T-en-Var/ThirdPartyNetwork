<template>
  <div
    :class="
      customClass ? '' : 'space-y-3 rounded-xl border bg-white p-4 shadow-sm dark:bg-neutral-900'
    "
  >
    <div
      v-if="!displayOnly"
      class="text-sm font-medium text-neutral-800 dark:text-neutral-200"
    >
      {{ file.name }}
    </div>
    <div class="overflow-hidden rounded">
      <template v-if="isImage || isThumbnail">
        <img
          :src="isThumbnail ? cloudinaryVideoThumbnail(file.url) : file.url"
          :alt="`${file.name}`"
          :class="customClass ?? 'max-h-[400px] w-full rounded object-contain'"
        />
      </template>
      <template v-else-if="isAudio">
        <audio
          controls
          :src="file.url"
          :class="customClass ?? 'w-full'"
        />
      </template>
      <template v-else-if="isVideo">
        <video
          controls
          :src="file.url"
          :class="customClass ?? 'max-h-[400px] w-full rounded'"
        />
      </template>
      <template v-else>
        <div
          class="flex items-center justify-between rounded bg-neutral-100 p-3 dark:bg-neutral-800"
        >
          <span class="truncate text-neutral-700 dark:text-neutral-300">
            {{ file.name }}
          </span>
          <a
            v-if="allowDownload"
            :href="file.url"
            download
            class="text-sm text-blue-600 hover:underline"
          >
            Download
          </a>
        </div>
      </template>
    </div>
    <div
      v-if="!displayOnly"
      class="flex justify-between text-xs text-neutral-500"
    >
      <span
        >{{ file.format?.toUpperCase() || 'Unknown' }} • {{ readableSize(file.sizeBytes) }}</span
      >
      <span v-if="file.duration">Duration: {{ file.duration }}</span>
    </div>
    <div
      v-if="!displayOnly && allowDownload && (isImage || isVideo || isAudio)"
      class="pt-1"
    >
      <span
        @click="downloadFile(file.url, file.name)"
        class="inline-block cursor-pointer text-sm text-blue-600 hover:underline"
      >
        Download
      </span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { readableSize } from '@/lib/utils';
import type { NetworkFile } from '@/types';
import { computed } from 'vue';

const props = defineProps<{
  file: NetworkFile;
  allowDownload?: boolean;
  class?: string;
  displayOnly?: boolean;
  isThumbnail?: boolean;
}>();

const customClass = computed(() => props.class);

const isImage = computed(() => props.file.mediaType === 'image');
const isVideo = computed(() => props.file.mediaType === 'video');
const isAudio = computed(() =>
  ['mp3', 'wav', 'ogg', 'aac', 'flac'].includes(props.file.format?.toLowerCase() ?? ''),
);

function downloadFile(fileUrl: string, filename: string) {
  fetch(fileUrl)
    .then((response) => response.blob())
    .then((blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      a.click();
      window.URL.revokeObjectURL(url);
    });
}

function cloudinaryVideoThumbnail(fileUrl: string) {
  // Cloudinary "video -> image" transformation
  return fileUrl.replace('/upload/', '/upload/w_300,h_200,c_fill/').replace(/\.[^.]+$/, '.jpg');
}
</script>

<style scoped>
/* optional custom styles */
</style>
