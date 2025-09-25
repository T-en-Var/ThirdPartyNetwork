<template>
  <div class="w-full min-h-screen flex justify-center items-center bg-gray-100 relative p-5 mt-8">
    <div
      class="absolute top-5 left-5 flex items-center gap-1 justify-center cursor-pointer text-sm text-slate-700 px-4 py-2 rounded bg-white shadow hover:bg-gray-100 hover:shadow-md transition"
      @click="goBack">
      <span class="relative w-4 h-4">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
          stroke-linecap="round" stroke-linejoin="round">
          <path d="M19 12H5" />
          <path d="M12 19l-7-7 7-7" />
        </svg>
      </span>
      <span>
        Back
      </span>
    </div>

    <div v-if="validUrl" class="w-full max-w-md">
      <div class="bg-white rounded-lg shadow-lg overflow-hidden">
        <div class="flex justify-center py-6 relative">
          <div class="absolute top-4 left-4 z-10">
            <div class="w-14 h-14 bg-white rounded-md shadow flex items-center justify-center p-2">
              <div v-if="networkDetails.loading.value" class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin"></div>
              <CloudinaryFile v-else-if="networkDetails.network.value?.imageFile" :display-only="true"
                :file="networkDetails.network.value?.imageFile" class="w-10 max-h-10 object-cover" />
              <div v-else class="logo">
                <img
                  :src="`https://ui-avatars.com/api/?name=${networkDetails.network.value?.name}&size=24&background=random`"
                  :alt="networkDetails.network.value?.name" />
              </div>
            </div>
          </div>
        </div>

        <div class="w-full px-8 pb-10">
          <div class="mb-6 text-center">
            <h2 class="text-2xl font-semibold text-slate-800 mb-1">Sign In</h2>
            <p class="text-gray-600 text-sm">
              {{ networkDetails.network.value
                ? `Access your ${networkDetails.network.value?.name} account`
                : 'Access your account' }}
            </p>
          </div>

          <p v-if="error" class="text-red-700 text-sm text-center mb-4 px-4 py-2 rounded bg-red-100">{{ error }}</p>

          <form @submit.prevent="login" class="flex flex-col gap-5">
            <div>
              <label for="email" class="block mb-2 text-sm font-medium text-gray-700">Email</label>
              <input id="email" v-model="email" type="text" required placeholder="Enter your username"
                class="w-full px-4 py-3 border border-gray-300 rounded-md text-gray-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" />
            </div>

            <div>
              <label for="password" class="block mb-2 text-sm font-medium text-gray-700">Password</label>
              <input id="password" v-model="password" type="password" required placeholder="Enter your password"
                class="w-full px-4 py-3 border border-gray-300 rounded-md text-gray-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" />
            </div>

            <button type="submit"
              class="w-full py-3 bg-blue-600 text-white rounded-md font-medium hover:bg-blue-700 transition disabled:bg-blue-300"
              :disabled="isLoading">
              {{ isLoading ? 'Signing in...' : 'Sign In' }}
            </button>
          </form>

          <div class="flex justify-center items-center gap-2 mt-6 text-sm text-gray-600">
            <p>Don't have an account?</p>
            <button @click="navigateToSignup" class="text-blue-600 hover:underline font-medium">Create Account</button>
          </div>
        </div>
      </div>
    </div>

    <div v-else>
      <p class="text-red-700 text-sm text-center mb-4 px-4 py-2 rounded bg-red-100">Invalid URL. Please contact your administrator.</p>
    </div>
    
    <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
      :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
      @close="showConfirmationModal = false" @confirm="confirmationAction" />
  </div>
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
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const isSubmitting = ref(false);
const confirmationAction = ref(() => { });

const global = useGlobalStore();
const router = useRouter();
const route = useRoute();

const email = ref('');
const password = ref('');
const error = ref('');
const isLoading = ref(false);

const networkDetails = useNetworkDetails();
const networkId = computed(() => route.params.networkId as string);
const clientId = computed(() => route.query.clientId as string);
const codeChallenge = computed(() => route.query.codeChallenge as string);
const state = computed(() => route.query.state as string);
const back = computed(() => atob(route.query.back as string));

let temporaryAccessToken = '';

const validUrl = computed(() => {
  return networkId.value && clientId.value && codeChallenge.value && state.value;
})

onMounted(async () => {
  localStorage.removeItem('temporaryAccessToken');
  await networkDetails.fetchNetworkDetails(networkId.value);
});

const goBack = () => {
  window.location.href = back.value;
};

const navigateToSignup = () => {
  router.push(`/networks/${networkId.value}/signup?clientId=${clientId.value}&codeChallenge=${codeChallenge.value}&state=${state.value}&back=${btoa(back.value)}`);
};

const login = async () => {
  if (!networkDetails.network.value) {
    error.value = 'Network could not be loaded, please try again later.';
    return;
  }

  if (!email.value || !password.value) {
    error.value = 'Please enter both username and password';
    return;
  }

  global.startFetching();
  isLoading.value = true;
  error.value = '';

  const loginUrl = `/auth/${networkId.value}/login?clientId=${clientId.value}&redirectUri=${networkDetails.network.value.redirectURI}&codeChallenge=${codeChallenge.value}`

  let response;

  try {
    response = await rawApi.post<AuthorizationCode>(loginUrl, { email: email.value, password: password.value });
  } catch (err) {
    error.value = (err as AxiosError<ErrorMessage>).response?.data.message || "Something went wrong, please try again";
    global.stopFetching();
    isLoading.value = false;
    return;
  }

  if (response.status === 400) {
    error.value = 'Invalid credentials. Please try again.';
    global.stopFetching();
    isLoading.value = false;
    return;
  }

  const redirectUrl = `${networkDetails.network.value?.redirectURI}?code=${response.data.code}&state=${state.value}`;
  const jwt = decodeJWT<AccessTokenClaims>(response.data.accessToken);

  if (jwt.AccessIncomplete === "true") {
    temporaryAccessToken = response.data.accessToken;
    global.stopFetching();
    isLoading.value = false;
    handleIncompleteAccess(redirectUrl);
    return;
  }

  global.stopFetching();
  isLoading.value = false;

  window.location.href = redirectUrl;
};

function handleIncompleteAccess(redirectUri: string) {
  showConfirmationModal.value = true;
  confirmationTitle.value = 'This network has updated their Access requirements'
  confirmationMessage.value = 'Please check your shared accesses and see that they comply with the network\'s requirements.'
  confirmButtonText.value = 'Continue'
  confirmButtonColor.value = 'white'

  confirmationAction.value = () => {
    localStorage.setItem('temporaryAccessToken', temporaryAccessToken);
    router.push(`/networks/${networkId.value}/complete-access?redirectUri=${btoa(redirectUri)}`);
  }
}
</script>