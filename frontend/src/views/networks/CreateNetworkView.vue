<template>
  <div class="container mx-auto px-4 py-8 mt-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <!-- <h1 class="text-2xl font-semibold text-gray-800">Networks</h1> -->
        <h1 class="text-2xl font-semibold text-gray-800">Create New Network</h1>
      </div>
     
      <!-- Error Message Component -->
      <div class="mx-6 mt-6">
        <ErrorAlert 
          :message="error" 
          @dismiss="error = ''" 
        />
      </div>
      
      <div class="p-6">
        <form @submit.prevent="handleSubmit">
          <!-- Basic Network Information Section -->
          <div class="mb-8">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <!-- Network Name -->
              <div class="col-span-2">
                <label for="networkName" class="block text-sm font-medium text-gray-700 mb-2">
                  Network Name <span class="text-red-500">*</span>
                </label>
                <input
                  id="networkName"
                  v-model="formData.name"
                  type="text"
                  class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Enter network name"
                  required
                />
                <p class="mt-1 text-xs text-gray-500">Network name must be unique across the system</p>
              </div>
              
              <!-- Network Image URL -->
              <div>
                <label for="imageUrl" class="block text-sm font-medium text-gray-700 mb-2">
                  Network Logo URL
                </label>
                <input
                  id="imageUrl"
                  v-model="formData.imageUrl"
                  type="url"
                  class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="https://example.com/logo.png"
                />
                <p class="mt-1 text-xs text-gray-500">URL to the network's logo image (optional)</p>
              </div>

              <!-- Network Login RedirectURI -->
              <div>
                <label for="imageUrl" class="block text-sm font-medium text-gray-700 mb-2">
                  Network Login Callback
                </label>
                <input
                  id="imageUrl"
                  v-model="formData.redirectURI"
                  type="url"
                  class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="https://example.com/callback"
                />
                <p class="mt-1 text-xs text-gray-500">URL to navigate to in order to complete authentication (optional)</p>
              </div>
            </div>
            
            <!-- Network Description -->
            <div class="mb-6">
              <label for="description" class="block text-sm font-medium text-gray-700 mb-2">
                Description
              </label>
              <textarea
                id="description"
                v-model="formData.description"
                rows="3"
                class="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Enter network description"
              ></textarea>
              <p class="mt-1 text-xs text-gray-500">Brief description of the network's purpose</p>
            </div>
            
            <!-- Network Visibility -->
            <div class="mb-6">
              <div class="flex items-center">
                <input
                  id="isPublic"
                  v-model="formData.isPublic"
                  type="checkbox"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label for="isPublic" class="ml-2 block text-sm text-gray-700">
                  Make this network public
                </label>
              </div>
              <p class="mt-1 text-xs text-gray-500 ml-6">Public networks are discoverable by all users</p>
            </div>
          </div>
          
          <!-- Form Actions -->
          <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
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
  description: '',
  isPublic: false,
  imageUrl: undefined as string | undefined,
  redirectURI: '',
});

async function handleSubmit() {
  // Clear any previous errors
  error.value = '';
  global.startFetching();
  isSubmitting.value = true;
 
  try {
    const networkResponse = await api.post<Network, CreateNetwork>('/networks/', {
      name: formData.name.trim(),
      description: formData.description.trim(),
      imageUrl: formData.imageUrl,
      redirectURI: formData.redirectURI,
      isPublic: formData.isPublic,
    });
   
    const networkId = networkResponse.data.id;
   
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