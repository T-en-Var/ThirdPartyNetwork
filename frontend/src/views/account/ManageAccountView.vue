<template>
  <div class="flex h-full pt-16">
    <!-- Sidebar -->
    <AccountSidebar />

    <!-- Main Content -->
    <div class="flex-1 p-6 overflow-auto">
      <!-- Loading and Error States -->
      <LoadingErrorComponent :loading="authStore.loading" :error="authStore.error"
        button-value="Reload page" @button-action="router.go(0)"
        :has-value="!!authStore.currentUserProxy" />

      <!-- Route View (Page Content) -->
      <div v-if="authStore.currentUserProxy">
        <RouterView
          v-if="!authStore.loading && !authStore.error && authStore.currentUserProxy"
          @create-proxy="showAddUserProxyModal = true"
          @edit-proxy="handleEditProxyClick" 
          @switch-proxy="handleSwitchProxyClick"
          @update-user-proxy="handleUpdateProxy" 
          @delete-user-proxy="() => { }"
          @confirm="confirm" 
          @blog-edit="handle.blog.edit"
          @blog-create="handle.blog.create" 
          @blog-delete="handle.blog.delete"
          @configuration-edit="handle.configuration.edit"
          @configuration-create="handle.configuration.create"
          @configuration-delete="handle.configuration.delete"
          @custom-pages-edit="handle.customPages.edit"
          @custom-pages-create="handle.customPages.create"
          @custom-pages-delete="handle.customPages.delete"
          @add-file="showAddFileModal = true" 
          @file-update="handle.files.update"
          @file-delete="handle.files.delete" />
      </div>

      <div v-if="authStore.currentUserProxy">
        <!-- Modals -->
        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle"
          :message="confirmationMessage" :button-text="confirmButtonText"
          :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />

        <AddUserProxyModal v-if="showAddUserProxyModal && defaultProxy"
          :is-submitting="isSubmitting" :default-proxy="defaultProxy"
          @close="showAddUserProxyModal = false" @create-proxy="createUserProxy" />

        <AddFileModal v-if="showAddFileModal"
          :network-ids="userProxy!.networkUsers.map(nu => nu.network.id)"
          @close="showAddFileModal = false" @uploaded="handle.files.openEdit" />
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

import type { ConfirmForm, UpdateFile, NetworkFile, UserProxy, UserProxyCreate, UserProxyUpdate, CreateBlog, Blog, CreateConfiguration, Configuration, CreateCustomPage, CustomPage, CreatePageBlock, PageBlock } from '@/types';

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';
import AddUserProxyModal from '@/components/modals/account/AddUserProxyModal.vue';
import api from '@/api/api';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import useNetworks from '@/composables/useNetworks';
import useUsers from '@/composables/useUsers';
import type { UseMutationReturn } from '@/composables/useApi';
import useBlogs from '@/composables/network/useBlogs';
import useCustomPages from '@/composables/network/useCustomPages';
import useFiles from '@/composables/network/useFiles';
import useConfigurations from '@/composables/network/useConfigurations';
import { useEventStore } from '@/stores/event';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();
const events = useEventStore();

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
const confirmationAction = ref<() => Promise<void> | void>(() => { });
const userProxy = ref<UserProxy | null>(null);
const { execute: fetchUser, data: user } = useUsers().fetchUser;

onMounted(async () => {
  globalStore.startFetching();
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error("UserProxy not found");

  await fetchUser(currentUser.user.id);
  if (!user.value) throw new Error("User not found");

  userProxy.value = user.value.userProxies.find(up => up.id === currentUser.id) ?? null;
  if (!userProxy.value) throw new Error("UserProxy not found");

  await mainNetwork.execute();
  globalStore.stopFetching();
});

const allProxies = computed(() => {
  if (!user.value) return [];
  return user.value.userProxies;
});

const defaultProxy = computed(() => {
  if (!authStore.currentUserProxy) return null;
  return allProxies.value.find(x => x.isDefault)!;
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
  alert("not implemented");
}

async function handleUpdateProxy(userProxy: UserProxyUpdate) {
  isSubmitting.value = true;
  globalStore.startFetching();
  try {
    await api.put(`/users/${authStore.currentUserProxy!.user.id}/proxies/${userProxy.id}`, userProxy);
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
  configurations: useConfigurations()
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

const genericMutation = <T, P extends unknown[]>(
  composable: UseMutationReturn<T, P>,
  options?: {
    callback?: (result: T, ...args: P) => void;
    errorHandler?: (err: unknown) => void;
  }
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
  blog: {
    create: async (networkId: string, payload: CreateBlog) => await genericMutation(composables.blogs.createBlog, {
      callback: (result) => events.emit.blogs.create(result),
    })(networkId, payload),

    edit: (blog: Blog) => router.push(`/account/blogs/${blog.id}/edit`),

    delete: (blog: Blog) =>
      confirm({
        title: 'Remove Blog',
        message: `Are you sure you want to delete '${blog.title}'?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () => await genericMutation(composables.blogs.deleteBlog)(blog.networkId, blog.id),
      })
  },

  configuration: {
    create: async (networkId: string, payload: CreateConfiguration) => await genericMutation(composables.configurations.createConfiguration, {
      callback: (result) => events.emit.configurations.create(result),
    })(networkId, payload),

    edit: (configuration: Configuration) => router.push(`/account/configurations/${configuration.id}/edit`),

    delete: (configuration: Configuration) =>
      confirm({
        title: 'Remove Configuration',
        message: `Are you sure you want to delete '${configuration.key}'?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () => await genericMutation(composables.configurations.deleteConfiguration)(configuration.networkId, configuration.id),
      })
  },

  customPages: {
    create: async (networkId: string, payload: CreateCustomPage) => await genericMutation(composables.customPages.createCustomPage, {
      callback: (result) => events.emit.customPages.create(result),
    })(networkId, payload),

    edit: (customPage: CustomPage) => router.push(`/account/custom-pages/${customPage.id}/edit`),

    delete: (customPage: CustomPage) =>
      confirm({
        title: 'Remove CustomPage',
        message: `Are you sure you want to delete '${customPage.name}'?`,
        buttonText: 'Remove',
        buttonColor: 'red',
        action: async () => await genericMutation(composables.customPages.deleteCustomPage)(customPage.networkId, customPage.id),
      })
  },

  files: {
    update: async (id: string, networkId: string, networkFile: UpdateFile) =>
      await genericMutation(composables.files.updateFile)(networkId, id, networkFile),

    delete: (file: NetworkFile) => {
      showEditFileModal.value = false;
      confirm({
        title: 'Delete File',
        message: `Are you sure you want to delete the file '${file.name}'?`,
        buttonText: 'Confirm',
        buttonColor: 'red',
        action: async () => await genericMutation(composables.files.deleteFile)(file.networkId, file.id),
      });
    },

    openEdit: (file: NetworkFile) => events.emit.file.openEdit(file),
  },

  pageBlocks: {
    create: async (networkId: string, customPageId: string, pageBlock: CreatePageBlock) => await genericMutation(composables.customPages.createPageBlock)(networkId, customPageId, pageBlock),

    edit: (customPage: CustomPage, pageBlock: PageBlock) => router.push(`/account/custom-pages/${customPage.id}/blocks/${pageBlock.id}/edit`),

    update: async (customPageId: string, pageBlock: PageBlock) => await genericMutation(composables.customPages.updatePageBlock)(pageBlock.networkId, customPageId, pageBlock.id, pageBlock),
  }
};
</script>
