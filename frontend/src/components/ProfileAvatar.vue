<template>
  <div
    class="avatar"
    :style="{ width: `${sizeValue}px`, height: `${sizeValue}px` }"
  >
    <CloudinaryFile
      v-if="userProxy.imageFile"
      :file="userProxy.imageFile"
      :display-only="true"
      class="avatar__image"
    />
    <img
      v-else
      :src="avatarSrc"
      :alt="altText"
      class="avatar__image"
    />
  </div>
</template>

<script setup lang="ts">
import type { UserProxy } from '@/types';
import { computed } from 'vue';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

interface Props {
  userProxy: UserProxy;
  size?: number;
}

const props = defineProps<Props>();

const sizeValue = computed(() => props.size || 80);

const avatarSrc = computed(() => {
  const name = `${props.userProxy.firstName}+${props.userProxy.lastName}`;
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`;
});

const altText = computed(() => {
  return props.userProxy.username || `${props.userProxy.firstName} ${props.userProxy.lastName}`;
});
</script>
