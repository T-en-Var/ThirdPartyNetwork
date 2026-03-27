<template>
  <modal-container title="Add Configuration" @close="$emit('close')">
    <create-user-content-container :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid" :network-id="networkId"
      :network-ids="networkIds" button-text="Add Configuration"
      @submit="handleSubmit">
      <div>
        <label class="block text-sm font-semibold text-gray-800 mb-2">Key</label>
        <input v-model="key" type="text" placeholder="Enter unique key"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
          required />
      </div>

      <div>
        <label class="block text-sm font-semibold text-gray-800 mb-2">Value
          (JSON)</label>
        <div class="overflow-visible">
          <JsonEditorVue v-model="jsonValue" :stringified="false"
            class="h-64 border rounded-md" />
        </div>
      </div>
    </create-user-content-container>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import CreateUserContentContainer from '@/components/modals/CreateUserContentContainer.vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import JsonEditorVue from 'json-editor-vue';
import type { CreateConfiguration, CreateUserContentForm } from '@/types';

defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
}>();

const inputIsValid = computed(() => !!key.value);

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, payload: CreateConfiguration): void;
}>();

const key = ref('');
const jsonValue = ref<object>({});

function handleSubmit(form: CreateUserContentForm) {
  emit('submit',
    form.networkId,
    {
      key: key.value.trim(),
      accessLevel: form.accessLevel,
      value: jsonValue.value
    }
  );
}
</script>
