<template>
  <div
    :class="classes"
    role="status"
  >
    <span class="sr-only">Loading...</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
    colorClass?: string;
  }>(),
  {
    size: 'md',
    colorClass: 'spinner-border--primary',
  },
);

const classes = computed(() => ['spinner-border', `spinner-border--${props.size}`, props.colorClass]);
</script>
