<template>
  <div
    @click="toggleVisibility"
    class="cursor-pointer"
    :class="{ 'font-medium': !sensitive || isVisible }"
  >
    <template v-if="sensitive && !isVisible">
      <span class="inline-block bg-gray-200 blur-sm select-none">••••••••</span>
      <span class="text-xs text-blue-600 ml-2">(Click to view)</span>
    </template>
    <template v-else>
      <span  class="break-all">{{ value || 'Not provided' }}</span>
      <span v-if="sensitive" class="text-xs text-blue-600 ml-2">(Click to hide)</span>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, defineEmits, defineProps } from 'vue';

defineProps({
  value: {
    type: String,
    default: '',
  },
  sensitive: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['toggle']);

const isVisible = ref(false);

const toggleVisibility = () => {
  isVisible.value = !isVisible.value;
  emit('toggle');
};
</script>
