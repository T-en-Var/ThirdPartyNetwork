<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200">
        <h1 class="text-2xl font-semibold text-gray-800">Create New Network</h1>
      </div>
     
      <!-- Error Message Component -->
      <div class="mx-6 mt-6">
        <ErrorAlert 
          :message="error" 
          @dismiss="error = ''" 
        />
      </div>
      <!-- End Error Message Component -->
      
      <div class="p-6">
        <form @submit.prevent="handleSubmit">
          <div class="mb-6">
            <label for="networkName" class="block text-sm font-medium text-gray-700 mb-2">
              Network Name
            </label>
            <input
              id="networkName"
              v-model="formData.name"
              type="text"
              class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Enter network name"
              required
            />
          </div>
          <div class="flex justify-end space-x-3">
            <button
              type="button"
              class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              @click="navigateBack"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              :disabled="isSubmitting"
            >
              <span v-if="isSubmitting" class="flex items-center">
                <span class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"></span>
                Creating...
              </span>
              <span v-else>Create Network</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import type { ErrorMessage, Network, CreateNetwork } from '@/types';
import api from '@/api/api';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';
import ErrorAlert from '@/components/ErrorAlert.vue';

const router = useRouter();
const global = useGlobalStore();
const isSubmitting = ref(false);
const error = ref('');

const formData = reactive({
  name: '',
  isSystemProtected: false,
  selectedRoles: [] as string[],
  selectedAccesses: [] as string[],
  requiredAccesses: {} as Record<string, boolean>
});

async function handleSubmit() {
  // Clear any previous errors
  error.value = '';
  global.startFetching();
  isSubmitting.value = true;
 
  try {
    // First create the network
    const networkResponse = await api.post<Network, CreateNetwork>('/networks/', {
      name: formData.name.trim(),
    });
   
    const networkId = networkResponse.data.id;
   
    // Navigate to the network management page
    router.push(`/networks/${networkId}/manage`);
  } catch (err) {
    console.error('Error creating network:', err);
    const axiosError = err as AxiosError<ErrorMessage>;
    
    // Handle specific error cases
    if (axiosError.response?.status === 409) {
      error.value = `A network with the name "${formData.name.trim()}" already exists. Please choose a different name.`;
    } else {
      error.value = axiosError.response?.data?.message || 'Failed to create network. Please try again later.';
    }
  } finally {
    isSubmitting.value = false;
    global.stopFetching();
  }
}

function navigateBack() {
  router.push('/networks');
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