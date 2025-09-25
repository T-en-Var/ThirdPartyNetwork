<!-- EditUserProxyPage.vue -->
<template>
  <div class="max-w-4xl mx-auto p-6">
    <LoadingErrorComponent :loading="loading" :error="error" button-value="Go back" @button-action="router.go(-1)" />

    <div v-if="!loading && !error && userProxy" class="space-y-10">
      <div class="flex justify-between items-stretch gap-10">
        <!-- Profile Info -->
        <section class="flex-grow">
          <h2 class="text-lg font-semibold text-gray-800 mb-4">Profile Info</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700">Username</label>
              <div class="relative mt-1">
                <!-- Gray @ prefix -->
                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">@</span>
                <input v-model="form.username" type="text"
                  class="block w-full rounded-lg border border-gray-300 pl-7 pr-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
              </div>
            </div>

            <div></div>
            <div>
              <label class="block text-sm font-medium text-gray-700">First Name</label>
              <input v-model="form.firstName" type="text"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Last Name</label>
              <input v-model="form.lastName" type="text"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
            </div>

            <div class="col-span-full">
              <label :class="`flex items-center gap-2 mt-2 ${userProxy.isDefault ? '' : 'cursor-pointer'}`">
                <input type="checkbox" v-model="form.isDefault" :disabled="userProxy.isDefault"
                  class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
                <span class="text-sm font-medium text-gray-800">Set as default proxy</span>
              </label>
              <p class="text-xs text-gray-500 mt-1">
                <span v-if="userProxy.isDefault">
                  This proxy is currently your default. You cannot uncheck it because you must always have one default
                  proxy.
                </span>
                <span v-else>
                  Check this box to make this proxy your default. Only one proxy can be the default at a time.
                </span>
              </p>
            </div>

          </div>
        </section>

        <!-- Profile Image -->
        <section class="flex flex-col">
          <h2 class="text-lg font-semibold text-gray-800 mb-4">Profile Image</h2>
          <div class="flex flex-col items-center flex-grow">
            <!-- If an image is already set -->
            <div v-if="uploadedFile" class="flex flex-col items-center flex-grow">
              <CloudinaryFile :file="uploadedFile" :display-only="true" class="h-24 w-24 rounded-full object-cover" />
              <div class="flex gap-3 mt-6">
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
          <UploadFileModal v-if="showUploadModal" :networks="networks" @close="showUploadModal = false"
            @uploaded="handleImageUploaded" />
        </section>
      </div>

      <div class="border-b border-gray-200 w-full"></div>

      <!-- Email -->
      <section>
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Email</h2>
        <input v-model="form.email" type="email"
          class="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
      </section>

      <div class="border-b border-gray-200 w-full"></div>

      <!-- Password -->
      <section>
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Password</h2>

        <!-- Existing password -->
        <div v-if="userProxy.hasPassword || form.isDefault"
          class="rounded-md border border-gray-200 bg-gray-50 p-4 mb-4 space-y-3">
          <label class="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" v-model="form.keepPassword" :disabled="form.isDefault"
              class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
            <span class="text-sm font-medium text-gray-800">
              Keep existing password
            </span>
          </label>

          <p class="text-xs text-gray-600">
            <span v-if="form.isDefault">
              This proxy is set as default and must have a password. You cannot remove it. Leaving the field empty will
              keep the old password.
            </span>
            <span v-else>
              Uncheck to remove the password. If you enter a new one below, it will
              <span class="font-semibold text-red-600">overwrite</span> the old password.
            </span>
          </p>
        </div>

        <!-- Password inputs -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700">New Password</label>
            <input v-model="form.password" type="password"
              :class="['mt-1 block w-full rounded-lg border px-3 py-2 text-sm shadow-sm focus:ring focus:ring-blue-100',
                form.isDefault && !userProxy.hasPassword && !form.password ? 'border-red-500' : 'border-gray-300 focus:border-blue-600']" />
            <p v-if="form.isDefault && !userProxy.hasPassword && !form.password" class="text-xs text-red-500 mt-1">
              Default proxies must have a password.
            </p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Retype Password</label>
            <input v-model="form.password_retype" type="password"
              class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
            <p v-if="form.password && form.password !== form.password_retype" class="text-xs text-red-500 mt-1">
              Passwords do not match
            </p>
          </div>
        </div>

        <!-- If no password exists and not default -->
        <div v-if="!userProxy.hasPassword && !form.isDefault" class="mt-2 text-xs text-gray-500">
          You currently do not have a password for this proxy. It's recommended to set one.
        </div>
      </section>

      <div class="border-b border-gray-200 w-full"></div>

      <!-- Networks -->
      <section>
        <h2 class="text-lg font-semibold text-gray-800 mb-4">Networks</h2>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <NetworkCard v-for="network in networks" :key="network.id" :network="network"
            :can-manage="checkCanManage(network)" />
          <div v-if="!networks.length" class="col-span-full text-center p-10 text-gray-500">
            You haven't joined any networks yet. Browse available networks to join.
          </div>
        </div>
      </section>

      <div class="border-b border-gray-200 w-full"></div>

      <!-- Danger Zone -->
      <section class="border border-red-300 bg-red-50 rounded-lg p-6">
        <h2 class="text-lg font-semibold text-red-700 mb-4">Danger Zone</h2>
        <p class="text-sm text-red-600 mb-4">
          Deleting this proxy is permanent and cannot be undone. Please proceed with caution.
        </p>
        <button @click="deleteProxy"
          class="px-4 py-2 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 focus:ring-2 focus:ring-red-400 focus:outline-none">
          Delete Proxy
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
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue'
import type { Network, NetworkFile, UserProxyForm, UserProxyUpdate } from '@/types'
import { useHistoryStore } from '@/stores/history';
import useUserProxy from '@/composables/useUserProxy';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingError.vue';
import NetworkCard from '@/components/NetworkCard.vue';
import { useAuthStore } from '@/stores/auth';
import UploadFileModal from '@/components/modals/network/AddFileModal.vue';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const historyStore = useHistoryStore();

const { userProxy, loading, error, fetchUserProxy } = useUserProxy();

const uploadedFile = ref<NetworkFile | null>(null);
const showUploadModal = ref(false);

const emit = defineEmits<{
  (e: 'updateUserProxy', payload: UserProxyUpdate): void,
  (e: 'deleteUserProxy', id: string): void
}>();

const userProxyId = computed(() => route.params.userProxyId as string)

watch(
  () => userProxyId.value,
  async (newId) => {
    if (newId) {
      await handleMounted()
    }
  },
  { immediate: true }  // basically onMounted
)

async function handleMounted() {
  await fetchUserProxy(userProxyId.value)
  
  if (!userProxy.value) throw new Error("UserProxy not found");
  historyStore.userProxyVisit(userProxy.value);

  form.value = {
    username: userProxy.value.username,
    firstName: userProxy.value.firstName,
    lastName: userProxy.value.lastName,
    email: userProxy.value.email,
    isDefault: userProxy.value.isDefault,
    keepPassword: true,
    password: '',
    password_retype: '',
  }

  if (userProxy.value.imageFile) form.value.fileLink = { id: userProxy.value.imageFile.id }
  uploadedFile.value = userProxy.value.imageFile ?? null;

  original = JSON.stringify(form.value)
}

// form state
const form = ref<UserProxyForm>({ keepPassword: true, isDefault: false })

let original = ''
const hasChanges = computed(() => JSON.stringify(form.value) !== original)

const networks = computed<Network[]>(() => userProxy.value?.networkUsers.map(nu => nu.network) ?? [])

// actions
function resetForm() {
  Object.assign(form.value, JSON.parse(original))
}

function saveChanges() {
  if (!userProxy.value) return
  if (form.value.password && form.value.password !== form.value.password_retype) return

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

  emit('updateUserProxy', payload)

  // too lazy..
  // surely you'd fix this ;)
  // async function save(proxy: UserProxyUpdate) {
  //   await props.onUpdateUserProxy(proxy)
  //   console.log("done saving!") // now runs after parent is done
  // }

  setTimeout(async () => {
    await handleMounted()
  }, 100);
}

function deleteProxy() {
  if (!userProxy.value) return;

  emit('deleteUserProxy', userProxy.value.id)
}

const checkCanManage = (network: Network) => authStore.claimChecker.canManageNetwork(network);

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
