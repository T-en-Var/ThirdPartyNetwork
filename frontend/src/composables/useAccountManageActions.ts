import { ref } from 'vue';
import { useRouter } from 'vue-router';

import api from '@/api/api';
import { useAuthStore } from '@/stores/auth';
import { useGlobalStore } from '@/stores/global';
import { useEventStore } from '@/stores/event';
import { DEFAULT_STORES, useHistoryStore } from '@/stores/history';

import useUsers from '@/composables/useUsers';
import useBlogs from '@/composables/account/useBlogs';
import useCustomPages from '@/composables/account/useCustomPages';
import useFiles from '@/composables/account/useFiles';
import useConfigurations from '@/composables/account/useConfigurations';
import type { UseCachedApiReturn, UseMutationReturn } from '@/composables/useApi';

import type {
  ConfirmForm,
  UpdateFile,
  NetworkFile,
  UserProxyCreate,
  UserProxyUpdate,
  CreateBlog,
  Blog,
  CreateConfiguration,
  Configuration,
  CreateCustomPage,
  CustomPage,
  CreatePageBlock,
  PageBlock,
} from '@/types';

/**
 * Encapsulates all of the "manage account" action dispatch logic (blog / configuration /
 * custom page / page block / file mutations, plus user-proxy management) that used to live
 * inline in `ManageAccountView.vue`. The view is left responsible only for template wiring
 * and its own page-lifecycle concerns (fetching the current user/proxy, main network, etc).
 *
 * Because the acting user + user proxy are only known after an async fetch completes, the
 * per-entity `handle` dispatch table is built lazily via `initialize(userId, userProxyId)`
 * rather than being available synchronously (mirroring the previous `buildHandle` pattern).
 */
export default function useAccountManageActions() {
  const router = useRouter();
  const authStore = useAuthStore();
  const globalStore = useGlobalStore();
  const events = useEventStore();
  const history = useHistoryStore(DEFAULT_STORES.account);

  const composables = {
    blogs: useBlogs(),
    customPages: useCustomPages(),
    files: useFiles(),
    configurations: useConfigurations(),
  };

  const isSubmitting = ref(false);

  const showConfirmationModal = ref(false);
  const showAddUserProxyModal = ref(false);
  const showAddFileModal = ref(false);
  const showEditFileModal = ref(false);

  const confirmationTitle = ref('');
  const confirmationMessage = ref('');
  const confirmButtonText = ref('');
  const confirmButtonColor = ref('');
  const confirmationAction = ref<() => Promise<void> | void>(() => {});

  let currentUserId: string | null = null;

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

  async function confirmAction() {
    await confirmationAction.value();
    showConfirmationModal.value = false;
  }

  function handleReturn(section: string) {
    router.push(`/account/${section}`);
  }

  async function createUserProxy(newUserProxy: UserProxyCreate) {
    if (!currentUserId) return;

    const { execute: createUserProxyExecute } = useUsers().createUserProxy;
    isSubmitting.value = true;
    try {
      await createUserProxyExecute(currentUserId, newUserProxy);
      showAddUserProxyModal.value = false;
    } catch (err) {
      console.error('Error adding user proxy:', err);
    } finally {
      isSubmitting.value = false;
    }
  }

  function handleEditProxyClick(id: string) {
    router.push(`/account/proxies/${id}/edit`);
  }

  function handleSwitchProxyClick(id: string) {
    void id;
    alert('not implemented');
  }

  async function handleUpdateProxy(userProxy: UserProxyUpdate) {
    isSubmitting.value = true;
    globalStore.startFetching();
    try {
      await api.put(
        `/users/${authStore.currentUserProxy!.user.id}/proxies/${userProxy.id}`,
        userProxy,
      );
    } catch (err) {
      console.error('Error adding user proxy:', err);
    } finally {
      isSubmitting.value = false;
      globalStore.stopFetching();
    }
  }

  const genericFetch = <T, P extends unknown[]>(
    composable: UseCachedApiReturn<T, [string, ...P, string]>,
    options?: {
      callback?: (result: T, ...args: [string, ...P, string]) => void;
      errorHandler?: (err: unknown) => void;
    },
  ) => {
    const { execute, data } = composable;

    const wrapper = (...args: P) => {
      const fetchFunction = async (networkId: string, entryId: string) => {
        try {
          await execute(networkId, ...args, entryId);

          if (data.value && options?.callback) {
            options.callback(data.value, networkId, ...args, entryId);
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

  const buildHandle = (userId: string, userProxyId: string) => {
    return {
      blog: {
        fetch: genericFetch(composables.blogs.fetchBlog)(userId, userProxyId),
        fetchAll: genericFetchAll(composables.blogs.fetchBlogs)(userId, userProxyId),

        edit: (blog: Blog) => {
          history.visit.blogs(blog);
          router.push(`/networks/${blog.networkId}/manage/blogs/${blog.id}/edit`);
        },

        create: async (networkId: string, payload: CreateBlog) =>
          await genericMutation(composables.blogs.createBlog, {
            callback: (result) => events.emit.blogs.create(result),
          })(networkId, userId, userProxyId, payload),

        delete: (blog: Blog) =>
          confirm({
            title: 'Remove Blog',
            message: `Are you sure you want to remove '${blog.title}'?`,
            buttonText: 'Remove',
            buttonColor: 'red',
            action: async () =>
              await genericMutation(composables.blogs.deleteBlog, {
                callback: () => history.remove.blogs(blog),
              })(blog.networkId, userId, userProxyId, blog.id),
          }),
      },

      configuration: {
        fetch: genericFetch(composables.configurations.fetchConfiguration)(userId, userProxyId),
        fetchAll: genericFetchAll(composables.configurations.fetchConfigurations)(
          userId,
          userProxyId,
        ),

        edit: (configuration: Configuration) => {
          history.visit.configurations(configuration);
          router.push(
            `/networks/${configuration.networkId}/manage/configurations/${configuration.id}/edit`,
          );
        },

        create: async (networkId: string, payload: CreateConfiguration) =>
          await genericMutation(composables.configurations.createConfiguration, {
            callback: (result) => events.emit.configurations.create(result),
          })(networkId, userId, userProxyId, payload),

        update: async (networkId: string, customPageId: string, payload: Configuration) =>
          await genericMutation(composables.configurations.updateConfiguration, {
            callback: (result) => events.emit.configurations.update(result),
          })(networkId, userId, userProxyId, customPageId, payload),

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
              })(configuration.networkId, userId, userProxyId, configuration.id),
          }),
      },

      customPages: {
        fetch: genericFetch(composables.customPages.fetchCustomPage)(userId, userProxyId),
        fetchAll: genericFetchAll(composables.customPages.fetchCustomPages)(userId, userProxyId),

        edit: (customPage: CustomPage) => {
          history.visit.customPages(customPage);
          router.push(
            `/networks/${customPage.networkId}/manage/custom-pages/${customPage.id}/edit`,
          );
        },

        create: async (networkId: string, payload: CreateCustomPage) =>
          await genericMutation(composables.customPages.createCustomPage, {
            callback: (result) => events.emit.customPages.create(result),
          })(networkId, userId, userProxyId, payload),

        update: async (networkId: string, customPageId: string, payload: CreateCustomPage) =>
          await genericMutation(composables.customPages.updateCustomPage, {
            callback: (result) => events.emit.customPages.update(result),
          })(networkId, userId, userProxyId, customPageId, payload),

        delete: (customPage: CustomPage) =>
          confirm({
            title: 'Remove Custom Page',
            message: `Are you sure you want to remove '${customPage.name}'?`,
            buttonText: 'Remove',
            buttonColor: 'red',
            action: async () =>
              await genericMutation(composables.customPages.deleteCustomPage, {
                callback: () => history.remove.customPages(customPage),
              })(customPage.networkId, userId, userProxyId, customPage.id),
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
          })(networkId, userId, userProxyId, customPageId, pageBlock),

        update: async (networkId: string, customPageId: string, pageBlock: PageBlock) =>
          await genericMutation(composables.customPages.updatePageBlock)(
            networkId,
            userId,
            userProxyId,
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
                  cleanUpPageBlockOrphans(pageBlock.networkId, customPageId, pageBlockId);
                },
              })(pageBlock.networkId, userId, userProxyId, pageBlock.customPageId, pageBlock.id),
          }),
      },

      files: {
        fetch: genericFetch(composables.files.fetchFile)(userId, userProxyId),
        fetchAll: genericFetchAll(composables.files.fetchFiles)(userId, userProxyId),

        update: async (id: string, networkId: string, networkFile: UpdateFile) =>
          await genericMutation(composables.files.updateFile, {
            callback: (result) => events.emit.file.update(result),
          })(networkId, userId, userProxyId, id, networkFile),

        delete: (file: NetworkFile) => {
          showEditFileModal.value = false;
          confirm({
            title: 'Delete File',
            message: `Are you sure you want to delete the file '${file.name}'?`,
            buttonText: 'Confirm',
            buttonColor: 'red',
            action: async () =>
              await genericMutation(composables.files.deleteFile)(
                file.networkId,
                userId,
                userProxyId,
                file.id,
              ),
          });
        },

        openEdit: (file: NetworkFile) => events.emit.file.openEdit(file),
      },
    };
  };

  const handle = ref<ReturnType<typeof buildHandle> | null>(null);

  function initialize(userId: string, userProxyId: string) {
    currentUserId = userId;
    handle.value = buildHandle(userId, userProxyId);
  }

  async function cleanUpPageBlockOrphans(
    networkId: string,
    customPageId: string,
    pageBlockId: string,
  ) {
    if (!handle.value) return;

    const customPage = await handle.value.customPages.fetch(networkId, customPageId);
    if (!customPage.value) return;

    customPage.value.pages.forEach((block) => {
      if (block.parentPageId !== pageBlockId) return;

      handle.value!.pageBlocks.update(block.networkId, block.customPageId, {
        ...block,
        parentPageId: undefined,
      });
    });
  }

  return {
    history,
    isSubmitting,

    showConfirmationModal,
    showAddUserProxyModal,
    showAddFileModal,
    showEditFileModal,

    confirmationTitle,
    confirmationMessage,
    confirmButtonText,
    confirmButtonColor,

    handle,
    initialize,

    confirm,
    confirmAction,
    handleReturn,

    createUserProxy,
    handleEditProxyClick,
    handleSwitchProxyClick,
    handleUpdateProxy,
  };
}
