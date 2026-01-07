<template>
  <ContentLayout title="Update Network Access">
    <div v-if="pageLoading || loading" class="flex justify-center py-10">
      <LoadingSpinner />
    </div>

    <div v-else-if="!network || !currentUser">
      <p class="text-red-700 p-4 bg-red-100 rounded" role="alert">
        Failed to load network or user details. Please try logging in again.
      </p>
    </div>

    <div v-else>
      <div
        class="mb-8 bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden hover:shadow-md transition-shadow cursor-pointer"
        @click="handleNetworkDetails" aria-label="View network details">
        <div class="p-4 flex items-center">
          <div class="mr-4">
            <NetworkLogo :loading="loading" :image-file="network.imageFile" :network-name="network.name" />
          </div>
          <div class="flex-1">
            <div class="flex justify-between items-center">
              <div>
                <h2 class="text-xl font-semibold text-gray-800">{{ network.name }}</h2>
                <p v-if="network.networkUsers?.length > 0" class="text-sm text-gray-600">{{ network.networkUsers?.length
                  || 0 }} members</p>
              </div>
              <span v-if="network.isSystemProtected"
                class="px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                Protected
              </span>
            </div>
            <p class="text-gray-600 text-sm mt-2">{{ network.description || 'No description available.' }}</p>
          </div>
        </div>
      </div>

      <NetworkAccessList :network-accesses="network.networkAccesses" :initial-user-accesses="userAccesses"
        :network-user-accesses="currentNetworkUser?.networkUserAccesses" :loading="loading"
        @access-change="updateAccessConsent" ref="accessListRef" />

      <div class="mb-8 mt-6">
        <h2 class="text-lg font-medium text-gray-800 mb-4">Currently Logged In As</h2>
        <UserProxyDisplay :userProxy="currentUser" :sensitiveFields="['email', 'lastName']"
          :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']" @switch-account="switchAccount" />
      </div>

      <div class="m-6">
        <ErrorAlert :message="submitError" @dismiss="submitError = ''" />
      </div>

    </div>

    <template #footer>
      <form @submit.prevent="handleUpdateAccesses">
        <div class="flex justify-end space-x-3">
          <button type="button"
            class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition"
            @click="navigateBack">
            Back
          </button>
          <button type="submit"
            :class="`px-4 py-2 text-white rounded-md transition ${canSubmit && !isSubmitting ? 'bg-blue-600 hover:bg-blue-700' : 'bg-gray-400 cursor-not-allowed'}`"
            :disabled="!canSubmit || isSubmitting" aria-label="Confirm and update data access settings">
            <span v-if="isSubmitting" class="flex items-center">
              <span
                class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"
                aria-hidden="true"></span>
              Updating...
            </span>
            <span v-else>Update Access</span>
          </button>
        </div>
      </form>
    </template>
  </ContentLayout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import type { ErrorMessage, NetworkUser, UserProxy } from '@/types';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';

// Reusable Components
import ContentLayout from '@/components/ContentLayout.vue';
import NetworkAccessList from '@/components/NetworkAccessList.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import NetworkLogo from '@/components/NetworkLogo.vue';

// Existing Components
import ErrorAlert from '@/components/ErrorAlert.vue';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue';
import api from '@/api/api';
import useNetworkDetails from '@/composables/useNetworkDetails';

// --- Types ---
interface UserAccessState {
  value: boolean;
  userChecked: boolean;
}

// --- Setup ---
const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const { network, loading, fetchNetworkDetails } = useNetworkDetails();

// --- State ---
const isSubmitting = ref(false);
const submitError = ref('');
const currentUser = ref<UserProxy | null>(null);
const currentNetworkUser = ref<NetworkUser | null>(null);
const userAccesses = ref<Record<string, UserAccessState>>({});
const pageLoading = ref(true);
const accessListRef = ref<InstanceType<typeof NetworkAccessList> | null>(null);

let temporaryAccessToken = '';

// --- Computed Properties ---

const networkId = computed(() => route.params.networkId as string);

const canSubmit = computed(() => {
  if (!network.value || pageLoading.value || isSubmitting.value) return false;

  const currentAccesses = accessListRef.value?.userAccesses.value || userAccesses.value;
  const accessesRecord = currentAccesses as Record<string, UserAccessState>;
  const requiredAccesses = network.value.networkAccesses.filter(na => na.isRequired);

  return requiredAccesses.every(na => accessesRecord[na.accessId]?.value);
});


// --- Initialization ---

onMounted(async () => {
  temporaryAccessToken = localStorage.getItem('temporaryAccessToken') || '';
  console.log('Temporary Access Token:', temporaryAccessToken);

  // Redirect to login if no temporary token is present
  if (!temporaryAccessToken) {
    router.push(`/networks/${networkId.value}/login?redirectUri=${route.query.redirectUri}`);
    pageLoading.value = false;
    return;
  }

  await fetchNetworkDetails(networkId.value);

  if (network.value) {
    try {
      const userResponse = await api.get<UserProxy>(`/me`, {
        headers: { "Authorization": `Bearer ${temporaryAccessToken}` }
      });

      if (userResponse.status !== 200) {
        throw new Error("Failed to fetch user data.");
      }

      currentUser.value = userResponse.data;

      const networkUser = currentUser.value.networkUsers.find(nu => nu.networkId === networkId.value);
      if (!networkUser) {
        throw new Error("User not linked to this network.");
      }
      currentNetworkUser.value = networkUser;

      // Initialize the access state based on current accepted status
      const initialAccessState: Record<string, UserAccessState> = {};
      for (const na of network.value.networkAccesses) {
        const isAccepted = networkUser.networkUserAccesses.some(nua => nua.accessId === na.accessId && nua.isAccepted);
        initialAccessState[na.accessId] = { value: isAccepted, userChecked: false };
      }
      userAccesses.value = initialAccessState;

    } catch (e) {
      console.error(e);
      submitError.value = "Authentication error. Please log in again.";
    }
  }

  pageLoading.value = false;
});


// --- Methods ---

/**
 * Updates the local access state, primarily used for validation of required fields.
 * This is triggered by the NetworkAccessList component.
 */
function updateAccessConsent(accessId: string, isChecked: boolean, isRequired: boolean) {
  void isRequired;
  if (!network.value) return;

  userAccesses.value[accessId] = { value: isChecked, userChecked: true };
}

/**
 * Handles the final submission, updating accepted and rejected accesses.
 */
async function handleUpdateAccesses() {
  if (!currentUser.value || !network.value || !currentNetworkUser.value) return;

  const rawFinalAccessState = accessListRef.value?.userAccesses.value || userAccesses.value;
  const finalAccessState = rawFinalAccessState as Record<string, UserAccessState>;

  submitError.value = '';
  global.startFetching();
  isSubmitting.value = true;

  try {
    const acceptedAccesses: string[] = [];
    const rejectedAccesses: string[] = [];

    for (const access of network.value.networkAccesses) {
      const isCurrentlyAccepted = currentNetworkUser.value.networkUserAccesses.find(n => n.accessId === access.accessId)?.isAccepted || false;

      const shouldBeAccepted = finalAccessState[access.accessId]?.value ?? false;

      if (shouldBeAccepted && !isCurrentlyAccepted) {
        acceptedAccesses.push(access.accessId);
      } else if (!shouldBeAccepted && isCurrentlyAccepted) {
        rejectedAccesses.push(access.accessId);
      }
    }

    await Promise.all([
      ...acceptedAccesses.map(accessId =>
        api.put(`/networks/${networkId.value}/users/${currentNetworkUser.value?.id}/accesses/${accessId}/`, { isAccepted: true }, {
          headers: { "Authorization": `Bearer ${temporaryAccessToken}` }
        })
      ),
      ...rejectedAccesses.map(accessId =>
        api.put(`/networks/${networkId.value}/users/${currentNetworkUser.value?.id}/accesses/${accessId}/`, { isAccepted: false }, {
          headers: { "Authorization": `Bearer ${temporaryAccessToken}` }
        })
      )
    ]);

    if (acceptedAccesses.length === 0 && rejectedAccesses.length === 0) {
      const accessId = network.value.networkAccesses[0]?.accessId;
      const isAccepted = currentNetworkUser.value.networkUserAccesses.find(n => n.accessId === accessId)?.isAccepted || false;

      await api.put(`/networks/${networkId.value}/users/${currentNetworkUser.value?.id}/accesses/${accessId}/`,
        { isAccepted: isAccepted },
        { headers: { "Authorization": `Bearer ${temporaryAccessToken}` } }
      );
    }

    const redirectUrl = atob(route.query.redirectUri as string);
    localStorage.removeItem('temporaryAccessToken');
    window.location.href = redirectUrl;

  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;
    submitError.value = axiosError.response?.data?.message || 'Failed to update access. Please try again later.';

    if (axiosError.response?.status === 403) {
      submitError.value += "\nYour session has likely expired. Please log in again.";
    }
  } finally {
    isSubmitting.value = false;
    global.stopFetching();
  }
}

function navigateBack() {
  router.back();
}

function switchAccount() {
  const url = btoa(route.fullPath);
  router.push(`/account?redirect=${url}`);
}

function handleNetworkDetails() {
  router.push(`/networks/${networkId.value}`);
}
</script>