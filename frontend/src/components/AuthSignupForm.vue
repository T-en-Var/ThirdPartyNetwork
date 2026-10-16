<template>
  <form
    @submit.prevent="handleSubmit"
    class="form"
  >
    <div class="auth-signup-form__row">
      <div class="auth-signup-form__col form-field">
        <label
          :for="`${idPrefix}-firstname`"
          class="form-label"
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
          class="form-input"
        />
      </div>
      <div class="auth-signup-form__col form-field">
        <label
          :for="`${idPrefix}-lastname`"
          class="form-label"
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
          class="form-input"
        />
      </div>
    </div>

    <div class="auth-signup-form__row">
      <div class="auth-signup-form__col form-field">
        <label
          :for="`${idPrefix}-username`"
          class="form-label"
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
          class="form-input"
        />
      </div>
      <div class="auth-signup-form__col form-field">
        <label
          :for="`${idPrefix}-email`"
          class="form-label"
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
          class="form-input"
        />
      </div>
    </div>

    <div class="auth-signup-form__row">
      <div class="auth-signup-form__col form-field">
        <label
          :for="`${idPrefix}-password`"
          class="form-label"
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
          class="form-input"
        />
      </div>
      <div class="auth-signup-form__col form-field">
        <label
          :for="`${idPrefix}-confirm-password`"
          class="form-label"
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
          class="form-input"
        />
        <p
          v-if="passwordMismatch"
          class="form-error"
          role="alert"
        >
          Passwords do not match.
        </p>
      </div>
    </div>

    <div class="auth-signup-form__tos">
      <input
        :id="`${idPrefix}-tos`"
        v-model="confirmToS"
        type="checkbox"
        required
        aria-required="true"
        class="checkbox"
      />
      <label
        :for="`${idPrefix}-tos`"
        class="auth-signup-form__tos-label"
      >
        I accept the
        <a
          href="#"
          @click.prevent="emit('tos', { ...form })"
          class="auth-signup-form__tos-link"
          >Terms and Conditions</a
        >.
      </label>
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
import { computed, ref } from 'vue';
import type { CreateUserProxyDto } from '@/types';

const props = withDefaults(
  defineProps<{
    submitting?: boolean;
    initialValues?: Partial<CreateUserProxyDto>;
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
  submit: [form: CreateUserProxyDto];
  tos: [form: CreateUserProxyDto];
}>();

const form = ref<CreateUserProxyDto>({
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
