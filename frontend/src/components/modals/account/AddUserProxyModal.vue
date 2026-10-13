<template>
  <modal-container
    title="Create New Proxy"
    @close="$emit('close')"
    :close-on-escape="!isSubmitting"
  >
    <form
      @submit.prevent="handleSubmit"
      class="form add-user-proxy-modal"
    >
      <!-- Info message -->
      <div class="alert alert--warning">
        Any empty fields will be automatically filled with values from your
        <strong>default proxy</strong>.
      </div>

      <!-- Username -->
      <div>
        <label
          for="username"
          class="form-label"
          >Username</label
        >
        <input
          id="username"
          v-model="form.username"
          type="text"
          :placeholder="`Defaults to: ${defaultProxy.username}`"
          class="form-input"
        />
      </div>

      <!-- First Name -->
      <div>
        <label
          for="firstName"
          class="form-label"
          >Firstname</label
        >
        <input
          id="firstName"
          v-model="form.firstName"
          type="text"
          :placeholder="`Defaults to: ${defaultProxy.firstName}`"
          class="form-input"
        />
      </div>

      <!-- Last Name -->
      <div>
        <label
          for="lastName"
          class="form-label"
          >Lastname</label
        >
        <input
          id="lastName"
          v-model="form.lastName"
          type="text"
          :placeholder="`Defaults to: ${defaultProxy.lastName}`"
          class="form-input"
        />
      </div>

      <!-- Email -->
      <div>
        <label
          for="email"
          class="form-label"
          >Email</label
        >
        <input
          id="email"
          v-model="form.email"
          type="email"
          :placeholder="`Defaults to: ${defaultProxy.email}`"
          class="form-input"
        />
      </div>

      <!-- Password -->
      <div>
        <label
          for="password"
          class="form-label"
          >Password</label
        >
        <input
          id="password"
          v-model="form.password"
          type="password"
          placeholder="Defaults to your default proxy's password"
          class="form-input"
        />
      </div>

      <div v-if="form.password">
        <span
          class="form-error"
          v-if="form.password != form.password_retype"
          >Passwords do not match!</span
        >
        <label
          for="password_retype"
          class="form-label"
          >Retype Password</label
        >
        <input
          id="password"
          v-model="form.password_retype"
          type="password"
          placeholder="Defaults to your default proxy's password"
          class="form-input"
        />
      </div>

      <!-- Image Upload Section -->
      <div class="add-user-proxy-modal__image-row">
        <div>
          <label class="form-label">Profile Image</label>

          <!-- If image is uploaded -->
          <div
            v-if="uploadedFile"
            class="add-user-proxy-modal__image-preview"
          >
            <CloudinaryFile
              :file="uploadedFile"
              :display-only="true"
              class="add-user-proxy-modal__image"
            />
            <button
              type="button"
              class="add-user-proxy-modal__link-btn add-user-proxy-modal__link-btn--danger"
              @click="removeImage"
            >
              Remove Image
            </button>
          </div>

          <!-- If no image -->
          <div v-else>
            <button
              type="button"
              class="btn btn--outline"
              @click="openUploadModal"
            >
              Upload Image
            </button>
          </div>
        </div>

        <div v-if="!uploadedFile">
          <span class="form-label">Defaults to</span>
          <CloudinaryFile
            v-if="defaultProxy.imageFile"
            :file="defaultProxy.imageFile"
            :display-only="true"
            class="avatar__image add-user-proxy-modal__default-image"
          />
          <span
            class="add-user-proxy-modal__no-image-text"
            v-else
            >Your default proxy has no image to default to.</span
          >
        </div>
      </div>

      <div class="add-user-proxy-modal__info-row">
        <svg
          class="add-user-proxy-modal__info-icon"
          viewBox="-0.5 0 25 25"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M12 21.5C17.1086 21.5 21.25 17.3586 21.25 12.25C21.25 7.14137 17.1086 3 12 3C6.89137 3 2.75 7.14137 2.75 12.25C2.75 17.3586 6.89137 21.5 12 21.5Z"
            stroke="#000000"
            stroke-width="1.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
          <path
            d="M12.9309 8.15005C12.9256 8.39231 12.825 8.62272 12.6509 8.79123C12.4767 8.95974 12.2431 9.05271 12.0008 9.05002C11.8242 9.04413 11.6533 8.98641 11.5093 8.884C11.3652 8.7816 11.2546 8.63903 11.1911 8.47415C11.1275 8.30927 11.1139 8.12932 11.152 7.95675C11.19 7.78419 11.278 7.6267 11.405 7.50381C11.532 7.38093 11.6923 7.29814 11.866 7.26578C12.0397 7.23341 12.2192 7.25289 12.3819 7.32181C12.5446 7.39072 12.6834 7.506 12.781 7.65329C12.8787 7.80057 12.9308 7.97335 12.9309 8.15005ZM11.2909 16.5301V11.1501C11.2882 11.0556 11.3046 10.9615 11.3392 10.8736C11.3738 10.7857 11.4258 10.7057 11.4922 10.6385C11.5585 10.5712 11.6378 10.518 11.7252 10.4822C11.8126 10.4464 11.9064 10.4286 12.0008 10.43C12.094 10.4299 12.1863 10.4487 12.272 10.4853C12.3577 10.5218 12.4352 10.5753 12.4997 10.6426C12.5642 10.7099 12.6143 10.7895 12.6472 10.8767C12.6801 10.9639 12.6949 11.0569 12.6908 11.1501V16.5301C12.6908 16.622 12.6727 16.713 12.6376 16.7979C12.6024 16.8828 12.5508 16.96 12.4858 17.025C12.4208 17.09 12.3437 17.1415 12.2588 17.1767C12.1738 17.2119 12.0828 17.23 11.9909 17.23C11.899 17.23 11.8079 17.2119 11.723 17.1767C11.6381 17.1415 11.5609 17.09 11.4959 17.025C11.4309 16.96 11.3793 16.8828 11.3442 16.7979C11.309 16.713 11.2909 16.622 11.2909 16.5301Z"
            fill="#000000"
          />
        </svg>
        <div v-if="form.password || form.email">
          <span v-if="form.password && !form.email"
            >By entering a password without an email: you will be able to log in into this proxy by
            using {{ form.email }} with the new password for this proxy</span
          >
          <span v-if="!form.password && form.email"
            >By entering an email without a password: you will be able to log in into this proxy by
            using {{ form.email }} with the password used for your default proxy</span
          >
          <span v-if="form.password && form.email"
            >By entering an email and a password: you will be able to log in into this proxy by
            using {{ form.email }} with the new password for this proxy</span
          >
          without the need to switch proxies after logging in.
        </div>
        <div v-else>
          You will be able to switch to this proxy after logging in to your default proxy.
        </div>
      </div>

      <!-- Actions -->
      <div class="form__actions">
        <button
          type="button"
          class="btn btn--outline"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        <button
          type="submit"
          class="btn btn--primary"
          :disabled="isSubmitting"
        >
          <span v-if="isSubmitting">Creating...</span>
          <span v-else>Create Proxy</span>
        </button>
      </div>

      <!-- Upload Modal -->
      <AddFileModal
        v-if="showUploadModal"
        :networks="allNetworks"
        @close="showUploadModal = false"
        @uploaded="handleImageUploaded"
        media-type="image"
      />
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import type { UserProxy, UserProxyCreate, NetworkFile, Network, UserProxyForm } from '@/types';

const props = defineProps<{
  isSubmitting?: boolean;
  defaultProxy: UserProxy;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'create-proxy', payload: UserProxyCreate): void;
}>();

const form = ref<UserProxyForm>({ keepPassword: true, isDefault: false });

const uploadedFile = ref<NetworkFile | null>();
const allNetworks = computed<Network[]>(() => {
  const proxies = [
    props.defaultProxy,
    ...props.defaultProxy.user.userProxies.filter((u) => u != null),
  ];
  const networks = proxies.flatMap((p) => (p.networkUsers ?? []).map((nu) => nu.network));
  const unique = new Map(networks.map((n) => [n.id, n]));
  return [...unique.values()];
});

const showUploadModal = ref(false);

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

function handleSubmit() {
  if (form.value.password && form.value.password != form.value.password_retype) return;

  const payload: UserProxyCreate = {
    ...form.value,
    username: form.value.username || undefined,
    firstName: form.value.firstName || undefined,
    lastName: form.value.lastName || undefined,
    email: form.value.email || undefined,
    password: form.value.password || undefined,
    fileLink: form.value.fileLink || undefined,
  };

  emit('create-proxy', payload);
}
</script>
