<template>
  <modal-container
    title="Add File to Network"
    @close="$emit('close')"
    :close-on-escape="!loading"
  >
    <div>
      <div class="add-file-modal__tabs-wrap">
        <nav
          class="add-file-modal__tabs"
          aria-label="Tabs"
        >
          <button
            type="button"
            @click="activeTab = 'upload'"
            :disabled="loading"
            :class="[
              'add-file-modal__tab',
              activeTab === 'upload' ? 'add-file-modal__tab--active' : '',
            ]"
          >
            Upload Image
          </button>
          <button
            type="button"
            @click="activeTab = 'existing'"
            :disabled="loading"
            :class="[
              'add-file-modal__tab',
              activeTab === 'existing' ? 'add-file-modal__tab--active' : '',
            ]"
          >
            My Files
          </button>
        </nav>
      </div>

      <UserContentForm
        v-if="activeTab === 'upload'"
        :is-submitting="isSubmitting"
        :input-is-valid="inputIsValid"
        :network-id="networkId"
        :network-ids="networkIds"
        :error="error"
        button-text="Upload File"
        @submit="handleSubmit"
        @close="emit('close')"
      >
        <div
          @dragover.prevent="handleDragOver"
          @dragenter.prevent="handleDragEnter"
          @dragleave.prevent="handleDragLeave"
          @drop.prevent="handleDrop"
          class="add-file-modal__dropzone"
          :class="[dragging ? 'add-file-modal__dropzone--active' : '']"
          @click="triggerFileInput"
        >
          <input
            ref="fileInput"
            type="file"
            class="add-file-modal__file-input"
            :accept="mapMediaType[mediaType]"
            @change="handleFileChange"
            :disabled="loading"
          />

          <div v-if="selectedFile">
            <p class="add-file-modal__file-name">{{ selectedFile.name }}</p>
            <p class="add-file-modal__file-size">{{ readableSize(selectedFile.size) }}</p>
          </div>
          <div
            v-else
            class="add-file-modal__dropzone-placeholder"
          >
            Drag & drop a file here or click to select
          </div>
        </div>
      </UserContentForm>

      <div v-if="activeTab === 'existing'">
        <div class="add-file-modal__picker">
          <label class="form-label">Choose one of your images</label>
          <div class="add-file-modal__file-grid">
            <label
              v-for="f in files"
              :key="f.id"
              class="add-file-modal__file-option"
              :class="[
                selectedExistingFile?.id === f.id ? 'add-file-modal__file-option--selected' : '',
              ]"
            >
              <!-- Radio input (hidden, but accessible) -->
              <input
                type="radio"
                class="sr-only"
                :value="f"
                v-model="selectedExistingFile"
              />

              <!-- File preview -->
              <CdnFile
                v-if="f"
                :display-only="true"
                :file="f"
                class="add-file-modal__file-option-preview"
              />

              <!-- Name overlay -->
              <div class="add-file-modal__file-option-name">
                {{ f.name }}
              </div>
            </label>
          </div>
          <p
            v-if="!files?.length"
            class="add-file-modal__no-files"
          >
            No images available.
          </p>
        </div>

        <div class="form__actions">
          <button
            type="button"
            class="btn btn--outline"
            @click="$emit('close')"
            :disabled="isSubmitting"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="btn btn--primary"
            @click="submitFile"
            :disabled="isSubmitting || !inputIsValid"
          >
            <span v-if="isSubmitting">Submitted...</span>
            <span v-else>Choose File</span>
          </button>
        </div>
      </div>

      <ForceLoadModal
        v-if="loading"
        :title="'Uploading file'"
        :progress="progress"
      />
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import useFiles from '@/composables/account/useFiles';
import { mapMediaType, readableSize } from '@/lib/utils';
import type { UserProxyDto, FileDto, CreateUserContentForm } from '@/types';
import { useAuthStore } from '@/stores/auth';
import CdnFile from '@/components/cdn/CdnFile.vue';
import ForceLoadModal from '../ForceWaitModal.vue';
import UserContentForm from '../../UserContentForm.vue';

const filesState = useFiles();
const { progress } = filesState;
const { execute: uploadFile } = filesState.uploadFile;
const { execute: fetchUserFiles, data: files } = filesState.fetchFiles;

const error = computed(() => {
  return [filesState.uploadFile.error.value, filesState.fetchFiles.error.value]
    .filter(Boolean)
    .join(' - ');
});

const loading = computed(
  () => filesState.uploadFile.loading.value || filesState.uploadFile.loading.value,
);

withDefaults(
  defineProps<{
    networkId?: string;
    networkIds?: string[];
    mediaType?: string;
    canUpload?: boolean;
    canChoose?: boolean;
  }>(),
  { mediaType: 'any', canUpload: true, canChoose: true },
);

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', file: FileDto): void;
}>();

const authStore = useAuthStore();
const userProxy = ref<UserProxyDto | null>(null);
const activeTab = ref<'upload' | 'existing'>('upload');

onMounted(async () => {
  userProxy.value = await authStore.getUserProxy();

  if (userProxy.value) {
    await fetchUserFiles(userProxy.value.id);
  }
});

const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);

const dragging = ref(false);
function handleDragEnter() {
  dragging.value = true;
}
function handleDragLeave(e: DragEvent) {
  if (e.currentTarget === e.target) dragging.value = false;
}
function handleDragOver() {
  dragging.value = true;
}
function handleDrop(e: DragEvent) {
  dragging.value = false;
  if (e.dataTransfer?.files.length) {
    selectedFile.value = e.dataTransfer.files[0];
  }
}

function triggerFileInput() {
  if (!loading.value) fileInput.value?.click();
}
function handleFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  if (target.files?.length) {
    selectedFile.value = target.files[0];
  }
}

const selectedExistingFile = ref<FileDto | null>(null);

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

function submitFile() {
  const file = selectedExistingFile.value;
  if (!file) return;

  emit('submit', file);
}

async function handleSubmit(form: CreateUserContentForm) {
  if (!inputIsValid.value) return;

  if (activeTab.value === 'existing') {
    emit('submit', selectedExistingFile.value!);
    selectedExistingFile.value = null;
  }

  if (activeTab.value === 'upload') {
    if (!userProxy.value) throw new Error('No user found');

    try {
      const file = await uploadFile(form.networkId, selectedFile.value!);

      emit('submit', file);
      selectedFile.value = null;
    } catch (_) {
      void _;
    }
  }
}
</script>
