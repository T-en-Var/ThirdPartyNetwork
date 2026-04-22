<template>
  <div
    :class="{
      'font-medium': !sensitive || isVisible,
      'flex flex-nowrap items-center justify-start whitespace-nowrap text-nowrap': 1,
    }"
  >
    <template v-if="sensitive && !isVisible">
      <span class="inline-block select-none blur-sm">••••••••</span>
      <span
        class="ml-2 cursor-pointer text-xs text-blue-600"
        @click="toggleVisibility"
        >({{ buttonText }})</span
      >
    </template>
    <template v-else>
      <span class="break-all">{{ value || 'Not provided' }}</span>
      <span
        v-if="sensitive"
        class="ml-2 cursor-pointer text-xs text-blue-600"
        @click="toggleVisibility"
        >(Click to hide)</span
      >
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

defineProps({
  value: {
    type: String,
    default: '',
  },
  sensitive: {
    type: Boolean,
    default: false,
  },
  buttonText: {
    type: String,
    default: 'Click to view',
  },
});

const emit = defineEmits(['toggle']);

const isVisible = ref(false);

const toggleVisibility = () => {
  isVisible.value = !isVisible.value;
  emit('toggle');
};
</script>
