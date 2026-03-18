<template>
  <div class="flex h-full pt-16">
    <!-- Sidebar -->
    <AccountSidebar />

    <!-- Main Content -->
    <div class="flex-1 p-6 overflow-auto">
      <!-- Loading and Error States -->
      <LoadingErrorComponent :loading="authStore.loading"
        :error="authStore.error ?? undefined" button-value="Reload page"
        @button-action="router.go(0)" :has-value="!!authStore.currentUserProxy" />

      <!-- Route View (Page Content) -->
      <div v-if="authStore.currentUserProxy">
        <RouterView
          v-if="!authStore.loading && !authStore.error && authStore.currentUserProxy"
          @create-proxy="showAddUserProxyModal = true"
          @edit-proxy="handleEditProxyClick" @switch-proxy="handleSwitchProxyClick"
          @update-user-proxy="handleUpdateProxy" @add-file="handleAddFile"
          @edit-file="handleEditFile" @remove-file="handleRemoveFile" @delete-user-proxy="() => {}" />
      </div>

      <div v-if="authStore.currentUserProxy">
        <!-- Modals -->
        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle"
          :message="confirmationMessage" :button-text="confirmButtonText"
          :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />

        <AddUserProxyModal v-if="showAddUserProxyModal && defaultProxy"
          :is-submitting="isSubmitting" :default-proxy="defaultProxy"
          @close="showAddUserProxyModal = false" @create-proxy="createUserProxy" />

        <AddFileModal v-if="showAddFileModal"
          :networks="userProxy!.networkUsers.map(nu => nu.network)"
          @close="showAddFileModal = false" @uploaded="handleUploadedFile" />

        <EditFileModal v-if="showEditFileModal && fileToEdit"
          :is-submitting="isSubmitting" :file="fileToEdit"
          @close="showEditFileModal = false" @update-file="updateFile" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useGlobalStore } from '@/stores/global';

// Components
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import type { EditFileForm, NetworkFile, UserProxy, UserProxyCreate, UserProxyUpdate } from '@/types';

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';
import AddUserProxyModal from '@/components/modals/account/AddUserProxyModal.vue';
import api from '@/api/api';
import AddFileModal from '@/components/modals/network/AddFileModal.vue';
import EditFileModal from '@/components/modals/network/EditFileModal.vue';
import useNetworks from '@/composables/useNetworks';
import useUsers from '@/composables/useUsers';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();

const mainNetwork = useNetworks().fetchMainNetwork;

const isSubmitting = ref(false);

const showConfirmationModal = ref(false);
const showAddUserProxyModal = ref(false);
const showAddFileModal = ref(false);
const showEditFileModal = ref(false);

const fileToEdit = ref<NetworkFile | null>(null);

const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref<() => Promise<void> | void>(() => { });
const userProxy = ref<UserProxy | null>(null);
const { execute: fetchUser, data: user } = useUsers().fetchUser;

onMounted(async () => {
  globalStore.startFetching();
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error("UserProxy not found");

  await fetchUser(currentUser.user.id);
  if (!user.value) throw new Error("User not found");

  userProxy.value = user.value.userProxies.find(up => up.id === currentUser.id) ?? null;
  if (!userProxy.value) throw new Error("UserProxy not found");

  await mainNetwork.execute();
  globalStore.stopFetching();
});

const allProxies = computed(() => {
  if (!user.value) return [];
  return user.value.userProxies;
});

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find(x => x.isDefault)!;
});

// Methods
async function confirmAction() {
  await confirmationAction.value();
  showConfirmationModal.value = false;
}

async function createUserProxy(newUserProxy: UserProxyCreate) {
  if (!user.value) return;
  
  const { execute: createUserProxy, error } = useUsers().createUserProxy;
  isSubmitting.value = true;
  try {
    await createUserProxy(user.value.id, newUserProxy);
    showAddUserProxyModal.value = false;
  } catch (err) {
    console.error('Error adding user proxy:', err);
  } finally {
    isSubmitting.value = false;
    console.log(error)
  }
}

function handleEditProxyClick(id: string) {
  router.push(`/account/proxies/${id}/edit`);
}

function handleSwitchProxyClick(id: string) {
  void id;
  alert("not implemented");
}

async function handleUpdateProxy(userProxy: UserProxyUpdate) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.put(`/users/${authStore.currentUserProxy!.user.id}/proxies/${userProxy.id}`, userProxy);
  } catch (err) {
    console.error('Error adding user proxy:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleAddFile() {
  showAddFileModal.value = true;
}

async function handleUploadedFile(file: NetworkFile) {
  void file;
  showAddFileModal.value = false;
  router.go(0);
}

function handleEditFile(file: NetworkFile) {
  fileToEdit.value = file;
  showEditFileModal.value = true;
}

async function updateFile(id: string, networkId: string, file: EditFileForm) {
  if (!authStore.currentUserProxy) return;
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.put(`/networks/${networkId}/files/${id}`, file);
    showEditFileModal.value = false;
    router.go(0);
  } catch (err) {
    console.error('Error updating file:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleRemoveFile(file: NetworkFile) {
  confirmationTitle.value = 'Remove File';
  confirmationMessage.value = `Are you sure you want to remove the file "${file.name || 'unnamed'}"? This action cannot be undone.`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';
  confirmationAction.value = async () => {
    if (!authStore.currentUserProxy) return;
    isSubmitting.value = true;
    globalStore.startFetching();
    try {
      await api.delete(`/networks/${file.networkId}/files/${file.id}`);
      router.go(0);
    } catch (err) {
      console.error('Error removing file:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}
</script>
