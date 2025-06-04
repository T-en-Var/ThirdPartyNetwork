<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-2xl font-semibold text-gray-800">Update Access</h1>
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
                  <img
                    :src="network?.imageUrl ? network.imageUrl : `https://ui-avatars.com/api/?name=${network?.name}&size=64&background=random`"
                    :alt="network?.name || 'Network Logo'" class="object-contain rounded" />
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
        <div class="mb-8" v-if="loading || pageLoading">
          <div class="flex justify-center">
            <div class="spinner-border"></div>
          </div>
        </div>

        <div v-else-if="network && network.networkAccesses.length > 0">
          <h3 class="text-lg font-medium text-gray-800 mb-4">Required Data Access</h3>
          <p class="text-gray-600 mb-4">Update your consent for the following data accesses:</p>

          <div class="space-y-4 mb-6">
            <div v-for="access in network.networkAccesses" :key="access.accessId"
              class="p-4 border border-gray-200 rounded-md">
              <div class="flex items-start">
                <div class="flex items-center h-6">
                  <input :id="access.accessId" v-model="userAccesses[access.accessId].value" type="checkbox"
                    class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    :disabled="access.isRequired && currentNetworkUser?.networkUserAccesses.some(n => n.accessId == access.accessId && n.isAccepted)"
                    @change="validateRequiredAccesses" />
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
            :fieldsToDisplay="['username', 'firstName', 'lastName', 'email']" @switch-account="switchAccount" />
        </div>

        <div class="m-6">
          <ErrorAlert :message="submitError" @dismiss="submitError = ''" />
        </div>

        <!-- Access Update Form -->
        <form @submit.prevent="handleUpdateAccesses">
          <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
            <button type="button" class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              @click="navigateBack">
              Back
            </button>
            <button type="submit" :class="`px-4 py-2 ${canSubmit && !isSubmitting ? 'bg-blue-600 hover:bg-blue-700' : 'bg-gray-400 cursor-not-allowed'} text-white rounded-md `"
              :disabled="!canSubmit || isSubmitting">
              <span v-if="isSubmitting" class="flex items-center">
                <span
                  class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"></span>
                Updating...
              </span>
              <span v-else>Update Access</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import type { ErrorMessage, NetworkUser, UserProxy } from '@/types';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';
import ErrorAlert from '@/components/ErrorAlert.vue';
import UserProxyDisplay from '@/components/UserProxyDisplay.vue'
import api from '@/api/api';
import useNetworkDetails from '@/composables/useNetworkDetails';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();
const isSubmitting = ref(false);
const submitError = ref('');
const userAccesses = ref<{ [key: string]: { value: boolean, userChecked: boolean } }>({});
const currentUser = ref<UserProxy | null>(null);
const currentNetworkUser = ref<NetworkUser | null>(null);
const { network, loading, error, fetchNetworkDetails } = useNetworkDetails();
let temporaryAccessToken = '';
const pageLoading = ref(true);
const canSubmit = computed(() => {
  if (!network.value || pageLoading.value) return false;

  for (const na of network.value.networkAccesses) {
    if (na.isRequired && !userAccesses.value[na.accessId].value) return false;
  }

  return true;
})

onMounted(async () => {
  const networkId = route.params.networkId as string;

  temporaryAccessToken = localStorage.getItem('temporaryAccessToken') || '';
  if (!temporaryAccessToken) {
    router.push(`/networks/${networkId}/login`)
      pageLoading.value = false;
      return;
  }

  await fetchNetworkDetails(networkId);

  if (network.value) {
    const userResponse = await api.get<UserProxy>(`/me`, {
      headers: {
        "Authorization": `Bearer ${temporaryAccessToken}`
      }
    });

    if (userResponse.status != 200) {
      pageLoading.value = false;
      return;
    }

    currentUser.value = userResponse.data;

    const networkUser = currentUser.value.networkUsers.find(nu => nu.networkId == networkId);
    if (!networkUser) {
      pageLoading.value = false;
      return;
    }
    currentNetworkUser.value = networkUser;

    for (const na of network.value.networkAccesses) {
      const isCheckedByUser = networkUser?.networkUserAccesses.some(nua => nua.accessId == na.accessId && nua.isAccepted);
      userAccesses.value[na.accessId] = { value: isCheckedByUser, userChecked: false };
    }
  }
  pageLoading.value = false;
});

function validateRequiredAccesses(e: Event) {
  if (!network.value) return;

  const currentElementId = (e.target as HTMLInputElement).id;

  network.value.networkAccesses.forEach(access => {
    if (access.isRequired && access.accessId == currentElementId) {
      const oldValue = userAccesses.value[access.accessId];
      userAccesses.value[access.accessId] = { value: oldValue.userChecked ? true : oldValue.value, userChecked: true };
    }
  });
}

async function handleUpdateAccesses() {
  if (!currentUser.value || !network.value) return;

  const networkId = route.params.networkId as string;
  const networkUserId = currentNetworkUser.value?.id;

  error.value = '';
  global.startFetching();
  isSubmitting.value = true;

  try {
    const acceptedAccesses = network.value!.networkAccesses
      .filter(access => !access.isRequired && userAccesses.value[access.accessId])
      .map(access => access.accessId);

    await Promise.all(
      acceptedAccesses.map(accessId =>
        api.put(`/networks/${networkId}/users/${networkUserId}/accesses/${accessId}/`, { isAccepted: true }, {
          headers: {
            "Authorization": `Bearer ${temporaryAccessToken}`
          }
        })
      )
    );

    const redirectUrl = atob(route.query.redirectUri as string);
    window.location.href = redirectUrl;
  } catch (err) {
    const axiosError = err as AxiosError<ErrorMessage>;
    submitError.value = axiosError.response?.data?.message || 'Failed to update access. Please try again later.';

    if (axiosError.response?.status == 403) {
      submitError.value += "\nYour session has likely expired, please log in again to try again."
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
  console.log('switch');
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
