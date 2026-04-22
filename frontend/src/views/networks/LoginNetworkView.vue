<template>
  <AuthLayout
    :back-url="route.query.back as string"
    max-width="md"
    :current-step="1"
  >
    <div
      v-if="networkDetails.loading.value"
      class="w-full max-w-md text-center"
    >
      <p class="text-sm text-gray-600">Loading network details...</p>
      <div class="mt-4 flex justify-center">
        <svg
          class="h-6 w-6 animate-spin text-blue-600"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
      </div>
    </div>

    <div
      v-else-if="networkNotFoundError"
      class="w-full max-w-md text-center"
    >
      <NetworkNotFound :network-id="networkId" />
    </div>

    <div v-else-if="validUrl">
      <AuthFormCard
        title="Sign In"
        :subtitle="
          networkDetails.data.value
            ? `Access your ${networkDetails.data.value.name} account`
            : 'Access your account'
        "
        :error="error"
        :network-details="networkDetails"
      >
        <form
          @submit.prevent="login"
          class="flex flex-col gap-5"
        >
          <div>
            <label
              for="email"
              class="mb-2 block text-sm font-medium text-gray-700"
              >Email/Username</label
            >
            <input
              id="email"
              v-model="email"
              type="text"
              required
              autocomplete="username"
              placeholder="Enter your email or username"
              aria-required="true"
              class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label
              for="password"
              class="mb-2 block text-sm font-medium text-gray-700"
              >Password</label
            >
            <input
              id="password"
              v-model="password"
              type="password"
              required
              autocomplete="current-password"
              placeholder="Enter your password"
              aria-required="true"
              class="w-full rounded-md border border-gray-300 px-4 py-3 text-gray-800 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <button
            type="submit"
            class="w-full rounded-md bg-blue-600 py-3 font-medium text-white transition hover:bg-blue-700 disabled:bg-blue-300"
            :disabled="pageLoading"
            aria-label="Log in to your account"
            aria-live="polite"
          >
            {{ pageLoading ? 'Logging in...' : 'Log In' }}
          </button>
        </form>

        <template #footer>
          <div class="mt-6 flex items-center justify-center gap-2 text-sm text-gray-600">
            <p>Don't have an account?</p>
            <button
              @click="navigateToSignup"
              class="font-medium text-blue-600 hover:underline"
            >
              Create Account
            </button>
          </div>
        </template>
      </AuthFormCard>
    </div>

    <!-- 4. Invalid URL check -->
    <div
      v-else
      class="w-full max-w-md text-center"
    >
      <p
        class="mb-4 rounded bg-red-100 px-4 py-2 text-sm text-red-700"
        role="alert"
        aria-live="assertive"
      >
        Invalid URL. Please ensure all required parameters are present.
      </p>
    </div>

    <ConfirmationModal
      v-if="showConfirmationModal"
      :title="confirmationTitle"
      :message="confirmationMessage"
      :button-text="confirmButtonText"
      :color="confirmButtonColor"
      :is-submitting="isSubmitting"
      @close="showConfirmationModal = false"
      @confirm="confirmationAction"
    />
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { AxiosError } from 'axios';
import type { AccessTokenClaims, AuthorizationCode, ErrorMessage } from '@/types';
import { decodeJWT } from '@/lib/utils';
import { useGlobalStore } from '@/stores/global';
import rawApi from '@/api/rawApi';

import AuthLayout from '@/components/AuthLayout.vue';
import AuthFormCard from '@/components/AuthFormCard.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import NetworkNotFound from '@/components/NetworkNotFound.vue';
import useNetworks from '@/composables/useNetworks';

const global = useGlobalStore();
const router = useRouter();
const route = useRoute();
const networkDetails = useNetworks().fetchNetworkDetails;

const email = ref('');
const password = ref('');
const error = ref('');
const pageLoading = ref(false);

let temporaryAccessToken = '';

const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const isSubmitting = ref(false);
const confirmationAction = ref(() => {});

const networkId = computed(() => route.params.networkId as string);
const clientId = computed(() => route.query.clientId as string);
const codeChallenge = computed(() => route.query.codeChallenge as string);
const state = computed(() => route.query.state as string);
const backUrlQuery = computed(() => route.query.back as string);

const networkNotFoundError = computed(() => {
  return !!networkId.value && !networkDetails.data.value;
});

const validUrl = computed(() => {
  return !!networkId.value && !!clientId.value && !!codeChallenge.value && !!state.value;
});

onMounted(async () => {
  localStorage.removeItem('temporaryAccessToken');
  await networkDetails.execute(networkId.value);
  pageLoading.value = false;
});

const navigateToSignup = () => {
  router.push(
    `/networks/${networkId.value}/signup?clientId=${clientId.value}&codeChallenge=${codeChallenge.value}&state=${state.value}&back=${backUrlQuery.value}`,
  );
};

const login = async () => {
  error.value = '';

  const network = networkDetails.data.value;
  if (!network) {
    error.value = 'Network details are not available yet.';
    return;
  }

  if (!email.value || !password.value) {
    error.value = 'Please enter both email/username and password.';
    return;
  }

  global.startFetching();
  pageLoading.value = true;

  const loginUrl = `/auth/${networkId.value}/login?clientId=${clientId.value}&redirectUri=${network.redirectURI}&codeChallenge=${codeChallenge.value}`;

  try {
    const response = await rawApi.post<AuthorizationCode>(loginUrl, {
      email: email.value,
      password: password.value,
    });

    const accessToken = response.data.accessToken;
    const jwt = decodeJWT<AccessTokenClaims>(accessToken);

    if (jwt.AccessIncomplete === 'true') {
      temporaryAccessToken = accessToken;
      const redirectUrl = `${network.redirectURI}?code=${response.data.code}&state=${state.value}`;
      handleIncompleteAccess(redirectUrl);
      return;
    }

    const redirectUrl = `${network.redirectURI}?code=${response.data.code}&state=${state.value}`;
    window.location.href = redirectUrl;
  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;

    if (axiosError.response?.status === 400 || axiosError.response?.status === 401) {
      error.value = 'Invalid credentials. Please check your username and password.';
    } else if (axiosError.response?.data?.message) {
      error.value = axiosError.response.data.message;
    } else {
      error.value =
        'An unexpected network error occurred. Please check your connection or try again later.';
    }
  } finally {
    global.stopFetching();
    pageLoading.value = false;
  }
};

function handleIncompleteAccess(redirectUri: string) {
  showConfirmationModal.value = true;
  confirmationTitle.value = 'This network has updated their Access requirements';
  confirmationMessage.value =
    "Please check your shared accesses and see that they comply with the network's requirements.";
  confirmButtonText.value = 'Continue';
  confirmButtonColor.value = 'blue';

  confirmationAction.value = () => {
    localStorage.setItem('temporaryAccessToken', temporaryAccessToken);
    router.push(`/networks/${networkId.value}/complete-access?redirectUri=${btoa(redirectUri)}`);
  };
}
</script>
