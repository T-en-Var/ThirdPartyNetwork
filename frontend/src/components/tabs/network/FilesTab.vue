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
      }]" :show-network="false" @add-new="() => $emit('add-file')"
        @edit="(f) => $emit('edit-file', f)" @remove="(f) => $emit('remove-file', f)">
      </UserContentViewer>
    </div>
  </div>
</template>

<script setup lang="ts">
import useFiles from '@/composables/useFiles';
import type { NetworkFile, ConfirmForm } from '@/types';
import { computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import UserContentViewer from '@/components/UserContentViewer.vue';

const route = useRoute();

const { execute: fetchFiles, data: rawFiles } = useFiles().fetchNetworkFiles;

const networkId = route.params.networkId as string;

const files = computed(() => (rawFiles.value ?? [] as NetworkFile[]).map(f => {
  return {...f, sizeBytes: Math.round(f.sizeBytes / 1024)};
}));

onMounted(async () => {
  await fetchFiles(networkId);
});

defineEmits<{
  (e: 'confirm', form: ConfirmForm): void;
  (e: 'add-file'): void;
  (e: 'edit-file', file: NetworkFile): void;
  (e: 'remove-file', file: NetworkFile): void;
}>();
</script>
