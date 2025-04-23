<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200">
        <h1 class="text-2xl font-semibold text-gray-800">Create New Network</h1>
      </div>
      
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
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import type { Role, Access } from '@/types';
// import { useAxios } from '@/composables/useAxios';
import api from '@/api/api';

const router = useRouter();
// const { axiosInstance } = useAxios();

const availableRoles = ref<Role[]>([]);
const loadingRoles = ref(true);
const rolesError = ref('');

const availableAccesses = ref<Access[]>([]);
const loadingAccesses = ref(true);
const accessesError = ref('');

const isSubmitting = ref(false);

const formData = reactive({
  name: '',
  isSystemProtected: false,
  selectedRoles: [] as string[],
  selectedAccesses: [] as string[],
  requiredAccesses: {} as Record<string, boolean>
});

onMounted(async () => {
  await Promise.all([
    fetchRoles(),
    fetchAccesses()
  ]);
});

async function fetchRoles() {
  loadingRoles.value = true;
  rolesError.value = '';
  
  try {
    const response = await api.get<Role[]>('/api/roles/');
    availableRoles.value = response.data.results || [];
  } catch (error: any) {
    rolesError.value = error.response?.data?.message || 'Failed to fetch roles';
    console.error('Error fetching roles:', error);
  } finally {
    loadingRoles.value = false;
  }
}

async function fetchAccesses() {
  loadingAccesses.value = true;
  accessesError.value = '';
  
  try {
    const response = await api.get<Access[]>('/api/accesses/');
    availableAccesses.value = response.data.results || [];
  } catch (error: any) {
    accessesError.value = error.response?.data?.message || 'Failed to fetch accesses';
    console.error('Error fetching accesses:', error);
  } finally {
    loadingAccesses.value = false;
  }
}

async function handleSubmit() {
  isSubmitting.value = true;
  
  try {
    // First create the network
    const networkResponse = await api.post('/api/networks/', {
      name: formData.name,
      isSystemProtected: formData.isSystemProtected
    });
    
    const networkId = networkResponse.data.id;
    
    // Add selected roles
    if (formData.selectedRoles.length > 0) {
      await Promise.all(formData.selectedRoles.map(roleId => 
        api.post('/api/network-roles/', {
          networkId,
          roleId
        })
      ));
    }
    
    // Add selected accesses
    if (formData.selectedAccesses.length > 0) {
      await Promise.all(formData.selectedAccesses.map(accessId => 
        api.post('/api/network-accesses/', {
          networkId,
          accessId,
          isRequired: !!formData.requiredAccesses[accessId]
        })
      ));
    }
    
    // Navigate to the network management page
    router.push(`/networks/${networkId}`);
  } catch (error: any) {
    console.error('Error creating network:', error);
    alert(error.response?.data?.message || 'Failed to create network');
  } finally {
    isSubmitting.value = false;
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