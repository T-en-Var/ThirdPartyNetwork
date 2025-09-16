<template>
  <modal-container title="Edit File" @close="$emit('close')" :close-on-escape="!isSubmitting">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- File Name -->
      <div>
        <label for="fileName" class="block text-sm font-semibold text-gray-800 mb-2">File Name</label>
        <input id="fileName" v-model="form.name" type="text" required
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <div>
        <CloudinaryFile :file="file" :allow-download="true" />
      </div>

      <!-- Visibility -->
      <div class="flex items-center">
        <input id="isPublic" v-model="form.isPublic" type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500" />
        <label for="isPublic" class="ml-2 block text-sm text-gray-700">
          Make file public
        </label>
      </div>

      <!-- Actions -->
      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          :disabled="isSubmitting || !form.name">
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>Update</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import type { EditFileForm, NetworkFile } from '@/types';

const props = defineProps<{
  file: NetworkFile;
  isSubmitting?: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'update-file', id: string, updated: EditFileForm): void;
}>();

const form = ref({
  name: props.file.name,
  isPublic: props.file.isPublic
});

// Update form if props.file changes
watch(() => props.file, (newFile) => {
  form.value.name = newFile.name;
  form.value.isPublic = newFile.isPublic;
});

function handleSubmit() {
  emit('update-file',
    props.file.id,
    {
      name: form.value.name,
      isPublic: form.value.isPublic
    });
}
</script>
