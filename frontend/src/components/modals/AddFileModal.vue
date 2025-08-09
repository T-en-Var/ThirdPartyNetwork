<template>
  <modal-container title="Add File to Network (50MB)" @close="$emit('close')" :close-on-escape="!loading">
    <form @submit.prevent="handleUpload" class="space-y-4">
      <!-- Drag & Drop or File Select -->
      <div @dragover.prevent @drop.prevent="handleDrop"
        class="border-dashed border-2 border-gray-300 rounded-lg p-6 text-center cursor-pointer hover:bg-gray-50"
        @click="triggerFileInput">
        <input ref="fileInput" type="file" class="hidden" @change="handleFileChange" :disabled="loading" />

        <div v-if="selectedFile">
          <p class="text-gray-800 font-semibold">{{ selectedFile.name }}</p>
          <p class="text-sm text-gray-500">{{ readableSize(selectedFile.size) }}</p>
        </div>
        <div v-else class="text-gray-500">
          Drag & drop a file here or click to select
        </div>
      </div>

      <!-- Upload Button -->
      <div class="flex justify-end space-x-3">
        <button type="button" @click="$emit('close')" :disabled="loading"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50">
          Cancel
        </button>
        <button type="submit" :disabled="!selectedFile || loading"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed">
          <span v-if="loading">Uploading...</span>
          <span v-else>Upload</span>
        </button>
      </div>

      <!-- Progress Bar -->
      <div v-if="loading" class="w-full bg-gray-200 rounded-full h-2 mt-2 overflow-hidden">
        <div class="bg-indigo-600 h-2 transition-all duration-200" :style="{ width: progress + '%' }"></div>
      </div>

      <!-- Error Message -->
      <p v-if="error" class="text-red-600 text-sm mt-2">{{ error }}</p>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import useFiles from '@/composables/useFiles';
import { readableSize } from '@/lib/utils';
import type { Network, NetworkFile } from '@/types';

const props = defineProps<{
  network: Network;
}>()

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'open-edit-file', file: NetworkFile): void;
}>();

const { uploadFile, file, loading, error, progress } = useFiles();

const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);

watch(file, (newFile) => {
  if (newFile) {
    emit('open-edit-file', newFile);
    selectedFile.value = null;
  }
});

function triggerFileInput() {
  if (!loading.value) fileInput.value?.click();
}

function handleFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0];
  }
}

function handleDrop(e: DragEvent) {
  if (e.dataTransfer?.files.length) {
    selectedFile.value = e.dataTransfer.files[0];
  }
}

async function handleUpload() {
  if (!selectedFile.value) return;
  await uploadFile(props.network.id, selectedFile.value);
}
</script>
