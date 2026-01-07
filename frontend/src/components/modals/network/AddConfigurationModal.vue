<template>
  <modal-container title="Add Configuration" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label class="block text-sm font-semibold text-gray-800 mb-2">Key</label>
        <input v-model="form.key" type="text" placeholder="Enter unique key"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
          required />
      </div>

      <AccessLevelPicker v-model="form.accessLevel" />

      <div>
        <label class="block text-sm font-semibold text-gray-800 mb-2">Value
          (JSON)</label>
        <div class="overflow-visible">
          <JsonEditorVue v-model="jsonValue" :stringified="false" class="h-64 border rounded-md" />
        </div>
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">Cancel</button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          :disabled="isSubmitting || !form.key">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Configuration</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import JsonEditorVue from 'json-editor-vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import type { CreateConfiguration } from '@/types';

withDefaults(defineProps<{ isSubmitting?: boolean; }>(), { isSubmitting: false });

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'createConfiguration', payload: CreateConfiguration): void;
}>();

const form = ref({ key: '', accessLevel: 0 });
const jsonValue = ref<object>({});

function handleSubmit() {
  emit('createConfiguration', { key: form.value.key.trim(), accessLevel: form.value.accessLevel, value: jsonValue.value });
}
</script>
