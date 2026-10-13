<template>
  <teleport to="body">
    <div
      class="modal-overlay"
      aria-labelledby="modal-title"
      role="dialog"
      aria-modal="true"
    >
      <div class="modal-overlay__wrapper">
        <div
          class="modal-overlay__backdrop"
          aria-hidden="true"
          @click="enableClosing && closeOnOutsideClick ? $emit('close') : () => {}"
        ></div>

        <span
          class="modal-overlay__spacer"
          aria-hidden="true"
          >&#8203;</span
        >

        <div
          class="modal"
          :class="[customClass, sizeClasses]"
        >
          <div class="modal__body">
            <div class="modal__content">
              <div class="modal__text">
                <h3
                  class="modal__title"
                  id="modal-title"
                >
                  {{ title }}
                </h3>

                <div class="modal__main">
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

const props = withDefaults(
  defineProps<{
    title: string;
    customClass?: string;
    closeOnEscape?: boolean;
    closeOnOutsideClick?: boolean;
    enableClosing?: boolean;
    size?: 'sm' | 'md' | 'lg';
  }>(),
  {
    customClass: '',
    closeOnEscape: true,
    closeOnOutsideClick: true,
    enableClosing: true,
    size: 'sm',
  },
);

const emit = defineEmits(['close']);

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'lg':
      return 'modal--lg';
    case 'md':
      return 'modal--md';
    case 'sm':
    default:
      return 'modal--sm';
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
