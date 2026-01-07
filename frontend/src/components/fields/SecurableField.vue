<template>
  <div
    @click="toggleVisibility"
    class="cursor-pointer"
    :class="{ 'font-medium': !sensitive || isVisible, 'flex justify-start items-center': true }"
  >
    <template v-if="sensitive && !isVisible">
      <span class="inline-block blur-sm select-none">••••••••</span>
      <span class="text-xs text-blue-600 ml-2">({{ buttonText }})</span>
    </template>
    <template v-else>
      <span  class="break-all">{{ value || 'Not provided' }}</span>
      <span v-if="sensitive" class="text-xs text-blue-600 ml-2">(Click to hide)</span>
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
