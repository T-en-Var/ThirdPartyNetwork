<template>
  <div :class="classes" role="status">
    <span class="sr-only">Loading...</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  colorClass?: string;
}>(), {
  size: 'md',
  colorClass: 'text-blue-500'
});

// Map size props to Tailwind width/height classes
const sizeClasses = {
  xs: 'w-3 h-3 border-[2px]',
  sm: 'w-4 h-4 border-[2px]',
  md: 'w-8 h-8 border-[3px]',
  lg: 'w-12 h-12 border-[4px]',
  xl: 'w-16 h-16 border-[5px]',
};

const classes = computed(() => [
  'spinner-border inline-block align-middle',
  sizeClasses[props.size],
  props.colorClass
]);
</script>

<style scoped>
.spinner-border {
  border-style: solid;
  border-color: currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}

@keyframes spinner-border {
  to { transform: rotate(360deg); }
}
</style>