<template>
  <div class="container mx-auto mt-8 px-4 py-8">
    <div class="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-md">
      <div class="flex items-center justify-between border-b border-gray-200 bg-gray-100 px-6 py-4">
        <h1 class="text-2xl font-semibold text-gray-800">Network Details</h1>
        <div class="flex space-x-3">
          <button
            @click="handleNavigateBack"
            class="rounded-md border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
          >
            Back to Networks
          </button>
          <button
            v-if="canManageNetwork"
            @click="handleManageNetwork"
            class="rounded-md bg-blue-600 px-4 py-2 text-white shadow-sm hover:bg-blue-700"
          >
            Manage Network
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div
        v-if="loading"
        class="flex h-64 items-center justify-center"
      >
        <div class="h-12 w-12 animate-spin rounded-full border-b-2 border-blue-600"></div>
      </div>

      <!-- Error State -->
      <div
        v-else-if="error"
        class="mx-6 my-6"
      >
        <ErrorAlert
          :message="error"
          @dismiss="error = ''"
        />
        <div class="mt-4 flex justify-center">
          <button
            @click="router.push('/networks')"
            class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            Return to Networks
          </button>
        </div>
      </div>

      <!-- Network Content -->
      <div
        v-else-if="network"
        class="p-6"
      >
        <!-- Network Header with Logo -->
        <div class="flex justify-between">
          <div class="mb-6 flex items-center">
            <div
              class="mr-4 flex h-16 w-16 items-center justify-center rounded-lg border border-gray-200 bg-white p-1 shadow-sm"
            >
              <div
                v-if="loading"
                class="h-7 w-7 animate-spin rounded-full border-4 border-gray-300 border-t-indigo-500"
              ></div>
              <CloudinaryFile
                v-else-if="network?.imageFile"
                :display-only="true"
                :file="network?.imageFile"
                class="max-h-10 w-10 object-cover"
              />
              <div
                v-else
                class="logo"
              >
                <img
                  :src="`https://ui-avatars.com/api/?name=${network?.name}&size=24&background=random`"
                  :alt="network?.name"
                />
              </div>
            </div>
            <div>
              <h2 class="flex items-center text-xl font-bold text-gray-800">
                {{ network.name }}
                <span
                  v-if="network.isSystemProtected"
                  class="ml-3 rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-800"
                >
                  Protected
                </span>
                <span
                  v-if="network.isPublic"
                  class="ml-3 rounded-full bg-blue-100 px-3 py-1 text-xs font-medium text-blue-800"
                >
                  Public
                </span>
              </h2>
              <p class="text-gray-600">{{ network.description || 'No description available' }}</p>
            </div>
          </div>

          <div>
            <div>
              <button
                v-if="!isUserInNetwork"
                @click="handleJoinNetwork"
                class="w-full rounded-md bg-blue-600 px-16 py-3 text-white shadow-sm hover:bg-blue-700"
              >
                Join Network
              </button>

              <button
                v-else-if="!network.isSystemProtected"
                @click="handleLeaveNetwork"
                class="w-full rounded-md bg-red-500 px-16 py-3 text-white shadow-sm hover:bg-red-600"
              >
                Leave Network
              </button>
            </div>
          </div>
        </div>

        <!-- Network Access Requirements -->
        <div class="mb-6">
          <h3 class="mb-3 text-lg font-medium text-gray-800">Access Requirements</h3>

          <div
            v-if="network?.networkAccesses.length > 0"
            class="rounded-lg border border-gray-200 bg-gray-50 p-4"
          >
            <details class="group">
              <summary
                class="flex cursor-pointer items-center justify-between text-sm font-semibold text-gray-700"
              >
                Network Accesses ({{ network?.networkAccesses.length }})
                <span class="ml-2 transition-transform group-open:rotate-180">&#9660;</span>
              </summary>
              <ul class="mt-4 space-y-3">
                <li
                  v-for="networkAccess in network?.networkAccesses"
                  :key="networkAccess.accessId"
                  class="flex items-start rounded-md border border-gray-200 bg-gray-50 p-3"
                >
                  <div class="mt-1 flex-shrink-0">
                    <span
                      v-if="networkAccess.isRequired"
                      class="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"
                    >
                      Required
                    </span>
                    <span
                      v-else
                      class="inline-flex items-center rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-800"
                    >
                      Optional
                    </span>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-semibold text-gray-800">
                      {{ networkAccess.access.name }}
                    </div>
                    <div class="text-sm text-gray-600">
                      {{ networkAccess.access.description }}
                    </div>
                  </div>
                </li>
              </ul>
            </details>
          </div>

          <div
            v-else
            class="rounded-lg border border-gray-200 bg-gray-50 p-6 text-center"
          >
            <p class="text-gray-600">This network has no required accesses.</p>
          </div>
        </div>

        <!-- Connected Websites -->
        <div class="mb-6">
          <h3 class="mb-3 text-lg font-medium text-gray-800">Connected Websites</h3>

          <div
            v-if="connectedWebsites.length > 0 && false"
            class="grid grid-cols-1 gap-4 md:grid-cols-2"
          >
            <div
              v-for="(website, index) in connectedWebsites"
              :key="index"
              class="rounded-lg border border-gray-200 p-4 transition-shadow hover:shadow-md"
            >
              <div class="flex items-center">
                <img
                  :src="website.icon || `https://www.google.com/s2/favicons?domain=${website.url}`"
                  :alt="website.name"
                  class="mr-3 h-6 w-6"
                />
                <div>
                  <h4 class="font-medium text-gray-800">{{ website.name }}</h4>
                  <a
                    :href="website.url"
                    target="_blank"
                    class="text-sm text-blue-600 hover:underline"
                  >
                    {{ website.url }}
                  </a>
                </div>
              </div>
            </div>
          </div>

          <div
            v-else
            class="rounded-lg border border-gray-200 bg-gray-50 p-6 text-center"
          >
            <p class="text-gray-600">
              No websites are currently using this network for authentication.
            </p>
          </div>
        </div>

        <!-- Network Information -->
        <div class="mb-6">
          <h3 class="mb-3 text-lg font-medium text-gray-800">Network Information</h3>

          <div class="rounded-lg border border-gray-200 bg-gray-50 p-6">
            <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div>
                <h4 class="text-sm font-medium text-gray-500">Network ID</h4>
                <p class="mt-1 rounded bg-gray-100 px-2 py-1 font-mono text-gray-800">
                  {{ network.id }}
                </p>
              </div>
              <div>
                <h4 class="text-sm font-medium text-gray-500">Created</h4>
                <p class="mt-1 text-gray-800">
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
        <div v-if="isUserInNetwork && currentNetworkUser">
          <h3 class="mb-3 text-lg font-medium text-gray-800">User Information</h3>

          <div class="rounded-lg border border-gray-200 bg-gray-50 p-6">
            <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div>
                <h4 class="text-sm font-medium text-gray-500">Network User ID</h4>
                <p class="mt-1 rounded bg-gray-100 px-2 py-1 font-mono text-gray-800">
                  {{ currentNetworkUser.id }}
                </p>
              </div>
              <div>
                <h4 class="text-sm font-medium text-gray-500">Joined</h4>
                <p class="mt-1 text-gray-800">
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
              class="mt-6"
            >
              <details class="group">
                <summary
                  class="flex cursor-pointer items-center justify-between text-sm font-semibold text-gray-700"
                >
                  User Accesses ({{ network.networkAccesses.length }})
                  <span class="ml-2 transition-transform group-open:rotate-180">&#9660;</span>
                </summary>
                <ul class="mt-4 space-y-3">
                  <li
                    v-for="access in network.networkAccesses"
                    :key="access.accessId"
                    class="flex justify-between rounded-md border border-gray-200 bg-gray-50 p-3"
                  >
                    <div class="flex items-start">
                      <div class="mt-1 flex-shrink-0">
                        <span
                          v-if="
                            currentNetworkUser.networkUserAccesses.find(
                              (nua) => nua.accessId == access.accessId,
                            )?.isAccepted
                          "
                          class="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"
                        >
                          Accepted
                        </span>
                        <span
                          v-else
                          class="inline-flex items-center rounded-full bg-red-100 px-2 py-0.5 text-xs font-medium text-red-800"
                        >
                          Rejected
                        </span>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-semibold text-gray-800">
                          {{ access.access.name }}
                        </div>
                        <div class="text-sm text-gray-600">{{ access.access.description }}</div>
                      </div>
                    </div>
                    <div class="flex items-center">
                      <button
                        v-if="
                          !access.isRequired ||
                          !currentNetworkUser?.networkUserAccesses.find(
                            (x) => x.accessId == access.accessId,
                          )?.isAccepted
                        "
                        @click="handleToggle(access.accessId)"
                        class="mr-3 rounded-md border border-gray-200 bg-gray-100 px-4 py-2 text-sm text-gray-800"
                      >
                        Toggle
                      </button>
                      <span
                        v-if="access.isRequired"
                        class="mr-3 rounded-md border border-gray-200 bg-gray-100 px-4 py-2 text-sm text-gray-500"
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
              class="mt-6 text-sm text-gray-500"
            >
              This user has no recorded network accesses.
            </div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="mt-6 flex justify-end space-x-3 border-t border-gray-200 pt-4">
          <button
            type="button"
            class="rounded-md border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
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
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import api from '@/api/api';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import useNetworks from '@/composables/useNetworks';

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

// Simulated connected websites - in a real app, you would fetch this from API
const connectedWebsites = ref([
  {
    name: 'Vue Login Test',
    url: 'http://localhost:5174',
    icon: 'https://ui-avatars.com/api/?name=VT&size=32&background=random',
  },
  // {
  //   name: 'Example Support System',
  //   url: 'https://support.example.com',
  //   icon: 'https://ui-avatars.com/api/?name=ES&size=32&background=random'
  // }
]);

const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const isSubmitting = ref(false);
const confirmationAction = ref(() => {});

function handleLeaveNetwork() {
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
        (nu) => nu.userProxyId == userProxyId,
      )?.id;

      if (networkUserId == null) throw new Error('No NetworkUserId');

      await api.delete(`/networks/${networkId}/users/${networkUserId}`);
      router.push('/networks');
    } finally {
      isSubmitting.value = false;
      showConfirmationModal.value = false;
    }
  };
}

async function handleToggle(accessId: string) {
  const access = currentNetworkUser.value?.networkUserAccesses.find((x) => x.accessId == accessId);
  const networkAccess = network.value?.networkAccesses.find((x) => x.accessId == accessId);

  if (!access) return alert('Access not found.');

  showConfirmationModal.value = true;
  confirmationTitle.value = 'Confirm Access Toggle';
  confirmationMessage.value = `Are you sure you want to ${access.isAccepted ? 'REVOKE' : 'PERMIT'} access to '${networkAccess?.access.name}' to this network?`;
  confirmButtonText.value = 'Confirm';
  confirmButtonColor.value = 'green';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    try {
      const userProxyId = authStore.getUserProxyId();
      const networkId = route.params.networkId as string;
      const networkUserId = network.value?.networkUsers.find(
        (nu) => nu.userProxyId == userProxyId,
      )?.id;

      await api.put(`/networks/${networkId}/users/${networkUserId}/accesses/${accessId}`, {
        isAccepted: !access.isAccepted,
      });

      access.isAccepted = !access.isAccepted;
    } finally {
      isSubmitting.value = false;
      showConfirmationModal.value = false;
    }
  };
}

// Check if user has permission to manage the network
const canManageNetwork = computed(
  () => network.value && authStore.canI.manageNetwork(network.value),
);

const currentNetworkUser = computed(() => {
  if (!authStore.isAuthenticated) return null;

  const userProxyId = authStore.getUserProxyId();
  if (!network.value || !userProxyId) return null;

  // Check if user exists in the network's users
  return network.value.networkUsers?.find((networkUser) => networkUser.userProxyId === userProxyId);
});

// Check if user is already in the network
const isUserInNetwork = computed(() => {
  return currentNetworkUser.value != null;
});

onMounted(async () => {
  const networkId = route.params.networkId as string;
  global.startFetching();
  try {
    await fetchNetworkDetails(networkId);
  } finally {
    global.stopFetching();
  }
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
