<template>
  <div class="card">
    <div class="files-tab__content">
      <UserContentViewer
        title="Files"
        :entries="files || []"
        :extra-columns="[
          {
            key: 'name',
            label: 'Name',
            type: 'string',
            filter: false,
          },
          {
            key: 'mediaType',
            label: 'Media',
            type: 'string',
            filter: true,
          },
          {
            key: 'format',
            label: 'Format',
            type: 'string',
            filter: true,
          },
          {
            key: 'sizeBytes',
            label: 'Bytes (KB)',
            type: 'number',
            filter: true,
          },
        ]"
        :show-network="false"
        @add-new="handleCreateFile"
        @edit="handleEditFile"
        @remove="handleRemoveFile"
      >
      </UserContentViewer>

      <EditFileModal
        v-if="showEditModal && selectedFile"
        :file="selectedFile"
        :is-submitting="isSubmitting"
        @close="showEditModal = false"
        @update-file="handleUpdateFile"
        @delete-file="handleRemoveFile"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import useFiles from '@/composables/network/useFiles';
import type { FileDto, ConfirmForm, UpdateFileDto } from '@/types';
import { computed, onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import { useEventStore } from '@/stores/event';
import EditFileModal from '@/components/modals/usercontent/EditFileModal.vue';

const events = useEventStore();

const { execute: fetchFile, data: fetchedFile } = useFiles().fetchFile;

const showEditModal = ref(false);
const isSubmitting = ref(false);
const selectedFile = ref<FileDto | null>(null);

const rawFiles = ref<FileDto[]>([]);
const files = computed(() =>
  (rawFiles.value ?? ([] as FileDto[])).map((f) => {
    return { ...f, sizeBytes: f.sizeBytes / 1024 };
  }),
);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchFiles: () => Promise<Ref<FileDto[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'add-file'): void;
  (e: 'file-edit', file: FileDto): void;
  (e: 'file-update', id: string, networkId: string, networkFile: UpdateFileDto): void;
  (e: 'file-delete', file: FileDto): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  const remoteRef = await props.fetchFiles();

  watch(remoteRef, (newVal) => (rawFiles.value = newVal ?? []), { immediate: true });

  events.listen.file.openEdit(handleEditFile);
});

async function handleCreateFile() {
  emit('add-file');
}

async function handleEditFile(file: FileDto) {
  await fetchFile(file.networkId, file.id);

  if (!fetchedFile.value) throw new Error('File not found');

  selectedFile.value = fetchedFile.value;
  showEditModal.value = true;
}

function handleUpdateFile(id: string, networkId: string, networkFile: UpdateFileDto) {
  events.listen.file.update(() => (showEditModal.value = false), true);

  emit('file-update', id, networkId, networkFile);
}

function handleRemoveFile(file: FileDto) {
  emit('file-delete', file);
}
</script>
