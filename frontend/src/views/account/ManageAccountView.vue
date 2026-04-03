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
      <div v-if="authStore.currentUserProxy && handle">
        <RouterView
          v-if="!authStore.loading && !authStore.error && authStore.currentUserProxy"
          :network-ids="network.id"
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
          @create-proxy="showAddUserProxyModal = true"
          @edit-proxy="handleEditProxyClick"
          @switch-proxy="handleSwitchProxyClick"
          @update-user-proxy="handleUpdateProxy"
          @delete-user-proxy="() => {}"
        />
      </div>

      <div v-if="authStore.currentUserProxy && handle">
        <!-- Modals -->
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

        <AddUserProxyModal
          v-if="showAddUserProxyModal && defaultProxy"
          :is-submitting="isSubmitting"
          :default-proxy="defaultProxy"
          @close="showAddUserProxyModal = false"
          @create-proxy="createUserProxy"
        />

        <AddFileModal
          v-if="showAddFileModal"
          :network-ids="userProxy!.networkUsers.map((nu) => nu.network.id)"
          @close="showAddFileModal = false"
          @uploaded="handle.files.openEdit"
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

import type {
  ConfirmForm,
  UpdateFile,
  NetworkFile,
  UserProxy,
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

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';
import AddUserProxyModal from '@/components/modals/account/AddUserProxyModal.vue';
import api from '@/api/api';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import useNetworks from '@/composables/useNetworks';
import useUsers from '@/composables/useUsers';
import type { UseCachedApiReturn, UseMutationReturn } from '@/composables/useApi';
import useBlogs from '@/composables/account/useBlogs';
import useCustomPages from '@/composables/account/useCustomPages';
import useFiles from '@/composables/account/useFiles';
import useConfigurations from '@/composables/account/useConfigurations';
import { useEventStore } from '@/stores/event';
import { DEFAULT_STORES, useHistoryStore } from '@/stores/history';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();
const events = useEventStore();
const history = useHistoryStore(DEFAULT_STORES.account);

const mainNetwork = useNetworks().fetchMainNetwork;

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

  handle.value = buildHandle(user.value.id, userProxy.value.id)
});

const allProxies = computed(() => {
  if (!user.value) return [];
  return user.value.userProxies;
});

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find((x) => x.isDefault)!;
});

async function confirmAction() {
  await confirmationAction.value();
  showConfirmationModal.value = false;
}

async function createUserProxy(newUserProxy: UserProxyCreate) {
  if (!user.value) return;

  const { execute: createUserProxy } = useUsers().createUserProxy;
  isSubmitting.value = true;
  try {
    await createUserProxy(user.value.id, newUserProxy);
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

const composables = {
  networks: useNetworks(),

  blogs: useBlogs(),
  customPages: useCustomPages(),
  files: useFiles(),
  configurations: useConfigurations(),
};

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

function handleReturn(section: string) {
  router.push(`/account/${section}`);
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

const handle = ref<ReturnType<typeof buildHandle> | null>(null);

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
        router.push(`/networks/${configuration.networkId}/manage/configurations/${configuration.id}/edit`);
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
        router.push(`/networks/${customPage.networkId}/manage/custom-pages/${customPage.id}/edit`);
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
        console.log('Yes!');
        showEditFileModal.value = false;
        confirm({
          title: 'Delete File',
          message: `Are you sure you want to delete the file '${file.name}'?`,
          buttonText: 'Confirm',
          buttonColor: 'red',
          action: async () =>
            await genericMutation(composables.files.deleteFile)(file.networkId, userId, userProxyId, file.id),
        });
      },

      openEdit: (file: NetworkFile) => events.emit.file.openEdit(file),
    },
  };
};

async function cleanUpPageBlockOrphans(networkId: string, customPageId: string, pageBlockId: string) {
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
</script>
