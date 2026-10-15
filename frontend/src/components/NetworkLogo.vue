<template>
  <div class="network-logo">
    <div
      v-if="loading"
      class="spinner-border network-logo__spinner"
      role="progressbar"
      aria-valuenow="0"
      aria-valuemin="0"
      aria-valuemax="100"
      aria-label="Loading logo"
    ></div>

    <CdnFile
      v-else-if="imageFile"
      :display-only="true"
      :file="imageFile"
      class="network-logo__image"
      alt="Network Logo"
    />

    <div
      v-else
      class="network-logo__fallback"
    >
      <img
        :src="`https://ui-avatars.com/api/?name=${networkName}&size=24&background=random`"
        :alt="`${networkName} logo`"
        class="network-logo__fallback-image"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FileDto } from '@/types';
import CdnFile from './cdn/CdnFile.vue';

const props = defineProps<{
  loading: boolean;
  imageFile?: FileDto | null;
  networkName: string;
}>();

const networkName = props.networkName || 'Network';
</script>
