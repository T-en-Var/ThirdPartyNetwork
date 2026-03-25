<template>
  <div class="flex h-full pt-16">
    <NetworkSidebar />

    <div class="flex-1 p-6 overflow-auto">
      <LoadingErrorComponent :loading="networksState.fetchNetwork.loading.value"
        :error="networksState.fetchNetwork.error.value"
        :has-value="!!networksState.fetchNetwork.data"
        button-value="Return to networks"
        @button-action="router.push(`/networks`)" />

      <RouterView v-if="!networksState.fetchNetwork.loading.value && !networksState.fetchNetwork.error.value
        && networksState.fetchNetwork.data.value"
        :network="networksState.fetchNetwork.data.value"
        @confirm="handleConfirmationModal" 
        
        @add-user="showAddUserModal = true"
        @manage-user="openManageUserModal" 
        @remove-user="confirmRemoveUser"
        @add-role="showAddRoleModal = true" 
        @manage-role="openEditRoleModal"
        @remove-role="confirmRemoveRole" 
        @add-access="showAddAccessModal = true"
        @toggle-access-required="confirmToggleAccessRequired"
        @remove-access="confirmRemoveAccess" 

        @add-file="showAddFileModal = true"
        @edit-file="openEditFileModal" 
        @remove-file="confirmRemoveFile"
        @edit-page-block="handleEditPageBlock"
        @create-page-block="handleShowCreatePageBlockModal"
        @update-page-block="handleUpdatePageBlock"

        @update-network="handleUpdateNetwork"
        @delete-network="handleDeleteNetwork" />

      <div v-if="networksState.fetchNetwork.data.value!">
        <AddUserModal v-if="showAddUserModal"
          :network="networksState.fetchNetwork.data.value!"
          :is-submitting="isSubmitting" @close="showAddUserModal = false"
          @add-user="addUserToNetwork" />

        <EditUserModal v-if="showManageUserModal && selectedUser"
          :network="networksState.fetchNetwork.data.value!"
          :selected-user="selectedUser" :is-submitting="isSubmitting"
          :manage-user-form="manageUserForm" @close="showManageUserModal = false"
          @update="updateUserSettings" />

        <AddRoleModal v-if="showAddRoleModal"
          :network="networksState.fetchNetwork.data.value!"
          :is-submitting="isSubmitting" @close="showAddRoleModal = false"
          @add-role="addRoleToNetwork" />

        <EditRoleModal v-if="showEditRoleModal && selectedRole"
          :network="networksState.fetchNetwork.data.value!"
          :selected-role="selectedRole" :manage-role-form="manageRoleForm"
          @close="showEditRoleModal = false" @update="updateRolePermissions" />

        <AddAccessModal v-if="showAddAccessModal"
          :network="networksState.fetchNetwork.data.value!"
          :is-submitting="isSubmitting" @close="showAddAccessModal = false"
          @add-access="addAccessToNetwork" />

        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle"
          :message="confirmationMessage" :button-text="confirmButtonText"
          :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />

        <AddFileModal v-if="showAddFileModal" media-type="any"
          :network="networksState.fetchNetwork.data.value!"
          @close="showAddFileModal = false" @uploaded="openEditFileModal" />

        <EditFileModal v-if="showEditFileModal && selectedFile" :file="selectedFile"
          :is-submitting="isSubmitting" @close="showEditFileModal = false"
          @update-file="editFile" @delete-file="confirmRemoveFile" />

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
import { ref, reactive, onMounted, onUnmounted, watch, type Ref, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import usePermissions from '@/composables/usePermissions';
import { useGlobalStore } from '@/stores/global';

// Components
import NetworkSidebar from '@/components/sidebar/NetworkSidebar.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AddUserModal from '@/components/modals/network/AddUserModal.vue';
import EditUserModal from '@/components/modals/network/EditUserModal.vue';
import AddRoleModal from '@/components/modals/network/AddRoleModal.vue';
import EditRoleModal from '@/components/modals/network/EditRoleModal.vue';
import AddAccessModal from '@/components/modals/network/AddAccessModal.vue';
import AddFileModal from '@/components/modals/network/AddFileModal.vue';
import EditFileModal from '@/components/modals/network/EditFileModal.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import type { NetworkUser, Role, NetworkAccess, NetworkFile, PageBlock, CreatePageBlock, NetworkAccessCreate, Network, NetworkUpdate, UpdateNetworkUser, CustomPage } from '@/types';
import type { RoleForm, ManageUserForm, EditFileForm, ConfirmForm } from '@/types/forms';

import api from '@/api/api';
import useFiles from '@/composables/useFiles';
import AddPageBlockModal from '@/components/modals/network/AddPageBlockModal.vue';
import useCustomPages from '@/composables/useCustomPages';
import useRoles from '@/composables/useRoles';
import useNetworks from '@/composables/useNetworks';
import useNetworkUsers from '@/composables/useNetworkUsers';
import useAccesses from '@/composables/useAccesses';

const router = useRouter();
const route = useRoute();

const globalStore = useGlobalStore();
const permissionsState = usePermissions();
const accessesState = useAccesses();
const networkUsersState = useNetworkUsers();
const networksState = useNetworks();
const rolesState = useRoles();

const customPagesState = useCustomPages();
const filesState = useFiles();

const showAddUserModal = ref(false);
const showManageUserModal = ref(false);
const showAddRoleModal = ref(false);
const showEditRoleModal = ref(false);
const showAddAccessModal = ref(false);
const showConfirmationModal = ref(false);
const showAddFileModal = ref(false);
const showEditFileModal = ref(false);
const showCreatePageBlockModal = ref(false);
const createPageBlockCustomPage = ref<CustomPage | null>(null);

const selectedUser = ref<NetworkUser | null>(null);
const selectedRole = ref<Role | null>(null);
const selectedFile = ref<NetworkFile | null>(null);
const isSubmitting = ref(false);

const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => { });

const manageUserForm = reactive<ManageUserForm>({ roleIds: [], entitlements: {} });
const manageRoleForm = reactive<RoleForm>({
  name: '',
  description: '',
  permissionIds: [],
  isDefault: false,
  entitlements: {}
});

const originalFavicon = ref<string | null>(null);
const networkId = computed(() => networksState.fetchNetwork.data.value!.id);

onMounted(async () => {
  const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
  if (link) {
    originalFavicon.value = link.href;
  }

  const { execute: fetchNetwork } = networksState.fetchNetwork;

  const networkId = route.params.networkId as string;
  await fetchNetwork(networkId);
});

onUnmounted(() => {
  if (originalFavicon.value) {
    const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
    if (link) {
      link.href = originalFavicon.value;
    }
  }
});

watch(() => networksState.fetchNetwork.data.value?.imageFile?.url, (newUrl) => {
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
function handleConfirmationModal(form: ConfirmForm) {
  confirmationTitle.value = form.title;
  confirmationMessage.value = form.message;
  confirmButtonText.value = form.buttonText;
  confirmButtonColor.value = form.buttonColor;

  confirmationAction.value = async () => {
    await form.action();
  };

  showConfirmationModal.value = true;
}


function openManageUserModal(user: NetworkUser) {
  selectedUser.value = user;
  manageUserForm.roleIds = user.networkUserRoles?.map(nur => nur.role.id) || [];
  manageUserForm.entitlements = user.entitlements;

  showManageUserModal.value = true;
}

function confirmRemoveUser(networkUser: NetworkUser) {
  confirmationTitle.value = 'Remove User';
  confirmationMessage.value = `Are you sure you want to remove ${networkUser.userProxy.firstName} ${networkUser.userProxy.lastName} from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    isSubmitting.value = true;

    const { execute: deleteNetworkUser } = networkUsersState.deleteNetworkUser;

    try {
      await deleteNetworkUser(networkId.value, networkUser.id);
    } catch (err) {
      console.error('Error removing network user:', err);
    } finally {
      isSubmitting.value = false;
    }
  };
  showConfirmationModal.value = true;
}

async function openEditRoleModal(role: Role) {
  selectedRole.value = role;

  const { execute: fetchPermissions } = permissionsState.fetchPermissions;

  await fetchPermissions();
  manageRoleForm.name = role.name;
  manageRoleForm.description = role.description;
  manageRoleForm.isDefault = role.isDefault;
  manageRoleForm.permissionIds = role.rolePermissions?.map(rp => rp.permission.id) || [];

  showEditRoleModal.value = true;
}

function confirmRemoveRole(role: Role) {
  confirmationTitle.value = 'Remove Role';
  confirmationMessage.value = `Are you sure you want to remove the role "${role.name}" from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    const { execute: deleteRole } = rolesState.deleteRole;

    try {
      await deleteRole(networkId.value, role.id);
    } catch (err) {
      console.error('Error removing role:', err);
    } finally {
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
    const { execute: updateNetworkAccess } = accessesState.updateNetworkAccess;
    isSubmitting.value = true;

    try {
      await updateNetworkAccess(networkId.value, networkAccess.accessId, !networkAccess.isRequired);
    } catch (err) {
      console.error('Error toggling access requirement:', err);
    } finally {
      isSubmitting.value = false;
    }
  };

  showConfirmationModal.value = true;
}

function confirmRemoveAccess(networkAccess: NetworkAccess) {
  confirmationTitle.value = 'Remove Access Requirement';
  confirmationMessage.value = `Are you sure you want to remove the access requirement "${networkAccess.access.name}" from this network?`;
  confirmButtonText.value = 'Remove';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    isSubmitting.value = true;
    const { execute: deleteNetworkAccess } = accessesState.deleteNetworkAccess;

    try {
      await deleteNetworkAccess(networkId.value, networkAccess.accessId);
      showConfirmationModal.value = false;
    } catch (err) {
      console.error('Error removing access requirement:', err);
    } finally {
      isSubmitting.value = false;
    }
  };

  showConfirmationModal.value = true;
}

async function addUserToNetwork(userData: { userId: string, roleIds: string[]; }) {
  isSubmitting.value = true;
  const { execute: createNetworkUser } = networkUsersState.createNetworkUser;

  try {
    await createNetworkUser(networkId.value, {
      userProxyId: userData.userId,
      roleIds: userData.roleIds
    });
    showAddUserModal.value = false;
  } catch (err) {
    console.error('Error removing access requirement:', err);
  } finally {
    isSubmitting.value = false;
  }
}

async function updateUserSettings(localForm: ManageUserForm) {
  if (!selectedUser.value) return;

  const addedRoles = localForm.roleIds.filter(roleId => !manageUserForm.roleIds.includes(roleId));
  const removedRoles = manageUserForm.roleIds.filter(roleId => !localForm.roleIds.includes(roleId));

  const { execute: updateNetworkUser } = networkUsersState.updateNetworkUser;

  const payload: UpdateNetworkUser = {
    entitlements: localForm.entitlements,
  };

  try {
    await updateNetworkUser(networkId.value, selectedUser.value.id, payload, addedRoles, removedRoles);

    manageUserForm.roleIds = localForm.roleIds;
    manageUserForm.entitlements = localForm.entitlements;

    showManageUserModal.value = false;
  } catch (err) {
    console.error('Error updating user settings:', err);
  } finally {
    isSubmitting.value = false;
  }
}

async function addRoleToNetwork(localForm: Ref<RoleForm>) {
  const { execute: createRole } = rolesState.createRole;

  const payload = {
    name: localForm.value.name.trim(),
    description: localForm.value.description.trim(),
    isDefault: localForm.value.isDefault,
  };

  try {
    await createRole(networkId.value, payload, localForm.value.permissionIds);

    showAddRoleModal.value = false;
  } catch (err) {
    console.error('Error adding role:', err);
  } finally {
    isSubmitting.value = false;
  }
}

async function updateRolePermissions(localForm: RoleForm) {
  if (!selectedRole.value) return;
  isSubmitting.value = true;

  const { execute: updateRole } = rolesState.updateRole;
  const payload = {
    name: localForm.name.trim(),
    description: localForm.description.trim(),
    isDefault: localForm.isDefault,
    entitlements: localForm.entitlements
  };

  try {
    const roleId = selectedRole.value!.id;

    const addedPerms = localForm.permissionIds.filter(permId => !manageRoleForm.permissionIds.includes(permId));
    const removedPerms = manageRoleForm.permissionIds.filter(permId => !localForm.permissionIds.includes(permId));

    await updateRole(networkId.value, roleId, payload, addedPerms, removedPerms);

    showEditRoleModal.value = false;
  } catch (err) {
    console.error('Error updating role permissions:', err);
  } finally {
    isSubmitting.value = false;
  }
}

async function addAccessToNetwork(accessData: NetworkAccessCreate) {
  if (!accessData.access) return;
  isSubmitting.value = true;

  const { execute: createAccess } = accessesState.createAccess;

  try {
    await createAccess(networkId.value, accessData.access.id, accessData.isRequired);

    showAddAccessModal.value = false;
  } catch (err) {
    console.error('Error adding access requirement:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function confirmRemoveFile(networkFile: NetworkFile) {
  showEditFileModal.value = false;

  confirmationTitle.value = 'Delete File';
  confirmationMessage.value = `Are you sure you want to delete the file '${networkFile.name}'?`;
  confirmButtonText.value = 'Confirm';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    isSubmitting.value = true;

    const { execute: deleteFile } = filesState.deleteFile;

    try {
      await deleteFile(networkId.value, networkFile.id);
    } catch (err) {
      console.error('Error deleting file:', err);
    } finally {
      isSubmitting.value = false;
    }
  };

  showConfirmationModal.value = true;
}

async function openEditFileModal(file: NetworkFile) {
  await filesState.fetchFile.execute(networkId.value, file.id);

  if (showAddFileModal.value) showAddFileModal.value = false;

  selectedFile.value = file;
  showEditFileModal.value = true;
}

async function editFile(id: string, networkId: string, networkFile: EditFileForm) {
  isSubmitting.value = true;

  const { execute: updateFile } = filesState.updateFile;

  try {
    await updateFile(networkId, id, networkFile);
    showEditFileModal.value = false;
  } catch (err) {
    console.error('Error editing file:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function handleShowCreatePageBlockModal(customPage: CustomPage) {
  showCreatePageBlockModal.value = true;
  createPageBlockCustomPage.value = customPage;
}

function handleCloseCreatePageBlockModal() {
  showCreatePageBlockModal.value = false;
  createPageBlockCustomPage.value = null;
}

async function handleCreatePageBlock(customPageId: string, pageBlock: CreatePageBlock) {
  isSubmitting.value = true;
  const { execute: createPageBlock } = customPagesState.createPageBlock;

  try {
    await createPageBlock(networkId.value, customPageId, pageBlock);
    handleCloseCreatePageBlockModal();
  } catch (err) {
    console.error('Error creating page block:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function handleEditPageBlock(customPage: CustomPage, pageBlock: PageBlock) {
  router.push(`/networks/${networkId.value}/manage/custom-pages/${customPage.id}/blocks/${pageBlock.id}/edit`);
}

async function handleUpdatePageBlock(customPageId: string, pageBlock: PageBlock) {
  isSubmitting.value = true;
  const { execute: updatePageBlock } = customPagesState.updatePageBlock;

  try {
    await updatePageBlock(networkId.value, customPageId, pageBlock.id, pageBlock);
  } catch (err) {
    console.error('Error editing page block:', err);
  } finally {
    isSubmitting.value = false;
  }
}

async function handleUpdateNetwork(networkId: string, networkUpdate: NetworkUpdate) {
  isSubmitting.value = true;
  const { execute: updateNetwork } = networksState.updateNetwork;

  try {
    await updateNetwork(networkId, networkUpdate);
  } catch (err) {
    console.error('Error updating network:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function handleDeleteNetwork(network: Network) {
  confirmationTitle.value = 'Delete network';
  confirmationMessage.value = `Are you sure you want to delete ${network.name} permanently?`;
  confirmButtonText.value = 'Proceed';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    confirmationTitle.value = 'Delete network';
    confirmationMessage.value = `Are you REALLY SURE you want to delete ${network.name} permanently?`;
    confirmButtonText.value = 'DO IT';
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
  };

  showConfirmationModal.value = true;
}

function confirmAction() {
  showConfirmationModal.value = false;
  confirmationAction.value();
}

</script>
