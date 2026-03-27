<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <UserContentViewer title="Files" :entries="files || []" :extra-columns="[{
        key: 'name',
        label: 'Name',
        type: 'string',
        filter: false,
      }, {
        key: 'mediaType',
        label: 'Media',
        type: 'string',
        filter: true,
      }, {
        key: 'format',
        label: 'Format',
        type: 'string',
        filter: true,
      }, {
        key: 'sizeBytes',
        label: 'Bytes (KB)',
        type: 'number',
        filter: true,
      }]" :show-network="false" @add-new="handleCreateFile"
        @edit="(f) => $emit('file-edit', f)" @remove="handleRemoveFile">
      </UserContentViewer>

      <EditFileModal v-if="showEditModal && selectedFile" :file="selectedFile"
        :is-submitting="isSubmitting" @close="showEditModal = false"
        @update-file="handleUpdateFile" @delete-file="handleRemoveFile" />
    </div>
  </div>
</template>

<script setup lang="ts">
import useFiles from '@/composables/useFiles';
import type { NetworkFile, ConfirmForm, UpdateFile } from '@/types';
import { computed, onMounted, ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const { execute: fetchFiles, data: rawFiles } = useFiles().fetchNetworkFiles;
const { execute: fetchFile, data: fetchedFile } = useFiles().fetchFile;

const showEditModal = ref(false);
const isSubmitting = ref(false);
const selectedFile = ref<NetworkFile | null>(null);

  
const props = defineProps<{
  networkId: string;
}>();

const emit = defineEmits<{
  (e: 'add-file'): void;
  (e: 'file-edit', file: NetworkFile): void;
  (e: 'file-update', id: string, networkId: string, networkFile: UpdateFile): void;
  (e: 'file-delete', file: NetworkFile): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();


onMounted(async () => {
  await fetchFiles(props.networkId);

  events.listen.file.openEdit(handleEditFile);
});


const files = computed(() => (rawFiles.value ?? [] as NetworkFile[]).map(f => {
  return { ...f, sizeBytes: Math.round(f.sizeBytes / 1024) };
}));


async function handleCreateFile() {
  emit('add-file');
}

async function handleEditFile(file: NetworkFile) {
  await fetchFile(file.networkId, file.id);

  if (!fetchedFile.value) throw new Error("File not found");

  selectedFile.value = fetchedFile.value;
  showEditModal.value = true;
}

function handleUpdateFile(id: string, networkId: string, networkFile: UpdateFile) {
  emit('file-update', id, networkId, networkFile);
}

function handleRemoveFile(file: NetworkFile) {
  emit('file-delete', file);
}
</script>
