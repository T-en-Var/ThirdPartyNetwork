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

    <div class="w-full max-w-xl" v-if="signUpStep == 1">
      <div class="w-full bg-white rounded-lg shadow-lg overflow-hidden">
        <div class="flex justify-center py-6 relative">
          <div class="absolute top-4 left-4 z-10">
            <div class="w-14 h-14 bg-white rounded-md shadow flex items-center justify-center p-2">
              <img v-if="!networkDetails.loading.value" :src="imageSrc" alt="Network Logo"
                class="w-full h-full object-contain" />
              <div v-else class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin"></div>
            </div>
          </div>
        </div>

        <div class="w-full px-8 pb-10">
          <div class="mb-6 text-center">
            <h2 class="text-2xl font-semibold text-slate-800 mb-1">Create Account</h2>
            <p class="text-sm text-slate-600">
              {{ networkDetails.network.value ? `Join ${networkDetails.network.value.name}` : 'Join the platform' }}
            </p>
          </div>

          <p v-if="error" class="text-red-700 text-sm text-center mb-4 px-4 py-2 rounded bg-red-100">{{ error }}</p>

          <form @submit.prevent="signUp" class="flex flex-col gap-5">
            <div class="flex flex-wrap gap-5">
              <div class="flex-1 min-w-[140px]">
                <label for="signup-firstname" class="block mb-2 text-sm text-slate-700 font-medium">First Name</label>
                <input id="signup-firstname" v-model="signupFirstname" type="text" required
                  placeholder="Your first name"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div class="flex-1 min-w-[140px]">
                <label for="signup-lastname" class="block mb-2 text-sm text-slate-700 font-medium">Last Name</label>
                <input id="signup-lastname" v-model="signupLastname" type="text" required placeholder="Your last name"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>

            <div class="flex flex-wrap gap-5">
              <div class="flex-1 min-w-[140px]">
                <label for="signup-username" class="block mb-2 text-sm text-slate-700 font-medium">Username</label>
                <input id="signup-username" v-model="signupUsername" type="text" required
                  placeholder="Choose a username"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div class="flex-1 min-w-[140px]">
                <label for="signup-email" class="block mb-2 text-sm text-slate-700 font-medium">Email</label>
                <input id="signup-email" v-model="signupEmail" type="email" required placeholder="Your email"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>

            <div class="flex flex-wrap gap-5">
              <div class="flex-1 min-w-[140px]">
                <label for="signup-password" class="block mb-2 text-sm text-slate-700 font-medium">Password</label>
                <input id="signup-password" v-model="signupPassword" type="password" required
                  placeholder="Create a password"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div class="flex-1 min-w-[140px]">
                <label for="confirm-password" class="block mb-2 text-sm text-slate-700 font-medium">Confirm
                  Password</label>
                <input id="confirm-password" v-model="confirmPassword" type="password" required
                  placeholder="Confirm your password"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>

            <div class="flex items-center gap-3 mt-1">
              <input id="confirm-tos" v-model="confirmToS" type="checkbox" required
                class="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500" />
              <label for="confirm-tos" class="text-sm text-slate-700">
                I accept the <span class="text-blue-600 font-medium cursor-pointer hover:underline"
                  @click="redirectToTos">Terms and Conditions</span>.
              </label>
            </div>

            <button type="submit"
              class="w-full py-3 mt-3 text-white bg-blue-600 hover:bg-blue-700 font-medium rounded transition disabled:bg-blue-300 disabled:cursor-not-allowed"
              :disabled="isSigningUp">
              {{ isSigningUp ? 'Creating account...' : 'Create Account' }}
            </button>
          </form>

          <div class="mt-6 text-center text-sm text-slate-600">
            <p>Already have an account?</p>
            <button @click="navigateToLogin" class="text-blue-600 font-medium hover:underline mt-1">Sign In</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="signUpStep == 2">
      <div class="max-w-2xl bg-white rounded-lg shadow-lg overflow-hidden p-8">
        <!-- Network Information -->
        <div class="mb-8 bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden transition-shadow">
          <div class="p-4">
            <div class="flex items-center mb-4">
              <div class="flex justify-center pr-4 relative">
                <div class="w-14 h-14 bg-white rounded-md shadow flex items-center justify-center p-2">
                  <img v-if="!networkDetails.loading.value" :src="imageSrc" alt="Network Logo"
                    class="w-full h-full object-contain" />
                  <div v-else class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin">
                  </div>
                </div>
              </div>
              <div class="flex-1">
                <div class="flex justify-between items-center">
                  <div>
                    <h2 class="text-xl font-semibold text-gray-800">{{ networkDetails.network.value?.name }}</h2>
                  </div>
                  <span v-if="networkDetails.network.value?.isSystemProtected"
                    class="px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    Protected
                  </span>
                </div>
              </div>
            </div>
            <p class="text-gray-600 text-sm">{{ networkDetails.network.value?.description || 'No description available.'
            }}</p>
          </div>
        </div>

        <!-- Required Accesses Section -->
        <div class="mb-8" v-if="isLoading">
          <div class="flex justify-center">
            <div class="spinner-border"></div>
          </div>
        </div>

        <div v-else-if="networkDetails.network.value && networkDetails.network.value.networkAccesses.length > 0">
          <h3 class="text-lg font-medium text-gray-800 mb-4">Required Data Access</h3>
          <p class="text-gray-600 mb-4">Update your consent for the following data accesses:</p>

          <div class="space-y-4 mb-6">
            <div v-for="access in networkDetails.network.value.networkAccesses" :key="access.accessId"
              class="p-4 border border-gray-200 rounded-md">
              <div class="flex items-start">
                <div class="flex items-center h-6">
                  <input :id="access.accessId" v-model="userAccesses[access.accessId].value" type="checkbox"
                    class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    @change="validateRequiredAccesses" />
                </div>
                <div class="ml-3">
                  <label :for="access.accessId" class="block text-sm font-medium text-gray-700">
                    {{ access.access.name }}
                    <span v-if="access.isRequired" class="text-red-500 ml-1">(Required)</span>
                  </label>
                  <p class="text-sm text-gray-500">{{ access.access.description }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="networkDetails.network.value?.networkAccesses.length == 0">
          <p class="text-gray-600">This network does not require any special permissions.</p>
        </div>

        <div class="mb-8">
          <h2 class="text-lg font-medium text-gray-800 mb-4">Your account</h2>
          <UserProxyDisplay
            :userProxy="{ firstName: signupFirstname, createdOn: new Date(), id: '', isDefault: true, networkUsers: [], user: { id: '', createdOn: new Date(), userProxies: [] }, email: signupEmail, imageUrl: '', lastName: signupLastname, username: signupUsername }"
            :sensitiveFields="['email', 'lastName']" :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']"
            :showSwitch="false" />
        </div>

        <div class="m-6">
          <ErrorAlert :message="error" @dismiss="error = ''" />
        </div>

        <!-- Access Update Form -->
        <form @submit.prevent="handleSubmit">
          <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
            <button type="submit"
              :class="`px-4 py-2 ${canSubmit && !isSubmitting ? 'bg-blue-600 hover:bg-blue-700' : 'bg-gray-400 cursor-not-allowed'} text-white rounded-md `"
              :disabled="!canSubmit || isSubmitting">
              <span v-if="isSubmitting" class="flex items-center">
                <span
                  class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"></span>
                Creating...
              </span>
              <span v-else>Create Account</span>
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="signUpStep == 3">
      <div class="text-center space-y-4">
        <h1 class="text-2xl font-semibold text-gray-800">
          Hold on, we're create your account...
        </h1>
        <p class="text-gray-600">
          Please wait while we complete your authentication.
        </p>
        <div class="flex justify-center">
          <svg class="animate-spin h-6 w-6 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none"
            viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
            <path class="opacity-75" fill="currentColor"
              d="M4 12a8 8 0 018-8v4l3-3-3-3v4a8 8 0 000 16v-4l-3 3 3 3v-4a8 8 0 01-8-8z" />
          </svg>
        </div>
      </div>
    </div>

    <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
      :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
      @close="showConfirmationModal = false" @confirm="confirmationAction" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { AccessTokenClaims, AuthorizationCode, ErrorMessage, UserProxy } from '@/types';
import useNetworkDetails from '@/composables/useNetworkDetails';
import { decodeJWT, isValidHttpUrl } from '@/lib/utils';
import { useGlobalStore } from '@/stores/global';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue';
import type { AxiosError } from 'axios';
import api from '@/api/api';
import rawApi from '@/api/rawApi';
import ErrorAlert from '@/components/ErrorAlert.vue';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();

const networkId = computed(() => route.params.networkId as string);
const clientId = computed(() => route.query.clientId as string);
const codeChallenge = computed(() => route.query.codeChallenge as string);
const state = computed(() => route.query.state as string);
const back = computed(() => atob(route.query.back as string));

const signupUsername = ref('');
const signupFirstname = ref('');
const signupLastname = ref('');
const signupEmail = ref('');
const signupPassword = ref('');
const confirmPassword = ref('');
const confirmToS = ref(false);

const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const isSubmitting = ref(false);
const confirmationAction = ref(() => { });

const error = ref('');
const isLoading = ref(false);
const isSigningUp = ref(false);
const imageSrc = ref(`https://ui-avatars.com/api/?name=default&background=random`);

const signUpStep = ref(1);

const networkDetails = useNetworkDetails();

const userAccesses = ref<{ [key: string]: { value: boolean, userChecked: boolean } }>({});

const canSubmit = computed(() => {
  if (!networkDetails.network.value || isLoading.value) return false;

  for (const na of networkDetails.network.value.networkAccesses) {
    if (na.isRequired && !userAccesses.value[na.accessId].value) return false;
  }

  return true;
})

onMounted(async () => {
  await networkDetails.fetchNetworkDetails(networkId.value);

  for (const access of networkDetails.network.value?.networkAccesses || []) {
    userAccesses.value[access.accessId] = { value: false, userChecked: false }
  }

  if (route.query.uname) signupUsername.value = route.query.uname as string;
  if (route.query.fname) signupFirstname.value = route.query.fname as string;
  if (route.query.lname) signupLastname.value = route.query.lname as string;
  if (route.query.email) signupEmail.value = route.query.email as string;

  if (networkDetails.network.value?.imageUrl && isValidHttpUrl(networkDetails.network.value?.imageUrl)) {
    imageSrc.value = networkDetails.network.value?.imageUrl
  } else {
    const name = `${networkDetails.network.value?.name}`
    imageSrc.value = `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`
  }
});

const goBack = () => {
  switch (signUpStep.value) {
    case 1:
      window.location.href = back.value;
      break;

    default:
      signUpStep.value--;
      break;
  }
};

const navigateToLogin = () => {
  router.push(`/networks/${networkId.value}/login?clientId=${clientId.value}&codeChallenge=${codeChallenge.value}&state=${state.value}&back=${btoa(back.value)}`);
};

const redirectToTos = () => {
  // Extract current query parameters
  const currentQuery = { ...route.query }

  // Remove any existing signupInfo keys
  delete currentQuery.uname
  delete currentQuery.fname
  delete currentQuery.lname
  delete currentQuery.email

  // Build cleaned query string from route
  const cleanedPath = route.path + buildQueryString(currentQuery)

  // Construct signupInfo
  const signupInfo = `&uname=${signupUsername.value}&fname=${signupFirstname.value}&lname=${signupLastname.value}&email=${signupEmail.value}`

  // Encode and redirect
  router.push(`/tos?redirect=${btoa(cleanedPath + signupInfo)}&fromExternal=&hideNavbar=`)
}

function buildQueryString(queryObj: typeof route.query) {
  const entries = Object.entries(queryObj)
  if (entries.length === 0) return ''
  return '?' + entries.map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value as string)}`).join('&')
}

const signUp = async () => {
  if (!networkDetails.network.value) {
    error.value = 'Network could not be loaded, please try again later.';
    return;
  }

  if (!signupUsername.value || !signupPassword.value || !confirmPassword.value ||
    !signupEmail.value || !signupFirstname.value || !signupLastname.value) {
    error.value = 'Please fill in all fields';
    return;
  }

  if (signupPassword.value !== confirmPassword.value) {
    error.value = 'Passwords do not match';
    return;
  }

  if (!confirmToS.value) {
    error.value = 'Required to accept the Terms of Service';
    return;
  }

  handleShowNetworkAccesses();
};

function handleShowNetworkAccesses() {
  signUpStep.value = 2;
}

async function handleSubmit() {
  if (!networkDetails.network.value) {
    error.value = 'Network could not be loaded, please try again later.';
    return;
  }

  signUpStep.value = 3;

  isLoading.value = true;
  global.startFetching();

  const signupUrl = `/auth/${networkId.value}/register?clientId=${clientId.value}&redirectUri=${networkDetails.network.value?.redirectURI}&codeChallenge=${codeChallenge.value}`

  const response = await rawApi.post<AuthorizationCode>(signupUrl, {
    username: signupUsername.value.trim(),
    firstName: signupFirstname.value.trim(),
    lastName: signupLastname.value.trim(),
    email: signupEmail.value.trim(),
    password: signupPassword.value,
  });
  if (response.status !== 200) {
    throw new Error("Response was not statusCode 200.");
  }

  console.log(response.data)

  const userResponse = await api.get<UserProxy>(`/me`, { headers: { Authorization: `Bearer ${response.data.accessToken}` } })

  handleUpdateAccesses(userResponse.data.networkUsers[0].id, response.data.accessToken);

  const redirectUrl = `${networkDetails.network.value?.redirectURI}?code=${response.data.code}&state=${state.value}`;
  const jwt = decodeJWT<AccessTokenClaims>(response.data.accessToken);

  if (jwt.AccessIncomplete === "true") {
    localStorage.setItem("temporaryAccessToken", response.data.accessToken);
    throw new Error("AccessIncomplete")
  }

  window.location.href = redirectUrl;
}

async function handleUpdateAccesses(networkUserId: string, temporaryAccessToken: string) {
  if (!networkDetails.network.value) return;

  const networkId = route.params.networkId as string;

  try {
    const acceptedAccesses = networkDetails.network.value!.networkAccesses
      .filter(access => userAccesses.value[access.accessId].value)
      .map(access => access.accessId);

    await Promise.all([
      ...acceptedAccesses.map(accessId =>
        api.put(`/networks/${networkId}/users/${networkUserId}/accesses/${accessId}/`, { isAccepted: true }, {
          headers: { "Authorization": `Bearer ${temporaryAccessToken}` }
        })
      )]
    );
  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;
    error.value = axiosError.response?.data?.message || 'Failed to update access. Please try again later.';

    if (axiosError.response?.status == 403) {
      error.value += "\nYour session has likely expired, please log in again to try again."
    }
  }
}

function validateRequiredAccesses(e: Event) {
  if (!networkDetails.network.value) return;

  const currentElementId = (e.target as HTMLInputElement).id;

  networkDetails.network.value.networkAccesses.forEach(access => {
    if (access.isRequired && access.accessId == currentElementId) {
      const oldValue = userAccesses.value[access.accessId];
      userAccesses.value[access.accessId] = { value: oldValue.userChecked ? true : oldValue.value, userChecked: true };
    }
  });
}
</script>

<style scoped>
.logo-container {
  top: 16px;
  left: 16px;
  z-index: 10;
}

.logo {
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 6px;
  font-weight: 600;
  font-size: 12px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.logo img {
  max-width: 100%;
  max-height: 100%;
  border-radius: 6px;
  object-fit: contain;
}
</style>
