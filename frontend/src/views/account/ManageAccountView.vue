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
          @create-proxy="showAddUserProxyModel = true" @edit-proxy="handleEditProxyClick"
          @switch-proxy="handleSwitchProxyClick" @update-user-proxy="handleUpdateProxy" />
      </div>

      <div v-if="authStore.currentUserProxy">
        <!-- Modals -->
        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
          :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />

        <AddUserProxyModal v-if="showAddUserProxyModel" :is-submitting="isSubmitting" :default-proxy="defaultProxy!"
          @close="showAddUserProxyModel = false" @create-proxy="createUserProxy" />
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

import type { UserProxyCreate, UserProxyUpdate } from '@/types';

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';
import AddUserProxyModal from '@/components/modals/account/AddUserProxyModal.vue';
import api from '@/api/api';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();
const showConfirmationModal = ref(false);
const isSubmitting = ref(false);

const showAddUserProxyModel = ref(false);

const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => { });

onMounted(async () => {
  globalStore.startFetching();
  await authStore.getUserProxy();
  globalStore.stopFetching();
});

const allProxies = computed(() => {
  if (!authStore.currentUserProxy) return [];
  return [authStore.currentUserProxy, ...authStore.currentUserProxy?.user.userProxies.filter(u => u != null)];
})

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find(x => x.isDefault);
})

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
    window.location.reload()
    showAddUserProxyModel.value = false;
  } catch (err) {
    console.error('Error adding user proxy:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleEditProxyClick(id: string) {
  router.push(`/account/proxies/${id}/edit`)
}

function handleSwitchProxyClick(id: string) {
  void id
  alert("not implemented")
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

</script>
