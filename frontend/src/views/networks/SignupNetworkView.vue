<template>
  <AuthLayout :back-url="(route.query.back as string)" max-width="xl" :current-step="signUpStep"
    @go-back-step="goBackStep">

    <div v-if="networkDetails.loading.value" class="text-center w-full max-w-md">
      <p class="text-gray-600">Loading network details...</p>
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

    <AuthFormCard v-else-if="signUpStep == 1" title="Create Account"
      :subtitle="networkDetails.network.value ? `Join ${networkDetails.network.value.name}` : 'Join the platform'"
      :error="error" :network-details="networkDetails">
      <form @submit.prevent="signUp" class="flex flex-col gap-5">
        <div class="flex flex-wrap gap-5">
          <div class="flex-1 min-w-[140px]">
            <label for="signup-firstname" class="block mb-2 text-sm text-slate-700 font-medium">First Name</label>
            <input id="signup-firstname" v-model="signupFirstname" type="text" required autocomplete="given-name"
              placeholder="Your first name" aria-required="true"
              class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div class="flex-1 min-w-[140px]">
            <label for="signup-lastname" class="block mb-2 text-sm text-slate-700 font-medium">Last Name</label>
            <input id="signup-lastname" v-model="signupLastname" type="text" required autocomplete="family-name"
              placeholder="Your last name" aria-required="true"
              class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
        </div>

        <div class="flex flex-wrap gap-5">
          <div class="flex-1 min-w-[140px]">
            <label for="signup-username" class="block mb-2 text-sm text-slate-700 font-medium">Username</label>
            <input id="signup-username" v-model="signupUsername" type="text" required autocomplete="username"
              placeholder="Choose a username" aria-required="true"
              class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div class="flex-1 min-w-[140px]">
            <label for="signup-email" class="block mb-2 text-sm text-slate-700 font-medium">Email</label>
            <input id="signup-email" v-model="signupEmail" type="email" required autocomplete="email"
              placeholder="Your email" aria-required="true"
              class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
        </div>

        <div class="flex flex-wrap gap-5">
          <div class="flex-1 min-w-[140px]">
            <label for="signup-password" class="block mb-2 text-sm text-slate-700 font-medium">Password</label>
            <input id="signup-password" v-model="signupPassword" type="password" required autocomplete="new-password"
              placeholder="Create a password" aria-required="true"
              class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div class="flex-1 min-w-[140px]">
            <label for="confirm-password" class="block mb-2 text-sm text-slate-700 font-medium">Confirm
              Password</label>
            <input id="confirm-password" v-model="confirmPassword" type="password" required
              autocomplete="new-password" placeholder="Confirm your password" aria-required="true"
              class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
        </div>
        <div class="flex items-center gap-3 mt-1">
          <input id="confirm-tos" v-model="confirmToS" type="checkbox" required aria-required="true"
            class="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500" />
          <label for="confirm-tos" class="text-sm text-slate-700">
            I accept the <a href="#" @click.prevent="redirectToTos"
              class="text-blue-600 font-medium cursor-pointer hover:underline">Terms and Conditions</a>.
          </label>
        </div>

        <button type="submit"
          class="w-full py-3 mt-3 text-white bg-blue-600 hover:bg-blue-700 font-medium rounded transition disabled:bg-blue-300 disabled:cursor-not-allowed"
          :disabled="isSigningUp" aria-live="polite">
          {{ isSigningUp ? 'Creating account...' : 'Create Account' }}
        </button>
      </form>

      <template #footer>
        <div class="mt-6 text-center text-sm text-slate-600">
          <p>Already have an account?</p>
          <button @click="navigateToLogin" class="text-blue-600 font-medium hover:underline mt-1">Sign In</button>
        </div>
      </template>
    </AuthFormCard>

    <!-- 4. Signup Step 2: Access Consent -->
    <AuthFormCard v-else-if="signUpStep == 2" title="Confirm Access"
      subtitle="Review the required data access for this network." :error="error" :network-details="networkDetails"
      max-width="2xl">
      <div class="mb-8 bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden p-4">
        <h2 class="text-xl font-semibold text-gray-800">{{ networkDetails.network.value?.name }}</h2>
        <p class="text-gray-600 text-sm mt-1">{{ networkDetails.network.value?.description || 'No description available.' }}
        </p>
      </div>

      <div v-if="isLoading" class="mb-8">
        <LoadingSpinner />
      </div>

      <div v-else-if="networkDetails.network.value && networkDetails.network.value.networkAccesses.length > 0">
        <h3 class="text-lg font-medium text-gray-800 mb-4">Required Data Access</h3>
        <p class="text-gray-600 mb-4">Update your consent for the following data accesses:</p>

        <div class="space-y-4 mb-6">
          <div v-for="access in networkDetails.network.value.networkAccesses" :key="access.accessId"
            class="p-4 border border-gray-200 rounded-md">
            <div class="flex items-start">
              <input :id="access.accessId" v-model="userAccesses[access.accessId].value" type="checkbox"
                class="mt-1 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                @change="validateRequiredAccesses" :aria-required="access.isRequired ? 'true' : 'false'" />
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

      <div v-else>
        <p class="text-gray-600">This network does not require any special permissions.</p>
      </div>

      <div class="mb-8">
        <h2 class="text-lg font-medium text-gray-800 mb-4">Your account</h2>
        <UserProxyDisplay
          :userProxy="{ firstName: signupFirstname, createdOn: new Date(), id: '', isDefault: true, networkUsers: [], user: { id: '', createdOn: new Date(), userProxies: [] }, email: signupEmail, lastName: signupLastname, username: signupUsername, hasPassword: false }"
          :sensitiveFields="['email', 'lastName']" :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']"
          :showSwitch="false" />
      </div>

      <form @submit.prevent="handleSubmit">
        <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
          <button type="submit"
            :class="`px-4 py-2 text-white rounded-md transition ${canSubmit && !isSubmitting ? 'bg-blue-600 hover:bg-blue-700' : 'bg-gray-400 cursor-not-allowed'}`"
            :disabled="!canSubmit || isSubmitting" aria-label="Create account and confirm access" aria-live="polite">
            <span v-if="isSubmitting" class="flex items-center">
              <span
                class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"
                aria-hidden="true"></span>
              Creating...
            </span>
            <span v-else>Create Account</span>
          </button>
        </div>
      </form>
    </AuthFormCard>

    <!-- 5. Signup Step 3: Final Loading -->
    <div v-else-if="signUpStep == 3" class="text-center space-y-4 w-full max-w-xl">
      <h1 class="text-2xl font-semibold text-gray-800">
        Hold on, we're creating your account...
      </h1>
      <p class="text-gray-600">
        Please wait while we complete your authentication.
      </p>
      <LoadingSpinner />
    </div>

    <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
      :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
      @close="showConfirmationModal = false" @confirm="confirmationAction" />
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { AccessTokenClaims, AuthorizationCode, ErrorMessage, UserProxy, NetworkAccess } from '@/types';
import useNetworkDetails from '@/composables/useNetworkDetails';
import { decodeJWT } from '@/lib/utils';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';
import api from '@/api/api';
import rawApi from '@/api/rawApi';

// Reusable Components
import AuthLayout from '@/components/AuthLayout.vue';
import AuthFormCard from '@/components/AuthFormCard.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue';
import NetworkNotFound from '@/components/NetworkNotFound.vue';

// --- Setup ---
const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const networkDetails = useNetworkDetails();

// --- Computed Route Params/Queries (Vue Style-Guide C: Keep complex logic in script) ---
const networkId = computed(() => route.params.networkId as string);
const clientId = computed(() => route.query.clientId as string);
const codeChallenge = computed(() => route.query.codeChallenge as string);
const state = computed(() => route.query.state as string);
const backUrlQuery = computed(() => route.query.back as string);

// --- State Management ---
const signupUsername = ref('');
const signupFirstname = ref('');
const signupLastname = ref('');
const signupEmail = ref('');
const signupPassword = ref('');
const confirmPassword = ref('');
const confirmToS = ref(false);

const signUpStep = ref(1); // 1: Form, 2: Access, 3: Loading
const error = ref('');
const isLoading = ref(false);
const isSigningUp = ref(false); // Used for the step 1 button state

// Accesses: Map<AccessId, {value: consent, userChecked: flag}>
const userAccesses = ref<Record<string, { value: boolean, userChecked: boolean }>>({});

// Modal State
const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => { });
const isSubmitting = ref(false); // Used for the final step 2 button state

// --- Computed Properties ---

const networkNotFoundError = computed(() => {
  return !!networkId.value && !networkDetails.network.value;
});

const canSubmit = computed(() => {
  if (!networkDetails.network.value || isSubmitting.value) return false;

  // Check if all required accesses have been consented to
  const requiredAccesses = networkDetails.network.value.networkAccesses.filter((na: NetworkAccess) => na.isRequired);
  return requiredAccesses.every((na: NetworkAccess) => userAccesses.value[na.accessId]?.value);
});


// --- Lifecycle Hooks ---
onMounted(async () => {
  await networkDetails.fetchNetworkDetails(networkId.value);

  // Initialize userAccesses state only if network is loaded successfully
  for (const access of networkDetails.network.value?.networkAccesses || []) {
    userAccesses.value[access.accessId] = { value: false, userChecked: false }
  }

  // Pre-fill form from query params
  if (route.query.uname) signupUsername.value = route.query.uname as string;
  if (route.query.fname) signupFirstname.value = route.query.fname as string;
  if (route.query.lname) signupLastname.value = route.query.lname as string;
  if (route.query.email) signupEmail.value = route.query.email as string;
});

// --- Methods ---

/**
 * Handles going back one step in the multi-step signup process.
 * Used by AuthLayout's @go-back-step event.
 */
const goBackStep = () => {
  if (signUpStep.value > 1) {
    signUpStep.value--;
  }
};

const navigateToLogin = () => {
  router.push(`/networks/${networkId.value}/login?clientId=${clientId.value}&codeChallenge=${codeChallenge.value}&state=${state.value}&back=${backUrlQuery.value}`);
};

// DRY: Helper function for query string building
function buildQueryString(queryObj: typeof route.query) {
  const entries = Object.entries(queryObj)
  if (entries.length === 0) return ''
  return '?' + entries.map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value as string)}`).join('&')
}

const redirectToTos = () => {
  // Logic to build a redirect URL with current form data and existing query params
  const currentQuery = { ...route.query }
  delete currentQuery.uname
  delete currentQuery.fname
  delete currentQuery.lname
  delete currentQuery.email

  const cleanedPath = route.path + buildQueryString(currentQuery)
  const signupInfo = `&uname=${signupUsername.value}&fname=${signupFirstname.value}&lname=${signupLastname.value}&email=${signupEmail.value}`

  router.push(`/tos?redirect=${btoa(cleanedPath + signupInfo)}&fromExternal=&hideNavbar=`)
}

const signUp = async () => {
  error.value = ''; // Clear previous errors

  if (!networkDetails.network.value) {
    error.value = 'Network details are not available yet.';
    return;
  }

  // Basic form validation
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

  // Proceed to the next step: Access Consent
  signUpStep.value = 2;
};

// Function called on final submit in Step 2
async function handleSubmit() {
  if (!canSubmit.value) {
    error.value = 'Please accept all required accesses.';
    return;
  }

  isSubmitting.value = true;
  signUpStep.value = 3; // Switch to loading screen
  global.startFetching();

  const network = networkDetails.network.value;

  if (!network) {
    error.value = 'Network details missing.';
    global.stopFetching();
    isSubmitting.value = false;
    signUpStep.value = 2; // Go back to access step
    return;
  }

  const signupUrl = `/auth/${networkId.value}/register?clientId=${clientId.value}&redirectUri=${network.redirectURI}&codeChallenge=${codeChallenge.value}`

  try {
    // 1. Register User
    const response = await rawApi.post<AuthorizationCode>(signupUrl, {
      username: signupUsername.value.trim(),
      firstName: signupFirstname.value.trim(),
      lastName: signupLastname.value.trim(),
      email: signupEmail.value.trim(),
      password: signupPassword.value,
    });

    const accessToken = response.data.accessToken;

    // 2. Fetch User Proxy (to get networkUserId for access updates)
    const userResponse = await api.get<UserProxy>(`/me`, { headers: { Authorization: `Bearer ${accessToken}` } });
    const networkUserId = userResponse.data.networkUsers[0].id;

    // 3. Update Accesses
    await handleUpdateAccesses(networkUserId, accessToken);

    // 4. Decode JWT and check for incomplete access
    const jwt = decodeJWT<AccessTokenClaims>(accessToken);

    if (jwt.AccessIncomplete === "true") {
      localStorage.setItem("temporaryAccessToken", accessToken);
      const redirectUrl = `${network.redirectURI}?code=${response.data.code}&state=${state.value}`;
      handleIncompleteAccess(redirectUrl);
      return;
    }

    // 5. Final Redirect
    const redirectUrl = `${network.redirectURI}?code=${response.data.code}&state=${state.value}`;
    window.location.href = redirectUrl;

  } catch (err) {
    global.stopFetching();
    isSubmitting.value = false;

    const axiosError = err as AxiosError<ErrorMessage>;

    // Handle specific Axios status codes
    if (axiosError.response?.status === 409) {
      error.value = "An account with this username or email already exists. Please sign in.";
      signUpStep.value = 1; // Go back to form to encourage login
    } else if (axiosError.response?.status === 400) {
      error.value = axiosError.response.data?.message || "Invalid registration data provided.";
      signUpStep.value = 1; // Go back to form
    } else if (axiosError.response?.data?.message) {
      error.value = axiosError.response.data.message;
      signUpStep.value = 2; // Default to access step to show error
    } else {
      error.value = "An unexpected error occurred during account creation. Please try again.";
      signUpStep.value = 2;
    }
  }
}

/**
 * Sends PUT requests to update user access consent.
 */
async function handleUpdateAccesses(networkUserId: string, temporaryAccessToken: string) {
  if (!networkDetails.network.value) return;

  const acceptedAccesses = networkDetails.network.value.networkAccesses
    .filter(access => userAccesses.value[access.accessId].value)
    .map(access => access.accessId);

  try {
    await Promise.all(
      acceptedAccesses.map(accessId =>
        api.put(`/networks/${networkId.value}/users/${networkUserId}/accesses/${accessId}/`, { isAccepted: true }, {
          headers: { "Authorization": `Bearer ${temporaryAccessToken}` }
        })
      )
    );
  } catch (err) {
    console.error("Failed to update accesses:", err);
    throw err; // Re-throw to be caught in handleSubmit
  }
}

function handleIncompleteAccess(redirectUri: string) {
  showConfirmationModal.value = true;
  confirmationTitle.value = 'Account Created, Additional Access Required';
  confirmationMessage.value = 'This network requires additional confirmation of your data access. Please continue to confirm and complete your registration.';
  confirmButtonText.value = 'Continue to Access';
  confirmButtonColor.value = 'blue';

  confirmationAction.value = () => {
    // Navigate to a dedicated page to complete access consent
    router.push(`/networks/${networkId.value}/complete-access?redirectUri=${btoa(redirectUri)}`);
  };
}

/**
 * Validation for required accesses on checkbox change.
 */
function validateRequiredAccesses(e: Event) {
  if (!networkDetails.network.value) return;

  const currentElementId = (e.target as HTMLInputElement).id;
  const isChecked = (e.target as HTMLInputElement).checked;

  const access = networkDetails.network.value.networkAccesses.find(a => a.accessId === currentElementId);

  if (access && access.isRequired) {
    // For required fields, we track if the user has explicitly checked/unchecked
    // This maintains the 'required' status validation logic in `canSubmit`
    userAccesses.value[currentElementId].value = isChecked;
    userAccesses.value[currentElementId].userChecked = true;
  } else if (access) {
    userAccesses.value[currentElementId].value = isChecked;
  }
}
</script>