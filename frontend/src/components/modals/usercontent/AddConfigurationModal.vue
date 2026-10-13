<template>
  <modal-container
    title="Add Configuration"
    @close="$emit('close')"
  >
    <UserContentForm
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="networkId"
      :network-ids="networkIds"
      button-text="Add Configuration"
      @submit="handleSubmit"
    >
      <div class="form-field">
        <label class="form-label">Key</label>
        <input
          v-model="key"
          type="text"
          placeholder="Enter unique key"
          class="form-input"
          required
        />
      </div>

      <div class="form-field">
        <label class="form-label">Value (JSON)</label>
        <div class="add-configuration-modal__json-wrap">
          <JsonEditorVue
            v-model="jsonValue"
            :stringified="false"
            class="add-configuration-modal__json-editor"
          />
        </div>
      </div>
    </UserContentForm>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import UserContentForm from '@/components/UserContentForm.vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import JsonEditorVue from 'json-editor-vue';
import type { CreateConfiguration, CreateUserContentForm } from '@/types';

defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, payload: CreateConfiguration): void;
}>();

const key = ref('');
const jsonValue = ref<object>({});

const inputIsValid = computed(() => !!key.value);

function handleSubmit(form: CreateUserContentForm) {
  emit('submit', form.networkId, {
    key: key.value.trim(),
    accessLevel: form.accessLevel,
    value: jsonValue.value,
  });
}
</script>
