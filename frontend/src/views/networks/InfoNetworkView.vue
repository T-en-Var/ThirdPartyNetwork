<template>
  <div class="info-network-view">
    <div class="info-network-view__panel">
      <div class="info-network-view__header">
        <h1 class="info-network-view__heading">Network Details</h1>
        <div class="info-network-view__header-actions">
          <button
            @click="handleNavigateBack"
            class="btn btn--outline"
          >
            Back to Networks
          </button>
          <button
            v-if="canManageNetwork"
            @click="handleManageNetwork"
            class="btn btn--primary"
          >
            Manage Network
          </button>
        </div>
      </div>

      <LoadingErrorComponent
        :loading="loading"
        :error="error"
        :has-value="!!network"
        button-value="Return to Networks"
        @button-action="router.push('/networks')"
      />

      <!-- Network Content -->
      <div
        v-if="!loading && !error && network"
        class="info-network-view__content"
      >
        <!-- Network Header with Logo -->
        <div class="info-network-view__network-header">
          <div class="info-network-view__network-identity">
            <div class="info-network-view__network-logo">
              <NetworkLogo
                :loading="loading"
                :image-file="network.imageFile"
                :network-name="network.name"
              />
            </div>
            <div>
              <h2 class="info-network-view__network-title">
                {{ network.name }}
                <span
                  v-if="network.isSystemProtected"
                  class="badge badge--success"
                >
                  Protected
                </span>
                <span
                  v-if="network.isPublic"
                  class="badge badge--info"
                >
                  Public
                </span>
              </h2>
              <p class="info-network-view__network-description">
                {{ network.description || 'No description available' }}
              </p>
            </div>
          </div>

          <div>
            <button
              v-if="!isUserInNetwork"
              @click="handleJoinNetwork"
              class="btn btn--primary btn--full"
            >
              Join Network
            </button>

            <button
              v-else-if="!network.isSystemProtected"
              @click="handleLeaveNetwork"
              class="btn btn--danger btn--full"
            >
              Leave Network
            </button>
          </div>
        </div>

        <!-- Network Access Requirements -->
        <div class="info-network-view__section">
          <h3 class="info-network-view__section-title">Access Requirements</h3>

          <div
            v-if="network?.networkAccesses.length > 0"
            class="info-network-view__box"
          >
            <details class="info-network-view__disclosure">
              <summary class="info-network-view__summary">
                Network Accesses ({{ network?.networkAccesses.length }})
                <span class="info-network-view__summary-icon">&#9660;</span>
              </summary>
              <ul class="info-network-view__list">
                <li
                  v-for="networkAccess in network?.networkAccesses"
                  :key="networkAccess.access.id"
                  class="info-network-view__list-item"
                >
                  <div class="info-network-view__list-item-badge">
                    <span
                      v-if="networkAccess.isRequired"
                      class="badge badge--success"
                    >
                      Required
                    </span>
                    <span
                      v-else
                      class="badge badge--warning"
                    >
                      Optional
                    </span>
                  </div>
                  <div class="info-network-view__list-item-copy">
                    <div class="info-network-view__list-item-name">
                      {{ networkAccess.access.name }}
                    </div>
                    <div class="info-network-view__list-item-description">
                      {{ networkAccess.access.description }}
                    </div>
                  </div>
                </li>
              </ul>
            </details>
          </div>

          <div
            v-else
            class="info-network-view__empty-box"
          >
            <p>This network has no required accesses.</p>
          </div>
        </div>

        <!-- Network Information -->
        <div class="info-network-view__section">
          <h3 class="info-network-view__section-title">Network Information</h3>

          <div class="info-network-view__box">
            <div class="info-network-view__detail-grid">
              <div>
                <h4 class="info-network-view__detail-label">Network ID</h4>
                <p class="info-network-view__detail-value info-network-view__detail-value--mono">
                  {{ network.id }}
                </p>
              </div>
              <div>
                <h4 class="info-network-view__detail-label">Created</h4>
                <p class="info-network-view__detail-value">
                  {{
                    network.createdOn
                      ? new Date(network.createdOn).toLocaleString('nl-NL')
                      : 'Unknown'
                  }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- User Information -->
        <div v-if="isUserInNetwork && currentNetworkUser && currentUser">
          <h3 class="info-network-view__section-title">{{ currentUser.username }}'s Information</h3>

          <div class="info-network-view__box">
            <div class="info-network-view__detail-grid">
              <div>
                <h4 class="info-network-view__detail-label">Network User ID</h4>
                <p class="info-network-view__detail-value info-network-view__detail-value--mono">
                  {{ currentNetworkUser.id }}
                </p>
              </div>
              <div>
                <h4 class="info-network-view__detail-label">Joined</h4>
                <p class="info-network-view__detail-value">
                  {{
                    currentNetworkUser.createdOn
                      ? new Date(currentNetworkUser.createdOn).toLocaleString('nl-NL')
                      : 'Unknown'
                  }}
                </p>
              </div>
            </div>

            <!-- Dropdown / details block -->
            <div
              v-if="network.networkAccesses?.length"
              class="info-network-view__user-accesses"
            >
              <details class="info-network-view__disclosure">
                <summary class="info-network-view__summary">
                  User Accesses ({{ network.networkAccesses.length }})
                  <span class="info-network-view__summary-icon">&#9660;</span>
                </summary>
                <ul class="info-network-view__list">
                  <li
                    v-for="access in network.networkAccesses"
                    :key="access.access.id"
                    class="info-network-view__list-item info-network-view__list-item--split"
                  >
                    <div class="info-network-view__list-item-badge">
                      <span
                        v-if="
                          currentNetworkUser.networkUserAccesses.find(
                            (nua) => nua.access.id === access.access.id,
                          )?.isAccepted
                        "
                        class="badge badge--success"
                      >
                        Accepted
                      </span>
                      <span
                        v-else
                        class="badge badge--danger"
                      >
                        Rejected
                      </span>
                    </div>
                    <div class="info-network-view__list-item-copy">
                      <div class="info-network-view__list-item-name">
                        {{ access.access.name }}
                      </div>
                      <div class="info-network-view__list-item-description">
                        {{ access.access.description }}
                      </div>
                    </div>
                    <div class="info-network-view__list-item-actions">
                      <button
                        v-if="
                          !access.isRequired ||
                          !currentNetworkUser?.networkUserAccesses.find(
                            (x) => x.access.id === access.access.id,
                          )?.isAccepted
                        "
                        @click="handleToggle(access.access.id)"
                        class="btn btn--outline btn--sm"
                      >
                        Toggle
                      </button>
                      <span
                        v-if="access.isRequired"
                        class="badge badge--neutral"
                      >
                        Required
                      </span>
                    </div>
                  </li>
                </ul>
              </details>
            </div>
            <div
              v-else
              class="info-network-view__no-accesses"
            >
              This user has no recorded network accesses.
            </div>
          </div>
        </div>

        <!-- Action Error -->
        <ErrorAlert
          v-if="actionError"
          :message="actionError"
          @dismiss="actionError = ''"
        />

        <!-- Action Buttons -->
        <div class="info-network-view__footer-actions">
          <button
            type="button"
            class="btn btn--outline"
            @click="router.push('/networks')"
          >
            Back to Networks
          </button>
        </div>
      </div>
    </div>

    <confirmation-modal
      v-if="showConfirmationModal"
      :title="confirmationTitle"
      :message="confirmationMessage"
      :button-text="confirmButtonText"
      :color="confirmButtonColor"
      :is-submitting="isSubmitting"
      @close="showConfirmationModal = false"
      @confirm="confirmationAction"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useGlobalStore } from '@/stores/global';
import ErrorAlert from '@/components/ErrorAlert.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import api from '@/api/api';
import NetworkLogo from '@/components/NetworkLogo.vue';
import useNetworks from '@/composables/useNetworks';
import { extractApiErrorMessage } from '@/lib/utils';
import { type UserProxyDto } from '@/types';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const global = useGlobalStore();

const {
  data: network,
  loading,
  error,
  execute: fetchNetworkDetails,
} = useNetworks().fetchNetworkDetails;

const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const isSubmitting = ref(false);
const confirmationAction = ref(() => {});
const actionError = ref('');
const currentUser = ref<UserProxyDto | null>(null);

function handleLeaveNetwork() {
  actionError.value = '';
  showConfirmationModal.value = true;
  confirmationTitle.value = 'Confirm Leave Network';
  confirmationMessage.value = 'Are you sure you want to leave this network?';
  confirmButtonText.value = 'Leave';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    try {
      const userProxyId = authStore.getUserProxyId();
      const networkId = route.params.networkId as string;
      const networkUserId = network.value?.networkUsers.find(
        (nu) => nu.userProxyId === userProxyId,
      )?.id;

      if (networkUserId == null) throw new Error('No NetworkUserId');

      await api.delete(`/networks/${networkId}/users/${networkUserId}`);
      router.push('/networks');
    } catch (err) {
      actionError.value = extractApiErrorMessage(
        err,
        'Failed to leave network. Please try again later.',
      );
    } finally {
      isSubmitting.value = false;
      showConfirmationModal.value = false;
    }
  };
}

async function handleToggle(accessId: string) {
  const access = currentNetworkUser.value?.networkUserAccesses.find(
    (x) => x.access.id === accessId,
  );
  const networkAccess = network.value?.networkAccesses.find((x) => x.access.id === accessId);

  actionError.value = '';

  if (!access && !networkAccess) {
    actionError.value = 'Access not found.';
    return;
  }

  showConfirmationModal.value = true;
  confirmationTitle.value = 'Confirm Access Toggle';
  confirmationMessage.value = `Are you sure you want to ${access?.isAccepted ? 'REVOKE' : 'PERMIT'} access to '${networkAccess?.access.name}' to this network?`;
  confirmButtonText.value = 'Confirm';
  confirmButtonColor.value = 'green';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    try {
      const userProxyId = authStore.getUserProxyId();
      const networkId = route.params.networkId as string;
      const networkUserId = network.value?.networkUsers.find(
        (nu) => nu.userProxyId === userProxyId,
      )?.id;

      await api.put(`/networks/${networkId}/users/${networkUserId}/accesses/${accessId}`, {
        isAccepted: !access?.isAccepted,
      });

      initialFetch()
    } catch (err) {
      actionError.value = extractApiErrorMessage(
        err,
        'Failed to update access. Please try again later.',
      );
    } finally {
      isSubmitting.value = false;
      showConfirmationModal.value = false;
    }
  };
}

const canManageNetwork = computed(
  () => network.value && authStore.canI.manageNetwork(network.value),
);

const currentNetworkUser = computed(() => {
  if (!authStore.isAuthenticated) return null;

  const userProxyId = authStore.getUserProxyId();
  if (!network.value || !userProxyId) return null;

  return network.value.networkUsers?.find((networkUser) => networkUser.userProxyId === userProxyId);
});

const isUserInNetwork = computed(() => {
  return currentNetworkUser.value != null;
});

async function initialFetch() {
  const networkId = route.params.networkId as string;
  global.startFetching();
  try {
    await fetchNetworkDetails(networkId);
    currentUser.value = await authStore.getUserProxy();
  } catch (err) {
    actionError.value = extractApiErrorMessage(
      err,
      'Failed to update access. Please try again later.',
    );
  } finally {
    global.stopFetching();
  }
}

onMounted(async () => {
  await initialFetch();
});

function handleNavigateBack() {
  router.push(`/networks`);
}

function handleManageNetwork() {
  router.push(`/networks/${network.value?.id}/manage`);
}

function handleJoinNetwork() {
  router.push(`/networks/${network.value?.id}/join`);
}
</script>
