<template>
  <modal-container title="Add Page Block" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Name -->
      <div>
        <label for="pageText" class="block text-sm font-semibold text-gray-800 mb-2">
          Page Block Text
        </label>
        <input id="pageName" v-model="form.text" type="text" placeholder="Enter text"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all"
          required />
      </div>

      <!-- Slug -->
      <div>
        <label for="pagePosition" class="block text-sm font-semibold text-gray-800 mb-2">
          Page Block Posistion
        </label>
        <input id="pageSlug" v-model="form.position" type="number"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all"
          required />
        <p class="mt-1 text-xs text-gray-500">You can change all this later.</p>
      </div>

      <!-- Actions -->
      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm disabled:cursor-not-allowed hover:bg-indigo-700 disabled:bg-gray-600 disabled:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting || !form.text || (form.position != 0 && !form.position)">
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
import type { CreatePageBlock, CustomPage } from '@/types';

const props = withDefaults(
  defineProps<{
    isSubmitting?: boolean;
    customPage: CustomPage
  }>(),
  { isSubmitting: false }
);

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'createPageBlock', id: string, pageBlock: CreatePageBlock): void;
}>();

const form = ref({
  text: '',
  position: 1,
  data: {},
  customPageId: props.customPage.id
});

function handleSubmit() {
  emit('createPageBlock', props.customPage.id, { ...form.value });
}
</script>
