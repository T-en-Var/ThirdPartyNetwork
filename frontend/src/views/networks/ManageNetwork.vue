<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Loading and Error States -->
    <loading-error-component :loading="networkState.loading.value" :error="networkState.error.value"
      @return-to-networks="router.push('/networks')" />

    <!-- Network Management UI -->
    <div v-if="!networkState.loading.value && !networkState.error.value && networkState.network.value != null"
      class="space-y-8">
      <!-- Network Header -->
      <network-header :network="networkState.network.value" @navigate-back="router.push('/networks')"
        @edit-network="showEditNetworkModal = true" />

      <!-- Tab Navigation -->
      <tab-container :network="networkState.network.value" :active-tab="activeTab" @tab-changed="activeTab = $event"
        @add-user="showAddUserModal = true" @add-role="showAddRoleModal = true" @add-access="showAddAccessModal = true"
        @manage-user="openManageUserModal" @remove-user="confirmRemoveUser" @manage-role="openManageRoleModal"
        @remove-role="confirmRemoveRole" @toggle-access-required="toggleAccessRequired"
        @remove-access="confirmRemoveAccess" />

      <!-- Modals -->
      <edit-network-modal v-if="showEditNetworkModal" :network="networkState.network.value"
        :is-submitting="isSubmitting" @close="showEditNetworkModal = false" @update="updateNetwork" />

      <add-user-modal v-if="showAddUserModal" :network="networkState.network.value" :is-submitting="isSubmitting"
        @close="showAddUserModal = false" @add-user="addUserToNetwork" />

      <manage-user-modal v-if="showManageUserModal" :network="networkState.network.value" :selected-user="selectedUser"
        :is-submitting="isSubmitting" :manage-user-form="manageUserForm" @close="showManageUserModal = false"
        @update="updateUserSettings" />

      <add-role-modal v-if="showAddRoleModal" :network="networkState.network.value" :is-submitting="isSubmitting"
        @close="showAddRoleModal = false" @add-role="addRoleToNetwork" />

      <manage-role-modal v-if="showManageRoleModal" :network="networkState.network.value" :selected-role="selectedRole"
        :is-submitting="isSubmitting" :available-permissions="permissionsState.permissions.value"
        :loading-permissions="permissionsState.loading.value" :manage-role-form="manageRoleForm"
        @close="showManageRoleModal = false" @update="updateRolePermissions" />

      <add-access-modal v-if="showAddAccessModal" :network="networkState.network.value" :is-submitting="isSubmitting"
        @close="showAddAccessModal = false" @add-access="addAccessToNetwork" />

      <confirmation-modal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
        :button-text="confirmButtonText" :is-submitting="isSubmitting" @close="showConfirmationModal = false"
        @confirm="confirmAction" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import useNetwork from '@/composables/useNetwork.ts';
import type { NetworkUser, NetworkAccess, Role } from '@/types';

// Components
import LoadingErrorComponent from '@/components/LoadingError.vue';
import NetworkHeader from '@/components/NetworkHeader.vue';
import TabContainer from '@/components/tabs/TabContainer.vue';
import EditNetworkModal from '@/components/modals/EditNetworkModal.vue';
import AddUserModal from '@/components/modals/AddUserModal.vue';
import ManageUserModal from '@/components/modals/ManageUserModal.vue';
import AddRoleModal from '@/components/modals/AddRoleModal.vue';
import ManageRoleModal from '@/components/modals/ManageRoleModal.vue';
import AddAccessModal from '@/components/modals/AddAccessModal.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import api from '@/api/api';
import { useGlobalStore } from '@/stores/global';
import usePermissions from '@/composables/usePermissions';

const globalStore = useGlobalStore();

const router = useRouter();
const route = useRoute();
const networkState = useNetwork();
// const permissionsState = usePermissions();

// Tab state
const activeTab = ref('users');

// Modal states
const showEditNetworkModal = ref(false);
const showAddUserModal = ref(false);
const showManageUserModal = ref(false);
const showAddRoleModal = ref(false);
const showManageRoleModal = ref(false);
const showAddAccessModal = ref(false);
const showConfirmationModal = ref(false);

// User states
const selectedUser = ref<NetworkUser | null>(null);

// Role states
const selectedRole = ref<Role | null>(null);

// Permission states
const permissionsState = usePermissions();

// Confirmation states
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmationAction = ref(() => { });

// Submission state
const isSubmitting = ref(false);

// Form states
type ManageUserForm = {
  roleIds: string[];
  accessStatuses: Record<string, 'pending' | 'accepted'>;
};
const manageUserForm = reactive<ManageUserForm>({
  roleIds: [],
  accessStatuses: {}
});

type ManageRoleForm = {
  permissionIds: string[];
}
const manageRoleForm = reactive<ManageRoleForm>({
  permissionIds: []
});

onMounted(async () => {
  const networkId = route.params.networkId as string;
  await networkState.fetchNetwork(networkId);
});

// Methods
function openManageUserModal(user: NetworkUser) {
  selectedUser.value = user;
  manageUserForm.roleIds = user.networkUserRoles?.map(nur => nur.role.id) || [];
  manageUserForm.accessStatuses = {};

  user.networkUserAccesses?.forEach(nua => {
    manageUserForm.accessStatuses[nua.access.id] = nua.isAccepted ? 'accepted' : 'pending';
  });

  showManageUserModal.value = true;
}

function confirmRemoveUser(user: NetworkUser) {
  confirmationTitle.value = 'Remove User';
  confirmationMessage.value = `Are you sure you want to remove ${user.userProxy.firstName} ${user.userProxy.lastName} from this network?`;
  confirmButtonText.value = 'Remove';
  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/users/${user.id}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
      showConfirmationModal.value = false;
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

  // Fetch all permissions and then set selected ones
  await permissionsState.fetchPermissions();
  manageRoleForm.permissionIds = role.rolePermissions?.map(rp => rp.permission.id) || [];

  showManageRoleModal.value = true;
}

function confirmRemoveRole(role: Role) {
  confirmationTitle.value = 'Remove Role';
  confirmationMessage.value = `Are you sure you want to remove the role "${role.name}" from this network?`;
  confirmButtonText.value = 'Remove';
  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/roles/${role.id}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
      showConfirmationModal.value = false;
    } catch (err) {
      console.error('Error removing role:', err);
    } finally {
      globalStore.stopFetching();
      isSubmitting.value = false;
    }
  };
  showConfirmationModal.value = true;
}

function toggleAccessRequired(networkAccess: NetworkAccess) {
  isSubmitting.value = true;
  globalStore.startFetching();

  api.patch(`/networks/${networkState.network.value!.id}/accesses/${networkAccess.accessId}/`, {
    isRequired: !networkAccess.isRequired
  })
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
}

function confirmRemoveAccess(networkAccess: NetworkAccess) {
  confirmationTitle.value = 'Remove Access Requirement';
  confirmationMessage.value = `Are you sure you want to remove the access requirement "${networkAccess.access.name}" from this network?`;
  confirmButtonText.value = 'Remove';
  confirmationAction.value = async () => {
    globalStore.startFetching();
    isSubmitting.value = true;
    try {
      await api.delete(`/networks/${networkState.network.value!.id}/accesses/${networkAccess.accessId}/`);
      await networkState.fetchNetwork(networkState.network.value!.id);
      showConfirmationModal.value = false;
    } catch (err) {
      console.error('Error removing access requirement:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  };
  showConfirmationModal.value = true;
}

async function updateNetwork(updatedData: { name: string, isSystemProtected: boolean }) {
  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    await api.patch(`/networks/${networkState.network.value!.id}/`, updatedData);
    await networkState.fetchNetwork(networkState.network.value!.id);
    showEditNetworkModal.value = false;
  } catch (err) {
    console.error('Error updating network:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function addUserToNetwork(userData: { userId: string, roleIds: string[] }) {
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
    // Update roles
    await api.patch(`/networks/${networkState.network.value!.id}/users/${selectedUser.value.id}/`, {
      roleIds: localForm.roleIds
    });

    // Update access statuses
    const accessPromises = Object.entries(localForm.accessStatuses).map(([accessId, status]) => {
      return api.patch(`/networks/${networkState.network.value!.id}/users/${selectedUser.value!.id}/accesses/${accessId}/`, {
        isAccepted: status === 'accepted'
      });
    });

    await Promise.all(accessPromises);
    await networkState.fetchNetwork(networkState.network.value!.id);
    showManageUserModal.value = false;

    manageUserForm.roleIds = localForm.roleIds;
    manageUserForm.accessStatuses = localForm.accessStatuses;
  } catch (err) {
    console.error('Error updating user settings:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function addRoleToNetwork(roleData: { roleId: string }) {
  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    await api.post(`/networks/${networkState.network.value!.id}/roles/`, {
      roleId: roleData.roleId
    });
    await networkState.fetchNetwork(networkState.network.value!.id);
    showAddRoleModal.value = false;
  } catch (err) {
    console.error('Error adding role:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function updateRolePermissions(localForm: ManageRoleForm) {
  if (!selectedRole.value) return;

  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    await api.patch(`/roles/${selectedRole.value.id}/`, {
      permissionIds: localForm.permissionIds
    });
    await networkState.fetchNetwork(networkState.network.value!.id);
    showManageRoleModal.value = false;
    manageRoleForm.permissionIds = localForm.permissionIds
  } catch (err) {
    console.error('Error updating role permissions:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

async function addAccessToNetwork(accessData: { accessId: string, isRequired: boolean }) {
  globalStore.startFetching();
  isSubmitting.value = true;
  try {
    await api.post(`/networks/${networkState.network.value!.id}/accesses/`, {
      accessId: accessData.accessId,
      isRequired: accessData.isRequired
    });
    await networkState.fetchNetwork(networkState.network.value!.id);
    showAddAccessModal.value = false;
  } catch (err) {
    console.error('Error adding access requirement:', err);
  } finally {
    isSubmitting.value = false;
    globalStore.stopFetching();
  }
}

function confirmAction() {
  confirmationAction.value();
}
</script>
