<template>
  <div class="container mx-auto px-4 py-8 mt-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-2xl font-semibold text-gray-800">Join Network</h1>
      </div>

      <div class="p-6">
        <!-- Network Information -->
        <div
          class="mb-8 bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden hover:shadow-md transition-shadow cursor-pointer"
          @click="handleNetworkDetails">
          <div class="p-4">
            <div class="flex items-center mb-4">
              <div class="logo-container mr-4">
                <div class="logo">
                  <div v-if="loading"
                    class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin"></div>
                  <CloudinaryFile v-else-if="network?.imageFile" :display-only="true" :file="network?.imageFile"
                    class="w-10 max-h-10 object-cover" />
                  <div v-else class="logo">
                    <img :src="`https://ui-avatars.com/api/?name=${network?.name}&size=24&background=random`"
                      :alt="network?.name" />
                  </div>
                </div>
              </div>
              <div class="flex-1">
                <div class="flex justify-between items-center">
                  <div>
                    <h2 class="text-xl font-semibold text-gray-800">{{ network?.name }}</h2>
                    <p class="text-sm text-gray-600">{{ network?.networkUsers?.length || 0 }} members</p>
                  </div>
                  <span v-if="network?.isSystemProtected"
                    class="px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    Protected
                  </span>
                </div>
              </div>
            </div>
            <p class="text-gray-600 text-sm">{{ network?.description || 'No description available.' }}</p>
          </div>
        </div>

        <!-- Required Accesses Section -->
        <div class="mb-8" v-if="loading">
          <div class="flex justify-center">
            <div class="spinner-border"></div>
          </div>
        </div>

        <div v-else-if="network && network.networkAccesses.length > 0">
          <h3 class="text-lg font-medium text-gray-800 mb-4">Required Data Access</h3>
          <p class="text-gray-600 mb-4">This network requests access the following information to function properly:</p>

          <div class="space-y-4 mb-6">
            <div v-for="access in network.networkAccesses" :key="access.accessId"
              class="p-4 border border-gray-200 rounded-md">
              <div class="flex items-start">
                <div class="flex items-center h-6">
                  <input :id="access.accessId" v-model="userAccesses[access.accessId]" type="checkbox"
                    class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    :disabled="access.isRequired" @change="validateRequiredAccesses" />
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
        <div v-else-if="network">
          <p class="text-gray-600">This network does not require any special permissions.</p>
        </div>

        <div class="mb-8">
          <h2 class="text-lg font-medium text-gray-800 mb-4">Currently Logged In As</h2>
          <UserProxyDisplay :userProxy="currentUser" :sensitiveFields="['email', 'lastName']"
            :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']" @switch-account="switchAccount">
          </UserProxyDisplay>
        </div>

        <div class="m-6">
          <ErrorAlert :message="submitError" @dismiss="submitError = ''" />
        </div>

        <!-- Join Network Form -->
        <form @submit.prevent="handleJoinNetwork">
          <!-- Form Actions -->
          <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
            <button type="button" class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              @click="navigateBack">
              Cancel
            </button>
            <button type="submit" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              :disabled="isSubmitting">
              <span v-if="isSubmitting" class="flex items-center">
                <span
                  class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"></span>
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
import type { CreateNetworkUser, ErrorMessage, NetworkUser, UserProxy } from '@/types';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';
import ErrorAlert from '@/components/ErrorAlert.vue';
import { useAuthStore } from '@/stores/auth';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue'
import api from '@/api/api';
import useNetworkDetails from '@/composables/useNetworkDetails';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const isSubmitting = ref(false);
const submitError = ref('');
const userAccesses = ref<{ [key: string]: boolean }>({});
const authStore = useAuthStore();

const currentUser = ref<UserProxy | null>(null);

const { network, loading, error, fetchNetworkDetails } = useNetworkDetails();

onMounted(async () => {
  const networkId = route.params.networkId as string;
  await fetchNetworkDetails(networkId);
  currentUser.value = await authStore.getUserProxy()

  if (network.value) {
    for (const access of network.value.networkAccesses) {
      userAccesses.value[access.accessId] = access.isRequired ? true : false;
    }
  }
});

function validateRequiredAccesses() {
  if (!network.value) return;

  network.value.networkAccesses.forEach(access => {
    if (access.isRequired) {
      userAccesses.value[access.accessId] = true;
    }
  });
}

async function handleJoinNetwork() {
  if (!currentUser.value || !network.value) return;
  const networkId = route.params.networkId as string;
  const userId = currentUser.value.id;

  // Clear any previous errors
  error.value = '';
  global.startFetching();
  isSubmitting.value = true;

  try {
    // We assume in the backend, when joining a network, all accesses
    // which are required are accepted and which are not are not.
    const { data: networkUser } = await api.post<NetworkUser, CreateNetworkUser>(
      `/networks/${networkId}/users/${userId}`, {}
    );

    const acceptedAccesses = network.value!.networkAccesses
      .filter(access => !access.isRequired && userAccesses.value[access.accessId])
      .map(access => access.accessId);

    await Promise.all(
      acceptedAccesses.map(accessId =>
        api.put(`/networks/${networkId}/users/${networkUser.id}/accesses/${accessId}/`, { isAccepted: true })
      )
    );

    router.push(`/networks/${networkId}`);
  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;
    if (axiosError.response?.status == 409) {
      submitError.value = "You already joined this network!"
    } else {
      submitError.value = axiosError.response?.data?.message || 'Failed to join network. Please try again later.';
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
  alert('not implemented, sorry');
}

function handleNetworkDetails() {
  const networkId = route.params.networkId as string;
  router.push(`/networks/${networkId}`);
}
</script>

<style scoped>
.spinner-border {
  width: 1.5rem;
  height: 1.5rem;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}

@keyframes spinner-border {
  100% {
    transform: rotate(360deg);
  }
}
</style>