<template>
  <div :class="customClass ? '' : 'cdn-file'">
    <div
      v-if="!displayOnly"
      class="cdn-file__name"
    >
      {{ file.name }}
    </div>
    <div class="cdn-file__preview">
      <template v-if="previewUrl && !loadFailed && isImage">
        <img
          :src="previewUrl"
          :alt="file.name"
          :class="customClass ?? 'cdn-file__media'"
          @error="loadFailed = true"
        />
      </template>
      <template v-else-if="previewUrl && !loadFailed && isAudio">
        <audio
          controls
          :src="previewUrl"
          :class="customClass ?? 'cdn-file__media cdn-file__media--audio'"
          @error="loadFailed = true"
        />
      </template>
      <template v-else-if="previewUrl && !loadFailed && isVideo">
        <video
          controls
          :src="previewUrl"
          :class="customClass ?? 'cdn-file__media'"
          @error="loadFailed = true"
        />
      </template>
      <template v-else>
        <div class="cdn-file__fallback">
          <span class="cdn-file__fallback-name">
            {{ file.name }}{{ loadFailed ? ' (preview unavailable)' : '' }}
          </span>
          <a
            v-if="allowDownload && previewUrl"
            :href="previewUrl"
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
      v-if="!displayOnly && allowDownload && previewUrl && (isImage || isVideo || isAudio)"
      class="cdn-file__download"
    >
      <span
        @click="downloadFile(previewUrl, file.name)"
        class="cdn-file__download-trigger"
      >
        Download
      </span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { readableSize } from '@/lib/utils';
import type { FileDto, FileLightDto } from '@/types';
import { computed, ref, watch } from 'vue';

const props = defineProps<{
  file: FileDto | FileLightDto;
  allowDownload?: boolean;
  class?: string;
  displayOnly?: boolean;
}>();

const customClass = computed(() => props.class);

const previewUrl = computed(() => ('viewingUrl' in props.file ? props.file.viewingUrl : null));
const loadFailed = ref(false);
watch(previewUrl, () => (loadFailed.value = false));

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
</script>
