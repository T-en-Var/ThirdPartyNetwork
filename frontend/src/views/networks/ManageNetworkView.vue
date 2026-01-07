<template>
  <div class="flex h-full pt-16">
    <NetworkSidebar />

    <div class="flex-1 p-6 overflow-auto">
      <LoadingErrorComponent :loading="networkState.loading.value"
        :error="networkState.error.value" button-value="Return to networks"
        @button-action="router.push(`/networks`)" />

      <RouterView
        v-if="!networkState.loading.value && !networkState.error.value && networkState.network.value"
        :network="networkState.network.value" @add-user="showAddUserModal = true"
        @manage-user="openManageUserModal" @remove-user="confirmRemoveUser"
        @add-role="showAddRoleModal = true" @manage-role="openManageRoleModal"
        @remove-role="confirmRemoveRole" @add-access="showAddAccessModal = true"
        @toggle-access-required="confirmToggleAccessRequired"
        @remove-access="confirmRemoveAccess" @add-file="showAddFileModal = true"
        @edit-file="openEditFileModal" @remove-file="confirmRemoveFile"
        @toggle-file-visibility="confirmToggleFileVisibility"
        @open-create-custom-page-modal="showCreateCustomPageModal = true"
        @open-create-blog-modal="showCreateBlogModal = true"
        @open-create-configuration-modal="showCreateConfigurationModal = true"
        @edit-blog="handleEditBlog"
        @remove-blog="handleRemoveBlog"
        @edit-configuration="handleEditConfiguration"
        @update-configuration="handleUpdateConfiguration"
        @remove-configuration="handleRemoveConfiguration"
        @edit-custom-page="handleEditCustomPage"
        @remove-custom-page="handleRemoveCustomPage"
        @update-custom-page="handleUpdateCustomPage"
        @edit-page-block="handleEditPageBlock"
        @create-page-block="handleShowCreatePageBlockModal"
        @update-page-block="handleUpdatePageBlock"
        @update-network="handleUpdateNetwork"
        @delete-network="handleDeleteNetwork" />

      <div v-if="networkState.network.value">
        <AddUserModal v-if="showAddUserModal" :network="networkState.network.value"
          :is-submitting="isSubmitting" @close="showAddUserModal = false"
          @add-user="addUserToNetwork" />

        <ManageUserModal v-if="showManageUserModal && selectedUser"
          :network="networkState.network.value" :selected-user="selectedUser"
          :is-submitting="isSubmitting" :manage-user-form="manageUserForm"
          @close="showManageUserModal = false" @update="updateUserSettings" />

        <AddRoleModal v-if="showAddRoleModal" :network="networkState.network.value"
          :is-submitting="isSubmitting" @close="showAddRoleModal = false"
          @add-role="addRoleToNetwork" />

        <ManageRoleModal v-if="showManageRoleModal && selectedRole"
          :network="networkState.network.value" :selected-role="selectedRole"
          :manage-role-form="manageRoleForm" @close="showManageRoleModal = false"
          @update="updateRolePermissions" />

        <AddAccessModal v-if="showAddAccessModal"
          :network="networkState.network.value" :is-submitting="isSubmitting"
          @close="showAddAccessModal = false" @add-access="addAccessToNetwork" />

        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle"
          :message="confirmationMessage" :button-text="confirmButtonText"
          :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />

        <AddFileModal v-if="showAddFileModal" media-type="any"
          :network="networkState.network.value" @close="showAddFileModal = false"
          @uploaded="openEditFileModal" />

        <EditFileModal v-if="showEditFileModal && fileState.file.value"
          :file="fileState.file.value" :is-submitting="isSubmitting"
          @close="showEditFileModal = false" @update-file="editFile"
          @delete-file="confirmRemoveFile" />

        <AddCustomPageModal v-if="showCreateCustomPageModal"
          :is-submitting="isSubmitting" @create-custom-page="handleCreateCustomPage"
          @close="showCreateCustomPageModal = false" />

        <AddBlogModal v-if="showCreateBlogModal"
          :is-submitting="isSubmitting" :network-id="networkState.network.value!.id" @create-blog="handleCreateBlog"
          @close="showCreateBlogModal = false" />

        <AddConfigurationModal v-if="showCreateConfigurationModal"
          :is-submitting="isSubmitting"
          @create-configuration="handleCreateConfiguration"
          @close="showCreateConfigurationModal = false" />

        <AddPageBlockModal
          v-if="showCreatePageBlockModal && createPageBlockCustomPage"
          :custom-page="createPageBlockCustomPage" :is-submitting="isSubmitting"
          @create-page-block="handleCreatePageBlock"
          @close="handleCloseCreatePageBlockModal" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, watch, type Ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import useNetwork from '@/composables/useNetwork';
import usePermissions from '@/composables/usePermissions';
import { useGlobalStore } from '@/stores/global';

// Components
import NetworkSidebar from '@/components/sidebar/NetworkSidebar.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AddUserModal from '@/components/modals/network/AddUserModal.vue';
import ManageUserModal from '@/components/modals/network/ManageUserModal.vue';
import AddRoleModal from '@/components/modals/network/AddRoleModal.vue';
import ManageRoleModal from '@/components/modals/network/ManageRoleModal.vue';
import AddAccessModal from '@/components/modals/network/AddAccessModal.vue';
import AddFileModal from '@/components/modals/network/AddFileModal.vue';
import EditFileModal from '@/components/modals/network/EditFileModal.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import type { NetworkUser, Role, NetworkAccess, NetworkFile, CreateRole, UpdateRole, CustomPage, CreateCustomPage, PageBlock, CreatePageBlock, NetworkAccessCreate, Network, NetworkUpdate, CreateConfiguration, Blog, CreateBlog } from '@/types';
import type { RoleForm, ManageUserForm, EditFileForm } from '@/types/forms';

import api from '@/api/api';
import useFiles from '@/composables/useFiles';
import AddCustomPageModal from '@/components/modals/network/AddCustomPageModal.vue';
import AddBlogModal from '@/components/modals/network/AddBlogModal.vue';
import AddPageBlockModal from '@/components/modals/network/AddPageBlockModal.vue';
import useCustomPages from '@/composables/useCustomPages';
import AddConfigurationModal from '@/components/modals/network/AddConfigurationModal.vue';
import useConfigurations from '@/composables/useConfigurations';

const router = useRouter();
const route = useRoute();

const globalStore = useGlobalStore();
const permissionsState = usePermissions();
const networkState = useNetwork();
const fileState = useFiles();
const customPagesState = useCustomPages();
const configurationsState = useConfigurations();

const showAddUserModal = ref(false);
const showManageUserModal = ref(false);
const showAddRoleModal = ref(false);
const showManageRoleModal = ref(false);
const showAddAccessModal = ref(false);
const showConfirmationModal = ref(false);
const showAddFileModal = ref(false);
const showEditFileModal = ref(false);
const showCreateCustomPageModal = ref(false);
const showCreateConfigurationModal = ref(false);
const showCreateBlogModal = ref(false);
const showCreatePageBlockModal = ref(false);
const createPageBlockCustomPage = ref<CustomPage | null>(null);

const selectedUser = ref<NetworkUser | null>(null);
const selectedRole = ref<Role | null>(null);
const isSubmitting = ref(false);

const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => { });

const manageUserForm = reactive<ManageUserForm>({ roleIds: [] });
const manageRoleForm = reactive<RoleForm>({
  name: '',
  description: '',
  permissionIds: [],
  isDefault: false,
});

const originalFavicon = ref<string | null>(null);

onMounted(async () => {
  const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
  if (link) {
    originalFavicon.value = link.href;
  }

  const networkId = route.params.networkId as string;
  await networkState.fetchNetwork(networkId);
});

onUnmounted(() => {
  if (originalFavicon.value) {
    const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
    if (link) {
      link.href = originalFavicon.value;
    }
  }
});

watch(() => networkState.network.value?.imageFile?.url, (newUrl) => {
  if (newUrl) {
    let link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
    if (!link) {
      link = document.createElement('link');
      link.rel = 'icon';
      document.head.appendChild(link);
    }
    link.href = newUrl;
  }
});

// Methods
function openManageUserModal(user: NetworkUser) {
  selectedUser.value = user;
  manageUserForm.roleIds = user.networkUserRoles?.map(nur => nur.role.id) || [];

  showManageUserModal.value = true;
}

function confirmRemoveUser(user: NetworkUser) {
  confirmationTitle.value = 'Remove User';
  confirmationMessage.value = `Are you sure you want to remove ${user.userProxy.firstName} ${user.userProxy.lastName} from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/users/${user.id}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
    } catch (err) {
      console.error('Error removing user:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

async function openManageRoleModal(role: Role) {
  selectedRole.value = role;

  await permissionsState.fetchPermissions();
  manageRoleForm.name = role.name;
  manageRoleForm.description = role.description;
  manageRoleForm.isDefault = role.isDefault;
  manageRoleForm.permissionIds = role.rolePermissions?.map(rp => rp.permission.id) || [];

  showManageRoleModal.value = true;
}

function confirmRemoveRole(role: Role) {
  confirmationTitle.value = 'Remove Role';
  confirmationMessage.value = `Are you sure you want to remove the role "${role.name}" from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/roles/${role.id}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
    } catch (err) {
      console.error('Error removing role:', err);
    } finally {
      globalStore.stopFetching();
      isSubmitting.value = false;
    }
  };
  showConfirmationModal.value = true;
}

function confirmToggleAccessRequired(networkAccess: NetworkAccess) {
  confirmationTitle.value = 'Toggle Access Requirement';
  confirmationMessage.value = networkAccess.isRequired
    ? `Are you sure you want to make the Access ${networkAccess.access.name} optional?`
    : `Are you sure you want to make the Access ${networkAccess.access.name} required?`;
  confirmButtonText.value = 'Confirm';
  confirmButtonColor.value = 'green';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    globalStore.startFetching();

    api.put(`/networks/${networkState.network.value!.id}/accesses/${networkAccess.accessId}?IsRequired=${!networkAccess.isRequired}`, {})
      .then(() => {
        networkState.fetchNetwork(networkState.network.value!.id);
      })
      .catch(err => {
        console.error('Error toggling access requirement:', err);
      })
      .finally(() => {
        isSubmitting.value = false;
        globalStore.stopFetching();
      });
  };

  showConfirmationModal.value = true;
}

function confirmRemoveAccess(networkAccess: NetworkAccess) {
  confirmationTitle.value = 'Remove Access Requirement';
  confirmationMessage.value = `Are you sure you want to remove the access requirement "${networkAccess.access.name}" from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/accesses/${networkAccess.accessId}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
    } catch (err) {
      console.error('Error removing access requirement:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };

  showConfirmationModal.value = true;
}

async function addUserToNetwork(userData: { userId: string, roleIds: string[]; }) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.post(`/networks/${networkState.network.value!.id}/users/`, {
      userProxyId: userData.userId,
      roleIds: userData.roleIds
    });
    await networkState.fetchNetwork(networkState.network.value!.id);
    showAddUserModal.value = false;
  } catch (err) {
    console.error('Error adding user:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function updateUserSettings(localForm: ManageUserForm) {
  if (!selectedUser.value) return;

  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    const networkId = networkState.network.value!.id;
    const userId = selectedUser.value!.id;

    const addedRoles = localForm.roleIds.filter(roleId => !manageUserForm.roleIds.includes(roleId));
    const removedRoles = manageUserForm.roleIds.filter(roleId => !localForm.roleIds.includes(roleId));

    const hasChanges = addedRoles.length || removedRoles.length;

    if (hasChanges) {
      await Promise.all([
        ...addedRoles.map(roleId => api.post(`/networks/${networkId}/users/${userId}/roles/${roleId}/`, {})),
        ...removedRoles.map(roleId => api.delete(`/networks/${networkId}/users/${userId}/roles/${roleId}/`)),
      ]);

      await networkState.fetchNetwork(networkState.network.value!.id);
      manageUserForm.roleIds = localForm.roleIds;
    }

    showManageUserModal.value = false;
  } catch (err) {
    console.error('Error updating user settings:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function addRoleToNetwork(localForm: Ref<RoleForm>) {
  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    const networkId = networkState.network.value!.id;

    const response = await api.post<Role, CreateRole>(`/networks/${networkId}/roles/`, {
      name: localForm.value.name.trim(),
      description: localForm.value.description.trim(),
      isDefault: localForm.value.isDefault,
    });

    const roleId = response.data.id;

    await Promise.all([
      ...localForm.value.permissionIds.map(permId => api.post(`/networks/${networkId}/roles/${roleId}/permissions/${permId}/`, {})),
    ]);

    await networkState.fetchNetwork(networkState.network.value!.id);

    showAddRoleModal.value = false;
  } catch (err) {
    console.error('Error adding role:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function updateRolePermissions(localForm: Ref<RoleForm>) {
  if (!selectedRole.value) return;

  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    const networkId = networkState.network.value!.id;
    const roleId = selectedRole.value!.id;

    const addedPerms = localForm.value.permissionIds.filter(permId => !manageRoleForm.permissionIds.includes(permId));
    const removedPerms = manageRoleForm.permissionIds.filter(permId => !localForm.value.permissionIds.includes(permId));

    await api.put<Role, UpdateRole>(`/networks/${networkId}/roles/${roleId}`, {
      name: localForm.value.name.trim(),
      description: localForm.value.description.trim(),
      isDefault: localForm.value.isDefault,
    });

    const hasChanges = addedPerms.length || removedPerms.length;

    if (hasChanges) {
      await Promise.all([
        ...addedPerms.map(permId => api.post(`/networks/${networkId}/roles/${roleId}/permissions/${permId}/`, {})),
        ...removedPerms.map(permId => api.delete(`/networks/${networkId}/roles/${roleId}/permissions/${permId}/`)),
      ]);
    }

    await networkState.fetchNetwork(networkState.network.value!.id);
    showManageRoleModal.value = false;
  } catch (err) {
    console.error('Error updating role permissions:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function addAccessToNetwork(accessData: NetworkAccessCreate) {
  if (!accessData.access) return;
  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    await api.post(`/networks/${networkState.network.value!.id}/accesses/${accessData.access.id}`, { isRequired: accessData.isRequired });
    await networkState.fetchNetwork(networkState.network.value!.id);
    showAddAccessModal.value = false;
  } catch (err) {
    console.error('Error adding access requirement:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function confirmToggleFileVisibility(networkFile: NetworkFile) {
  confirmationTitle.value = 'Toggle File Visibility';
  confirmationMessage.value = networkFile.isPublic
    ? `Are you sure you want to make the File ${networkFile.name} private?`
    : `Are you sure you want to make the File ${networkFile.name} publicly visible?`;
  confirmButtonText.value = 'Confirm';
  confirmButtonColor.value = 'green';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    globalStore.startFetching();

    try {
      const response = await api.put<NetworkFile, EditFileForm>(`/networks/${networkState.network.value!.id}/files/${networkFile.id}`,
        { isPublic: !networkFile.isPublic, name: networkFile.name });
      fileState.insertFile(response.data);
    } catch (err) {
      console.error('Error toggling file visibility:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };

  showConfirmationModal.value = true;
}

function confirmRemoveFile(networkFile: NetworkFile) {
  showEditFileModal.value = false;

  confirmationTitle.value = 'Delete File';
  confirmationMessage.value = `Are you sure you want to delete the file '${networkFile.name}'?`;
  confirmButtonText.value = 'Confirm';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    globalStore.startFetching();

    try {
      await api.delete(`/networks/${networkState.network.value!.id}/files/${networkFile.id}`);
      fileState.files.value = fileState.files.value.filter(f => f.id != networkFile.id);
    } catch (err) {
      console.error('Error deleting file:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };

  showConfirmationModal.value = true;
}

async function openEditFileModal(file: NetworkFile) {
  fileState.file.value = file;
  showAddFileModal.value = false;
  showEditFileModal.value = true;
  fileState.insertFile(file);
}

async function editFile(id: string, networkFile: EditFileForm) {
  isSubmitting.value = true;
  globalStore.startFetching();

  try {
    const response = await api.put<NetworkFile, EditFileForm>(`/networks/${networkState.network.value!.id}/files/${id}`, networkFile);
    fileState.insertFile(response.data);
    showEditFileModal.value = false;
  } catch (err) {
    console.error('Error editing file:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

// @create-custom-page="handleCreateCustomPage"
async function handleCreateCustomPage(customPageData: CreateCustomPage) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.post(`/networks/${networkState.network.value!.id}/customPages/`, customPageData);
    customPagesState.fetchCustomPages(networkState.network.value!.id);
    showAddUserModal.value = false;
  } catch (err) {
    console.error('Error creating custom page:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function handleCreateBlog(blogCreate: CreateBlog) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    const response = await api.post<Blog, CreateBlog>(`/networks/${networkState.network.value!.id}/blogs/`, blogCreate);
    // assume response.data contains created blog with id
    const created = response.data;
    showCreateBlogModal.value = false;
    // navigate to edit page
    router.push(`/networks/${networkState.network.value!.id}/manage/blogs/${created.id}/edit`);
  } catch (err) {
    console.error('Error creating blog:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleEditBlog(blog: Blog) {
  router.push(`/networks/${networkState.network.value!.id}/manage/blogs/${blog.id}/edit`);
}

function handleRemoveBlog(blog: Blog) {
  confirmationTitle.value = 'Remove Blog';
  confirmationMessage.value = `Are you sure you want to remove ${blog.title} from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/blogs/${blog.id}/`);
      // refresh page or state
      router.go(0);
    } catch (err) {
      console.error('Error removing blog:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

async function handleCreateConfiguration(payload: { key: string; value: object; accessLevel: number; }) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.post(`/networks/${networkState.network.value!.id}/configurations`, payload);
    configurationsState.fetchNetworkConfigurations(networkState.network.value!.id);
    showCreateConfigurationModal.value = false;
    router.go(0);
  } catch (err) {
    console.error('Error creating configuration:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleEditConfiguration(cfg: { id: string; }) {
  router.push(`/networks/${networkState.network.value?.id}/manage/configurations/${cfg.id}/edit`);
}

async function handleUpdateConfiguration(cfgId: string, cfg: CreateConfiguration) {
  await configurationsState.updateConfiguration(networkState.network.value!.id, cfgId, cfg);
  router.go(0);
}

function handleRemoveConfiguration(cfg: { id: string; key?: string; }) {
  confirmationTitle.value = 'Remove Configuration';
  confirmationMessage.value = `Are you sure you want to remove configuration ${cfg.key ?? ''}?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/configurations/${cfg.id}/`);
      await configurationsState.fetchNetworkConfigurations(networkState.network.value!.id);
      router.push(`/networks/${networkState.network.value?.id}/manage/configurations`);
    } catch (err) {
      console.error('Error removing configuration:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

function handleEditCustomPage(customPage: CustomPage) {
  router.push(`/networks/${networkState.network.value?.id}/manage/custom-pages/${customPage.id}/edit`);
}

function handleRemoveCustomPage(customPage: CustomPage) {
  confirmationTitle.value = 'Remove Custom Page';
  confirmationMessage.value = `Are you sure you want to remove ${customPage.name} from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/customPages/${customPage.id}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
    } catch (err) {
      console.error('Error removing custom page:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

function handleShowCreatePageBlockModal(customPage: CustomPage) {
  showCreatePageBlockModal.value = true;
  createPageBlockCustomPage.value = customPage;
}

function handleCloseCreatePageBlockModal() {
  showCreatePageBlockModal.value = false;
  createPageBlockCustomPage.value = null;
}

async function handleUpdateCustomPage(id: string, customPage: CreateCustomPage) {
  isSubmitting.value = true;
  globalStore.startFetching();

  try {
    await api.put<CustomPage, CreateCustomPage>(`/networks/${networkState.network.value!.id}/customPages/${id}`, customPage);
    await networkState.fetchNetwork(networkState.network.value!.id);
  } catch (err) {
    console.error('Error editing custom page:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function handleCreatePageBlock(customPageId: string, pageBlock: CreatePageBlock) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.post(`/networks/${networkState.network.value!.id}/customPages/${customPageId}/pageBlocks`, pageBlock);
    await networkState.fetchNetwork(networkState.network.value!.id);
    handleCloseCreatePageBlockModal();
  } catch (err) {
    console.error('Error creating custop page:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleEditPageBlock(customPage: CustomPage, pageBlock: PageBlock) {
  router.push(`/networks/${networkState.network.value!.id}/manage/custom-pages/${customPage.id}/blocks/${pageBlock.id}/edit`);
}

async function handleUpdatePageBlock(customPageId: string, pageBlock: PageBlock) {
  isSubmitting.value = true;
  globalStore.startFetching();

  try {
    await api.put<PageBlock, PageBlock>(`/networks/${networkState.network.value!.id}/customPages/${customPageId}/pageBlocks/${pageBlock.id}`, pageBlock);
    await networkState.fetchNetwork(networkState.network.value!.id);
  } catch (err) {
    console.error('Error editing page block:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function handleUpdateNetwork(networkId: string, networkUpdate: NetworkUpdate) {
  isSubmitting.value = true;
  globalStore.startFetching();

  try {
    await api.put(`/networks/${networkId}/`, networkUpdate);
    await networkState.fetchNetwork(networkState.network.value!.id);
  } catch (err) {
    console.error('Error updating network:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function handleDeleteNetwork(network: Network) {
  confirmationTitle.value = 'Delete network';
  confirmationMessage.value = `Are you sure you want to delete ${network.name} permanently?`;
  confirmButtonText.value = 'Proceed';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${network.id}/`);
      router.push("/networks");
    } catch (err) {
      console.error('Error removing custom page:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

function confirmAction() {
  confirmationAction.value();
  showConfirmationModal.value = false;
}

</script>
