<!-- EditNetworkPage.vue -->
<template>
  <div class="edit-network-tab">
    <LoadingErrorComponent
      :loading="loading"
      :error="error"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!network"
    />

    <div
      v-if="!loading && !error && network"
      class="edit-network-tab__content"
    >
      <h2 class="edit-network-tab__heading">Network Details</h2>

      <div class="edit-network-tab__main">
        <section class="edit-network-tab__fields">
          <div class="edit-network-tab__fields-grid">
            <div class="form-field">
              <label class="form-label">Name</label>
              <input
                v-model="form.name"
                type="text"
                class="form-input"
              />
            </div>

            <div class="form-field">
              <label class="form-label">Callback URL</label>
              <input
                v-model="form.redirectURI"
                type="url"
                class="form-input"
              />
            </div>

            <div class="form-field edit-network-tab__field--full">
              <label class="form-label">Description</label>
              <textarea
                v-model="form.description"
                rows="4"
                class="form-textarea"
              />
            </div>

            <div class="form-field edit-network-tab__field--full">
              <label class="edit-network-tab__checkbox-label">
                <input
                  type="checkbox"
                  v-model="form.isPublic"
                  class="checkbox"
                />
                <span class="edit-network-tab__checkbox-text">Public Network</span>
              </label>
              <p class="edit-network-tab__checkbox-hint">
                Toggle this on to make the network visible to anyone. Otherwise, access will be
                restricted.
              </p>
            </div>
          </div>
        </section>

        <section class="edit-network-tab__image-section">
          <h2 class="edit-network-tab__image-heading">Network Image</h2>
          <div class="edit-network-tab__image-wrap">
            <!-- If an image is already set -->
            <div
              v-if="uploadedFile"
              class="edit-network-tab__image-current"
            >
              <CdnFile
                :file="uploadedFile"
                :display-only="true"
                class="edit-network-tab__image-preview"
              />
              <div class="edit-network-tab__image-actions">
                <button
                  type="button"
                  class="edit-network-tab__image-remove-btn"
                  @click="removeImage"
                >
                  Remove
                </button>
                <button
                  type="button"
                  class="edit-network-tab__image-change-btn"
                  @click="openUploadModal"
                >
                  Change
                </button>
              </div>
            </div>

            <div
              v-else
              class="edit-network-tab__image-empty"
            >
              <div class="edit-network-tab__image-placeholder">No Image</div>
              <button
                type="button"
                class="btn btn--outline"
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
        class="edit-network-tab__danger-zone"
      >
        <h2 class="edit-network-tab__danger-zone-title">Danger Zone</h2>
        <p class="edit-network-tab__danger-zone-text">
          Deleting this network is permanent and cannot be undone. Please proceed with caution.
        </p>
        <button
          @click="deleteNetwork"
          class="btn btn--danger"
        >
          Delete Network
        </button>
      </section>

      <transition name="fade">
        <div
          v-if="hasChanges"
          class="edit-network-tab__unsaved-bar"
        >
          <div class="edit-network-tab__unsaved-panel">
            <span class="edit-network-tab__unsaved-text">You have unsaved changes</span>
            <div class="edit-network-tab__unsaved-actions">
              <button
                @click="resetForm"
                class="btn btn--secondary"
              >
                Undo Changes
              </button>
              <button
                @click="saveChanges"
                class="btn btn--success"
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
import CdnFile from '@/components/cdn/CdnFile.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import type { FileDto, NetworkDto, UpdateNetworkDto } from '@/types';
import useNetworks from '@/composables/useNetworks';

const route = useRoute();
const router = useRouter();

const { data: network, loading, error, execute: fetchNetwork } = useNetworks().fetchNetwork;

const uploadedFile = ref<FileDto | null>(null);
const showUploadModal = ref(false);

const emit = defineEmits<{
  (e: 'updateNetwork', id: string, payload: UpdateNetworkDto): void;
  (e: 'deleteNetwork', network: NetworkDto): void;
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
    fileLink: network.value.imageFile ? { id: network.value.imageFile.id } : null,
    externalContentKinds: network.value.externalContentKinds,
  };

  uploadedFile.value = network.value.imageFile ?? null;
  original = JSON.stringify(form.value);
}

const form = ref<UpdateNetworkDto>({
  name: '',
  description: '',
  isPublic: false,
  redirectURI: '',
  fileLink: null,
  externalContentKinds: null,
});
let original = '';
const hasChanges = computed(() => JSON.stringify(form.value) !== original);

function resetForm() {
  Object.assign(form.value, JSON.parse(original));
}

function saveChanges() {
  if (!network.value) return;
  const payload: UpdateNetworkDto = {
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

function handleImageUploaded(file: FileDto) {
  form.value.fileLink = { id: file.id };
  uploadedFile.value = file;
  showUploadModal.value = false;
}

function removeImage() {
  form.value.fileLink = null;
  uploadedFile.value = null;
}
</script>
