import { ref, reactive, type ComputedRef, type Ref } from 'vue';
import { useRouter } from 'vue-router';

import usePermissions from '@/composables/usePermissions';
import useFiles from '@/composables/network/useFiles';
import useCustomPages from '@/composables/network/useCustomPages';
import useRoles from '@/composables/useRoles';
import useNetworks from '@/composables/useNetworks';
import useNetworkUsers from '@/composables/useNetworkUsers';
import useAccesses from '@/composables/useAccesses';
import useBlogs from '@/composables/network/useBlogs';
import useConfigurations from '@/composables/network/useConfigurations';
import type { UseCachedApiReturn, UseMutationReturn } from '@/composables/useApi';
import { useEventStore } from '@/stores/event';
import { getNameDisplayUserProxy } from '@/lib/user';
import { useHistoryStore } from '@/stores/history';

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

/**
 * Encapsulates all of the "manage network" action dispatch logic (blog / configuration /
 * custom page / page block / file / user / role / access mutations) that used to live inline
 * in `ManageNetworkView.vue`. The view is left responsible only for template wiring and its
 * own page-lifecycle concerns (fetching the network itself, favicon swapping, etc).
 */
export default function useNetworkManageActions(networkId: ComputedRef<string>) {
  const router = useRouter();
  const events = useEventStore();
  const history = useHistoryStore(networkId.value);

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

  const showModal = {
    manageUser: ref(false),
    addUser: ref(false),
    addRole: ref(false),
    editRole: ref(false),
    addAccess: ref(false),
    confirmation: ref(false),
    addFile: ref(false),
    editFile: ref(false),
  };

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

  function confirm(form: ConfirmForm) {
    confirmationTitle.value = form.title;
    confirmationMessage.value = form.message;
    confirmButtonText.value = form.buttonText;
    confirmButtonColor.value = form.buttonColor;

    confirmationAction.value = async () => {
      await form.action();
    };

    showModal.confirmation.value = true;
  }

  function handleReturn(section: string) {
    router.push(`/networks/${networkId.value}/manage/${section}`);
  }

  const genericFetch = <T, P extends unknown[]>(
    composable: UseCachedApiReturn<T, [...P, string]>,
    options?: {
      callback?: (result: T, ...args: [...P, string]) => void;
      errorHandler?: (err: unknown) => void;
    },
  ) => {
    const { execute, data } = composable;

    const wrapper = (...args: P) => {
      const fetchFunction = async (entryId: string) => {
        try {
          await execute(...args, entryId);

          if (data.value && options?.callback) {
            options.callback(data.value, ...args, entryId);
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

  const genericFetchAll = <T, P extends unknown[]>(
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
      fetch: genericFetch(composables.blogs.fetchBlog)(networkId.value),
      fetchAll: genericFetchAll(composables.blogs.fetchBlogs)(networkId.value),

      edit: (blog: Blog) => {
        history.visit.blogs(blog);
        router.push(`/networks/${networkId.value}/manage/blogs/${blog.id}/edit`);
      },

      create: async (networkId: string, payload: CreateBlog) =>
        await genericMutation(composables.blogs.createBlog, {
          callback: (result) => events.emit.blogs.create(result),
        })(networkId, payload),

      delete: (blog: Blog) =>
        confirm({
          title: 'Remove Blog',
          message: `Are you sure you want to remove '${blog.title}'?`,
          buttonText: 'Remove',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.blogs.deleteBlog, {
              callback: () => history.remove.blogs(blog),
            })(networkId.value, blog.id),
        }),
    },

    configuration: {
      fetch: genericFetch(composables.configurations.fetchConfiguration)(networkId.value),
      fetchAll: genericFetchAll(composables.configurations.fetchConfigurations)(networkId.value),

      edit: (configuration: Configuration) => {
        history.visit.configurations(configuration);
        router.push(
          `/networks/${networkId.value}/manage/configurations/${configuration.id}/edit`,
        );
      },

      create: async (networkId: string, payload: CreateConfiguration) =>
        await genericMutation(composables.configurations.createConfiguration, {
          callback: (result) => events.emit.configurations.create(result),
        })(networkId, payload),

      update: async (networkId: string, customPageId: string, payload: Configuration) =>
        await genericMutation(composables.configurations.updateConfiguration, {
          callback: (result) => events.emit.configurations.update(result),
        })(networkId, customPageId, payload),

      delete: (configuration: Configuration) =>
        confirm({
          title: 'Remove Configuration',
          message: `Are you sure you want to remove '${configuration.key}'?`,
          buttonText: 'Remove',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.configurations.deleteConfiguration, {
              callback: () => {
                history.remove.configurations(configuration);
                handleReturn('configurations');
              },
            })(networkId.value, configuration.id),
        }),
    },

    customPages: {
      fetch: genericFetch(composables.customPages.fetchCustomPage)(networkId.value),
      fetchAll: genericFetchAll(composables.customPages.fetchCustomPages)(networkId.value),

      edit: (customPage: CustomPage) => {
        history.visit.customPages(customPage);
        router.push(`/networks/${networkId.value}/manage/custom-pages/${customPage.id}/edit`);
      },

      create: async (networkId: string, payload: CreateCustomPage) =>
        await genericMutation(composables.customPages.createCustomPage, {
          callback: (result) => events.emit.customPages.create(result),
        })(networkId, payload),

      update: async (networkId: string, customPageId: string, payload: CreateCustomPage) =>
        await genericMutation(composables.customPages.updateCustomPage, {
          callback: (result) => events.emit.customPages.update(result),
        })(networkId, customPageId, payload),

      delete: (customPage: CustomPage) =>
        confirm({
          title: 'Remove Custom Page',
          message: `Are you sure you want to remove '${customPage.name}'?`,
          buttonText: 'Remove',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.customPages.deleteCustomPage, {
              callback: () => history.remove.customPages(customPage),
            })(networkId.value, customPage.id),
        }),

      nav: (networkId: string, customPageId: string) =>
        router.push(`/networks/${networkId}/manage/custom-pages/${customPageId}/edit`),
    },

    pageBlocks: {
      edit: (pageBlock: PageBlock) => {
        history.visit.pageBlocks(pageBlock);
        router.push(
          `/networks/${pageBlock.networkId}/manage/custom-pages/${pageBlock.customPageId}/blocks/${pageBlock.id}/edit`,
        );
      },

      create: async (networkId: string, customPageId: string, pageBlock: CreatePageBlock) =>
        await genericMutation(composables.customPages.createPageBlock, {
          callback: (result) => events.emit.pageBlocks.create(result),
        })(networkId, customPageId, pageBlock),

      update: async (networkId: string, customPageId: string, pageBlock: PageBlock) =>
        await genericMutation(composables.customPages.updatePageBlock)(
          networkId,
          customPageId,
          pageBlock.id,
          pageBlock,
        ),

      delete: (pageBlock: PageBlock) =>
        confirm({
          title: 'Remove Page Block',
          message: `Are you sure you want to remove '${pageBlock.text}'?`,
          buttonText: 'Remove',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.customPages.deletePageBlock, {
              callback: (_, __, customPageId, pageBlockId) => {
                history.remove.pageBlocks(pageBlock);
                cleanUpPageBlockOrphans(customPageId, pageBlockId);
              },
            })(pageBlock.networkId, pageBlock.customPageId, pageBlock.id),
        }),
    },

    files: {
      fetch: genericFetch(composables.files.fetchFile)(networkId.value),
      fetchAll: genericFetchAll(composables.files.fetchFiles)(networkId.value),

      update: async (id: string, networkId: string, networkFile: UpdateFile) =>
        await genericMutation(composables.files.updateFile, {
          callback: (result) => events.emit.file.update(result),
        })(networkId, id, networkFile),

      delete: (file: NetworkFile) => {
        showModal.editFile.value = false;
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
      create: async (payload: { userId: string; roleIds: string[] }) =>
        await genericMutation(composables.networkUsers.createNetworkUser, {
          callback: () => {
            showModal.addUser.value = false;
          },
        })(networkId.value, payload.userId, payload.roleIds),

      delete: (networkUser: NetworkUser) =>
        confirm({
          title: 'Remove User',
          message: `Are you sure you want to remove '${getNameDisplayUserProxy(networkUser.userProxy)}'?`,
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
              networkAccess.access.id,
              !networkAccess.isRequired,
            ),
        }),

      delete: (networkAccess: NetworkAccess) =>
        confirm({
          title: 'Remove Access Requirement',
          message: `Are you sure you want to remove the access requirement "${networkAccess.access.name}"?`,
          buttonText: 'Remove',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.accesses.deleteNetworkAccess)(
              networkId.value,
              networkAccess.access.id,
            ),
        }),
    },

    roles: {
      delete: (role: Role) =>
        confirm({
          title: 'Remove Role',
          message: `Are you sure you want to remove the role "${role.name}"?`,
          buttonText: 'Remove',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.roles.deleteRole)(networkId.value, role.id),
        }),
    },
  };

  function openManageUserModal(user: NetworkUser) {
    selectedUser.value = user;
    manageUserForm.roleIds = user.networkUserRoles?.map((nur) => nur.role.id) || [];
    manageUserForm.entitlements = user.entitlements;

    showModal.manageUser.value = true;
  }

  async function openEditRoleModal(role: Role) {
    selectedRole.value = role;

    const { execute: fetchPermissions } = composables.permissions.fetchPermissions;

    await fetchPermissions();
    manageRoleForm.name = role.name;
    manageRoleForm.description = role.description;
    manageRoleForm.isDefault = role.isDefault;
    manageRoleForm.permissionIds = role.rolePermissions?.map((rp) => rp.permission.id) || [];

    showModal.editRole.value = true;
  }

  async function updateUserSettings(localForm: ManageUserForm) {
    if (!selectedUser.value) return;
    isSubmitting.value = true;

    const addedRoles = localForm.roleIds.filter(
      (roleId) => !manageUserForm.roleIds.includes(roleId),
    );
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

      showModal.manageUser.value = false;
    } catch (err) {
      console.error('Error updating user settings:', err);
    } finally {
      isSubmitting.value = false;
    }
  }

  async function addRoleToNetwork(localForm: Ref<RoleForm>) {
    const { execute: createRole } = composables.roles.createRole;
    isSubmitting.value = true;

    const payload = {
      name: localForm.value.name.trim(),
      description: localForm.value.description.trim(),
      isDefault: localForm.value.isDefault,
    };

    try {
      await createRole(networkId.value, payload, localForm.value.permissionIds);

      showModal.addRole.value = false;
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

      showModal.editRole.value = false;
    } catch (err) {
      console.error('Error updating role permissions:', err);
    } finally {
      isSubmitting.value = false;
    }
  }

  async function cleanUpPageBlockOrphans(customPageId: string, pageBlockId: string) {
    const customPage = await handle.customPages.fetch(customPageId);
    if (!customPage.value) return;

    customPage.value.pages.forEach((block) => {
      if (block.parentPageId !== pageBlockId) return;

      handle.pageBlocks.update(block.networkId, block.customPageId, {
        ...block,
        parentPageId: undefined,
      });
    });
  }

  function confirmAction() {
    showModal.confirmation.value = false;
    confirmationAction.value();
  }

  return {
    fetchNetwork: composables.networks.fetchNetwork,

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
  };
}
