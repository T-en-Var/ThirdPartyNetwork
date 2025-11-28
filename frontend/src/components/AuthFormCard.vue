<template>
  <div class="bg-white rounded-lg shadow-lg overflow-hidden">
    <div class="flex justify-center py-6 relative">
      <div class="absolute top-4 left-4 z-10">
        <NetworkLogo :loading="networkDetails.loading.value" :image-file="networkDetails.network.value?.imageFile"
          :network-name="networkDetails.network.value?.name || 'Network'" />
      </div>
    </div>

    <div class="w-full px-8 pb-10">
      <div class="mb-6 text-center">
        <h2 class="text-2xl font-semibold text-slate-800 mb-1">{{ title }}</h2>
        <p class="text-sm text-slate-600">{{ subtitle }}</p>
      </div>

      <p v-if="error" class="text-red-700 text-sm text-center mb-4 px-4 py-2 rounded bg-red-100" role="alert">
        {{ error }}
      </p>

      <slot />

      <slot name="footer" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Ref } from 'vue';
import NetworkLogo from '@/components/NetworkLogo.vue';
import type { Network } from '@/types';

interface NetworkDetails {
  loading: Ref<boolean>;
  network: Ref<Network | null>;
}

defineProps<{
  title: string;
  subtitle: string;
  error: string;
  networkDetails: NetworkDetails;
}>();
</script>