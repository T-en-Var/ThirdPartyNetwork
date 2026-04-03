<template>
  <modal-container
    :title="title"
    @close="$emit('close')"
    :enable-closing="false"
  >
    <div class="space-y-4">
      <div class="text-sm text-gray-500">
        {{ message }}
      </div>
      <div
        v-if="progress && progress != 100"
        class="mt-2 h-2 w-full overflow-hidden rounded-full bg-gray-200"
      >
        <div
          class="h-2 bg-indigo-600 transition-all duration-200"
          :style="{ width: progress + '%' }"
        ></div>
      </div>
      <div
        v-else
        class="spinner-border mx-auto text-blue-500"
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

<style scoped>
.spinner-border {
  width: 3rem;
  height: 3rem;
  border: 3px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}
@keyframes spinner-border {
  100% {
    transform: rotate(360deg);
  }
}
</style>
