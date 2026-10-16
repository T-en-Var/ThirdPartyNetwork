<template>
  <ContentLayout title="Update Network Access">
    <div
      v-if="pageLoading || loading"
      class="complete-access-view__loading"
    >
      <LoadingSpinner />
    </div>

    <div v-else-if="!network || !currentUser">
      <p
        class="alert alert--error"
        role="alert"
      >
        Failed to load network or user details. Please try logging in again.
      </p>
    </div>

    <div v-else>
      <div
        class="complete-access-view__network-card"
        @click="handleNetworkDetails"
        aria-label="View network details"
      >
        <div class="complete-access-view__network-row">
          <div class="complete-access-view__network-logo">
            <NetworkLogo
              :loading="loading"
              :image-file="network.imageFile"
              :network-name="network.name"
            />
          </div>
          <div class="complete-access-view__network-info">
            <div class="complete-access-view__network-title-row">
              <div>
                <h2 class="complete-access-view__network-title">{{ network.name }}</h2>
                <p
                  v-if="network.networkUsers?.length > 0"
                  class="complete-access-view__network-subtitle"
                >
                  {{ network.networkUsers?.length || 0 }} members
                </p>
              </div>
              <span
                v-if="network.isSystemProtected"
                class="badge badge--success"
              >
                Protected
              </span>
            </div>
            <p class="complete-access-view__network-description">
              {{ network.description || 'No description available.' }}
            </p>
          </div>
        </div>
      </div>

      <NetworkAccessList
        :network-accesses="networkAccessesForList"
        :initial-user-accesses="userAccesses"
        :network-user-accesses="networkUserAccessesForList"
        :loading="loading"
        @access-change="updateAccessConsent"
        ref="accessListRef"
      />

      <div class="complete-access-view__current-user">
        <h2 class="complete-access-view__current-user-title">Currently Logged In As</h2>
        <UserProxyDisplay
          :userProxy="currentUser"
          :sensitiveFields="['email', 'lastName']"
          :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']"
          @switch-account="switchAccount"
        />
      </div>

      <div class="complete-access-view__error">
        <ErrorAlert
          :message="submitError"
          @dismiss="submitError = ''"
        />
      </div>
    </div>

    <template #footer>
      <form @submit.prevent="handleUpdateAccesses">
        <div class="complete-access-view__footer-actions">
          <button
            type="button"
            class="btn btn--outline"
            @click="navigateBack"
          >
            Back
          </button>
          <button
            type="submit"
            class="btn btn--primary"
            :disabled="!canSubmit || isSubmitting"
            aria-label="Confirm and update data access settings"
          >
            <span
              v-if="isSubmitting"
              class="complete-access-view__submitting"
            >
              <span
                class="spinner-border spinner-border--xs"
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
  NetworkAccessDto,
  NetworkDto,
  NetworkUserAccessDto,
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

// NetworkAccessList expects the full NetworkAccessDto/NetworkUserAccessDto shapes (with a flat
// `accessId` and a `.network`/`.networkUser` back-reference), but `NetworkDto.networkAccesses`
// and `NetworkUserUserProxyCascadeDto.networkUserAccesses` only carry the lighter "NtA" variants
// (no flat `accessId`, no back-reference) - adapt them here rather than touching the shared list
// component, which only ever reads `.access`/`.accessId`/`.isRequired`/`.isAccepted`.
const networkAccessesForList = computed<NetworkAccessDto[]>(
  () =>
    (network.value?.networkAccesses ?? []).map((na) => ({
      ...na,
      networkId: networkId.value,
      accessId: na.access.id,
      network: network.value!,
    })) as unknown as NetworkAccessDto[],
);

const networkUserAccessesForList = computed<NetworkUserAccessDto[] | undefined>(() =>
  currentNetworkUser.value?.networkUserAccesses.map((nua) => ({
    ...nua,
    networkUserId: currentNetworkUser.value!.id,
    accessId: nua.access.id,
    networkUser: currentNetworkUser.value!,
  })) as unknown as NetworkUserAccessDto[] | undefined,
);

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
