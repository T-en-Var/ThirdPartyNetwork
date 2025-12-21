<template>
  <div class="flex h-full pt-16">
    <!-- Sidebar -->
    <AccountSidebar />

    <!-- Main Content -->
    <div class="flex-1 p-6 overflow-auto">
      <!-- Loading and Error States -->
      <LoadingErrorComponent :loading="authStore.loading" :error="authStore.error" button-value="Reload page"
        @button-action="router.go(0)" />

      <!-- Route View (Page Content) -->
      <div v-if="authStore.currentUserProxy">
        <RouterView v-if="!authStore.loading && !authStore.error && authStore.currentUserProxy"
          @create-proxy="showAddUserProxyModal = true" @edit-proxy="handleEditProxyClick"
          @switch-proxy="handleSwitchProxyClick" @update-user-proxy="handleUpdateProxy" @add-file="handleAddFile"
          @edit-file="handleEditFile" @remove-file="handleRemoveFile"
          @toggle-file-visibility="handleToggleFileVisibility" />
      </div>

      <div v-if="authStore.currentUserProxy">
        <!-- Modals -->
        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
          :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />

        <AddUserProxyModal v-if="showAddUserProxyModal" :is-submitting="isSubmitting" :default-proxy="defaultProxy!"
          @close="showAddUserProxyModal = false" @create-proxy="createUserProxy" />

        <!-- NOTE: You may need to create these modal components or adjust the paths -->
        <AddFileModal v-if="showAddFileModal" :is-submitting="isSubmitting" @close="showAddFileModal = false"
          @create-file="createFile" />

        <EditFileModal v-if="showEditFileModal" :is-submitting="isSubmitting" :file="fileToEdit"
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

import type { NetworkFile, UserProxyCreate, UserProxyUpdate } from '@/types';

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';
import AddUserProxyModal from '@/components/modals/account/AddUserProxyModal.vue';
import api from '@/api/api';
import useMainNetwork from '@/composables/useMainNetwork';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();

const mainNetwork = useMainNetwork();

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
const confirmationAction = ref(() => { });

onMounted(async () => {
  globalStore.startFetching();
  await authStore.getUserProxy();
  await mainNetwork.fetchMainNetwork();
  globalStore.stopFetching();
});

const allProxies = computed(() => {
  if (!authStore.currentUserProxy) return [];
  return [authStore.currentUserProxy, ...authStore.currentUserProxy?.user.userProxies.filter(u => u != null)];
});

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find(x => x.isDefault);
});

// Methods
function confirmAction() {
  confirmationAction.value();
  showConfirmationModal.value = false;
}

async function createUserProxy(newUserProxy: UserProxyCreate) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.post(`/users/${authStore.currentUserProxy?.user.id}/proxies/`, newUserProxy);
    window.location.reload();
    showAddUserProxyModal.value = false;
  } catch (err) {
    console.error('Error adding user proxy:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
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

async function createFile(file: File) {
  if (!authStore.currentUserProxy) return;
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.post(`/networks/${mainNetwork.network.value?.id}/files`, file);
    await authStore.getUserProxy();
    showAddFileModal.value = false;
  } catch (err) {
    console.error('Error adding file:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleEditFile(file: NetworkFile) {
  fileToEdit.value = file;
  showEditFileModal.value = true;
}

async function updateFile(file: NetworkFile) {
  if (!authStore.currentUserProxy) return;
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.put(`/users/${authStore.currentUserProxy.user.id}/proxies/${authStore.currentUserProxy.id}/files/${file.id}`, file);
    await authStore.getUserProxy();
    showEditFileModal.value = false;
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
      await api.delete(`/users/${authStore.currentUserProxy.user.id}/proxies/${authStore.currentUserProxy.id}/files/${file.id}`);
      await authStore.getUserProxy();
    } catch (err) {
      console.error('Error removing file:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

async function handleToggleFileVisibility(file: NetworkFile) {
  if (!authStore.currentUserProxy) return;
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.patch(`/users/${authStore.currentUserProxy.user.id}/proxies/${authStore.currentUserProxy.id}/files/${file.id}`, { isPublic: !file.isPublic });
    await authStore.getUserProxy();
  } catch (err) {
    console.error('Error toggling file visibility:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}
</script>
