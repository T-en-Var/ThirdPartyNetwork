<template>
  <div class="container mx-auto px-4 py-10 mt-6">
    <div class="bg-white shadow-lg rounded-2xl overflow-hidden border border-gray-200">
      <LoadingErrorComponent :loading="networkState.loading.value" :error="networkState.error.value"
        button-value="Go back" @button-action="router.go(-1)" />

      <div v-if="!networkState.loading.value && !networkState.error.value && networkState.network.value">
        <!-- Header -->
        <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
          <h1 class="text-2xl font-semibold text-gray-800">Create New Network</h1>
        </div>

        <!-- Error Message -->
        <div class="mx-8 mt-6">
          <ErrorAlert :message="error" @dismiss="error = ''" />
        </div>

        <div class="p-8">
          <form @submit.prevent="handleSubmit" class="space-y-10">
            <!-- Basic Information -->
            <section>
              <h2 class="text-lg font-medium text-gray-800 mb-4">Basic Information</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <!-- Network Name -->
                <div>
                  <label for="networkName" class="block text-sm font-medium text-gray-700 mb-2">
                    Network Name <span class="text-red-500">*</span>
                  </label>
                  <input id="networkName" v-model="form.name" type="text"
                    class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Enter network name" required />
                  <p class="mt-1 text-xs text-gray-500">
                    Network name must be unique across the system
                  </p>
                </div>

                <!-- Network Image -->
                <div>
                  <h2 class="block text-sm font-medium text-gray-700 mb-2">Network Image</h2>
                  <div class="flex flex-col items-center">
                    <!-- Already uploaded -->
                    <div v-if="uploadedFile" class="flex flex-col items-center">
                      <CloudinaryFile :file="uploadedFile" :display-only="true"
                        class="h-24 w-24 rounded-full object-cover shadow" />
                      <div class="flex gap-4 mt-4">
                        <button type="button" class="text-sm text-red-600 hover:underline" @click="removeImage">
                          Remove
                        </button>
                        <button type="button" class="text-sm text-blue-600 hover:underline" @click="openUploadModal">
                          Change
                        </button>
                      </div>
                    </div>

                    <!-- No image yet -->
                    <div v-else class="flex flex-col items-center">
                      <div class="h-24 w-24 rounded-full bg-gray-100 flex items-center justify-center text-gray-400">
                        No Image
                      </div>
                      <button type="button"
                        class="mt-4 rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-700 shadow-sm hover:bg-gray-50"
                        @click="openUploadModal">
                        Upload Image
                      </button>
                    </div>
                  </div>
                  <AddFileModal v-if="showUploadModal" :network="networkState.network.value"
                    @close="showUploadModal = false" @uploaded="handleImageUploaded" media-type="image" />
                </div>
              </div>
            </section>

            <!-- Description -->
            <section>
              <h2 class="text-lg font-medium text-gray-800 mb-4">Description</h2>
              <textarea id="description" v-model="form.description" rows="3"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Enter network description"></textarea>
              <p class="mt-1 text-xs text-gray-500">
                Brief description of the network's purpose
              </p>
            </section>

            <!-- Visibility -->
            <section>
              <div class="flex items-center">
                <input id="isPublic" v-model="form.isPublic" type="checkbox"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded" />
                <label for="isPublic" class="ml-2 text-sm text-gray-700">
                  Make this network public
                </label>
              </div>
              <p class="mt-1 text-xs text-gray-500 ml-6">
                Public networks are discoverable by all users
              </p>
            </section>

            <!-- Accesses -->
            <section>
              <div class="flex justify-between items-center mb-4">
                <h2 class="text-lg font-medium text-gray-800">What information would you like to see of your users?</h2>
                <button type="button" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                  @click="showAddAccessModal = true">
                  Add Access
                </button>
              </div>

              <div class="space-y-3">
                <div v-for="(networkAccess, index) in fakeNetwork.networkAccesses" :key="index"
                  class="flex items-center justify-between bg-gray-50 p-4 rounded-lg border border-gray-200">
                  <div>
                    <h3 class="text-sm font-medium text-gray-900">{{ networkAccess.access.name }}</h3>
                    <p class="text-xs text-gray-600">{{ networkAccess.isRequired ? "Is required" : "Is optional" }}</p>
                  </div>
                  <button type="button" class="text-red-600 text-sm hover:underline" @click="removeAccess(index)">
                    Remove
                  </button>
                </div>
                <div v-if="!fakeNetwork.networkAccesses.length"
                  class="p-6 text-center text-gray-500 border border-dashed rounded-lg">
                  No accesses added yet
                </div>
              </div>


              <AddAccessModal v-if="showAddAccessModal" :network="fakeNetwork" :is-submitting="isSubmitting"
                @close="showAddAccessModal = false" @add-access="addAccessToNetwork" />
            </section>

            <!-- Form Actions -->
            <div class="flex justify-end space-x-3 pt-6 border-t border-gray-200">
              <button type="button" class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
                @click="navigateBack">
                Cancel
              </button>
              <button type="submit"
                class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 flex items-center"
                :disabled="isSubmitting">
                <span v-if="isSubmitting"
                  class="inline-block h-4 w-4 border-2 border-current border-r-transparent rounded-full animate-spin mr-2"></span>
                <span>{{ isSubmitting ? 'Creating...' : 'Create Network' }}</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <ForceLoadModal :title="titleValue" v-if="isSubmitting" :progress="progessValue" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import type { ErrorMessage, Network, CreateNetwork, NetworkFile, NetworkAccessCreate } from '@/types'
import api from '@/api/api'
import { useGlobalStore } from '@/stores/global'
import type { AxiosError } from 'axios'
import ErrorAlert from '@/components/ErrorAlert.vue'
import AddFileModal from '@/components/modals/network/AddFileModal.vue'
import useNetwork from '@/composables/useNetwork'
import AddAccessModal from '@/components/modals/network/AddAccessModal.vue'
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue'
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue'
import ForceLoadModal from '@/components/modals/ForceWaitModal.vue'

const router = useRouter()
const global = useGlobalStore()
const isSubmitting = ref(false)
const error = ref('');

const titleValue = ref('');
const progessValue = ref(0);

const uploadedFile = ref<NetworkFile | null>(null)
const showUploadModal = ref(false);
const showAddAccessModal = ref(false);

const networkState = useNetwork();

onMounted(async () => {
  await networkState.fetchNetwork(import.meta.env.VITE_NETWORK_ID)
})

const form = ref<CreateNetwork>({
  name: '',
  description: '',
  isPublic: false,
  fileLink: undefined,
  redirectURI: '',
})

const fakeNetwork = ref<Network>({ ...form.value, createdOn: new Date(), customPages: [], files: [], id: '', isSystemProtected: false, networkAccesses: [], networkUsers: [], roles: [], });


function addAccessToNetwork(networkAccess: NetworkAccessCreate) {
  if (!networkAccess.access) return;
  // Mock new access (replace with modal/selection logic later)
  fakeNetwork.value.networkAccesses.push({
    accessId: networkAccess.access.id,
    network: {} as Network,
    networkId: fakeNetwork.value.id,
    access: networkAccess.access!,
    isRequired: networkAccess.isRequired
  })
  showAddAccessModal.value = false
}

function removeAccess(index: number) {
  fakeNetwork.value.networkAccesses.splice(index, 1)
}


async function handleSubmit() {
  error.value = ''
  isSubmitting.value = true
  let newNetwork: Network;
  
  titleValue.value = 'Uploading network data'
  progessValue.value = 0

  try {
    global.startFetching()
    const networkResponse = await api.post<Network, CreateNetwork>('/networks/', {
      name: form.value.name.trim(),
      description: form.value.description.trim(),
      fileLink: form.value.fileLink,
      redirectURI: form.value.redirectURI,
      isPublic: form.value.isPublic,
    })

    newNetwork = networkResponse.data
  } catch (err) {
    console.error('Error creating network:', err)
    const axiosError = err as AxiosError<ErrorMessage>
    if (axiosError.response?.status === 409) {
      error.value = `A network with the name "${form.value.name.trim()}" already exists. Please choose a different name.`
    } else {
      error.value = axiosError.response?.data?.message || 'Failed to create network. Please try again later.'
    }
    isSubmitting.value = false
    return
  } finally {
    global.stopFetching()
  }

  titleValue.value = 'Uploading network accesses'
  progessValue.value = 25

  try {
    global.startFetching()

    await Promise.all([
      ...fakeNetwork.value.networkAccesses.map(networkAccess => {
        return api.post(`/networks/${newNetwork.id}/accesses/${networkAccess.access.id}`, { isRequired: networkAccess.isRequired })
      })
    ]);
  } catch (err) {
    console.error('Error adding network accesses:', err)
    const axiosError = err as AxiosError<ErrorMessage>
    error.value = axiosError.response?.data?.message || 'Failed to create Network Access requirements. Please try again later, or remove them here and add them later.'
    isSubmitting.value = false
    return
  } finally {
    global.stopFetching()
  }

  titleValue.value = 'Updating network accesses'
  progessValue.value = 75

  try {
    global.startFetching()

    await Promise.all([,
      ...fakeNetwork.value.networkAccesses.map(networkAccess =>
        api.put(`/networks/${newNetwork.id}/users/${newNetwork.networkUsers[0].id}/accesses/${networkAccess.accessId}/`, { isAccepted: true })
      )]
    );
  } catch (err) {
    console.error('Error accepting Network Access for creator:', err)
    const axiosError = err as AxiosError<ErrorMessage>
    error.value = axiosError.response?.data?.message || 'Failed to accept Network Access for your user. Please accept them yourself later. You will be redirected automatically after 5 seconds.'
    setTimeout(() => {
      router.push(`/networks/${newNetwork.id}/manage`)
    }, 5000);
    isSubmitting.value = false
    return
  } finally {
    global.stopFetching()
  }

  titleValue.value = 'Redirecting you'
  progessValue.value = 100

  router.push(`/networks/${newNetwork.id}/manage`)
}

function navigateBack() {
  router.push('/networks')
}
function openUploadModal() {
  showUploadModal.value = true
}
function handleImageUploaded(file: NetworkFile) {
  form.value.fileLink = { id: file.id }
  uploadedFile.value = file
  showUploadModal.value = false
}
function removeImage() {
  form.value.fileLink = undefined
  uploadedFile.value = null
}
</script>
