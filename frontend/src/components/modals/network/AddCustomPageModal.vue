<template>
  <modal-container title="Add Custom Page" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Name -->
      <div>
        <label for="pageName" class="block text-sm font-semibold text-gray-800 mb-2">
          Page Name
        </label>
        <input id="pageName" v-model="form.name" type="text" placeholder="Enter page name"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all"
          required />
      </div>

      <!-- Slug -->
      <div>
        <label for="pageSlug" class="block text-sm font-semibold text-gray-800 mb-2">
          Page Slug
        </label>
        <input id="pageSlug" v-model="form.slug" type="text" placeholder="Enter page slug"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all"
          required />
        <p class="mt-1 text-xs text-gray-500">Information which should make it easier to look up.</p>
      </div>

      <!-- Access Level -->
      <AccessLevelPicker v-model="form.accessLevel" />

      <!-- Actions -->
      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting || !form.name || !form.slug">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Page</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import type { CreateCustomPage } from '@/types';

withDefaults(
  defineProps<{
    isSubmitting?: boolean;
  }>(),
  { isSubmitting: false }
);

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'createCustomPage', payload: CreateCustomPage): void;
}>();

const form = ref({
  name: '',
  slug: '',
  accessLevel: 0,
});

function handleSubmit() {
  emit('createCustomPage', { ...form.value });
}
</script>
