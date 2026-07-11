<template>
  <div class="flex h-full pt-16">
    <!-- Sidebar -->
    <AccountSidebar />

    <!-- Main Content -->
    <div class="flex-1 overflow-auto p-6">
      <!-- Loading and Error States -->
      <LoadingErrorComponent
        :loading="authStore.loading"
        :error="authStore.error"
        button-value="Reload page"
        @button-action="router.go(0)"
        :has-value="!!authStore.currentUserProxy"
      />

      <!-- Route View (Page Content) -->
      <div v-if="authStore.currentUserProxy && manage.handle.value">
        <RouterView
          v-if="!authStore.loading && !authStore.error && authStore.currentUserProxy"
          :network-ids="composables.networks.fetchNetworks.data.value?.map((n) => n.id) ?? []"
          :history="manage.history"
          @confirm="manage.confirm"
          @return="manage.handleReturn"
          :fetch-blogs="manage.handle.value.blog.fetchAll"
          :fetch-blog="manage.handle.value.blog.fetch"
          @blog-edit="manage.handle.value.blog.edit"
          @blog-create="manage.handle.value.blog.create"
          @blog-delete="manage.handle.value.blog.delete"
          :fetch-configurations="manage.handle.value.configuration.fetchAll"
          :fetch-configuration="manage.handle.value.configuration.fetch"
          @configurations-edit="manage.handle.value.configuration.edit"
          @configurations-create="manage.handle.value.configuration.create"
          @configurations-update="manage.handle.value.configuration.update"
          @configurations-delete="manage.handle.value.configuration.delete"
          :fetch-custom-pages="manage.handle.value.customPages.fetchAll"
          :fetch-custom-page="manage.handle.value.customPages.fetch"
          @custom-pages-edit="manage.handle.value.customPages.edit"
          @custom-pages-create="manage.handle.value.customPages.create"
          @custom-pages-update="manage.handle.value.customPages.update"
          @custom-pages-delete="manage.handle.value.customPages.delete"
          @nav-to-custom-pages="manage.handle.value.customPages.nav"
          @page-blocks-edit="manage.handle.value.pageBlocks.edit"
          @page-blocks-create="manage.handle.value.pageBlocks.create"
          @page-blocks-update="manage.handle.value.pageBlocks.update"
          @page-blocks-delete="manage.handle.value.pageBlocks.delete"
          :fetch-files="manage.handle.value.files.fetchAll"
          :fetch-file="manage.handle.value.files.fetch"
          @create-proxy="manage.showAddUserProxyModal.value = true"
          @edit-proxy="manage.handleEditProxyClick"
          @switch-proxy="manage.handleSwitchProxyClick"
          @update-user-proxy="manage.handleUpdateProxy"
          @delete-user-proxy="() => {}"
        />
      </div>

      <div v-if="authStore.currentUserProxy && manage.handle.value">
        <!-- Modals -->
        <ConfirmationModal
          v-if="manage.showConfirmationModal.value"
          :title="manage.confirmationTitle.value"
          :message="manage.confirmationMessage.value"
          :button-text="manage.confirmButtonText.value"
          :color="manage.confirmButtonColor.value"
          :is-submitting="manage.isSubmitting.value"
          @close="manage.showConfirmationModal.value = false"
          @confirm="manage.confirmAction"
        />

        <AddUserProxyModal
          v-if="manage.showAddUserProxyModal.value && defaultProxy"
          :is-submitting="manage.isSubmitting.value"
          :default-proxy="defaultProxy"
          @close="manage.showAddUserProxyModal.value = false"
          @create-proxy="manage.createUserProxy"
        />

        <AddFileModal
          v-if="manage.showAddFileModal.value"
          :network-ids="userProxy!.networkUsers.map((nu) => nu.network.id)"
          @close="manage.showAddFileModal.value = false"
          @uploaded="manage.handle.value.files.openEdit"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useGlobalStore } from '@/stores/global';

import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import type { UserProxy } from '@/types';

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';
import AddUserProxyModal from '@/components/modals/account/AddUserProxyModal.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import useNetworks from '@/composables/useNetworks';
import useUsers from '@/composables/useUsers';
import useAccountManageActions from '@/composables/useAccountManageActions';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();

const mainNetwork = useNetworks().fetchMainNetwork;

const composables = {
  networks: useNetworks(),
};

const manage = useAccountManageActions();

const userProxy = ref<UserProxy | null>(null);
const { execute: fetchUser, data: user } = useUsers().fetchUser;

onMounted(async () => {
  globalStore.startFetching();
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error('UserProxy not found');

  await fetchUser(currentUser.user.id);
  if (!user.value) throw new Error('User not found');

  userProxy.value = user.value.userProxies.find((up) => up.id === currentUser.id) ?? null;
  if (!userProxy.value) throw new Error('UserProxy not found');

  await mainNetwork.execute();
  globalStore.stopFetching();

  manage.initialize(user.value.id, userProxy.value.id);
});

const allProxies = computed(() => {
  if (!user.value) return [];
  return user.value.userProxies;
});

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find((x) => x.isDefault)!;
});
</script>
