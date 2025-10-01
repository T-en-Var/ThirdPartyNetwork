<!-- EditNetworkPage.vue -->
<template>
  <div class="p-6 bg-white shadow-md rounded-lg overflow-hidden">
    <LoadingErrorComponent :loading="loading" :error="error" button-value="Go back" @button-action="router.go(-1)" />

    <div v-if="!loading && !error && network" class="space-y-10">
      <!-- Network Info -->
      <h2 class="text-xl font-medium text-gray-800 mb-4">Network Details</h2>

      <div class="flex justify-between items-stretch gap-10">
        <section class="flex-grow">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700">Name</label>
              <input v-model="form.name" type="text"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700">Callback URL</label>
              <input v-model="form.redirectURI" type="url"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
            </div>

            <div class="col-span-full">
              <label class="block text-sm font-medium text-gray-700">Description</label>
              <textarea v-model="form.description" rows="4"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
            </div>

            <div class="col-span-full">
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" v-model="form.isPublic"
                  class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
                <span class="text-sm font-medium text-gray-800">Public Network</span>
              </label>
              <p class="text-xs text-gray-500 mt-1">
                Toggle this on to make the network visible to anyone. Otherwise, access will be restricted.
              </p>
            </div>
          </div>
        </section>

        <!-- Network Image -->
        <section class="flex flex-col mr-6">
          <h2 class="block text-sm font-medium text-gray-700 text-center">Network Image</h2>
          <div class="flex flex-col items-center flex-grow">
            <!-- If an image is already set -->
            <div v-if="uploadedFile" class="flex flex-col items-center flex-grow">
              <CloudinaryFile :file="uploadedFile" :display-only="true" class="h-24 w-24 rounded-full object-cover" />
              <div class="flex gap-3">
                <button type="button" class="text-sm text-red-600 hover:underline" @click="removeImage">
                  Remove
                </button>
                <button type="button" class="text-sm text-blue-600 hover:underline" @click="openUploadModal">
                  Change
                </button>
              </div>
            </div>

            <!-- If no image yet -->
            <div v-else class="flex flex-col items-center justify-center flex-grow">
              <div class="h-24 w-24 rounded-full bg-gray-200 flex items-center justify-center text-gray-500">
                No Image
              </div>
              <button type="button"
                class="mt-4 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm text-gray-700 shadow-sm hover:bg-gray-50"
                @click="openUploadModal">
                Upload Image
              </button>
            </div>
          </div>

          <!-- Upload Modal -->
          <AddFileModal v-if="showUploadModal" :network="network" @close="showUploadModal = false"
            @uploaded="handleImageUploaded" media-type="image" />
        </section>
      </div>

      <!-- Danger Zone -->
      <section v-if="!network.isSystemProtected" class="border border-red-300 bg-red-50 rounded-lg p-6">
        <h2 class="text-lg font-semibold text-red-700 mb-4">Danger Zone</h2>
        <p class="text-sm text-red-600 mb-4">
          Deleting this network is permanent and cannot be undone. Please proceed with caution.
        </p>
        <button @click="deleteNetwork"
          class="px-4 py-2 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 focus:ring-2 focus:ring-red-400 focus:outline-none">
          Delete Network
        </button>
      </section>

      <!-- Floating Save Bar -->
      <transition name="fade">
        <div v-if="hasChanges" class="fixed left-0 bottom-0 w-screen flex justify-center">
          <div
            class="w-fit bg-gray-900 text-white flex justify-between items-center rounded-md gap-10 px-6 py-3 mb-3 shadow-lg">
            <span class="text-sm">You have unsaved changes</span>
            <div class="flex gap-3">
              <button @click="resetForm" class="px-3 py-1 text-sm rounded-md bg-gray-700 hover:bg-gray-600">
                Undo Changes
              </button>
              <button @click="saveChanges" class="px-4 py-2 text-sm rounded-md bg-green-600 hover:bg-green-700">
                Save Changes
              </button>
            </div>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue'
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue'
import AddFileModal from '@/components/modals/network/AddFileModal.vue'
import type { Network, NetworkFile, NetworkUpdate } from '@/types'
import useNetwork from '@/composables/useNetwork'

const route = useRoute()
const router = useRouter()

const { network, loading, error, fetchNetwork } = useNetwork()

const uploadedFile = ref<NetworkFile | null>(null)
const showUploadModal = ref(false)

const emit = defineEmits<{
  (e: 'updateNetwork', id: string, payload: NetworkUpdate): void,
  (e: 'deleteNetwork', network: Network): void
}>()

const networkId = computed(() => route.params.networkId as string)

watch(
  () => networkId.value,
  async (newId) => {
    if (newId) {
      await handleMounted()
    }
  },
  { immediate: true }
)

async function handleMounted() {
  await fetchNetwork(networkId.value)

  if (!network.value) throw new Error('Network not found')

  form.value = {
    name: network.value.name,
    description: network.value.description,
    isPublic: network.value.isPublic,
    redirectURI: network.value.redirectURI,
    fileLink: network.value.imageFile ? { id: network.value.imageFile.id } : undefined,
  }

  uploadedFile.value = network.value.imageFile ?? null
  original = JSON.stringify(form.value)
}

// form state
const form = ref<NetworkUpdate>({ name: '', description: '', isPublic: false, redirectURI: '' })
let original = ''
const hasChanges = computed(() => JSON.stringify(form.value) !== original)

// actions
function resetForm() {
  Object.assign(form.value, JSON.parse(original))
}

function saveChanges() {
  if (!network.value) return
  const payload: NetworkUpdate = {
    ...form.value,
    name: form.value.name.trim(),
    description: form.value.description.trim(),
    redirectURI: form.value.redirectURI.trim(),
  }
  emit('updateNetwork', network.value.id, payload)

  setTimeout(async () => {
    await handleMounted()
  }, 100)
}

function deleteNetwork() {
  if (!network.value) return
  emit('deleteNetwork', network.value)
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

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
