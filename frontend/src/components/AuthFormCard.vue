<template>
  <div class="auth-form-card">
    <div class="auth-form-card__logo-wrap">
      <div class="auth-form-card__logo">
        <NetworkLogo
          :loading="networkDetails.loading.value"
          :image-file="networkDetails.data.value?.imageFile"
          :network-name="networkDetails.data.value?.name || 'Network'"
        />
      </div>
    </div>

    <div class="auth-form-card__body">
      <div class="auth-form-card__heading">
        <h2 class="auth-form-card__title">{{ title }}</h2>
        <p class="auth-form-card__subtitle">{{ subtitle }}</p>
      </div>

      <p
        v-if="error"
        class="alert alert--error"
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
import type { NetworkDto } from '@/types';

interface NetworkDetails {
  loading: Ref<boolean>;
  data: Ref<NetworkDto | null>;
}

withDefaults(
  defineProps<{
    title: string;
    subtitle?: string;
    error?: string;
    networkDetails: NetworkDetails;
  }>(),
  {
    subtitle: '',
    error: '',
  },
);
</script>
