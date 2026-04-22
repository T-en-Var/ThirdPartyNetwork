<template>
  <teleport to="body">
    <div
      class="fixed inset-0 z-50 overflow-y-auto"
      aria-labelledby="modal-title"
      role="dialog"
      aria-modal="true"
    >
      <div
        class="flex min-h-screen items-end justify-center px-4 pb-20 pt-4 text-center sm:block sm:p-0"
      >
        <div
          class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          aria-hidden="true"
          @click="enableClosing && closeOnOutsideClick ? $emit('close') : () => {}"
        ></div>

        <span
          class="hidden sm:inline-block sm:h-screen sm:align-middle"
          aria-hidden="true"
          >&#8203;</span
        >

        <div
          class="relative inline-block transform overflow-hidden rounded-lg bg-white text-left align-bottom shadow-xl transition-all sm:my-8 sm:w-full sm:align-middle"
          :class="[customClass, sizeClasses]"
        >
          <div class="flex h-full flex-col bg-white px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
            <div class="h-full flex-1 sm:flex sm:items-start">
              <div class="mt-3 flex h-full w-full flex-col text-center sm:mt-0 sm:text-left">
                <h3
                  class="text-lg font-medium leading-6 text-gray-900"
                  id="modal-title"
                >
                  {{ title }}
                </h3>

                <div class="mt-4 flex-1">
                  <slot></slot>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </teleport>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, computed } from 'vue';
import { useModalStack } from '@/composables/useModalStack';

const props = defineProps({
  title: { type: String, required: true },
  customClass: { type: String, default: '' },
  closeOnEscape: { type: Boolean, default: true },
  closeOnOutsideClick: { type: Boolean, default: true },
  enableClosing: { type: Boolean, default: true },
  size: {
    type: String,
    default: 'sm',
    validator: (value: string) => ['sm', 'md', 'lg'].includes(value),
  },
});

const emit = defineEmits(['close']);

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'lg':
      return 'sm:max-w-[95vw] sm:min-h-[95vh]';
    case 'md':
      return 'sm:max-w-3xl';
    case 'sm':
    default:
      return 'sm:max-w-lg';
  }
});

const id = Math.random().toString(36).slice(2);
const { open, close: closeStack, isTop } = useModalStack(id);

function onKeydown(e: KeyboardEvent) {
  if (props.closeOnEscape && e.key === 'Escape' && isTop()) {
    if (props.enableClosing) emit('close');
  }
}

onMounted(() => {
  open();
  window.addEventListener('keydown', onKeydown);
});

onBeforeUnmount(() => {
  closeStack();
  window.removeEventListener('keydown', onKeydown);
});
</script>
