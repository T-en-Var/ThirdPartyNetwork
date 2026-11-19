<template>
  <modal-container
    :title="title"
    @close="$emit('close')"
  >
    <div class="confirmation-modal">
      <div class="confirmation-modal__message">
        {{ message }}
      </div>

      <div class="confirmation-modal__actions">
        <button
          type="button"
          class="btn btn--outline"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>

        <button
          type="button"
          :class="['btn', confirmButtonClass]"
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
import { computed } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';

const props = defineProps({
  title: {
    type: String,
    required: true,
  },
  message: {
    type: String,
    required: true,
  },
  buttonText: {
    type: String,
    default: 'Confirm',
  },
  isSubmitting: {
    type: Boolean,
    default: false,
  },
  color: {
    type: String,
    default: 'red',
  },
});

defineEmits(['close', 'confirm']);

const confirmButtonClass = computed(() => {
  const colorMap: Record<string, string> = {
    red: 'btn--danger',
    blue: 'btn--primary',
    green: 'btn--success',
    yellow: 'btn--warning',
    gray: 'btn--secondary',
    white: 'btn--white',
  };

  return colorMap[props.color] || colorMap.red;
});
</script>
