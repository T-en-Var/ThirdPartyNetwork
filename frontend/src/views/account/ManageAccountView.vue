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
      <div v-if="authStore.currentUserProxy && !isLoading && !!userId && !!userProxyId">
        <RouterView
          v-if="!authStore.loading && !authStore.error && authStore.currentUserProxy"
          @create-proxy="showAddUserProxyModal = true"
          @edit-proxy="handleEditProxyClick"
          @switch-proxy="handleSwitchProxyClick"
          @update-user-proxy="handleUpdateProxy"
          @delete-user-proxy="() => {}"
          @confirm="confirm"
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
      </div>

      <div v-if="authStore.currentUserProxy">
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

// Components
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
const isLoading = ref(true);

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

  initHandle();

  isLoading.value = false;
  console.log(userId.value, userProxyId.value);
});

const userId = computed(() => (user.value ? user.value.id : null));
const userProxyId = computed(() => (userProxy.value ? userProxy.value.id : null));

const allProxies = computed(() => {
  if (!user.value) return [];
  return user.value.userProxies;
});

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find((x) => x.isDefault)!;
});

// Methods
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

function initHandle() {
  console.log(userId.value, userProxyId.value);
  if (!userId.value || !userProxyId.value) return;
  handle.blog.fetch = genericFetch(composables.blogs.fetchBlogs)(userId.value, userProxyId.value);
  handle.configuration.fetch = genericFetch(composables.configurations.fetchConfigurations)(
    userId.value,
    userProxyId.value,
  );
  handle.customPages.fetch = genericFetch(composables.customPages.fetchCustomPages)(
    userId.value,
    userProxyId.value,
  );
  handle.files.fetch = genericFetch(composables.files.fetchFiles)(userId.value, userProxyId.value);
}

const handle = {
  blog: {
    fetch: () => {
      console.error('function was not initialized');
    },

    create: async (networkId: string, payload: CreateBlog) =>
      await genericMutation(composables.blogs.createBlog, {
        callback: (result) => events.emit.blogs.create(result),
      })(networkId, userId.value!, userProxyId.value!, payload),

    edit: (blog: Blog) => {
      history.visit.blogs(blog);
      router.push(`/account/networks/${blog.networkId}/blogs/${blog.id}/edit`);
    },

    delete: (blog: Blog) =>
      confirm({
        title: 'Remove Blog',
        message: `Are you sure you want to delete '${blog.title}'?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.blogs.deleteBlog, {
            callback: () => history.remove.blogs(blog),
          })(blog.networkId, userId.value!, userProxyId.value!, blog.id),
      }),
  },

  configuration: {
    fetch: () => {
      console.error('function was not initialized');
    },

    create: async (networkId: string, payload: CreateConfiguration) =>
      await genericMutation(composables.configurations.createConfiguration, {
        callback: (result) => events.emit.configurations.create(result),
      })(networkId, userId.value!, userProxyId.value!, payload),

    edit: (configuration: Configuration) => {
      history.visit.configurations(configuration);
      router.push(
        `/account/networks/${configuration.networkId}/configurations/${configuration.id}/edit`,
      );
    },

    delete: (configuration: Configuration) =>
      confirm({
        title: 'Remove Configuration',
        message: `Are you sure you want to delete '${configuration.key}'?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.configurations.deleteConfiguration, {
            callback: () => history.remove.configurations(configuration),
          })(configuration.networkId, userId.value!, userProxyId.value!, configuration.id),
      }),
  },

  customPages: {
    fetch: () => {
      console.error('function was not initialized');
    },

    create: async (networkId: string, payload: CreateCustomPage) =>
      await genericMutation(composables.customPages.createCustomPage, {
        callback: (result) => events.emit.customPages.create(result),
      })(networkId, userId.value!, userProxyId.value!, payload),

    edit: (customPage: CustomPage) => {
      history.visit.customPages(customPage);
      router.push(`/account/networks/${customPage.networkId}/custom-pages/${customPage.id}/edit`);
    },

    delete: (customPage: CustomPage) =>
      confirm({
        title: 'Remove CustomPage',
        message: `Are you sure you want to delete '${customPage.name}'?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () =>
          await genericMutation(composables.customPages.deleteCustomPage, {
            callback: () => history.remove.customPages(customPage),
          })(customPage.networkId, userId.value!, userProxyId.value!, customPage.id),
      }),
  },

  files: {
    fetch: () => {
      console.error('function was not initialized');
    },

    update: async (id: string, networkId: string, networkFile: UpdateFile) =>
      await genericMutation(composables.files.updateFile, {
        callback: (result) => events.emit.file.update(result),
      })(networkId, userId.value!, userProxyId.value!, id, networkFile),

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
            userId.value!,
            userProxyId.value!,
            file.id,
          ),
      });
    },

    openEdit: (file: NetworkFile) => events.emit.file.openEdit(file),
  },

  pageBlocks: {
    create: async (networkId: string, customPageId: string, pageBlock: CreatePageBlock) =>
      await genericMutation(composables.customPages.createPageBlock)(
        networkId,
        userId.value!,
        userProxyId.value!,
        customPageId,
        pageBlock,
      ),

    edit: (customPage: CustomPage, pageBlock: PageBlock) =>
      router.push(
        `/account/networks/${customPage.networkId}/custom-pages/${customPage.id}/blocks/${pageBlock.id}/edit`,
      ),

    update: async (customPageId: string, pageBlock: PageBlock) =>
      await genericMutation(composables.customPages.updatePageBlock)(
        pageBlock.networkId,
        userId.value!,
        userProxyId.value!,
        customPageId,
        pageBlock.id,
        pageBlock,
      ),
  },
};
</script>
