<template>
  <modal-container :title="title" @close="$emit('close')">
    <div class="space-y-4">
      <div class="text-sm text-gray-500">
        {{ message }}
      </div>

      <div class="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        <button
          type="button"
          class="rounded-md border border-transparent bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
          :disabled="isSubmitting"
          @click="$emit('confirm')"
        >
          <span v-if="isSubmitting">Processing...</span>
          <span v-else>{{ buttonText }}</span>
        </button>
      </div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import ModalContainer from '@/components/modals/ModalContainer.vue';

defineProps({
  title: {
    type: String,
    required: true
  },
  message: {
    type: String,
    required: true
  },
  buttonText: {
    type: String,
    default: 'Confirm'
  },
  isSubmitting: {
    type: Boolean,
    default: false
  }
});

defineEmits(['close', 'confirm']);
</script>