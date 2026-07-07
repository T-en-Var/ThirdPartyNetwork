<template>
  <form
    @submit.prevent="emit('submit', { email: email, password: password })"
    class="flex flex-col gap-5"
  >
    <div>
      <label
        :for="`${idPrefix}-email`"
        class="mb-2 block text-sm font-medium text-gray-700"
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
        class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
    </div>

    <div>
      <label
        :for="`${idPrefix}-password`"
        class="mb-2 block text-sm font-medium text-gray-700"
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
        class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
    </div>

    <button
      type="submit"
      class="w-full rounded-md bg-blue-600 py-3 font-medium text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
      :disabled="submitting"
      aria-live="polite"
    >
      {{ submitting ? submittingLabel : submitLabel }}
    </button>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { UserLogin } from '@/types';

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
  submit: [form: UserLogin];
}>();

const email = ref('');
const password = ref('');
</script>
