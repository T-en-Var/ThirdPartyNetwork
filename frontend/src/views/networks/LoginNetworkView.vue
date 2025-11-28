<template>
  <AuthLayout :back-url="(route.query.back as string)" max-width="md" :current-step="1">
    <div v-if="networkDetails.loading.value" class="text-center w-full max-w-md">
      <p class="text-gray-600 text-sm">Loading network details...</p>
      <div class="mt-4 flex justify-center">
        <svg class="animate-spin h-6 w-6 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
          </path>
        </svg>
      </div>
    </div>

    <div v-else-if="networkNotFoundError" class="text-center w-full max-w-md">
      <NetworkNotFound :network-id="networkId" />
    </div>

    <div v-else-if="validUrl">
      <AuthFormCard title="Sign In"
        :subtitle="networkDetails.network.value ? `Access your ${networkDetails.network.value?.name} account` : 'Access your account'"
        :error="error" :network-details="networkDetails">
        <form @submit.prevent="login" class="flex flex-col gap-5">
          <div>
            <label for="email" class="block mb-2 text-sm font-medium text-gray-700">Email/Username</label>
            <input id="email" v-model="email" type="text" required autocomplete="username"
              placeholder="Enter your email or username" aria-required="true"
              class="w-full px-4 py-3 border border-gray-300 rounded-md text-gray-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" />
          </div>

          <div>
            <label for="password" class="block mb-2 text-sm font-medium text-gray-700">Password</label>
            <input id="password" v-model="password" type="password" required autocomplete="current-password"
              placeholder="Enter your password" aria-required="true"
              class="w-full px-4 py-3 border border-gray-300 rounded-md text-gray-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" />
          </div>

          <button type="submit"
            class="w-full py-3 bg-blue-600 text-white rounded-md font-medium hover:bg-blue-700 transition disabled:bg-blue-300"
            :disabled="pageLoading" aria-label="Log in to your account" aria-live="polite">
            {{ pageLoading ? 'Logging in...' : 'Log In' }}
          </button>
        </form>

        <template #footer>
          <div class="flex justify-center items-center gap-2 mt-6 text-sm text-gray-600">
            <p>Don't have an account?</p>
            <button @click="navigateToSignup" class="text-blue-600 hover:underline font-medium">Create Account</button>
          </div>
        </template>
      </AuthFormCard>
    </div>

    <!-- 4. Invalid URL check -->
    <div v-else class="text-center w-full max-w-md">
      <p class="text-red-700 text-sm mb-4 px-4 py-2 rounded bg-red-100" role="alert" aria-live="assertive">
        Invalid URL. Please ensure all required parameters are present.
      </p>
    </div>

    <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
      :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
      @close="showConfirmationModal = false" @confirm="confirmationAction" />
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { AxiosError } from 'axios';
import type { AccessTokenClaims, AuthorizationCode, ErrorMessage } from '@/types';
import useNetworkDetails from '@/composables/useNetworkDetails';
import { decodeJWT } from '@/lib/utils';
import { useGlobalStore } from '@/stores/global';
import rawApi from '@/api/rawApi';

// Reusable Components (Assuming these paths are correct in the project structure)
import AuthLayout from '@/components/AuthLayout.vue';
import AuthFormCard from '@/components/AuthFormCard.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import NetworkNotFound from '@/components/NetworkNotFound.vue';

// --- Setup ---
const global = useGlobalStore();
const router = useRouter();
const route = useRoute();
const networkDetails = useNetworkDetails();

// --- State Management ---
const email = ref('');
const password = ref('');
const error = ref('');
const pageLoading = ref(false);

let temporaryAccessToken = '';

// Modal State
const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const isSubmitting = ref(false);
const confirmationAction = ref(() => { });

// --- Computed Route Params/Queries (Vue Style-Guide C) ---
const networkId = computed(() => route.params.networkId as string);
const clientId = computed(() => route.query.clientId as string);
const codeChallenge = computed(() => route.query.codeChallenge as string);
const state = computed(() => route.query.state as string);
const backUrlQuery = computed(() => route.query.back as string);

const networkNotFoundError = computed(() => {
  return !!networkId.value && !networkDetails.network.value;
});

const validUrl = computed(() => {
  // Only check URL validity if the network was loaded successfully
  console.log(networkId.value)
  console.log(clientId.value)
  console.log(codeChallenge.value)
  console.log(state.value)
  return !!networkId.value && !!clientId.value && !!codeChallenge.value && !!state.value;
});

// --- Lifecycle Hooks ---
onMounted(async () => {
  localStorage.removeItem('temporaryAccessToken');
  // Fetch details, error handling is handled by the `networkNotFoundError` computed property
  await networkDetails.fetchNetworkDetails(networkId.value);
  pageLoading.value = false;
});

// --- Methods ---

const navigateToSignup = () => {
  router.push(`/networks/${networkId.value}/signup?clientId=${clientId.value}&codeChallenge=${codeChallenge.value}&state=${state.value}&back=${backUrlQuery.value}`);
};

const login = async () => {
  error.value = ''; // Clear previous errors

  if (!networkDetails.network.value) {
    error.value = 'Network details are not available yet.';
    return;
  }

  if (!email.value || !password.value) {
    error.value = 'Please enter both email/username and password.';
    return;
  }

  global.startFetching();
  pageLoading.value = true;

  const network = networkDetails.network.value;
  const loginUrl = `/auth/${networkId.value}/login?clientId=${clientId.value}&redirectUri=${network.redirectURI}&codeChallenge=${codeChallenge.value}`;

  try {
    const response = await rawApi.post<AuthorizationCode>(loginUrl, { email: email.value, password: password.value });

    const accessToken = response.data.accessToken;
    const jwt = decodeJWT<AccessTokenClaims>(accessToken);

    // Check for incomplete access
    if (jwt.AccessIncomplete === "true") {
      temporaryAccessToken = accessToken;
      const redirectUrl = `${network.redirectURI}?code=${response.data.code}&state=${state.value}`;
      handleIncompleteAccess(redirectUrl);
      return;
    }

    // Final Redirect
    const redirectUrl = `${network.redirectURI}?code=${response.data.code}&state=${state.value}`;
    window.location.href = redirectUrl;

  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;

    // Handle specific Axios status codes for better error messages
    if (axiosError.response?.status === 400 || axiosError.response?.status === 401) {
      error.value = 'Invalid credentials. Please check your username and password.';
    } else if (axiosError.response?.data?.message) {
      error.value = axiosError.response.data.message;
    } else {
      error.value = "An unexpected network error occurred. Please check your connection or try again later.";
    }

  } finally {
    global.stopFetching();
    pageLoading.value = false;
  }
};

function handleIncompleteAccess(redirectUri: string) {
  showConfirmationModal.value = true;
  confirmationTitle.value = 'This network has updated their Access requirements';
  confirmationMessage.value = 'Please check your shared accesses and see that they comply with the network\'s requirements.';
  confirmButtonText.value = 'Continue';
  confirmButtonColor.value = 'blue';

  confirmationAction.value = () => {
    localStorage.setItem('temporaryAccessToken', temporaryAccessToken);
    // Navigate to a dedicated page to complete access consent
    router.push(`/networks/${networkId.value}/complete-access?redirectUri=${btoa(redirectUri)}`);
  };
}
</script>