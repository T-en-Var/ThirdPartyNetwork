<template>
  <AuthLayout
    :back-url="backUrl"
    max-width="md"
    :current-step="1"
  >
    <div
      v-if="networkDetails.loading.value"
      class="w-full text-center"
    >
      <p class="text-sm text-gray-600">Loading network details...</p>
      <div class="mt-4 flex justify-center">
        <LoadingSpinner />
      </div>
    </div>

    <div
      v-else-if="networkNotFound"
      class="w-full text-center"
    >
      <NetworkNotFound :network-id="networkId" />
    </div>

    <div
      v-else-if="!isValidUrl"
      class="w-full text-center"
    >
      <p
        class="mb-4 rounded bg-red-100 px-4 py-2 text-sm text-red-700"
        role="alert"
        aria-live="assertive"
      >
        Invalid URL. Please ensure all required parameters are present.
      </p>
    </div>

    <AuthFormCard
      v-else
      title="Sign In"
      :subtitle="
        networkDetails.data.value
          ? `Access your ${networkDetails.data.value.name} account`
          : 'Access your account'
      "
      :error="error"
      :network-details="networkDetails"
    >
      <AuthLoginForm
        :submitting="isSubmitting"
        submit-label="Log In"
        submitting-label="Logging in..."
        @submit="login"
      />

      <template #footer>
        <div class="mt-6 flex items-center justify-center gap-2 text-sm text-gray-600">
          <p>Don't have an account?</p>
          <button
            @click="goToSignup"
            class="font-medium text-blue-600 hover:underline"
          >
            Create Account
          </button>
        </div>
      </template>
    </AuthFormCard>

    <ConfirmationModal
      v-if="incompleteAccess"
      title="This network has updated their Access requirements"
      message="Please check your shared accesses and see that they comply with the network's requirements."
      button-text="Continue"
      color="blue"
      @close="incompleteAccess = null"
      @confirm="confirmIncompleteAccess"
    />
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { UserLogin } from '@/types';
import { useGlobalStore } from '@/stores/global';
import useNetworkAuthFlow, { TEMPORARY_ACCESS_TOKEN_KEY } from '@/composables/useNetworkAuthFlow';

import AuthLayout from '@/components/AuthLayout.vue';
import AuthFormCard from '@/components/AuthFormCard.vue';
import AuthLoginForm from '@/components/AuthLoginForm.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import NetworkNotFound from '@/components/NetworkNotFound.vue';

const global = useGlobalStore();
const flow = useNetworkAuthFlow();
const { networkDetails, networkId, backUrl, isValidUrl, networkNotFound, goToSignup } = flow;

const error = ref('');
const isSubmitting = ref(false);
const incompleteAccess = ref<{ accessToken: string; redirectUrl: string } | null>(null);

onMounted(async () => {
  localStorage.removeItem(TEMPORARY_ACCESS_TOKEN_KEY);
  await flow.loadNetwork();
});

const login = async (form: UserLogin) => {
  error.value = '';

  if (!networkDetails.data.value) {
    error.value = 'Network details are not available yet.';
    return;
  }

  global.startFetching();
  isSubmitting.value = true;

  try {
    const auth = await flow.authorize('login', {
      email: form.email.trim(),
      password: form.password,
    });

    const redirectUrl = flow.buildRedirectUrl(auth.code);

    if (flow.isAccessIncomplete(auth.accessToken)) {
      incompleteAccess.value = { accessToken: auth.accessToken, redirectUrl };
      return;
    }

    window.location.href = redirectUrl;
  } catch (err) {
    const { status, message } = flow.extractError(
      err,
      'An unexpected network error occurred. Please check your connection or try again later.',
    );

    error.value =
      status === 400 || status === 401
        ? 'Invalid credentials. Please check your username and password.'
        : message;
  } finally {
    global.stopFetching();
    isSubmitting.value = false;
  }
};

function confirmIncompleteAccess() {
  if (!incompleteAccess.value) return;
  flow.goToCompleteAccess(incompleteAccess.value.accessToken, incompleteAccess.value.redirectUrl);
}
</script>
