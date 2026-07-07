<template>
  <Teleport to="body">
    <div
      v-if="authStore.isModalOpen"
      class="fixed inset-0 z-[1000] flex items-center justify-center overflow-auto bg-black/60 p-5 backdrop-blur-sm max-sm:p-0"
      @click.self="closeModal"
    >
      <div
        role="dialog"
        aria-modal="true"
        :aria-label="isLoginMode ? 'Sign in' : 'Create account'"
        class="relative max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-lg bg-white shadow-xl max-sm:max-h-full max-sm:rounded-none"
      >
        <div class="absolute left-4 top-4 z-10 flex h-9 w-9 items-center justify-center">
          <img
            src="@/images/favicon-nobg.png"
            alt="Company Logo"
            class="max-h-full max-w-full object-contain"
          />
        </div>

        <button
          class="absolute right-4 top-3 z-10 flex h-8 w-8 items-center justify-center rounded text-2xl text-gray-500 transition hover:bg-gray-100 hover:text-gray-800"
          @click="closeModal"
          aria-label="Close"
        >
          ×
        </button>

        <div class="px-8 pb-8 pt-14 max-sm:px-5">
          <div class="mb-6 text-center">
            <h2 class="mb-1 text-2xl font-semibold text-gray-800">
              {{ isLoginMode ? 'Sign In' : 'Create Account' }}
            </h2>
            <p class="text-sm text-gray-600">
              {{ isLoginMode ? 'Access your account' : 'Join the platform' }}
            </p>
          </div>

          <p
            v-if="error"
            class="mb-4 rounded bg-red-100 px-4 py-2 text-center text-sm text-red-700"
            role="alert"
          >
            {{ error }}
          </p>

          <div
            v-if="isInDevelopment && isLoginMode"
            class="mb-4 flex flex-row items-center justify-center gap-8 text-sm"
          >
            <div
              v-for="account in devAccounts"
              :key="account.email"
              class="flex flex-col items-center"
            >
              <span>Log in as</span>
              <button
                type="button"
                class="cursor-pointer text-blue-500 underline"
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

          <div
            class="mt-6 flex flex-wrap items-center justify-center gap-2 text-center text-sm text-gray-600"
          >
            <p>{{ isLoginMode ? "Don't have an account?" : 'Already have an account?' }}</p>
            <button
              @click="switchMode"
              class="font-medium text-blue-600 hover:underline"
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
import { safeAtob } from '@/lib/utils';
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
  if (route.name !== 'terms-of-service') redirect = btoa(route.fullPath);

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
