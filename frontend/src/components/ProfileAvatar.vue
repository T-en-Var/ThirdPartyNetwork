<template>
  <div
    class="avatar"
    :style="{ width: `${sizeValue}px`, height: `${sizeValue}px` }"
  >
    <CdnFile
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
import type { FileDto, FileLightDto } from '@/types';
import { computed } from 'vue';
import CdnFile from '@/components/cdn/CdnFile.vue';

interface Props {
  userProxy: {
    username: string | null;
    firstName?: string | null;
    lastName?: string | null;
    imageFile: FileDto | FileLightDto | null;
  };
  size?: number;
}

const props = defineProps<Props>();

const sizeValue = computed(() => props.size || 80);

const avatarSrc = computed(() => {
  const name = `${props.userProxy.firstName ?? ''}+${props.userProxy.lastName ?? ''}`;
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`;
});

const altText = computed(() => {
  return (
    props.userProxy.username || `${props.userProxy.firstName ?? ''} ${props.userProxy.lastName ?? ''}`
  );
});
</script>
