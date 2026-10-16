<template>
  <form
    @submit.prevent="emit('submit', { email: email, password: password })"
    class="form"
  >
    <div class="form-field">
      <label
        :for="`${idPrefix}-email`"
        class="form-label"
        >{{ emailLabel }}</label
      >
      <input
        :id="`${idPrefix}-email`"
        v-model="email"
        type="text"
        required
        autocomplete="username"
        :placeholder="emailPlaceholder"
        aria-required="true"
        class="form-input"
      />
    </div>

    <div class="form-field">
      <label
        :for="`${idPrefix}-password`"
        class="form-label"
        >Password</label
      >
      <input
        :id="`${idPrefix}-password`"
        v-model="password"
        type="password"
        required
        autocomplete="current-password"
        placeholder="Enter your password"
        aria-required="true"
        class="form-input"
      />
    </div>

    <button
      type="submit"
      class="btn btn--primary btn--full"
      :disabled="submitting"
      aria-live="polite"
    >
      {{ submitting ? submittingLabel : submitLabel }}
    </button>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { LoginDto } from '@/types';

withDefaults(
  defineProps<{
    submitting?: boolean;
    emailLabel?: string;
    emailPlaceholder?: string;
    submitLabel?: string;
    submittingLabel?: string;
    idPrefix?: string;
  }>(),
  {
    submitting: false,
    emailLabel: 'Email/Username',
    emailPlaceholder: 'Enter your email or username',
    submitLabel: 'Sign In',
    submittingLabel: 'Signing in...',
    idPrefix: 'login',
  },
);

const emit = defineEmits<{
  submit: [form: LoginDto];
}>();

const email = ref('');
const password = ref('');
</script>
