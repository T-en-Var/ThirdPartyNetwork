<!-- EditNetworkPage.vue -->
<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <LoadingErrorComponent
      :loading="loading"
      :error="error"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!network"
    />

    <div
      v-if="!loading && !error && network"
      class="space-y-10"
    >
      <h2 class="mb-4 text-xl font-medium text-gray-800">Network Details</h2>

      <div class="flex items-stretch justify-between gap-10">
        <section class="flex-grow">
          <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
            <div>
              <label class="block text-sm font-medium text-gray-700">Name</label>
              <input
                v-model="form.name"
                type="text"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700">Callback URL</label>
              <input
                v-model="form.redirectURI"
                type="url"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
              />
            </div>

            <div class="col-span-full">
              <label class="block text-sm font-medium text-gray-700">Description</label>
              <textarea
                v-model="form.description"
                rows="4"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
              />
            </div>

            <div class="col-span-full">
              <label class="flex cursor-pointer items-center gap-2">
                <input
                  type="checkbox"
                  v-model="form.isPublic"
                  class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="text-sm font-medium text-gray-800">Public Network</span>
              </label>
              <p class="mt-1 text-xs text-gray-500">
                Toggle this on to make the network visible to anyone. Otherwise, access will be
                restricted.
              </p>
            </div>
          </div>
        </section>

        <section class="mr-6 flex flex-col">
          <h2 class="block text-center text-sm font-medium text-gray-700">Network Image</h2>
          <div class="flex flex-grow flex-col items-center">
            <!-- If an image is already set -->
            <div
              v-if="uploadedFile"
              class="flex flex-grow flex-col items-center"
            >
              <CloudinaryFile
                :file="uploadedFile"
                :display-only="true"
                class="h-24 w-24 rounded-full object-cover"
              />
              <div class="flex gap-3">
                <button
                  type="button"
                  class="text-sm text-red-600 hover:underline"
                  @click="removeImage"
                >
                  Remove
                </button>
                <button
                  type="button"
                  class="text-sm text-blue-600 hover:underline"
                  @click="openUploadModal"
                >
                  Change
                </button>
              </div>
            </div>

            <div
              v-else
              class="flex flex-grow flex-col items-center justify-center"
            >
              <div
                class="flex h-24 w-24 items-center justify-center rounded-full bg-gray-200 text-gray-500"
              >
                No Image
              </div>
              <button
                type="button"
                class="mt-4 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm text-gray-700 shadow-sm hover:bg-gray-50"
                @click="openUploadModal"
              >
                Upload Image
              </button>
            </div>
          </div>

          <AddFileModal
            v-if="showUploadModal"
            :network-id="network.id"
            @close="showUploadModal = false"
            @uploaded="handleImageUploaded"
            media-type="image"
          />
        </section>
      </div>

      <section
        v-if="!network.isSystemProtected"
        class="rounded-lg border border-red-300 bg-red-50 p-6"
      >
        <h2 class="mb-4 text-lg font-semibold text-red-700">Danger Zone</h2>
        <p class="mb-4 text-sm text-red-600">
          Deleting this network is permanent and cannot be undone. Please proceed with caution.
        </p>
        <button
          @click="deleteNetwork"
          class="rounded-md bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-400"
        >
          Delete Network
        </button>
      </section>

      <transition name="fade">
        <div
          v-if="hasChanges"
          class="fixed bottom-0 left-0 flex w-screen justify-center"
        >
          <div
            class="mb-3 flex w-fit items-center justify-between gap-10 rounded-md bg-gray-900 px-6 py-3 text-white shadow-lg"
          >
            <span class="text-sm">You have unsaved changes</span>
            <div class="flex gap-3">
              <button
                @click="resetForm"
                class="rounded-md bg-gray-700 px-3 py-1 text-sm hover:bg-gray-600"
              >
                Undo Changes
              </button>
              <button
                @click="saveChanges"
                class="rounded-md bg-green-600 px-4 py-2 text-sm hover:bg-green-700"
              >
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
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import type { Network, NetworkFile, NetworkUpdate } from '@/types';
import useNetworks from '@/composables/useNetworks';

const route = useRoute();
const router = useRouter();

const { data: network, loading, error, execute: fetchNetwork } = useNetworks().fetchNetwork;

const uploadedFile = ref<NetworkFile | null>(null);
const showUploadModal = ref(false);

const emit = defineEmits<{
  (e: 'updateNetwork', id: string, payload: NetworkUpdate): void;
  (e: 'deleteNetwork', network: Network): void;
}>();

const networkId = computed(() => route.params.networkId as string);

watch(
  networkId,
  async (newId) => {
    if (newId) {
      await handleMounted();
    }
  },
  { immediate: true },
);

async function handleMounted() {
  await fetchNetwork(networkId.value);

  if (!network.value) throw new Error('Network not found');

  form.value = {
    name: network.value.name,
    description: network.value.description,
    isPublic: network.value.isPublic,
    redirectURI: network.value.redirectURI,
    fileLink: network.value.imageFile ? { id: network.value.imageFile.id } : undefined,
  };

  uploadedFile.value = network.value.imageFile ?? null;
  original = JSON.stringify(form.value);
}

const form = ref<NetworkUpdate>({ name: '', description: '', isPublic: false, redirectURI: '' });
let original = '';
const hasChanges = computed(() => JSON.stringify(form.value) !== original);

function resetForm() {
  Object.assign(form.value, JSON.parse(original));
}

function saveChanges() {
  if (!network.value) return;
  const payload: NetworkUpdate = {
    ...form.value,
    name: form.value.name.trim(),
    description: form.value.description.trim(),
    redirectURI: form.value.redirectURI.trim(),
  };
  emit('updateNetwork', network.value.id, payload);

  setTimeout(async () => {
    await handleMounted();
  }, 100);
}

function deleteNetwork() {
  if (!network.value) return;
  emit('deleteNetwork', network.value);
}

function openUploadModal() {
  showUploadModal.value = true;
}

function handleImageUploaded(file: NetworkFile) {
  form.value.fileLink = { id: file.id };
  uploadedFile.value = file;
  showUploadModal.value = false;
}

function removeImage() {
  form.value.fileLink = undefined;
  uploadedFile.value = null;
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
