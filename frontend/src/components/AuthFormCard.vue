<template>
  <div class="overflow-hidden rounded-lg bg-white shadow-lg">
    <div class="relative flex justify-center py-6">
      <div class="absolute left-4 top-4 z-10">
        <NetworkLogo
          :loading="networkDetails.loading.value"
          :image-file="networkDetails.data.value?.imageFile"
          :network-name="networkDetails.data.value?.name || 'Network'"
        />
      </div>
    </div>

    <div class="w-full px-8 pb-10">
      <div class="mb-6 text-center">
        <h2 class="mb-1 text-2xl font-semibold text-slate-800">{{ title }}</h2>
        <p class="text-sm text-slate-600">{{ subtitle }}</p>
      </div>

      <p
        v-if="error"
        class="mb-4 rounded bg-red-100 px-4 py-2 text-center text-sm text-red-700"
        role="alert"
      >
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
  data: Ref<Network | null>;
}

defineProps<{
  title: string;
  subtitle: string;
  error: string;
  networkDetails: NetworkDetails;
}>();
</script>
