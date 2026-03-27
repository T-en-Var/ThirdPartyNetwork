<template>
  <modal-container title="Add File to Network (50MB)" @close="$emit('close')"
    :close-on-escape="!loading">
    <div class="border-b border-gray-200 mb-4">
      <nav class="-mb-px flex space-x-6" aria-label="Tabs">
        <button type="button" @click="activeTab = 'upload'" :disabled="loading"
          :class="[
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm',
            loading
              ? 'text-gray-300 cursor-not-allowed'
              : activeTab === 'upload'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          ]">
          Upload Image
        </button>
        <button type="button" @click="activeTab = 'existing'" :disabled="loading"
          :class="[
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

    <CreateUserContentContainer v-if="activeTab === 'upload'"
      :is-submitting="isSubmitting" :input-is-valid="inputIsValid"
      :network-id="networkId" :network-ids="networkIds" :error="error"
      button-text="Upload File" @submit="handleSubmit">

      <div @dragover.prevent="handleDragOver" @dragenter.prevent="handleDragEnter"
        @dragleave.prevent="handleDragLeave" @drop.prevent="handleDrop"
        class="border-dashed border-2 rounded-lg p-6 text-center cursor-pointer transition-colors"
        :class="[dragging ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300 hover:bg-gray-50']"
        @click="triggerFileInput">
        <input ref="fileInput" type="file" class="hidden"
          :accept="mapMediaType[mediaType]" @change="handleFileChange"
          :disabled="loading" />

        <div v-if="selectedFile">
          <p class="text-gray-800 font-semibold">{{ selectedFile.name }}</p>
          <p class="text-sm text-gray-500">{{ readableSize(selectedFile.size) }}
          </p>
        </div>
        <div v-else class="text-gray-500">
          Drag & drop a file here or click to select
        </div>
      </div>
    </CreateUserContentContainer>

    <div v-if="activeTab === 'existing'">
      <div class="space-y-2">
        <label class="block text-sm font-medium text-gray-700">
          Choose one of your images
        </label>
        <div class="grid grid-cols-3 gap-3 max-h-96 overflow-y-auto p-2">
          <label v-for="f in files" :key="f.id"
            class="relative cursor-pointer rounded-lg border transition hover:shadow-sm"
            :class="selectedExistingFile?.id === f.id
              ? 'border-indigo-500 ring-2 ring-indigo-500'
              : 'border-gray-300'">

            <!-- Radio input (hidden, but accessible) -->
            <input type="radio" class="sr-only" :value="f"
              v-model="selectedExistingFile" />

            <!-- File preview -->
            <CloudinaryFile v-if="f" :display-only="true" :file="f"
              class="w-full h-32 object-cover rounded-t-md" />

            <!-- Name overlay -->
            <div class="px-2 py-1 text-xs text-gray-700 truncate text-center">
              {{ f.name }}
            </div>
          </label>
        </div>
        <p v-if="!files?.length" class="text-sm text-gray-500">No images available.
        </p>
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">Cancel</button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          :disabled="isSubmitting || !inputIsValid">
          <span v-if="isSubmitting">Submitted...</span>
          <span v-else>Choose File</span>
        </button>
      </div>
    </div>

    <ForceLoadModal v-if="loading" :title="'Uploading file'" :progress="progress" />
  </modal-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import useFiles from '@/composables/network/useFiles';
import { mapMediaType, readableSize } from '@/lib/utils';
import type { UserProxy, NetworkFile, CreateUserContentForm } from '@/types';
import { useAuthStore } from '@/stores/auth';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import ForceLoadModal from '../ForceWaitModal.vue';
import CreateUserContentContainer from '../CreateUserContentContainer.vue';

const filesState = useFiles();
const { progress } = filesState;
const { execute: uploadFile } = filesState.uploadFile;
const { execute: fetchUserFiles, data: files } = filesState.fetchUserFiles;

const error = computed(() => {
  return [filesState.uploadFile.error.value, filesState.fetchUserFiles.error.value].filter(Boolean).join(' - ');
});

const loading = computed(() => filesState.uploadFile.loading.value || filesState.uploadFile.loading.value);

withDefaults(defineProps<{
  networkId?: string;
  networkIds?: string[];
  mediaType?: string;
  canUpload?: boolean;
  canChoose?: boolean;
}>(), { mediaType: 'any', canUpload: true, canChoose: true,  });

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'uploaded', file: NetworkFile): void;
}>();

const authStore = useAuthStore();
const userProxy = ref<UserProxy | null>(null);
const activeTab = ref<'upload' | 'existing'>('upload');

onMounted(async () => {
  userProxy.value = await authStore.getUserProxy();

  if (userProxy.value) {
    await fetchUserFiles(userProxy.value.user.id, userProxy.value.id);
  }
});

const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);
const selectedAccessLevel = ref<number>(0);

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

function triggerFileInput() { if (!loading.value) fileInput.value?.click(); }
function handleFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  if (target.files?.length) {
    selectedFile.value = target.files[0];
  }
}

const selectedExistingFile = ref<NetworkFile | null>(null);

const inputIsValid = computed(() => {
  if (activeTab.value === 'upload') {
    return !!selectedFile.value;
  }
  if (activeTab.value === 'existing') {
    return !!selectedExistingFile.value;
  }
  return false;
});

const isSubmitting = computed(() => loading.value);

async function handleSubmit(form: CreateUserContentForm) {
  if (!inputIsValid.value) return;

  if (activeTab.value === 'existing') {
    emit('uploaded', selectedExistingFile.value!);
    selectedExistingFile.value = null;
  }

  if (activeTab.value === 'upload') {
    try {
      const file = await uploadFile(form.networkId, selectedFile.value!, selectedAccessLevel.value);

      emit('uploaded', file);
      selectedFile.value = null;
    } catch (_) {
      void _;
    }
  }
}
</script>
