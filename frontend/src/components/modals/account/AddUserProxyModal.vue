<template>
  <modal-container title="Create New Proxy" @close="$emit('close')" :close-on-escape="!isSubmitting">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Info message -->
      <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3 text-sm text-yellow-800">
        Any empty fields will be automatically filled with values from your <strong>default proxy</strong>.
      </div>

      <!-- Username -->
      <div>
        <label for="username" class="block text-sm font-semibold text-gray-800 mb-2">Username</label>
        <input id="username" v-model="form.username" type="text" :placeholder="`Defaults to: ${defaultProxy.username}`"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <!-- First Name -->
      <div>
        <label for="firstName" class="block text-sm font-semibold text-gray-800 mb-2">Firstname</label>
        <input id="firstName" v-model="form.firstName" type="text"
          :placeholder="`Defaults to: ${defaultProxy.firstName}`"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <!-- Last Name -->
      <div>
        <label for="lastName" class="block text-sm font-semibold text-gray-800 mb-2">Lastname</label>
        <input id="lastName" v-model="form.lastName" type="text" :placeholder="`Defaults to: ${defaultProxy.lastName}`"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <!-- Email -->
      <div>
        <label for="email" class="block text-sm font-semibold text-gray-800 mb-2">Email</label>
        <input id="email" v-model="form.email" type="email" :placeholder="`Defaults to: ${defaultProxy.email}`"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <!-- Password -->
      <div>
        <label for="password" class="block text-sm font-semibold text-gray-800 mb-2">Password</label>
        <input id="password" v-model="form.password" type="password"
          placeholder="Defaults to your default proxy's password"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <div v-if="form.password">
        <span class="text-xs text-red-500" v-if="form.password != form.password_retype">Passwords do not match!</span>
        <label for="password_retype" class="block text-sm font-semibold text-gray-800 mb-2">Retype Password</label>
        <input id="password" v-model="form.password_retype" type="password"
          placeholder="Defaults to your default proxy's password"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all" />
      </div>

      <!-- Image Upload Section -->
      <div class="flex gap-8">
        <div>
          <label class="block text-sm font-semibold text-gray-800 mb-2">Profile Image</label>

          <!-- If image is uploaded -->
          <div v-if="uploadedFile" class="space-y-2">
            <CloudinaryFile :file="uploadedFile" :display-only="true" class="max-h-48 rounded-lg" />
            <button type="button" class="text-sm text-red-600 hover:underline" @click="removeImage">
              Remove Image
            </button>
          </div>

          <!-- If no image -->
          <div v-else>
            <button type="button"
              class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm text-gray-700 shadow-sm hover:bg-gray-50"
              @click="openUploadModal">
              Upload Image
            </button>
          </div>
        </div>

        <div v-if="!uploadedFile">
          <span class="block text-sm font-semibold text-gray-800 mb-2">Defaults to</span>
          <CloudinaryFile v-if="defaultProxy.imageFile" :file="defaultProxy.imageFile" :display-only="true" class="max-h-16 rounded-full" />
          <span class="text-sm" v-else>Your default proxy has no image to default to.</span>
        </div>
      </div>

      <div class="text-sm flex items-center gap-2">
        <svg class="min-w-6 h-6" viewBox="-0.5 0 25 25" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path
            d="M12 21.5C17.1086 21.5 21.25 17.3586 21.25 12.25C21.25 7.14137 17.1086 3 12 3C6.89137 3 2.75 7.14137 2.75 12.25C2.75 17.3586 6.89137 21.5 12 21.5Z"
            stroke="#000000" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
          <path
            d="M12.9309 8.15005C12.9256 8.39231 12.825 8.62272 12.6509 8.79123C12.4767 8.95974 12.2431 9.05271 12.0008 9.05002C11.8242 9.04413 11.6533 8.98641 11.5093 8.884C11.3652 8.7816 11.2546 8.63903 11.1911 8.47415C11.1275 8.30927 11.1139 8.12932 11.152 7.95675C11.19 7.78419 11.278 7.6267 11.405 7.50381C11.532 7.38093 11.6923 7.29814 11.866 7.26578C12.0397 7.23341 12.2192 7.25289 12.3819 7.32181C12.5446 7.39072 12.6834 7.506 12.781 7.65329C12.8787 7.80057 12.9308 7.97335 12.9309 8.15005ZM11.2909 16.5301V11.1501C11.2882 11.0556 11.3046 10.9615 11.3392 10.8736C11.3738 10.7857 11.4258 10.7057 11.4922 10.6385C11.5585 10.5712 11.6378 10.518 11.7252 10.4822C11.8126 10.4464 11.9064 10.4286 12.0008 10.43C12.094 10.4299 12.1863 10.4487 12.272 10.4853C12.3577 10.5218 12.4352 10.5753 12.4997 10.6426C12.5642 10.7099 12.6143 10.7895 12.6472 10.8767C12.6801 10.9639 12.6949 11.0569 12.6908 11.1501V16.5301C12.6908 16.622 12.6727 16.713 12.6376 16.7979C12.6024 16.8828 12.5508 16.96 12.4858 17.025C12.4208 17.09 12.3437 17.1415 12.2588 17.1767C12.1738 17.2119 12.0828 17.23 11.9909 17.23C11.899 17.23 11.8079 17.2119 11.723 17.1767C11.6381 17.1415 11.5609 17.09 11.4959 17.025C11.4309 16.96 11.3793 16.8828 11.3442 16.7979C11.309 16.713 11.2909 16.622 11.2909 16.5301Z"
            fill="#000000" />
        </svg>
        <div v-if="form.password || form.email">
          <span v-if="form.password && !form.email">By entering a password without an email: you will be able to log in
            into this proxy by using {{ form.email }} with the new password for this proxy</span>
          <span v-if="!form.password && form.email">By entering an email without a password: you will be able to log in
            into this proxy by using {{ form.email }} with the password used for your default proxy</span>
          <span v-if="form.password && form.email">By entering an email and a password: you will be able to log in into
            this proxy by using {{ form.email }} with the new password for this proxy</span>
          without the need to switch proxies after logging in.
        </div>
        <div v-else>
          You will be able to switch to this proxy after logging in to your default proxy.
        </div>
      </div>

      <!-- Actions -->
      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          :disabled="isSubmitting">
          <span v-if="isSubmitting">Creating...</span>
          <span v-else>Create Proxy</span>
        </button>
      </div>

      <!-- Upload Modal -->
      <AddFileModal v-if="showUploadModal" :networks="allNetworks" @close="showUploadModal = false"
        @uploaded="handleImageUploaded" media-type="image" />
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import AddFileModal from '@/components/modals/network/AddFileModal.vue';
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
  const proxies = [props.defaultProxy, ...props.defaultProxy.user.userProxies.filter(u => u != null)];
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
  if (!form.value.password || form.value.password != form.value.password_retype) return;

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