<template>
  <AuthLayout
    :back-url="backUrl"
    max-width="xl"
    :current-step="signUpStep"
    @go-back-step="goBackStep"
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

    <!-- Step 1: Account details -->
    <AuthFormCard
      v-else-if="signUpStep === 1"
      title="Create Account"
      :subtitle="
        networkDetails.data.value ? `Join ${networkDetails.data.value.name}` : 'Join the platform'
      "
      :error="error"
      :network-details="networkDetails"
    >
      <AuthSignupForm
        :initial-values="initialSignupValues"
        @submit="completeStep1"
        @tos="redirectToTos"
      />

      <template #footer>
        <div class="mt-6 flex items-center justify-center gap-2 text-sm text-gray-600">
          <p>Already have an account?</p>
          <button
            @click="goToLogin"
            class="font-medium text-blue-600 hover:underline"
          >
            Sign In
          </button>
        </div>
      </template>
    </AuthFormCard>

    <!-- Step 2: Access consent -->
    <AuthFormCard
      v-else-if="signUpStep === 2"
      title="Confirm Access"
      subtitle="Review the required data access for this network."
      :error="error"
      :network-details="networkDetails"
    >
      <div class="mb-8 overflow-hidden rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
        <h2 class="text-xl font-semibold text-gray-800">{{ networkDetails.data.value?.name }}</h2>
        <p class="mt-1 text-sm text-gray-600">
          {{ networkDetails.data.value?.description || 'No description available.' }}
        </p>
      </div>

      <NetworkAccessList
        :network-accesses="networkDetails.data.value?.networkAccesses ?? []"
        :initial-user-accesses="userAccesses"
        :network-user-accesses="[]"
        :loading="networkDetails.loading.value"
        @access-change="onAccessChange"
      />

      <div class="mb-8">
        <h2 class="mb-4 text-lg font-medium text-gray-800">Your account</h2>
        <UserProxyDisplay
          v-if="signupForm"
          :userProxy="{
            firstName: signupForm.firstName,
            createdOn: new Date(),
            id: '',
            isDefault: true,
            networkUsers: [],
            user: { id: '', createdOn: new Date(), userProxies: [] },
            email: signupForm.email,
            lastName: signupForm.lastName,
            username: signupForm.username,
            hasPassword: false,
          }"
          :sensitiveFields="['email', 'lastName']"
          :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']"
          :showSwitch="false"
        />
      </div>

      <form @submit.prevent="handleSubmit">
        <div class="flex justify-end space-x-3 border-t border-gray-200 pt-4">
          <button
            type="submit"
            :class="`rounded-md px-4 py-2 text-white transition ${canSubmit ? 'bg-blue-600 hover:bg-blue-700' : 'cursor-not-allowed bg-gray-400'}`"
            :disabled="!canSubmit"
            aria-label="Create account and confirm access"
            aria-live="polite"
          >
            <span
              v-if="isSubmitting"
              class="flex items-center"
            >
              <span
                class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"
                aria-hidden="true"
              ></span>
              Creating...
            </span>
            <span v-else>Create Account</span>
          </button>
        </div>
      </form>
    </AuthFormCard>

    <!-- Step 3: Finalizing -->
    <div
      v-else
      class="w-full space-y-4 text-center"
    >
      <h1 class="text-2xl font-semibold text-gray-800">Hold on, we're creating your account...</h1>
      <p class="text-gray-600">Please wait while we complete your authentication.</p>
      <LoadingSpinner />
    </div>

    <ConfirmationModal
      v-if="incompleteAccess"
      title="Account Created, Additional Access Required"
      message="This network requires additional confirmation of your data access. Please continue to confirm and complete your registration."
      button-text="Continue to Access"
      color="blue"
      @close="incompleteAccess = null"
      @confirm="confirmIncompleteAccess"
    />
  </AuthLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { UserProxy, UserSignup } from '@/types';
import { useGlobalStore } from '@/stores/global';
import api from '@/api/api';
import useNetworkAuthFlow from '@/composables/useNetworkAuthFlow';

import AuthLayout from '@/components/AuthLayout.vue';
import AuthFormCard from '@/components/AuthFormCard.vue';
import AuthSignupForm from '@/components/AuthSignupForm.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import NetworkAccessList from '@/components/NetworkAccessList.vue';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue';
import NetworkNotFound from '@/components/NetworkNotFound.vue';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const flow = useNetworkAuthFlow();
const { networkDetails, networkId, backUrl, isValidUrl, networkNotFound, goToLogin } = flow;

const signUpStep = ref(1);
const error = ref('');
const isSubmitting = ref(false);
const signupForm = ref<UserSignup | null>(null);
const userAccesses = ref<Record<string, { value: boolean; userChecked: boolean }>>({});
const incompleteAccess = ref<{ accessToken: string; redirectUrl: string } | null>(null);

const initialSignupValues = computed<Partial<UserSignup>>(
  () =>
    // Keep entered values when the user returns to step 1 (back button or a
    // failed submit); otherwise prefill from the query (return from the ToS page).
    signupForm.value ?? {
      username: (route.query.uname as string) || '',
      firstName: (route.query.fname as string) || '',
      lastName: (route.query.lname as string) || '',
      email: (route.query.email as string) || '',
    },
);

const canSubmit = computed(() => {
  if (!networkDetails.data.value || isSubmitting.value) return false;

  return networkDetails.data.value.networkAccesses
    .filter((na) => na.isRequired)
    .every((na) => userAccesses.value[na.accessId]?.value);
});

onMounted(async () => {
  await flow.loadNetwork();

  for (const access of networkDetails.data.value?.networkAccesses || []) {
    userAccesses.value[access.accessId] = { value: false, userChecked: false };
  }
});

const goBackStep = () => {
  if (signUpStep.value === 2) {
    signUpStep.value = 1;
  }
};

const completeStep1 = (form: UserSignup) => {
  error.value = '';
  signupForm.value = form;
  signUpStep.value = 2;
};

const onAccessChange = (accessId: string, isChecked: boolean) => {
  userAccesses.value[accessId] = { value: isChecked, userChecked: true };
};

const redirectToTos = (form: UserSignup) => {
  const target = router.resolve({
    path: route.path,
    query: {
      clientId: flow.clientId.value,
      codeChallenge: flow.codeChallenge.value,
      state: flow.state.value,
      back: backUrl.value,
      uname: form.username,
      fname: form.firstName,
      lname: form.lastName,
      email: form.email,
    },
  }).fullPath;

  router.push({
    path: '/tos',
    query: { redirect: btoa(target), fromExternal: '', hideNavbar: '' },
  });
};

async function handleSubmit() {
  if (!canSubmit.value || !signupForm.value) {
    error.value = 'Please accept all required accesses.';
    return;
  }

  if (!networkDetails.data.value) {
    error.value = 'Network details are not available yet.';
    return;
  }

  error.value = '';
  isSubmitting.value = true;
  signUpStep.value = 3;
  global.startFetching();

  try {
    const auth = await flow.authorize('register', signupForm.value);

    const userResponse = await api.get<UserProxy>(`/me`, {
      headers: { Authorization: `Bearer ${auth.accessToken}` },
    });
    const networkUserId = userResponse.data.networkUsers[0]?.id;
    if (!networkUserId) throw new Error('Created user is not linked to this network.');

    await acceptAccesses(networkUserId, auth.accessToken);

    const redirectUrl = flow.buildRedirectUrl(auth.code);

    if (flow.isAccessIncomplete(auth.accessToken)) {
      incompleteAccess.value = { accessToken: auth.accessToken, redirectUrl };
      return;
    }

    window.location.href = redirectUrl;
  } catch (err) {
    const { status, message } = flow.extractError(
      err,
      'An unexpected error occurred during account creation. Please try again.',
    );

    if (status === 409) {
      error.value = 'An account with this username or email already exists. Please sign in.';
      signUpStep.value = 1;
    } else if (status === 400) {
      error.value = message || 'Invalid registration data provided.';
      signUpStep.value = 1;
    } else {
      error.value = message;
      signUpStep.value = 2;
    }
  } finally {
    isSubmitting.value = false;
    global.stopFetching();
  }
}

async function acceptAccesses(networkUserId: string, accessToken: string) {
  const acceptedAccesses = (networkDetails.data.value?.networkAccesses ?? [])
    .filter((access) => userAccesses.value[access.accessId]?.value)
    .map((access) => access.accessId);

  await Promise.all(
    acceptedAccesses.map((accessId) =>
      api.put(
        `/networks/${networkId.value}/users/${networkUserId}/accesses/${accessId}/`,
        { isAccepted: true },
        {
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      ),
    ),
  );
}

function confirmIncompleteAccess() {
  if (!incompleteAccess.value) return;
  flow.goToCompleteAccess(incompleteAccess.value.accessToken, incompleteAccess.value.redirectUrl);
}
</script>
