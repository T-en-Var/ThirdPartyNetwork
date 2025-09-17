<template>
  <modal-container title="Add File to Network (50MB)" @close="$emit('close')" :close-on-escape="!loading">
    <form @submit.prevent="handleUpload" class="space-y-4">
      <!-- Network Selection -->
      <div v-if="showNetworkSelector" class="space-y-2">
        <label class="block text-sm font-medium text-gray-700" title="It will be stored as part of this network">Select
          a Network</label>
        <div class="space-y-2">
          <label v-for="n in allNetworks" :key="n.id"
            class="flex items-center space-x-2 cursor-pointer p-2 rounded-lg border transition hover:bg-gray-50"
            :class="selectedNetwork?.id === n.id ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300'">
            <input type="radio" class="text-indigo-600 focus:ring-indigo-500" :value="n" v-model="selectedNetwork" />
            <span class="text-sm text-gray-700">{{ n.name }}</span>
          </label>
        </div>
      </div>

      <!-- Drag & Drop or File Select -->
      <div @dragover.prevent="handleDragOver" @dragenter.prevent="handleDragEnter" @dragleave.prevent="handleDragLeave"
        @drop.prevent="handleDrop"
        class="border-dashed border-2 rounded-lg p-6 text-center cursor-pointer transition-colors" :class="[
          dragging
            ? 'border-indigo-500 bg-indigo-50'
            : 'border-gray-300 hover:bg-gray-50'
        ]" @click="triggerFileInput">
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
        <button type="submit" :disabled="!selectedFile || !selectedNetwork || loading"
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
import { computed, onMounted, ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import useFiles from '@/composables/useFiles';
import { readableSize } from '@/lib/utils';
import type { Network, NetworkFile } from '@/types';

const props = defineProps<{
  network?: Network;
  networks?: Network[];
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'uploaded', file: NetworkFile): void;
}>();

onMounted(() => {
  if (!props.network && !props.networks) {
    throw new Error('Define either network or networks.');
  }
  if (props.network) {
    selectedNetwork.value = props.network; // default to passed single network
  }
});

const { uploadFile, file, loading, error, progress } = useFiles();

const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);

// --- Network selection state ---
const allNetworks = computed(() => {
  if (props.networks) return props.networks;
  return props.network ? [props.network] : [];
});

const selectedNetwork = ref<Network | null>(null);

// If both network & networks provided → show selector with default preselected
const showNetworkSelector = computed(() => {
  return props.networks || (props.network && props.networks);
});

const dragging = ref(false);

function handleDragEnter() {
  dragging.value = true;
}

function handleDragLeave(e: DragEvent) {
  // Ensure leaving the drop zone triggers reset
  if (e.currentTarget === e.target) {
    dragging.value = false;
  }
}

function handleDragOver() {
  dragging.value = true; // keeps state while hovering
}

function handleDrop(e: DragEvent) {
  dragging.value = false; // reset state
  if (e.dataTransfer?.files.length) {
    selectedFile.value = e.dataTransfer.files[0];
  }
}


// --- Watch for uploaded file ---
watch(file, (newFile) => {
  if (newFile) {
    emit('uploaded', newFile);
    selectedFile.value = null;
  }
});

// --- File input logic ---
function triggerFileInput() {
  if (!loading.value) fileInput.value?.click();
}

function handleFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  if (target.files?.length) {
    selectedFile.value = target.files[0];
  }
}

// --- Upload logic ---
async function handleUpload() {
  if (!selectedFile.value || !selectedNetwork.value) return;
  await uploadFile(selectedNetwork.value.id, selectedFile.value);
}
</script>
