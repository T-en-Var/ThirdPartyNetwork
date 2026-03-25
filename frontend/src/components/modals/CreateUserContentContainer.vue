<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <access-level-picker v-model="accessLevel" />

    <slot></slot>

    <div v-if="warningText"
      class="p-2 text-yellow-600 bg-yellow-200 border-yellow-500 border-[1px] rounded-md">
      <span>{{ warningText }}</span>
    </div>

    <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
      <button type="button"
        class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
        @click="$emit('close')" :disabled="isSubmitting">Cancel</button>
      <button type="submit"
        class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
        :disabled="isSubmitting || !inputIsValid">
        <span v-if="isSubmitting">Submitted...</span>
        <span v-else>{{ buttonText }}</span>
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import type { CreateUserContentForm } from '@/types';
import { onMounted, ref } from 'vue';
import AccessLevelPicker from '../fields/AccessLevelPicker.vue';

const props = defineProps<{
  buttonText: string;
  isSubmitting: boolean;
  inputIsValid: boolean;
  networkId?: string;
  networkIds?: string[];
}>();

const accessLevel = ref(0);
const warningText = ref('');
let warningTextTimeout: ReturnType<typeof setTimeout> | null = null;


onMounted(() => {
  if (props.networkId === undefined && props.networkIds === undefined) throw new Error("'props.networkId === undefined && props.networkIds === undefined' Define either one or both");

});

const handleSubmit = () => {
  if (!props.inputIsValid) showWarning('Input is invalid.');
  if (props.isSubmitting) return;

  const form: CreateUserContentForm = {
    networkId: props.networkId ?? 'fakenews',
    accessLevel: accessLevel.value,
  };

  emit('submit', form);
};

const showWarning = (message: string) => {
  if (warningTextTimeout) clearTimeout(warningTextTimeout);
  warningTextTimeout = null;

  warningText.value = message;
  warningTextTimeout = setTimeout(() => warningText.value = '', 3000);
};

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', form: CreateUserContentForm): void;
}>();

</script>