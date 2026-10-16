<template>
  <div class="join-network-view">
    <div class="join-network-view__panel">
      <div class="join-network-view__header">
        <h1 class="join-network-view__heading">Join Network</h1>
      </div>

      <div class="join-network-view__body">
        <!-- Network Information -->
        <div
          class="join-network-view__network-card"
          @click="handleNetworkDetails"
        >
          <div class="join-network-view__network-card-body">
            <div class="join-network-view__network-row">
              <div class="join-network-view__network-logo">
                <NetworkLogo
                  :loading="loading"
                  :image-file="network?.imageFile"
                  :network-name="network?.name ?? ''"
                />
              </div>
              <div class="join-network-view__network-info">
                <div class="join-network-view__network-title-row">
                  <div>
                    <h2 class="join-network-view__network-title">{{ network?.name }}</h2>
                    <p class="join-network-view__network-subtitle">
                      {{ network?.networkUsers?.length || 0 }} members
                    </p>
                  </div>
                  <span
                    v-if="network?.isSystemProtected"
                    class="badge badge--success"
                  >
                    Protected
                  </span>
                </div>
              </div>
            </div>
            <p class="join-network-view__network-description">
              {{ network?.description || 'No description available.' }}
            </p>
          </div>
        </div>

        <!-- Required Accesses Section -->
        <div
          class="join-network-view__accesses-loading"
          v-if="loading"
        >
          <div class="spinner-border"></div>
        </div>

        <div v-else-if="network && network.networkAccesses.length > 0">
          <h3 class="join-network-view__section-title">Required Data Access</h3>
          <p class="join-network-view__section-subtitle">
            This network requests access the following information to function properly:
          </p>

          <div class="join-network-view__accesses-list">
            <div
              v-for="access in network.networkAccesses"
              :key="access.access.id"
              class="join-network-view__access-item"
            >
              <div class="join-network-view__access-row">
                <div class="join-network-view__access-checkbox-wrap">
                  <input
                    :id="access.access.id"
                    v-model="userAccesses[access.access.id].value"
                    type="checkbox"
                    class="checkbox"
                    :disabled="access.isRequired"
                    @change="validateRequiredAccesses"
                  />
                </div>
                <div class="join-network-view__access-copy">
                  <label
                    :for="access.access.id"
                    class="form-label"
                  >
                    {{ access.access.name }}
                    <span
                      v-if="access.isRequired"
                      class="join-network-view__required-tag"
                      >(Required)</span
                    >
                  </label>
                  <p class="join-network-view__access-description">{{ access.access.description }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div v-else-if="network">
          <p class="join-network-view__no-accesses">This network does not require any special permissions.</p>
        </div>

        <div class="join-network-view__current-user">
          <h2 class="join-network-view__current-user-title">Currently Logged In As</h2>
          <UserProxyDisplay
            :userProxy="currentUserProxy"
            :sensitiveFields="['email', 'lastName']"
            :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']"
            @switch-account="switchAccount"
          >
          </UserProxyDisplay>
        </div>

        <div class="join-network-view__error">
          <ErrorAlert
            :message="submitError"
            @dismiss="submitError = ''"
          />
        </div>

        <!-- Join Network Form -->
        <form @submit.prevent="handleJoinNetwork">
          <!-- Form Actions -->
          <div class="form__actions">
            <button
              type="button"
              class="btn btn--outline"
              @click="navigateBack"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="btn btn--primary"
              :disabled="isSubmitting"
            >
              <span
                v-if="isSubmitting"
                class="join-network-view__submitting"
              >
                <span class="spinner-border spinner-border--xs"></span>
                Joining...
              </span>
              <span v-else>Join Network</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import type { CreateNetworkUserDto, ErrorMessage, NetworkUserDto, UserProxyDto } from '@/types';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';
import ErrorAlert from '@/components/ErrorAlert.vue';
import { useAuthStore } from '@/stores/auth';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue';
import NetworkLogo from '@/components/NetworkLogo.vue';
import api from '@/api/api';
import useNetworks from '@/composables/useNetworks';
import useAccessConsent, { type AccessConsentState } from '@/composables/useAccessConsent';
import { safeBtoa } from '@/lib/utils';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const isSubmitting = ref(false);
const submitError = ref('');
const userAccesses = ref<Record<string, AccessConsentState>>({});
const authStore = useAuthStore();
const { buildInitialAccessState, applyAccessConsent } = useAccessConsent();

const currentUserProxy = ref<UserProxyDto | null>(null);

const { data: network, loading, execute: fetchNetworkDetails } = useNetworks().fetchNetworkDetails;

onMounted(async () => {
  const networkId = route.params.networkId as string;
  await fetchNetworkDetails(networkId);
  currentUserProxy.value = await authStore.getUserProxy();

  if (network.value) {
    userAccesses.value = buildInitialAccessState(
      network.value.networkAccesses,
      (access) => access.isRequired,
    );
  }
});

function validateRequiredAccesses() {
  if (!network.value) return;

  network.value.networkAccesses.forEach((access) => {
    if (access.isRequired) {
      userAccesses.value[access.access.id].value = true;
    }
  });
}

async function handleJoinNetwork() {
  if (!currentUserProxy.value || !network.value) return;
  const networkId = route.params.networkId as string;
  const userProxyId = currentUserProxy.value.id;

  global.startFetching();
  isSubmitting.value = true;

  try {
    const { data: networkUser } = await api.post<NetworkUserDto, CreateNetworkUserDto>(
      `/networks/${networkId}/users/${userProxyId}`,
      {},
    );

    const acceptedAccesses = network
      .value!.networkAccesses.filter(
        (access) => !access.isRequired && userAccesses.value[access.access.id]?.value,
      )
      .map((access) => access.access.id);

    await applyAccessConsent(networkId, networkUser.id, acceptedAccesses, true);

    router.push(`/networks/${networkId}`);
  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;
    if (axiosError.response?.status == 409) {
      submitError.value = 'You already joined this network!';
    } else {
      submitError.value =
        axiosError.response?.data?.message || 'Failed to join network. Please try again later.';
    }
  } finally {
    isSubmitting.value = false;
    global.stopFetching();
  }
}

function navigateBack() {
  router.push('/networks');
}

function switchAccount() {
  authStore.logout();
  router.push({ path: '/401', query: { redirect: safeBtoa(route.fullPath) } });
  authStore.setModalOpen(true);
  authStore.setModalMode('login');
}

function handleNetworkDetails() {
  const networkId = route.params.networkId as string;
  router.push(`/networks/${networkId}`);
}
</script>
