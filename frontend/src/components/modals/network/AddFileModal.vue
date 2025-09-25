<template>
  <modal-container title="Add File to Network (50MB)" @close="$emit('close')" :close-on-escape="!loading">
    <form @submit.prevent="handleUpload" class="space-y-4">

      <!-- Tabs -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-6" aria-label="Tabs">
          <button type="button" @click="activeTab = 'upload'" :disabled="loading" :class="[
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm',
            loading
              ? 'text-gray-300 cursor-not-allowed'
              : activeTab === 'upload'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          ]">
            Upload Image
          </button>
          <button type="button" @click="activeTab = 'existing'" :disabled="loading" :class="[
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm',
            loading
              ? 'text-gray-300 cursor-not-allowed'
              : activeTab === 'existing'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          ]">
            My Files
          </button>
        </nav>
      </div>

      <!-- Upload Tab -->
      <div v-if="activeTab === 'upload'" class="space-y-4">
        <!-- Network Selection -->
        <div v-if="showNetworkSelector" class="space-y-2">
          <label class="block text-sm font-medium text-gray-700">
            Select a Network to store the image
          </label>
          <div class="space-y-2">
            <label v-for="n in allNetworks" :key="n.id"
              class="flex items-center space-x-2 cursor-pointer p-2 rounded-lg border transition hover:bg-gray-50"
              :class="selectedNetwork?.id === n.id ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300'">
              <input type="radio" class="text-indigo-600 focus:ring-indigo-500" :value="n" v-model="selectedNetwork" />
              <span class="text-sm text-gray-700">{{ n.name }}</span>
            </label>
          </div>
        </div>

        <!-- Drag & Drop / File Upload -->
        <div @dragover.prevent="handleDragOver" @dragenter.prevent="handleDragEnter"
          @dragleave.prevent="handleDragLeave" @drop.prevent="handleDrop"
          class="border-dashed border-2 rounded-lg p-6 text-center cursor-pointer transition-colors"
          :class="[dragging ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300 hover:bg-gray-50']"
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
      </div>

      <!-- Existing Tab -->
      <div v-if="activeTab === 'existing'" class="space-y-2">
        <label class="block text-sm font-medium text-gray-700">
          Choose one of your images
        </label>
        <div class="grid grid-cols-3 gap-3 max-h-64 overflow-y-auto p-2">
          <label v-for="f in existingFiles" :key="f.id"
            class="relative cursor-pointer rounded-lg border transition hover:shadow-sm" :class="selectedExistingFile?.id === f.id
              ? 'border-indigo-500 ring-2 ring-indigo-500'
              : 'border-gray-300'">

            <!-- Radio input (hidden, but accessible) -->
            <input type="radio" class="sr-only" :value="f" v-model="selectedExistingFile" />

            <!-- File preview -->
            <CloudinaryFile v-if="f" :display-only="true" :file="f" class="w-full h-32 object-cover rounded-t-md" />

            <!-- Name overlay -->
            <div class="px-2 py-1 text-xs text-gray-700 truncate text-center">
              {{ f.name }}
            </div>
          </label>
        </div>
        <p v-if="!existingFiles.length" class="text-sm text-gray-500">No images available.</p>
      </div>

      <!-- Action Buttons -->
      <div class="flex justify-end space-x-3">
        <button type="button" @click="$emit('close')" :disabled="loading"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
          Cancel
        </button>
        <button type="submit" :disabled="submitDisabled"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed">
          <span v-if="loading && activeTab === 'upload'">Uploading...</span>
          <span v-else-if="activeTab === 'upload'">Upload</span>
          <span v-else>Select</span>
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
import type { UserProxy, Network, NetworkFile } from '@/types';
import { useAuthStore } from '@/stores/auth';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

const props = defineProps<{
  network?: Network;
  networks?: Network[];
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'uploaded', file: NetworkFile): void;
}>();

const authStore = useAuthStore();
const userProxy = ref<UserProxy | null>(null);
const activeTab = ref<'upload' | 'existing'>('upload');

onMounted(async () => {
  if (!props.network && !props.networks) {
    throw new Error('Define either network or networks.');
  }
  if (props.network) {
    selectedNetwork.value = props.network;
  }
  userProxy.value = await authStore.getUserProxy();
});

const { uploadFile, file, loading, error, progress } = useFiles();

const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);

const allNetworks = computed(() => {
  if (props.networks) return props.networks;
  return props.network ? [props.network] : [];
});
const selectedNetwork = ref<Network | null>(null);
const showNetworkSelector = computed(() => {
  return props.networks || (props.network && props.networks);
});

const dragging = ref(false);
function handleDragEnter() { dragging.value = true; }
function handleDragLeave(e: DragEvent) { if (e.currentTarget === e.target) dragging.value = false; }
function handleDragOver() { dragging.value = true; }
function handleDrop(e: DragEvent) {
  dragging.value = false;
  if (e.dataTransfer?.files.length) {
    selectedFile.value = e.dataTransfer.files[0];
  }
}

watch(file, (newFile) => {
  if (newFile) {
    emit('uploaded', newFile);
    selectedFile.value = null;
  }
});

function triggerFileInput() { if (!loading.value) fileInput.value?.click(); }
function handleFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  if (target.files?.length) {
    selectedFile.value = target.files[0];
  }
}

const existingFiles = computed<NetworkFile[]>(() => {
  if ((!props.network && !props.networks) || !userProxy.value) return [];
  const allProxies = [userProxy.value, ...(userProxy.value?.user.userProxies.filter(u => u != null) ?? [])];
  console.log("allProxies", allProxies)
  const files: NetworkFile[] = [];
  allProxies.forEach(up => {
    up.networkUsers.forEach(nu => {
      nu.files?.forEach(f => {
        if (f.mediaType === 'image') files.push(f);
      });
    });
  });
  return files;
});
const selectedExistingFile = ref<NetworkFile | null>(null);

const submitDisabled = computed(() => {
  if (loading.value) return true;
  if (activeTab.value === 'upload') {
    return !selectedFile.value || !selectedNetwork.value;
  }
  if (activeTab.value === 'existing') {
    return !selectedExistingFile.value;
  }
  return true;
});

async function handleUpload() {
  if (activeTab.value === 'existing') {
    if (selectedExistingFile.value) {
      emit('uploaded', selectedExistingFile.value);
      selectedExistingFile.value = null;
    }
    return;
  }

  if (activeTab.value === 'upload') {
    if (!selectedFile.value || !selectedNetwork.value) return;
    await uploadFile(selectedNetwork.value.id, selectedFile.value);
    selectedFile.value = null;
  }
}
</script>
