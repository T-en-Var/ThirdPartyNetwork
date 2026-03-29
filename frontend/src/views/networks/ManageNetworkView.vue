<template>
  <div class="flex h-full pt-16">
    <NetworkSidebar />

    <div class="flex-1 overflow-auto p-6">
      <LoadingErrorComponent
        :loading="composables.networks.fetchNetwork.loading.value"
        :error="composables.networks.fetchNetwork.error.value"
        :has-value="!!network"
        button-value="Return to networks"
        @button-action="router.push(`/networks`)"
      />

      <RouterView
        v-if="
          !composables.networks.fetchNetwork.loading.value &&
          !composables.networks.fetchNetwork.error.value &&
          network
        "
        :network="network"
        :network-id="network.id"
        @confirm="confirm"
        @add-user="showAddUserModal = true"
        @manage-user="openManageUserModal"
        @remove-user="handle.users.delete"
        @add-role="showAddRoleModal = true"
        @manage-role="openEditRoleModal"
        @remove-role="handle.roles.delete"
        @add-access="showAddAccessModal = true"
        @toggle-access-required="handle.accesses.toggle"
        @remove-access="handle.accesses.delete"
        @edit-page-block="handle.pageBlocks.edit"
        @create-page-block="handleShowCreatePageBlockModal"
        @update-page-block="handle.pageBlocks.update"
        @update-network="handle.network.update"
        @delete-network="handle.network.delete"
        :fetch-blogs="handle.blog.fetch"
        @blog-edit="handle.blog.edit"
        @blog-create="handle.blog.create"
        @blog-delete="handle.blog.delete"
        :fetch-configurations="handle.configuration.fetch"
        @configuration-edit="handle.configuration.edit"
        @configuration-create="handle.configuration.create"
        @configuration-delete="handle.configuration.delete"
        :fetch-customPages="handle.customPages.fetch"
        @custom-pages-edit="handle.customPages.edit"
        @custom-pages-create="handle.customPages.create"
        @custom-pages-delete="handle.customPages.delete"
        :fetch-files="handle.files.fetch"
        @add-file="showAddFileModal = true"
        @file-update="handle.files.update"
        @file-delete="handle.files.delete"
      />

      <div v-if="network">
        <EditUserModal
          v-if="showManageUserModal && selectedUser"
          :network="network"
          :selected-user="selectedUser"
          :is-submitting="isSubmitting"
          :manage-user-form="manageUserForm"
          @close="showManageUserModal = false"
          @update="updateUserSettings"
        />

        <AddRoleModal
          v-if="showAddRoleModal"
          :network="network"
          :is-submitting="isSubmitting"
          @close="showAddRoleModal = false"
          @add-role="addRoleToNetwork"
        />

        <EditRoleModal
          v-if="showEditRoleModal && selectedRole"
          :network="network"
          :selected-role="selectedRole"
          :manage-role-form="manageRoleForm"
          @close="showEditRoleModal = false"
          @update="updateRolePermissions"
        />

        <AddAccessModal
          v-if="showAddAccessModal"
          :network="network"
          :is-submitting="isSubmitting"
          @close="showAddAccessModal = false"
          @add-access="handle.accesses.create"
        />

        <ConfirmationModal
          v-if="showConfirmationModal"
          :title="confirmationTitle"
          :message="confirmationMessage"
          :button-text="confirmButtonText"
          :color="confirmButtonColor"
          :is-submitting="isSubmitting"
          @close="showConfirmationModal = false"
          @confirm="confirmAction"
        />

        <AddFileModal
          v-if="showAddFileModal"
          media-type="any"
          :network-id="network.id"
          @close="showAddFileModal = false"
          @uploaded="handle.files.openEdit"
        />

        <AddPageBlockModal
          v-if="showCreatePageBlockModal && createPageBlockCustomPage"
          :custom-page="createPageBlockCustomPage"
          :is-submitting="isSubmitting"
          @create-page-block="handle.pageBlocks.create"
          @close="handleCloseCreatePageBlockModal"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, watch, type Ref, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import usePermissions from '@/composables/usePermissions';

// Components
import NetworkSidebar from '@/components/sidebar/NetworkSidebar.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import EditUserModal from '@/components/modals/network/EditUserModal.vue';
import AddRoleModal from '@/components/modals/network/AddRoleModal.vue';
import EditRoleModal from '@/components/modals/network/EditRoleModal.vue';
import AddAccessModal from '@/components/modals/network/AddAccessModal.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import type {
  NetworkUser,
  Role,
  NetworkAccess,
  NetworkFile,
  PageBlock,
  CreatePageBlock,
  NetworkAccessCreate,
  Network,
  NetworkUpdate,
  UpdateNetworkUser,
  CustomPage,
  CreateBlog,
  Blog,
  CreateConfiguration,
  Configuration,
  CreateCustomPage,
} from '@/types';
import type { RoleForm, ManageUserForm, UpdateFile, ConfirmForm } from '@/types/forms';

import useFiles from '@/composables/network/useFiles';
import AddPageBlockModal from '@/components/modals/usercontent/AddPageBlockModal.vue';
import useCustomPages from '@/composables/network/useCustomPages';
import useRoles from '@/composables/useRoles';
import useNetworks from '@/composables/useNetworks';
import useNetworkUsers from '@/composables/useNetworkUsers';
import useAccesses from '@/composables/useAccesses';
import useBlogs from '@/composables/network/useBlogs';
import type { UseCachedApiReturn, UseMutationReturn } from '@/composables/useApi';
import useConfigurations from '@/composables/network/useConfigurations';
import { useEventStore } from '@/stores/event';
import { getNameDisplayUserProxy } from '@/lib/user';
import { useHistoryStore } from '@/stores/history';

const router = useRouter();
const route = useRoute();

const events = useEventStore();

const composables = {
  networks: useNetworks(),

  accesses: useAccesses(),
  networkUsers: useNetworkUsers(),
  permissions: usePermissions(),
  roles: useRoles(),

  blogs: useBlogs(),
  customPages: useCustomPages(),
  files: useFiles(),
  configurations: useConfigurations(),
};

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
const isSubmitting = ref(false);

const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => {});

const manageUserForm = reactive<ManageUserForm>({ roleIds: [], entitlements: {} });
const manageRoleForm = reactive<RoleForm>({
  name: '',
  description: '',
  permissionIds: [],
  isDefault: false,
  entitlements: {},
});

const originalFavicon = ref<string | null>(null);
const networkId = computed(() => route.params.networkId as string);
const network = computed(() => composables.networks.fetchNetwork.data.value);

const history = useHistoryStore(networkId.value);

onMounted(async () => {
  const link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
  if (link) {
    originalFavicon.value = link.href;
  }

  const { execute: fetchNetwork } = composables.networks.fetchNetwork;

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

watch(
  () => composables.networks.fetchNetwork.data.value?.imageFile?.url,
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

// Methods
function confirm(form: ConfirmForm) {
  confirmationTitle.value = form.title;
  confirmationMessage.value = form.message;
  confirmButtonText.value = form.buttonText;
  confirmButtonColor.value = form.buttonColor;

  confirmationAction.value = async () => {
    await form.action();
  };

  showConfirmationModal.value = true;
}

const genericFetch = <T, P extends unknown[]>(
  composable: UseCachedApiReturn<T, P>,
  options?: {
    callback?: (result: T, ...args: P) => void;
    errorHandler?: (err: unknown) => void;
  },
) => {
  const { execute, data } = composable;

  const wrapper = (...args: P) => {
    const fetchFunction = async () => {
      try {
        await execute(...args);

        if (data.value && options?.callback) {
          options.callback(data.value, ...args);
        }
      } catch (err) {
        options?.errorHandler?.(err);
        console.error(`Error executing fetch:`, err);
      }

      return data;
    };

    return fetchFunction;
  };

  return wrapper;
};

const genericMutation = <T, P extends unknown[]>(
  composable: UseMutationReturn<T, P>,
  options?: {
    callback?: (result: T, ...args: P) => void;
    errorHandler?: (err: unknown) => void;
  },
) => {
  const { execute } = composable;

  const wrapper = async (...args: P) => {
    isSubmitting.value = true;

    try {
      const result: T = await execute(...args);

      if (options?.callback) {
        options.callback(result, ...args);
      }
    } catch (err) {
      if (options?.errorHandler) {
        options.errorHandler(err);
        return;
      }
      console.error(`Error executing mutation:`, err);
    } finally {
      isSubmitting.value = false;
    }
  };

  return wrapper;
};

const handle = {
  network: {
    update: async (networkId: string, networkUpdate: NetworkUpdate) =>
      await genericMutation(composables.networks.updateNetwork)(networkId, networkUpdate),

    delete: (network: Network) =>
      confirm({
        title: 'Delete network',
        message: `Are you sure you want to delete ${network.name} permanently?`,
        buttonText: 'Proceed',
        buttonColor: 'red',
        action: async () =>
          confirm({
            title: 'Delete network',
            message: `Are you REALLY SURE you want to delete ${network.name} PERMANENTLY!?`,
            buttonText: 'DO IT',
            buttonColor: 'red',
            action: async () =>
              await genericMutation(composables.networks.deleteNetwork)(network.id),
          }),
      }),
  },

  blog: {
    fetch: genericFetch(composables.blogs.fetchBlogs)(networkId.value),

    create: async (networkId: string, payload: CreateBlog) =>
      await genericMutation(composables.blogs.createBlog, {
        callback: (result) => events.emit.blogs.create(result),
      })(networkId, payload),

    edit: (blog: Blog) => router.push(`/networks/${networkId.value}/manage/blogs/${blog.id}/edit`),

    delete: (blog: Blog) =>
      confirm({
        title: 'Remove Blog',
        message: `Are you sure you want to remove '${blog.title}' from this network?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.blogs.deleteBlog, {
            callback: () => history.remove.blogs(blog),
          })(networkId.value, blog.id),
      }),
  },

  configuration: {
    fetch: genericFetch(composables.configurations.fetchConfigurations)(networkId.value),

    create: async (networkId: string, payload: CreateConfiguration) =>
      await genericMutation(composables.configurations.createConfiguration, {
        callback: (result) => events.emit.configurations.create(result),
      })(networkId, payload),

    edit: (configuration: Configuration) =>
      router.push(`/networks/${networkId.value}/manage/configurations/${configuration.id}/edit`),

    delete: (configuration: Configuration) =>
      confirm({
        title: 'Remove Configuration',
        message: `Are you sure you want to remove '${configuration.key}' from this network?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.configurations.deleteConfiguration, {
            callback: () => history.remove.configurations(configuration),
          })(networkId.value, configuration.id),
      }),
  },

  customPages: {
    fetch: genericFetch(composables.customPages.fetchCustomPages)(networkId.value),

    create: async (networkId: string, payload: CreateCustomPage) =>
      await genericMutation(composables.customPages.createCustomPage, {
        callback: (result) => events.emit.customPages.create(result),
      })(networkId, payload),

    edit: (customPage: CustomPage) =>
      router.push(`/networks/${networkId.value}/manage/custom-pages/${customPage.id}/edit`),

    delete: (customPage: CustomPage) =>
      confirm({
        title: 'Remove CustomPage',
        message: `Are you sure you want to remove '${customPage.name}' from this network?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.customPages.deleteCustomPage, {
            callback: () => history.remove.customPages(customPage),
          })(networkId.value, customPage.id),
      }),
  },

  files: {
    fetch: genericFetch(composables.files.fetchFiles)(networkId.value),

    update: async (id: string, networkId: string, networkFile: UpdateFile) =>
      await genericMutation(composables.files.updateFile, {
        callback: (result) => events.emit.file.update(result),
      })(networkId, id, networkFile),

    delete: (file: NetworkFile) => {
      console.log('Yes!');
      showEditFileModal.value = false;
      confirm({
        title: 'Delete File',
        message: `Are you sure you want to delete the file '${file.name}'?`,
        buttonText: 'Confirm',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.files.deleteFile)(networkId.value, file.id),
      });
    },

    openEdit: (file: NetworkFile) => events.emit.file.openEdit(file),
  },

  users: {
    delete: (networkUser: NetworkUser) =>
      confirm({
        title: 'Remove User',
        message: `Are you sure you want to remove '${getNameDisplayUserProxy(networkUser.userProxy)}' from this network?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.networkUsers.deleteNetworkUser)(
            networkId.value,
            networkUser.id,
          ),
      }),
  },

  accesses: {
    create: async (accessData: NetworkAccessCreate) =>
      accessData.access &&
      (await genericMutation(composables.accesses.createAccess)(
        networkId.value,
        accessData.access.id,
        accessData.isRequired,
      )),

    toggle: (networkAccess: NetworkAccess) =>
      confirm({
        title: 'Toggle Access Requirement',
        message: `Are you sure you want to make the Access '${networkAccess.access.name}' ${networkAccess.isRequired ? 'optional' : 'required'}?`,
        buttonText: 'Confirm',
        buttonColor: 'green',
        action: async () =>
          await genericMutation(composables.accesses.updateNetworkAccess)(
            networkId.value,
            networkAccess.accessId,
            !networkAccess.isRequired,
          ),
      }),

    delete: (networkAccess: NetworkAccess) =>
      confirm({
        title: 'Remove Access Requirement',
        message: `Are you sure you want to remove the access requirement "${networkAccess.access.name}" from this network?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.accesses.deleteNetworkAccess)(
            networkId.value,
            networkAccess.accessId,
          ),
      }),
  },

  roles: {
    delete: (role: Role) =>
      confirm({
        title: 'Remove Role',
        message: `Are you sure you want to remove the role "${role.name}" from this network?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.roles.deleteRole)(networkId.value, role.id),
      }),
  },

  pageBlocks: {
    create: async (customPageId: string, pageBlock: CreatePageBlock) =>
      await genericMutation(composables.customPages.createPageBlock)(
        networkId.value,
        customPageId,
        pageBlock,
      ),

    edit: (customPage: CustomPage, pageBlock: PageBlock) =>
      router.push(
        `/networks/${networkId.value}/manage/custom-pages/${customPage.id}/blocks/${pageBlock.id}/edit`,
      ),

    update: async (customPageId: string, pageBlock: PageBlock) =>
      await genericMutation(composables.customPages.updatePageBlock)(
        networkId.value,
        customPageId,
        pageBlock.id,
        pageBlock,
      ),
  },
};

function openManageUserModal(user: NetworkUser) {
  selectedUser.value = user;
  manageUserForm.roleIds = user.networkUserRoles?.map((nur) => nur.role.id) || [];
  manageUserForm.entitlements = user.entitlements;

  showManageUserModal.value = true;
}

async function openEditRoleModal(role: Role) {
  selectedRole.value = role;

  const { execute: fetchPermissions } = composables.permissions.fetchPermissions;

  await fetchPermissions();
  manageRoleForm.name = role.name;
  manageRoleForm.description = role.description;
  manageRoleForm.isDefault = role.isDefault;
  manageRoleForm.permissionIds = role.rolePermissions?.map((rp) => rp.permission.id) || [];

  showEditRoleModal.value = true;
}

async function updateUserSettings(localForm: ManageUserForm) {
  if (!selectedUser.value) return;

  const addedRoles = localForm.roleIds.filter((roleId) => !manageUserForm.roleIds.includes(roleId));
  const removedRoles = manageUserForm.roleIds.filter(
    (roleId) => !localForm.roleIds.includes(roleId),
  );

  const { execute: updateNetworkUser } = composables.networkUsers.updateNetworkUser;

  const payload: UpdateNetworkUser = {
    entitlements: localForm.entitlements,
  };

  try {
    await updateNetworkUser(
      networkId.value,
      selectedUser.value.id,
      payload,
      addedRoles,
      removedRoles,
    );

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
  const { execute: createRole } = composables.roles.createRole;

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

  const { execute: updateRole } = composables.roles.updateRole;
  const payload = {
    name: localForm.name.trim(),
    description: localForm.description.trim(),
    isDefault: localForm.isDefault,
    entitlements: localForm.entitlements,
  };

  try {
    const roleId = selectedRole.value!.id;

    const addedPerms = localForm.permissionIds.filter(
      (permId) => !manageRoleForm.permissionIds.includes(permId),
    );
    const removedPerms = manageRoleForm.permissionIds.filter(
      (permId) => !localForm.permissionIds.includes(permId),
    );

    await updateRole(networkId.value, roleId, payload, addedPerms, removedPerms);

    showEditRoleModal.value = false;
  } catch (err) {
    console.error('Error updating role permissions:', err);
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

function confirmAction() {
  showConfirmationModal.value = false;
  confirmationAction.value();
}
</script>
