<template>
  <modal-container
    :title="title"
    @close="$emit('close')"
    :enable-closing="false"
  >
    <div class="force-wait-modal">
      <div class="force-wait-modal__message">
        {{ message }}
      </div>
      <div
        v-if="progress && progress != 100"
        class="force-wait-modal__progress"
      >
        <div
          class="force-wait-modal__progress-bar"
          :style="{ width: progress + '%' }"
        ></div>
      </div>
      <div
        v-else
        class="spinner-border spinner-border--lg spinner-border--primary force-wait-modal__spinner"
      ></div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import ModalContainer from '@/components/modals/ModalContainer.vue';

defineProps({
  title: {
    type: String,
    required: true,
  },
  progress: {
    type: Number,
    required: false,
  },
  message: {
    type: String,
    default: 'Please hold on while we process the request...',
  },
});

defineEmits(['close', 'confirm']);
</script>
