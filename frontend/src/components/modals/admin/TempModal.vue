<template>
  <modal-container
    :title="'Soon (TM)'"
    @close="$emit('close')"
  >
    <div>
      <span>Will open a new screen to view all user's content/info.</span>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import ModalContainer from '@/components/modals/ModalContainer.vue';

defineEmits<{
  (e: 'close'): void;
}>();
</script>
