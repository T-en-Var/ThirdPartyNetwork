<template>
  <modal-container
    title="Edit File"
    @close="$emit('close')"
    :close-on-escape="!isSubmitting"
  >
    <form
      @submit.prevent="handleSubmit"
      class="form"
    >
      <!-- File Name -->
      <div class="form-field">
        <label
          for="fileName"
          class="form-label"
          >File Name</label
        >
        <input
          id="fileName"
          v-model="form.name"
          type="text"
          required
          class="form-input"
        />
      </div>

      <div>
        <CloudinaryFile
          :file="file"
          :allow-download="true"
        />
      </div>

      <!-- Visibility -->
      <div class="edit-file-modal__checkbox-row">
        <input
          id="isPublic"
          v-model="form.isPublic"
          type="checkbox"
          class="checkbox"
        />
        <label
          for="isPublic"
          class="edit-file-modal__checkbox-label"
        >
          Make file public
        </label>
      </div>

      <!-- Access Level -->
      <AccessLevelPicker v-model="form.accessLevel" />

      <!-- Actions -->
      <div class="edit-file-modal__actions">
        <div class="edit-file-modal__actions-start">
          <button
            type="button"
            class="btn btn--danger"
            @click="$emit('delete-file', file)"
            :disabled="isSubmitting"
          >
            <span>Delete</span>
          </button>
        </div>

        <div class="edit-file-modal__actions-end">
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
            :disabled="isSubmitting || !form.name"
          >
            <span v-if="isSubmitting">Saving...</span>
            <span v-else>Update</span>
          </button>
        </div>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import type { UpdateFile, NetworkFile } from '@/types';

const props = defineProps<{
  file: NetworkFile;
  isSubmitting?: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'update-file', id: string, networkId: string, updated: UpdateFile): void;
  (e: 'delete-file', file: NetworkFile): void;
}>();

const form = ref({
  name: props.file.name,
  isPublic: props.file.isPublic,
  accessLevel: props.file.accessLevel ?? 0,
});

watch(
  () => props.file,
  (newFile) => {
    form.value.name = newFile.name;
    form.value.isPublic = newFile.isPublic;
    form.value.accessLevel = newFile.accessLevel ?? 0;
  },
);

function handleSubmit() {
  emit('update-file', props.file.id, props.file.networkId, {
    name: form.value.name,
    isPublic: form.value.isPublic,
    accessLevel: form.value.accessLevel,
  });
}
</script>
