@ -1,351 +0,0 @@
<!-- EditUserProxyPage.vue -->
<template>
  <div class="mx-auto max-w-4xl overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <LoadingErrorComponent
      :loading="loading"
      :error="error ?? undefined"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!userProxy"
    />

    <div
      v-if="!loading && !error && userProxy"
      class="space-y-10"
    >
      <div class="flex items-stretch justify-between gap-10">
        <section class="flex-grow">
          <h2 class="mb-4 text-lg font-semibold text-gray-800">Profile Info</h2>
          <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
            <div>
              <label class="block text-sm font-medium text-gray-700">Username</label>
              <div class="relative mt-1">
                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">@</span>
                <input
                  v-model="form.username"
                  type="text"
                  class="block w-full rounded-lg border border-gray-300 py-2 pl-7 pr-3 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
                />
              </div>
            </div>

            <div></div>
            <div>
              <label class="block text-sm font-medium text-gray-700">First Name</label>
              <input
                v-model="form.firstName"
                type="text"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Last Name</label>
              <input
                v-model="form.lastName"
                type="text"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
              />
            </div>

            <div class="col-span-full">
              <label
                :class="`mt-2 flex items-center gap-2 ${userProxy.isDefault ? '' : 'cursor-pointer'}`"
              >
                <input
                  type="checkbox"
                  v-model="form.isDefault"
                  :disabled="userProxy.isDefault"
                  class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="text-sm font-medium text-gray-800">Set as default proxy</span>
              </label>
              <p class="mt-1 text-xs text-gray-500">
                <span v-if="userProxy.isDefault">
                  This proxy is currently your default. You cannot uncheck it because you must
                  always have one default proxy.
                </span>
                <span v-else>
                  Check this box to make this proxy your default. Only one proxy can be the default
                  at a time.
                </span>
              </p>
            </div>
          </div>
        </section>

        <!-- Profile Image -->
        <section class="flex flex-col">
          <h2 class="mb-4 text-lg font-semibold text-gray-800">Profile Image</h2>
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
              <div class="mt-6 flex gap-3">
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

            <!-- If no image yet -->
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

          <!-- Upload Modal -->
          <AddFileModal
            v-if="showUploadModal"
            :networkIds="networks.map((n) => n.id)"
            @close="showUploadModal = false"
            @uploaded="handleImageUploaded"
            media-type="image"
          />
        </section>
      </div>

      <div class="w-full border-b border-gray-200"></div>

      <!-- Email -->
      <section>
        <h2 class="mb-4 text-lg font-semibold text-gray-800">Email</h2>
        <input
          v-model="form.email"
          type="email"
          class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
        />
      </section>

      <div class="w-full border-b border-gray-200"></div>

      <!-- Password -->
      <section>
        <h2 class="mb-4 text-lg font-semibold text-gray-800">Password</h2>

        <!-- Existing password -->
        <div
          v-if="userProxy.hasPassword || form.isDefault"
          class="mb-4 space-y-3 rounded-md border border-gray-200 bg-gray-50 p-4"
        >
          <label class="flex cursor-pointer items-center gap-2">
            <input
              type="checkbox"
              v-model="form.keepPassword"
              :disabled="form.isDefault"
              class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span class="text-sm font-medium text-gray-800"> Keep existing password </span>
          </label>

          <p class="text-xs text-gray-600">
            <span v-if="form.isDefault">
              This proxy is set as default and must have a password. You cannot remove it. Leaving
              the field empty will keep the old password.
            </span>
            <span v-else>
              Uncheck to remove the password. If you enter a new one below, it will
              <span class="font-semibold text-red-600">overwrite</span> the old password.
            </span>
          </p>
        </div>

        <!-- Password inputs -->
        <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
          <div>
            <label class="block text-sm font-medium text-gray-700">New Password</label>
            <input
              v-model="form.password"
              type="password"
              :class="[
                'mt-1 block w-full rounded-lg border px-3 py-2 text-sm shadow-sm focus:ring focus:ring-blue-100',
                form.isDefault && !userProxy.hasPassword && !form.password
                  ? 'border-red-500'
                  : 'border-gray-300 focus:border-blue-600',
              ]"
            />
            <p
              v-if="form.isDefault && !userProxy.hasPassword && !form.password"
              class="mt-1 text-xs text-red-500"
            >
              Default proxies must have a password.
            </p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Retype Password</label>
            <input
              v-model="form.password_retype"
              type="password"
              class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
            />
            <p
              v-if="form.password && form.password !== form.password_retype"
              class="mt-1 text-xs text-red-500"
            >
              Passwords do not match
            </p>
          </div>
        </div>

        <!-- If no password exists and not default -->
        <div
          v-if="!userProxy.hasPassword && !form.isDefault"
          class="mt-2 text-xs text-gray-500"
        >
          You currently do not have a password for this proxy. It's recommended to set one.
        </div>
      </section>

      <div class="w-full border-b border-gray-200"></div>

      <!-- Networks -->
      <section>
        <h2 class="mb-4 text-lg font-semibold text-gray-800">Networks</h2>
        <div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <NetworkCard
            v-for="network in networks"
            :key="network.id"
            :network="network"
            :can-manage="checkCanManage(network)"
          />
          <div
            v-if="!networks.length"
            class="col-span-full p-10 text-center text-gray-500"
          >
            You haven't joined any networks yet. Browse available networks to join.
          </div>
        </div>
      </section>

      <div class="w-full border-b border-gray-200"></div>

      <!-- Danger Zone -->
      <section class="rounded-lg border border-red-300 bg-red-50 p-6">
        <h2 class="mb-4 text-lg font-semibold text-red-700">Danger Zone</h2>
        <p class="mb-4 text-sm text-red-600">
          Deleting this proxy is permanent and cannot be undone. Please proceed with caution.
        </p>
        <button
          @click="deleteProxy"
          class="rounded-md bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-400"
        >
          Delete Proxy
        </button>
      </section>

      <!-- Floating Save Bar -->
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
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import type { Network, NetworkFile, UserProxy, UserProxyForm, UserProxyUpdate } from '@/types';
import { useHistoryStore } from '@/stores/history';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import NetworkCard from '@/components/NetworkCard.vue';
import { useAuthStore } from '@/stores/auth';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import useUsers from '@/composables/useUsers';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const { data: user, loading, error, execute: fetchUser } = useUsers().fetchUser;

const uploadedFile = ref<NetworkFile | null>(null);
const showUploadModal = ref(false);
const userProxy = ref<UserProxy | null>(null);

const emit = defineEmits<{
  (e: 'updateUserProxy', payload: UserProxyUpdate): void;
  (e: 'deleteUserProxy', id: string): void;
}>();

const userProxyId = computed(() => route.params.userProxyId as string);

const historyStore = useHistoryStore(userProxyId.value);

watch(
  userProxyId,
  async (newId) => {
    if (newId) {
      await handleMounted();
    }
  },
  { immediate: true },
);

async function handleMounted() {
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error('UserProxy not found');

  await fetchUser(currentUser.user.id);
  if (!user.value) throw new Error('User not found');

  userProxy.value = user.value.userProxies.find((up) => up.id == userProxyId.value) ?? null;
  if (!userProxy.value) throw new Error('UserProxy not found');

  historyStore.visit.userProxies(userProxy.value);

  form.value = {
    username: userProxy.value.username,
    firstName: userProxy.value.firstName,
    lastName: userProxy.value.lastName,
    email: userProxy.value.email,
    isDefault: userProxy.value.isDefault,
    keepPassword: true,
    password: '',
    password_retype: '',
  };

  if (userProxy.value.imageFile) form.value.fileLink = { id: userProxy.value.imageFile.id };
  uploadedFile.value = userProxy.value.imageFile ?? null;

  original = JSON.stringify(form.value);
}

const form = ref<UserProxyForm>({ keepPassword: true, isDefault: false });

let original = '';
const hasChanges = computed(() => JSON.stringify(form.value) !== original);

const networks = computed<Network[]>(
  () => userProxy.value?.networkUsers.map((nu) => nu.network) ?? [],
);

function resetForm() {
  Object.assign(form.value, JSON.parse(original));
}

function saveChanges() {
  if (!userProxy.value) return;
  if (form.value.password && form.value.password !== form.value.password_retype) return;

  const payload: UserProxyUpdate = {
    ...form.value,
    id: userProxy.value.id,
    username: form.value.username || undefined,
    firstName: form.value.firstName || undefined,
    lastName: form.value.lastName || undefined,
    email: form.value.email || undefined,
    password: form.value.password || undefined,
    fileLink: form.value.fileLink || undefined,
  };

  emit('updateUserProxy', payload);

  setTimeout(async () => {
    await handleMounted();
  }, 100);
}

function deleteProxy() {
  if (!userProxy.value) return;

  emit('deleteUserProxy', userProxy.value.id);
}

const checkCanManage = (network: Network) => authStore.canI.manageNetwork(network);

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
