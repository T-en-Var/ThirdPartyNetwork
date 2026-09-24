<template>
  <ContentLayout title="Update Network Access">
    <div
      v-if="pageLoading || loading"
      class="flex justify-center py-10"
    >
      <LoadingSpinner />
    </div>

    <div v-else-if="!network || !currentUser">
      <p
        class="rounded bg-red-100 p-4 text-red-700"
        role="alert"
      >
        Failed to load network or user details. Please try logging in again.
      </p>
    </div>

    <div v-else>
      <div
        class="mb-8 cursor-pointer overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md"
        @click="handleNetworkDetails"
        aria-label="View network details"
      >
        <div class="flex items-center p-4">
          <div class="mr-4">
            <NetworkLogo
              :loading="loading"
              :image-file="network.imageFile"
              :network-name="network.name"
            />
          </div>
          <div class="flex-1">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-gray-800">{{ network.name }}</h2>
                <p
                  v-if="network.networkUsers?.length > 0"
                  class="text-sm text-gray-600"
                >
                  {{ network.networkUsers?.length || 0 }} members
                </p>
              </div>
              <span
                v-if="network.isSystemProtected"
                class="rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-800"
              >
                Protected
              </span>
            </div>
            <p class="mt-2 text-sm text-gray-600">
              {{ network.description || 'No description available.' }}
            </p>
          </div>
        </div>
      </div>

      <NetworkAccessList
        :network-accesses="network.networkAccesses"
        :initial-user-accesses="userAccesses"
        :network-user-accesses="currentNetworkUser?.networkUserAccesses"
        :loading="loading"
        @access-change="updateAccessConsent"
        ref="accessListRef"
      />

      <div class="mb-8 mt-6">
        <h2 class="mb-4 text-lg font-medium text-gray-800">Currently Logged In As</h2>
        <UserProxyDisplay
          :userProxy="currentUser"
          :sensitiveFields="['email', 'lastName']"
          :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']"
          @switch-account="switchAccount"
        />
      </div>

      <div class="m-6">
        <ErrorAlert
          :message="submitError"
          @dismiss="submitError = ''"
        />
      </div>
    </div>

    <template #footer>
      <form @submit.prevent="handleUpdateAccesses">
        <div class="flex justify-end space-x-3">
          <button
            type="button"
            class="rounded-md border border-gray-300 px-4 py-2 text-gray-700 transition hover:bg-gray-50"
            @click="navigateBack"
          >
            Back
          </button>
          <button
            type="submit"
            :class="`rounded-md px-4 py-2 text-white transition ${canSubmit && !isSubmitting ? 'bg-blue-600 hover:bg-blue-700' : 'cursor-not-allowed bg-gray-400'}`"
            :disabled="!canSubmit || isSubmitting"
            aria-label="Confirm and update data access settings"
          >
            <span
              v-if="isSubmitting"
              class="flex items-center"
            >
              <span
                class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"
                aria-hidden="true"
              ></span>
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
import type {
  ErrorMessage,
  NetworkDto,
  NetworkUserUserProxyCascadeDto,
  UserProxyExtraCascadeDto,
} from '@/types';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';

import ContentLayout from '@/components/ContentLayout.vue';
import NetworkAccessList from '@/components/NetworkAccessList.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import NetworkLogo from '@/components/NetworkLogo.vue';

import ErrorAlert from '@/components/ErrorAlert.vue';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue';
import useNetworks from '@/composables/useNetworks';
import useAccessConsent, { type AccessConsentState } from '@/composables/useAccessConsent';
import { safeAtob } from '@/lib/utils';
import useTempAuth from '@/composables/useTempAuth';

type UserAccessState = AccessConsentState;

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const { buildInitialAccessState, applyAccessConsent } = useAccessConsent();
const tempUserState = useTempAuth();

const networksState = useNetworks();
const { data: network, loading, execute: fetchNetworkDetails } = networksState.fetchNetworkDetails;

const isSubmitting = ref(false);
const submitError = ref('');
const currentUser = ref<UserProxyExtraCascadeDto | null>(null);
const currentNetworkUser = ref<NetworkUserUserProxyCascadeDto | null>(null);
const userAccesses = ref<Record<string, UserAccessState>>({});
const pageLoading = ref(true);
const accessListRef = ref<InstanceType<typeof NetworkAccessList> | null>(null);

const networkId = computed(() => route.params.networkId as string);

const canSubmit = computed(() => {
  if (!network.value || pageLoading.value || isSubmitting.value) return false;

  const currentAccesses = accessListRef.value?.userAccesses.value || userAccesses.value;
  const accessesRecord = currentAccesses as Record<string, UserAccessState>;
  const requiredAccesses = network.value.networkAccesses.filter((na) => na.isRequired);

  return requiredAccesses.every((na) => accessesRecord[na.access.id]?.value);
});

const initialize = async () => {
  const temporaryAccessToken = tempUserState.getTemporaryAccessToken();
  if (!temporaryAccessToken) {
    router.push({
      path: `/networks/${networkId.value}/login`,
      query: { redirectUri: route.query.redirectUri },
    });
    pageLoading.value = false;
    return;
  }

  await fetchNetworkDetails(networkId.value);
  await buildData();

  pageLoading.value = false;
};

const buildData = async () => {
  if (!network.value) return;

  try {
    currentUser.value = await tempUserState.fetchUser();
  } catch (e) {
    console.error(e);
    submitError.value = 'Authentication error. Please log in again.';
    return;
  }

  const networkUser = currentUser.value.networkUsers.find((nu) => nu.networkId === networkId.value);
  if (!networkUser) {
    submitError.value = 'User not linked to this network.';
    return;
  }
  currentNetworkUser.value = networkUser;

  userAccesses.value = buildInitialAccessState(network.value.networkAccesses, (na) =>
    networkUser.networkUserAccesses.some((nua) => nua.access.id === na.access.id && nua.isAccepted),
  );
};

function updateAccessConsent(accessId: string, isChecked: boolean, isRequired: boolean) {
  void isRequired;
  if (!network.value) return;

  userAccesses.value[accessId] = { value: isChecked, userChecked: true };
}

const splitAccesses = (
  network: NetworkDto,
  currentNetworkUser: NetworkUserUserProxyCascadeDto,
  finalAccessState: Record<string, UserAccessState>,
) => {
  const acceptedAccesses: string[] = [];
  const rejectedAccesses: string[] = [];

  for (const access of network.networkAccesses) {
    const isCurrentlyAccepted =
      currentNetworkUser.networkUserAccesses.find((n) => n.access.id === access.access.id)
        ?.isAccepted || false;

    const shouldBeAccepted = finalAccessState[access.access.id]?.value ?? false;

    if (shouldBeAccepted && !isCurrentlyAccepted) {
      acceptedAccesses.push(access.access.id);
    } else if (!shouldBeAccepted && isCurrentlyAccepted) {
      rejectedAccesses.push(access.access.id);
    }
  }

  return { acceptedAccesses, rejectedAccesses };
};

const triggerUpdate = async (
  network: NetworkDto,
  currentNetworkUser: NetworkUserUserProxyCascadeDto,
) => {
  const accessId = network.networkAccesses[0]?.access.id;

  if (accessId) {
    const isAccepted =
      currentNetworkUser.networkUserAccesses.find((n) => n.access.id === accessId)?.isAccepted ||
      false;

    await applyAccessConsent(networkId.value, currentNetworkUser.id, [accessId], isAccepted);
  }
};

async function handleUpdateAccesses() {
  if (!currentUser.value || !network.value || !currentNetworkUser.value) return;

  const rawFinalAccessState = accessListRef.value?.userAccesses.value || userAccesses.value;
  const finalAccessState = rawFinalAccessState as Record<string, UserAccessState>;

  submitError.value = '';
  global.startFetching();
  isSubmitting.value = true;

  try {
    const { acceptedAccesses, rejectedAccesses } = splitAccesses(
      network.value,
      currentNetworkUser.value,
      finalAccessState,
    );

    await Promise.all([
      applyAccessConsent(networkId.value, currentNetworkUser.value.id, acceptedAccesses, true),
      applyAccessConsent(networkId.value, currentNetworkUser.value.id, rejectedAccesses, false),
    ]);

    if (acceptedAccesses.length === 0 && rejectedAccesses.length === 0) {
      await triggerUpdate(network.value, currentNetworkUser.value);
    }

    handleRedirect();
  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;
    handleUpdateError(axiosError);
  } finally {
    isSubmitting.value = false;
    global.stopFetching();
  }
}

const handleRedirect = () => {
  const redirectUrl = safeAtob(route.query.redirectUri as string | undefined) || '/';
  tempUserState.removeTemporaryAccessToken();
  window.location.href = redirectUrl;
};

const handleUpdateError = (axiosError: AxiosError<ErrorMessage>) => {
  submitError.value =
    axiosError.response?.data?.message || 'Failed to update access. Please try again later.';

  if (axiosError.response?.status === 403) {
    submitError.value += '\nYour session has likely expired. Please log in again.';
  }
};

function navigateBack() {
  router.back();
}

function switchAccount() {
  tempUserState.removeTemporaryAccessToken();
  router.push({
    path: `/networks/${networkId.value}/login`,
    query: { redirectUri: route.query.redirectUri },
  });
}

function handleNetworkDetails() {
  router.push(`/networks/${networkId.value}`);
}

onMounted(async () => {
  await initialize();
});
</script>
