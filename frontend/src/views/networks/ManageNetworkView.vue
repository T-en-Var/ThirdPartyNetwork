<template>
  <div class="flex h-full pt-16">
    <NetworkSidebar />

    <div class="flex-1 overflow-auto p-6">
      <LoadingErrorComponent
        :loading="fetchNetwork.loading.value"
        :error="fetchNetwork.error.value"
        :has-value="!!network"
        button-value="Return to networks"
        @button-action="router.push(`/networks`)"
      />

      <RouterView
        v-if="!fetchNetwork.loading.value && !fetchNetwork.error.value && network"
        :network-id="network.id"
        :history="history"
        @confirm="confirm"
        @return="handleReturn"
        :fetch-blogs="handle.blog.fetchAll"
        :fetch-blog="handle.blog.fetch"
        @blog-edit="handle.blog.edit"
        @blog-create="handle.blog.create"
        @blog-delete="handle.blog.delete"
        :fetch-configurations="handle.configuration.fetchAll"
        :fetch-configuration="handle.configuration.fetch"
        @configurations-edit="handle.configuration.edit"
        @configurations-create="handle.configuration.create"
        @configurations-update="handle.configuration.update"
        @configurations-delete="handle.configuration.delete"
        :fetch-custom-pages="handle.customPages.fetchAll"
        :fetch-custom-page="handle.customPages.fetch"
        @custom-pages-edit="handle.customPages.edit"
        @custom-pages-create="handle.customPages.create"
        @custom-pages-update="handle.customPages.update"
        @custom-pages-delete="handle.customPages.delete"
        @nav-to-custom-pages="handle.customPages.nav"
        @page-blocks-edit="handle.pageBlocks.edit"
        @page-blocks-create="handle.pageBlocks.create"
        @page-blocks-update="handle.pageBlocks.update"
        @page-blocks-delete="handle.pageBlocks.delete"
        :fetch-files="handle.files.fetchAll"
        :fetch-file="handle.files.fetch"
        :network="network"
        @add-file="showModal.addFile.value = true"
        @file-update="handle.files.update"
        @file-delete="handle.files.delete"
        @add-user="showModal.addUser.value = true"
        @manage-user="openManageUserModal"
        @remove-user="handle.users.delete"
        @add-role="showModal.addRole.value = true"
        @manage-role="openEditRoleModal"
        @remove-role="handle.roles.delete"
        @add-access="showModal.addAccess.value = true"
        @toggle-access-required="handle.accesses.toggle"
        @remove-access="handle.accesses.delete"
        @update-network="handle.network.update"
        @delete-network="handle.network.delete"
      />

      <div v-if="network">
        <EditUserModal
          v-if="showModal.manageUser.value && selectedUser"
          :network="network"
          :selected-user="selectedUser"
          :is-submitting="isSubmitting"
          :manage-user-form="manageUserForm"
          @close="showModal.manageUser.value = false"
          @update="updateUserSettings"
        />

        <AddUserModal
          v-if="showModal.addUser.value"
          :network="network"
          :is-submitting="isSubmitting"
          @close="showModal.addUser.value = false"
          @add-user="handle.users.create"
        />

        <AddRoleModal
          v-if="showModal.addRole.value"
          :network="network"
          :is-submitting="isSubmitting"
          @close="showModal.addRole.value = false"
          @add-role="addRoleToNetwork"
        />

        <EditRoleModal
          v-if="showModal.editRole.value && selectedRole"
          :network="network"
          :selected-role="selectedRole"
          :manage-role-form="manageRoleForm"
          @close="showModal.editRole.value = false"
          @update="updateRolePermissions"
        />

        <AddAccessModal
          v-if="showModal.addAccess.value"
          :network="network"
          :is-submitting="isSubmitting"
          @close="showModal.addAccess.value = false"
          @add-access="handle.accesses.create"
        />

        <ConfirmationModal
          v-if="showModal.confirmation.value"
          :title="confirmationTitle"
          :message="confirmationMessage"
          :button-text="confirmButtonText"
          :color="confirmButtonColor"
          :is-submitting="isSubmitting"
          @close="showModal.confirmation.value = false"
          @confirm="confirmAction"
        />

        <AddFileModal
          v-if="showModal.addFile.value"
          media-type="any"
          :network-id="network.id"
          @close="showModal.addFile.value = false"
          @uploaded="handle.files.openEdit"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';

import NetworkSidebar from '@/components/sidebar/NetworkSidebar.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import EditUserModal from '@/components/modals/network/EditUserModal.vue';
import AddUserModal from '@/components/modals/network/AddUserModal.vue';
import AddRoleModal from '@/components/modals/network/AddRoleModal.vue';
import EditRoleModal from '@/components/modals/network/EditRoleModal.vue';
import AddAccessModal from '@/components/modals/network/AddAccessModal.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import useNetworkManageActions from '@/composables/useNetworkManageActions';

const router = useRouter();
const route = useRoute();

const networkId = computed(() => route.params.networkId as string);

const {
  fetchNetwork,
  history,
  showModal,
  selectedUser,
  selectedRole,
  isSubmitting,
  confirmationTitle,
  confirmationMessage,
  confirmButtonText,
  confirmButtonColor,
  manageUserForm,
  manageRoleForm,
  handle,
  confirm,
  confirmAction,
  handleReturn,
  openManageUserModal,
  openEditRoleModal,
  updateUserSettings,
  addRoleToNetwork,
  updateRolePermissions,
} = useNetworkManageActions(networkId);

const network = computed(() => fetchNetwork.data.value);

const originalFavicon = ref<string | null>(null);

onMounted(async () => {
  const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
  if (link) {
    originalFavicon.value = link.href;
  }

  await fetchNetwork.execute(networkId.value);
});

onUnmounted(() => {
  if (originalFavicon.value) {
    const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
    if (link) {
      link.href = originalFavicon.value;
    }
  }
});

watch(
  () => fetchNetwork.data.value?.imageFile?.url,
  (newUrl) => {
    if (newUrl) {
      let link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
      if (!link) {
        link = document.createElement('link');
        link.rel = 'icon';
        document.head.appendChild(link);
      }
      link.href = newUrl;
    }
  },
);
</script>
