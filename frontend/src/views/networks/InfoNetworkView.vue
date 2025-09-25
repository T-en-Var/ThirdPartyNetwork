<template>
  <div class="container mx-auto px-4 py-8 mt-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-2xl font-semibold text-gray-800">Network Details</h1>
        <div class="flex space-x-3">
          <button @click="handleNavigateBack"
            class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50">
            Back to Networks
          </button>
          <button v-if="canManageNetwork" @click="handleManageNetwork"
            class="px-4 py-2 bg-blue-600 text-white rounded-md shadow-sm hover:bg-blue-700">
            Manage Network
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="mx-6 my-6">
        <ErrorAlert :message="error" @dismiss="error = ''" />
        <div class="flex justify-center mt-4">
          <button @click="router.push('/networks')"
            class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
            Return to Networks
          </button>
        </div>
      </div>

      <!-- Network Content -->
      <div v-else-if="network" class="p-6">
        <!-- Network Header with Logo -->
        <div class="flex justify-between">
          <div class="flex items-center mb-6">
            <div
              class="h-16 w-16 rounded-lg bg-white border border-gray-200 p-1 shadow-sm flex items-center justify-center mr-4">
              <div v-if="loading"
                class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin"></div>
              <CloudinaryFile v-else-if="network?.imageFile" :display-only="true" :file="network?.imageFile"
                class="w-10 max-h-10 object-cover" />
              <div v-else class="logo">
                <img :src="`https://ui-avatars.com/api/?name=${network?.name}&size=24&background=random`"
                  :alt="network?.name" />
              </div>
            </div>
            <div>
              <h2 class="text-xl font-bold text-gray-800 flex items-center">
                {{ network.name }}
                <span v-if="network.isSystemProtected"
                  class="ml-3 px-3 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full">
                  Protected
                </span>
                <span v-if="network.isPublic"
                  class="ml-3 px-3 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full">
                  Public
                </span>
              </h2>
              <p class="text-gray-600">{{ network.description || 'No description available' }}</p>
            </div>
          </div>

          <div>
            <div>
              <button v-if="!isUserInNetwork" @click="handleJoinNetwork"
                class="w-full px-16 py-3 bg-blue-600 text-white rounded-md shadow-sm hover:bg-blue-700">
                Join Network
              </button>

              <button v-else-if="!network.isSystemProtected" @click="handleLeaveNetwork"
                class="w-full px-16 py-3 bg-red-500 text-white rounded-md shadow-sm hover:bg-red-600">
                Leave Network
              </button>
            </div>
          </div>
        </div>

        <!-- Network Access Requirements -->
        <div class="mb-6">
          <h3 class="text-lg font-medium text-gray-800 mb-3">Access Requirements</h3>

          <div v-if="network?.networkAccesses.length > 0" class="bg-gray-50 rounded-lg p-4 border border-gray-200">
            <details class="group">
              <summary class="cursor-pointer flex items-center justify-between text-gray-700 text-sm font-semibold">
                Network Accesses ({{ network?.networkAccesses.length }})
                <span class="ml-2 group-open:rotate-180 transition-transform">&#9660;</span>
              </summary>
              <ul class="mt-4 space-y-3">
                <li v-for="networkAccess in network?.networkAccesses" :key="networkAccess.accessId"
                  class="flex items-start p-3 bg-gray-50 rounded-md border border-gray-200">
                  <div class="flex-shrink-0 mt-1">
                    <span v-if="networkAccess.isRequired"
                      class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                      Required
                    </span>
                    <span v-else
                      class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
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

          <div v-else class="bg-gray-50 border border-gray-200 rounded-lg p-6 text-center">
            <p class="text-gray-600">This network has no required accesses.</p>
          </div>
        </div>

        <!-- Connected Websites -->
        <div class="mb-6">
          <h3 class="text-lg font-medium text-gray-800 mb-3">Connected Websites</h3>

          <div v-if="connectedWebsites.length > 0 && false" class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div v-for="(website, index) in connectedWebsites" :key="index"
              class="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
              <div class="flex items-center">
                <img :src="website.icon || `https://www.google.com/s2/favicons?domain=${website.url}`"
                  :alt="website.name" class="w-6 h-6 mr-3" />
                <div>
                  <h4 class="font-medium text-gray-800">{{ website.name }}</h4>
                  <a :href="website.url" target="_blank" class="text-sm text-blue-600 hover:underline">
                    {{ website.url }}
                  </a>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="bg-gray-50 border border-gray-200 rounded-lg p-6 text-center">
            <p class="text-gray-600">No websites are currently using this network for authentication.</p>
          </div>
        </div>

        <!-- Network Information -->
        <div class="mb-6">
          <h3 class="text-lg font-medium text-gray-800 mb-3">Network Information</h3>

          <div class="bg-gray-50 rounded-lg p-6 border border-gray-200">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h4 class="text-sm font-medium text-gray-500">Network ID</h4>
                <p class="mt-1 text-gray-800 font-mono bg-gray-100 px-2 py-1 rounded">{{ network.id }}</p>
              </div>
              <div>
                <h4 class="text-sm font-medium text-gray-500">Created</h4>
                <p class="mt-1 text-gray-800">{{ network.createdOn ? new Date(network.createdOn).toLocaleString() :
                  'Unknown' }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- User Information -->
        <div v-if="isUserInNetwork && currentNetworkUser">
          <h3 class="text-lg font-medium text-gray-800 mb-3">User Information</h3>

          <div class="bg-gray-50 rounded-lg p-6 border border-gray-200">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h4 class="text-sm font-medium text-gray-500">Network User ID</h4>
                <p class="mt-1 text-gray-800 font-mono bg-gray-100 px-2 py-1 rounded">
                  {{ currentNetworkUser.id }}
                </p>
              </div>
              <div>
                <h4 class="text-sm font-medium text-gray-500">Joined</h4>
                <p class="mt-1 text-gray-800">
                  {{ currentNetworkUser.createdOn ? new Date(currentNetworkUser.createdOn).toLocaleString() : 'Unknown'
                  }}
                </p>
              </div>
            </div>

            <!-- Dropdown / details block -->
            <div v-if="network.networkAccesses?.length" class="mt-6">
              <details class="group">
                <summary class="cursor-pointer flex items-center justify-between text-gray-700 text-sm font-semibold">
                  User Accesses ({{ network.networkAccesses.length }})
                  <span class="ml-2 group-open:rotate-180 transition-transform">&#9660;</span>
                </summary>
                <ul class="mt-4 space-y-3">
                  <li v-for="access in network.networkAccesses" :key="access.accessId"
                    class="flex justify-between p-3 bg-gray-50 rounded-md border border-gray-200">
                    <div class="flex items-start">
                      <div class="flex-shrink-0 mt-1">
                        <span
                          v-if="currentNetworkUser.networkUserAccesses.find(nua => nua.accessId == access.accessId)?.isAccepted"
                          class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                          Accepted
                        </span>
                        <span v-else
                          class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                          Rejected
                        </span>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-semibold text-gray-800">{{ access.access.name }}</div>
                        <div class="text-sm text-gray-600">{{ access.access.description }}</div>
                      </div>
                    </div>
                    <button @click="handleToggle(access.accessId)"
                      class="px-4 py-2 mr-3 text-sm border rounded-md bg-gray-100 text-gray-800 border-gray-200">
                      Toggle
                    </button>
                  </li>
                </ul>
              </details>
            </div>
            <div v-else class="mt-6 text-sm text-gray-500">
              This user has no recorded network accesses.
            </div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="flex justify-end space-x-3 pt-4 mt-6 border-t border-gray-200">
          <button type="button" class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
            @click="router.push('/networks')">
            Back to Networks
          </button>
        </div>
      </div>
    </div>

    <confirmation-modal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
      :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
      @close="showConfirmationModal = false" @confirm="confirmationAction" />
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
import useNetworkDetails from '@/composables/useNetworkDetails';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const global = useGlobalStore();

const { network, loading, error, fetchNetworkDetails } = useNetworkDetails();

// Simulated connected websites - in a real app, you would fetch this from API
const connectedWebsites = ref([
  {
    name: 'Vue Login Test',
    url: 'http://localhost:5174',
    icon: 'https://ui-avatars.com/api/?name=VT&size=32&background=random'
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
const confirmationAction = ref(() => { });

function handleLeaveNetwork() {
  showConfirmationModal.value = true;
  confirmationTitle.value = 'Confirm Leave Network'
  confirmationMessage.value = 'Are you sure you want to leave this network?'
  confirmButtonText.value = 'Leave'
  confirmButtonColor.value = 'red'

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    try {
      const userProxyId = authStore.getUserProxyId();
      const networkId = route.params.networkId as string;
      const networkUserId = network.value?.networkUsers.find(nu => nu.userProxyId == userProxyId)?.id;

      if (networkUserId == null) throw new Error("No NetworkUserId");

      await api.delete(`/networks/${networkId}/users/${networkUserId}`)
      router.push('/networks');
    } finally {
      isSubmitting.value = false;
      showConfirmationModal.value = false;
    }
  }
}

async function handleToggle(accessId: string) {
  const access = currentNetworkUser.value?.networkUserAccesses.find(x => x.accessId == accessId);
  const networkAccess = network.value?.networkAccesses.find(x => x.accessId == accessId);

  if (!access) return alert("Access not found.")

  showConfirmationModal.value = true;
  confirmationTitle.value = 'Confirm Access Toggle'
  confirmationMessage.value = `Are you sure you want to ${access.isAccepted ? 'REVOKE' : 'PERMIT'} access to '${networkAccess?.access.name}' to this network?`
  confirmButtonText.value = 'Confirm'
  confirmButtonColor.value = 'green'

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    try {
      const userProxyId = authStore.getUserProxyId();
      const networkId = route.params.networkId as string;
      const networkUserId = network.value?.networkUsers.find(nu => nu.userProxyId == userProxyId)?.id;

      if (networkUserId == null) throw new Error("No NetworkUserId");

      await api.delete(`/networks/${networkId}/users/${networkUserId}`)
      router.push('/networks');
    } finally {
      isSubmitting.value = false;
      showConfirmationModal.value = false;
    }
  }
}

// Check if user has permission to manage the network
const canManageNetwork = computed(() => network.value && authStore.claimChecker.canManageNetwork(network.value));

const currentNetworkUser = computed(() => {
  if (!authStore.isAuthenticated()) return null;

  const userProxyId = authStore.getUserProxyId();
  if (!network.value || !userProxyId) return null;

  // Check if user exists in the network's users
  return network.value.networkUsers?.find(
    networkUser => networkUser.userProxyId === userProxyId
  );
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