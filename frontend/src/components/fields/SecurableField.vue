<template>
  <div
    class="securable-field"
    :class="{ 'securable-field--emphasized': !sensitive || isVisible }"
  >
    <template v-if="sensitive && !isVisible">
      <span class="securable-field__mask">••••••••</span>
      <span
        class="securable-field__toggle"
        @click="toggleVisibility"
        >({{ buttonText }})</span
      >
    </template>
    <template v-else>
      <span class="securable-field__value">{{ value || 'Not provided' }}</span>
      <span
        v-if="sensitive"
        class="securable-field__toggle"
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
