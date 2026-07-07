<template>
  <form
    @submit.prevent="handleSubmit"
    class="flex flex-col gap-5"
  >
    <div class="flex flex-wrap gap-5">
      <div class="min-w-[140px] flex-1">
        <label
          :for="`${idPrefix}-firstname`"
          class="mb-2 block text-sm font-medium text-gray-700"
          >First Name</label
        >
        <input
          :id="`${idPrefix}-firstname`"
          v-model="form.firstName"
          type="text"
          required
          autocomplete="given-name"
          placeholder="Your first name"
          aria-required="true"
          class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      <div class="min-w-[140px] flex-1">
        <label
          :for="`${idPrefix}-lastname`"
          class="mb-2 block text-sm font-medium text-gray-700"
          >Last Name</label
        >
        <input
          :id="`${idPrefix}-lastname`"
          v-model="form.lastName"
          type="text"
          required
          autocomplete="family-name"
          placeholder="Your last name"
          aria-required="true"
          class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>

    <div class="flex flex-wrap gap-5">
      <div class="min-w-[140px] flex-1">
        <label
          :for="`${idPrefix}-username`"
          class="mb-2 block text-sm font-medium text-gray-700"
          >Username</label
        >
        <input
          :id="`${idPrefix}-username`"
          v-model="form.username"
          type="text"
          required
          autocomplete="username"
          placeholder="Choose a username"
          aria-required="true"
          class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      <div class="min-w-[140px] flex-1">
        <label
          :for="`${idPrefix}-email`"
          class="mb-2 block text-sm font-medium text-gray-700"
          >Email</label
        >
        <input
          :id="`${idPrefix}-email`"
          v-model="form.email"
          type="email"
          required
          autocomplete="email"
          placeholder="Your email"
          aria-required="true"
          class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>

    <div class="flex flex-wrap gap-5">
      <div class="min-w-[140px] flex-1">
        <label
          :for="`${idPrefix}-password`"
          class="mb-2 block text-sm font-medium text-gray-700"
          >Password</label
        >
        <input
          :id="`${idPrefix}-password`"
          v-model="form.password"
          type="password"
          required
          autocomplete="new-password"
          placeholder="Create a password"
          aria-required="true"
          class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      <div class="min-w-[140px] flex-1">
        <label
          :for="`${idPrefix}-confirm-password`"
          class="mb-2 block text-sm font-medium text-gray-700"
          >Confirm Password</label
        >
        <input
          :id="`${idPrefix}-confirm-password`"
          v-model="confirmPassword"
          type="password"
          required
          autocomplete="new-password"
          placeholder="Confirm your password"
          aria-required="true"
          class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <p
          v-if="passwordMismatch"
          class="mt-1 text-sm text-red-600"
          role="alert"
        >
          Passwords do not match.
        </p>
      </div>
    </div>

    <div class="mt-1 flex items-center gap-3">
      <input
        :id="`${idPrefix}-tos`"
        v-model="confirmToS"
        type="checkbox"
        required
        aria-required="true"
        class="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
      />
      <label
        :for="`${idPrefix}-tos`"
        class="text-sm text-gray-700"
      >
        I accept the
        <a
          href="#"
          @click.prevent="emit('tos', { ...form })"
          class="cursor-pointer font-medium text-blue-600 hover:underline"
          >Terms and Conditions</a
        >.
      </label>
    </div>

    <button
      type="submit"
      class="mt-3 w-full rounded-md bg-blue-600 py-3 font-medium text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
      :disabled="submitting"
      aria-live="polite"
    >
      {{ submitting ? submittingLabel : submitLabel }}
    </button>
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import type { UserSignup } from '@/types';

const props = withDefaults(
  defineProps<{
    submitting?: boolean;
    initialValues?: Partial<UserSignup>;
    submitLabel?: string;
    submittingLabel?: string;
    idPrefix?: string;
  }>(),
  {
    submitting: false,
    initialValues: () => ({}),
    submitLabel: 'Create Account',
    submittingLabel: 'Creating account...',
    idPrefix: 'signup',
  },
);

const emit = defineEmits<{
  submit: [form: UserSignup];
  tos: [form: UserSignup];
}>();

const form = ref<UserSignup>({
  username: props.initialValues.username || '',
  firstName: props.initialValues.firstName || '',
  lastName: props.initialValues.lastName || '',
  email: props.initialValues.email || '',
  password: props.initialValues.password || '',
});

const confirmPassword = ref(props.initialValues.password || '');
const confirmToS = ref(false);
const submitAttempted = ref(false);

const passwordMismatch = computed(
  () => submitAttempted.value && form.value.password !== confirmPassword.value,
);

const handleSubmit = () => {
  submitAttempted.value = true;
  if (form.value.password !== confirmPassword.value) return;

  emit('submit', {
    username: form.value.username.trim(),
    firstName: form.value.firstName.trim(),
    lastName: form.value.lastName.trim(),
    email: form.value.email.trim(),
    password: form.value.password,
  });
};
</script>
