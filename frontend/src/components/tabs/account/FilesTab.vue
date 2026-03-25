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
      }]" :show-network="true" @add-new="() => $emit('add-file')"
        @edit="(f) => $emit('edit-file', f)" @remove="(f) => $emit('remove-file', f)">
      </UserContentViewer>
    </div>
  </div>
</template>

<script setup lang="ts">
import useFiles from '@/composables/useFiles';
import type { NetworkFile, ConfirmForm } from '@/types';
import { computed, onMounted } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();

const { execute: fetchUserFiles, data: rawFiles } = useFiles().fetchUserFiles;

const files = computed(() => (rawFiles.value ?? [] as NetworkFile[]).map(f => {
  return {...f, sizeBytes: Math.round(f.sizeBytes / 1024)};
}));

onMounted(async () => {
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error("Userproxy not found");
  await fetchUserFiles(currentUser.user.id, currentUser.id);
});

defineEmits<{
  (e: 'confirm', form: ConfirmForm): void;
  (e: 'add-file'): void;
  (e: 'edit-file', file: NetworkFile): void;
  (e: 'remove-file', file: NetworkFile): void;
}>();
</script>
