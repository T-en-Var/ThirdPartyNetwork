<template>
  <div :class="['rounded-full overflow-hidden', sizeClasses]">
    <CloudinaryFile v-if="userProxy.imageFile" :file="userProxy.imageFile" :display-only="true" class="h-full w-full object-cover" />
    <img v-else :src="avatarSrc" :alt="altText" class="h-full w-full object-cover" />
  </div>
</template>

<script setup lang="ts">
import type { UserProxy } from '@/types';
import { computed } from 'vue'
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

interface Props {
  userProxy: UserProxy
  size?: number // in pixels
}

const props = defineProps<Props>()

const sizeValue = computed(() => props.size || 80) // default size 80px

const sizeClasses = computed(() => {
  return `h-${sizeValue.value} w-${sizeValue.value}`
})

const avatarSrc = computed(() => {
  const name = `${props.userProxy.firstName}+${props.userProxy.lastName}`
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`
})

const altText = computed(() => {
  return props.userProxy.username || `${props.userProxy.firstName} ${props.userProxy.lastName}`
})
</script>
