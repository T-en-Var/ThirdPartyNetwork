<template>
  <Teleport to="body">
    <div
      v-if="authStore.isModalOpen"
      class="auth-modal__overlay"
      @click.self="closeModal"
    >
      <div
        role="dialog"
        aria-modal="true"
        :aria-label="isLoginMode ? 'Sign in' : 'Create account'"
        class="auth-modal__panel"
      >
        <div class="auth-modal__logo">
          <img
            src="@/images/favicon-nobg.png"
            alt="Company Logo"
            class="auth-modal__logo-image"
          />
        </div>

        <button
          class="auth-modal__close"
          @click="closeModal"
          aria-label="Close"
        >
          ×
        </button>

        <div class="auth-modal__body">
          <div class="auth-modal__heading">
            <h2 class="auth-modal__title">
              {{ isLoginMode ? 'Sign In' : 'Create Account' }}
            </h2>
            <p class="auth-modal__subtitle">
              {{ isLoginMode ? 'Access your account' : 'Join the platform' }}
            </p>
          </div>

          <p
            v-if="error"
            class="alert alert--error"
            role="alert"
          >
            {{ error }}
          </p>

          <div
            v-if="isInDevelopment && isLoginMode"
            class="auth-modal__dev-accounts"
          >
            <div
              v-for="account in devAccounts"
              :key="account.email"
              class="auth-modal__dev-account"
            >
              <span>Log in as</span>
              <button
                type="button"
                class="auth-modal__dev-account-btn"
                @click="login({ email: account.email, password: account.email })"
              >
                {{ account.label }}
              </button>
            </div>
          </div>

          <AuthLoginForm
            v-if="isLoginMode"
            id-prefix="modal-login"
            email-label="Email"
            email-placeholder="Enter your email"
            :submitting="isSubmitting"
            @submit="login"
          />
          <AuthSignupForm
            v-else
            id-prefix="modal-signup"
            :initial-values="signupPrefill"
            :submitting="isSubmitting"
            @submit="signUp"
            @tos="redirectToTos"
          />

          <div class="auth-modal__switch">
            <p>{{ isLoginMode ? "Don't have an account?" : 'Already have an account?' }}</p>
            <button
              @click="switchMode"
              class="auth-modal__switch-btn"
            >
              {{ isLoginMode ? 'Create Account' : 'Sign In' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { safeAtob, safeBtoa } from '@/lib/utils';
import type { UserLogin, UserSignup } from '@/types';

import AuthLoginForm from '@/components/AuthLoginForm.vue';
import AuthSignupForm from '@/components/AuthSignupForm.vue';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const error = ref('');
const isSubmitting = ref(false);

const isLoginMode = computed(() => authStore.modalMode === 'login');

const signupPrefill = computed<Partial<UserSignup>>(() => ({
  username: (route.query.s_username as string) || '',
  firstName: (route.query.s_firstname as string) || '',
  lastName: (route.query.s_lastname as string) || '',
  email: (route.query.s_email as string) || '',
}));

const onKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') closeModal();
};

watch(
  () => authStore.isModalOpen,
  (open) => {
    error.value = '';
    if (open) document.addEventListener('keydown', onKeydown);
    else document.removeEventListener('keydown', onKeydown);
  },
  { immediate: true },
);

onUnmounted(() => document.removeEventListener('keydown', onKeydown));

const switchMode = () => {
  authStore.setModalMode(isLoginMode.value ? 'signup' : 'login');
  error.value = '';
};

const closeModal = () => {
  authStore.setModalOpen(false);
  error.value = '';
};

const finishAuthentication = () => {
  closeModal();
  const redirectPath = safeAtob(route.query.redirect as string | undefined) || '/networks';
  router.push(redirectPath);
};

const login = async (form: UserLogin) => {
  error.value = '';
  isSubmitting.value = true;

  await authStore.login(form);
  isSubmitting.value = false;

  if (authStore.error) {
    error.value = authStore.error;
    return;
  }

  finishAuthentication();
};

const signUp = async (form: UserSignup) => {
  error.value = '';
  isSubmitting.value = true;

  await authStore.signUp(form);
  isSubmitting.value = false;

  if (authStore.error) {
    error.value = authStore.error;
    return;
  }

  finishAuthentication();
};

const redirectToTos = (form: UserSignup) => {
  closeModal();

  let redirect = route.query.redirect as string | undefined;
  if (route.name !== 'terms-of-service') redirect = safeBtoa(route.fullPath);

  router.push({
    path: '/tos',
    query: {
      redirect,
      s_username: form.username,
      s_firstname: form.firstName,
      s_lastname: form.lastName,
      s_email: form.email,
    },
  });
};

const isInDevelopment = computed(() => window.location.hostname === 'localhost');

const devAccounts = [
  { label: 'Admin', email: 'admin@gmail.com' },
  { label: 'User', email: 'normaluser@gmail.com' },
  { label: 'MyUser', email: 'myuser@gmail.com' },
];
</script>
