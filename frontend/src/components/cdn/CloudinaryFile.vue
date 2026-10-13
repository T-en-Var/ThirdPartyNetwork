<template>
  <div :class="customClass ? '' : 'cdn-file'">
    <div
      v-if="!displayOnly"
      class="cdn-file__name"
    >
      {{ file.name }}
    </div>
    <div class="cdn-file__preview">
      <template v-if="isImage || isThumbnail">
        <img
          :src="isThumbnail ? cloudinaryVideoThumbnail(file.url) : file.url"
          :alt="`${file.name}`"
          :class="customClass ?? 'cdn-file__media'"
        />
      </template>
      <template v-else-if="isAudio">
        <audio
          controls
          :src="file.url"
          :class="customClass ?? 'cdn-file__media cdn-file__media--audio'"
        />
      </template>
      <template v-else-if="isVideo">
        <video
          controls
          :src="file.url"
          :class="customClass ?? 'cdn-file__media'"
        />
      </template>
      <template v-else>
        <div class="cdn-file__fallback">
          <span class="cdn-file__fallback-name">
            {{ file.name }}
          </span>
          <a
            v-if="allowDownload"
            :href="file.url"
            download
            class="cdn-file__download-link"
          >
            Download
          </a>
        </div>
      </template>
    </div>
    <div
      v-if="!displayOnly"
      class="cdn-file__meta"
    >
      <span
        >{{ file.format?.toUpperCase() || 'Unknown' }} • {{ readableSize(file.sizeBytes) }}</span
      >
      <span v-if="file.duration">Duration: {{ file.duration }}</span>
    </div>
    <div
      v-if="!displayOnly && allowDownload && (isImage || isVideo || isAudio)"
      class="cdn-file__download"
    >
      <span
        @click="downloadFile(file.url, file.name)"
        class="cdn-file__download-trigger"
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
  return fileUrl.replace('/upload/', '/upload/w_300,h_200,c_fill/').replace(/\.[^.]+$/, '.jpg');
}
</script>
