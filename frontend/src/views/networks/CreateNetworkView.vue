<template>
  <div class="create-network-view">
    <div class="create-network-view__panel">
      <LoadingErrorComponent
        :loading="networkState.loading.value"
        :error="networkState.error.value ?? undefined"
        button-value="Go back"
        @button-action="router.go(-1)"
        :has-value="!!networkState.data.value"
      />

      <div
        v-if="!networkState.loading.value && !networkState.error.value && networkState.data.value"
      >
        <!-- Header -->
        <div class="create-network-view__header">
          <h1 class="create-network-view__heading">Create New Network</h1>
        </div>

        <!-- Error Message -->
        <div class="create-network-view__error">
          <ErrorAlert
            :message="error"
            @dismiss="error = ''"
          />
        </div>

        <div class="create-network-view__body">
          <form
            @submit.prevent="handleSubmit"
            class="form"
          >
            <!-- Basic Information -->
            <section>
              <h2 class="create-network-view__section-title">Basic Information</h2>
              <div class="create-network-view__basic-grid">
                <!-- Network Name -->
                <div class="form-field">
                  <label
                    for="networkName"
                    class="form-label"
                  >
                    Network Name <span class="create-network-view__required">*</span>
                  </label>
                  <input
                    id="networkName"
                    v-model="form.name"
                    type="text"
                    class="form-input"
                    placeholder="Enter network name"
                    required
                  />
                  <p class="form-help">
                    Network name must be unique across the system
                  </p>
                </div>

                <!-- Network Image -->
                <div class="form-field">
                  <h2 class="form-label">Network Image</h2>
                  <div class="create-network-view__image-wrap">
                    <!-- Already uploaded -->
                    <div
                      v-if="uploadedFile"
                      class="create-network-view__image-current"
                    >
                      <CdnFile
                        :file="uploadedFile"
                        :display-only="true"
                        class="create-network-view__image-preview"
                      />
                      <div class="create-network-view__image-actions">
                        <button
                          type="button"
                          class="create-network-view__image-remove-btn"
                          @click="removeImage"
                        >
                          Remove
                        </button>
                        <button
                          type="button"
                          class="create-network-view__image-change-btn"
                          @click="openUploadModal"
                        >
                          Change
                        </button>
                      </div>
                    </div>

                    <!-- No image yet -->
                    <div
                      v-else
                      class="create-network-view__image-empty"
                    >
                      <div class="create-network-view__image-placeholder">No Image</div>
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
                    :network-id="networkState.data.value.id"
                    @close="showUploadModal = false"
                    @uploaded="handleImageUploaded"
                    media-type="image"
                  />
                </div>
              </div>
            </section>

            <!-- Description -->
            <section>
              <h2 class="create-network-view__section-title">Description</h2>
              <textarea
                id="description"
                v-model="form.description"
                rows="3"
                class="form-textarea"
                placeholder="Enter network description"
              ></textarea>
              <p class="form-help">Brief description of the network's purpose</p>
            </section>

            <!-- Visibility -->
            <section>
              <div class="create-network-view__checkbox-row">
                <input
                  id="isPublic"
                  v-model="form.isPublic"
                  type="checkbox"
                  class="checkbox"
                />
                <label
                  for="isPublic"
                  class="form-label"
                >
                  Make this network public
                </label>
              </div>
              <p class="create-network-view__checkbox-hint">
                Public networks are discoverable by all users
              </p>
            </section>

            <!-- Form Actions -->
            <div class="form__actions">
              <button
                type="button"
                class="btn btn--outline"
                @click="navigateBack"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="btn btn--primary"
                :disabled="isSubmitting"
              >
                <span
                  v-if="isSubmitting"
                  class="spinner-border spinner-border--xs"
                ></span>
                <span>{{ isSubmitting ? 'Creating...' : 'Create Network' }}</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <ForceLoadModal
      :title="titleValue"
      v-if="isSubmitting"
      :progress="progessValue"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import {
  type ErrorMessage,
  type NetworkDto,
  type CreateNetworkDto,
  type FileDto,
} from '@/types';
import api from '@/api/api';
import { useGlobalStore } from '@/stores/global';
import type { AxiosError } from 'axios';
import ErrorAlert from '@/components/ErrorAlert.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import CdnFile from '@/components/cdn/CdnFile.vue';
import ForceLoadModal from '@/components/modals/ForceWaitModal.vue';
import { useAuthStore } from '@/stores/auth';
import useNetworks from '@/composables/useNetworks';

const router = useRouter();
const global = useGlobalStore();
const isSubmitting = ref(false);
const error = ref('');

const titleValue = ref('');
const progessValue = ref(0);

const uploadedFile = ref<FileDto | null>(null);
const showUploadModal = ref(false);

const networkState = useNetworks().fetchMainNetwork;
const authStore = useAuthStore();

onMounted(async () => {
  await networkState.execute();
});

const form = ref<CreateNetworkDto>({
  name: '',
  description: '',
  isPublic: false,
  fileLink: null,
  redirectURI: '',
});

async function handleSubmit() {
  error.value = '';
  isSubmitting.value = true;
  let newNetwork: NetworkDto;

  titleValue.value = 'Uploading network data';
  progessValue.value = 0;

  try {
    global.startFetching();
    const networkResponse = await api.post<NetworkDto, CreateNetworkDto>('/networks/', {
      name: form.value.name.trim(),
      description: form.value.description.trim(),
      fileLink: form.value.fileLink,
      redirectURI: form.value.redirectURI,
      isPublic: form.value.isPublic,
    });

    newNetwork = networkResponse.data;
  } catch (err) {
    console.error('Error creating network:', err);
    const axiosError = err as AxiosError<ErrorMessage>;
    if (axiosError.response?.status === 409) {
      error.value = `A network with the name "${form.value.name.trim()}" already exists. Please choose a different name.`;
    } else {
      error.value =
        axiosError.response?.data?.message || 'Failed to create network. Please try again later.';
    }
    isSubmitting.value = false;
    return;
  } finally {
    global.stopFetching();
  }

  titleValue.value = 'Updating network accesses';
  progessValue.value = 75;

  if (newNetwork.networkUsers.length === 0) throw new Error('no network creator');

  titleValue.value = 'Redirecting you';
  progessValue.value = 100;

  await authStore.refreshTokens();

  router.push(`/networks/${newNetwork.id}/manage`);
}

function navigateBack() {
  router.push('/networks');
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
