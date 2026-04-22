<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="flex flex-col gap-6">
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
import type { NetworkFile, ConfirmForm, UpdateFile } from '@/types';
import { computed, onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import { useEventStore } from '@/stores/event';
import EditFileModal from '@/components/modals/usercontent/EditFileModal.vue';

const events = useEventStore();

const { execute: fetchFile, data: fetchedFile } = useFiles().fetchFile;

const showEditModal = ref(false);
const isSubmitting = ref(false);
const selectedFile = ref<NetworkFile | null>(null);

const rawFiles = ref<NetworkFile[]>([]);
const files = computed(() =>
  (rawFiles.value ?? ([] as NetworkFile[])).map((f) => {
    return { ...f, sizeBytes: f.sizeBytes / 1024 };
  }),
);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchFiles: () => Promise<Ref<NetworkFile[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'add-file'): void;
  (e: 'file-edit', file: NetworkFile): void;
  (e: 'file-update', id: string, networkId: string, networkFile: UpdateFile): void;
  (e: 'file-delete', file: NetworkFile): void;
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

async function handleEditFile(file: NetworkFile) {
  await fetchFile(file.networkId, file.id);

  if (!fetchedFile.value) throw new Error('File not found');

  selectedFile.value = fetchedFile.value;
  showEditModal.value = true;
}

function handleUpdateFile(id: string, networkId: string, networkFile: UpdateFile) {
  events.listen.file.update(() => (showEditModal.value = false), true);

  emit('file-update', id, networkId, networkFile);
}

function handleRemoveFile(file: NetworkFile) {
  emit('file-delete', file);
}
</script>
