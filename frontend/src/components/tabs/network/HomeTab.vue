<template>
  <div class="space-y-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-800">
          {{ network.name }}
          <span class="ml-2 px-3 py-1 text-xs font-medium rounded-full"
            :class="network.isSystemProtected ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'">
            {{ network.isSystemProtected ? 'Protected' : 'Standard' }}
          </span>
        </h1>
      </div>

      <div class="p-6 pb-0">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div class="bg-gray-50 p-4 rounded-lg">
            <h3 class="text-sm font-medium text-gray-500 uppercase">Users</h3>
            <p class="mt-2 text-3xl font-semibold">{{ network.networkUsers.length }}
            </p>
          </div>
          <div class="bg-gray-50 p-4 rounded-lg">
            <h3 class="text-sm font-medium text-gray-500 uppercase">Roles</h3>
            <p class="mt-2 text-3xl font-semibold">{{ network.roles.length }}</p>
          </div>
          <div class="bg-gray-50 p-4 rounded-lg">
            <h3 class="text-sm font-medium text-gray-500 uppercase">Access
              Requirements</h3>
            <p class="mt-2 text-3xl font-semibold">{{ network.networkAccesses.length
            }}</p>
          </div>
        </div>
      </div>

      <div class="p-6 pt-0">
        <span
          class="bg-gray-50 px-2 py-1 text-gray-500 normal-case font-medium black rounded-lg font-mono">
          ID: {{ network.id }}
        </span>
      </div>
    </div>

    <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-800">Statistics</h1>
      </div>

      <div v-if="network.entitlement" class="p-6 pb-0">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">

          <div
            class="relative overflow-hidden border border-gray-100 p-5 rounded-xl transition-all duration-200"
            :class="{
              'bg-white shadow-sm hover:shadow-lg hover:-translate-y-1 cursor-pointer border-blue-100': network.entitlement.allowFiles,
              'bg-gray-50 opacity-75 cursor-not-allowed': !network.entitlement.allowFiles
            }"
            @click="network.entitlement.allowFiles && router.push('manage/files')">

            <div class="flex items-center justify-between mb-4">
              <div class="flex items-center gap-3">
                <h3 class="font-bold text-gray-700 tracking-tight">Files</h3>
              </div>
              <span v-if="!network.entitlement.allowFiles"
                class="px-2 py-1 text-[10px] font-bold uppercase tracking-wider text-gray-400 bg-gray-200 rounded-full">
                Locked
              </span>
            </div>

            <LoadingErrorComponent v-if="filesLoading || filesError"
              :loading="filesLoading" :error="filesError ?? undefined"
              :has-value="!!files" />

            <template v-else-if="network.entitlement.allowFiles">
              <div class="space-y-4">
                <div>
                  <div class="flex justify-between text-xs mb-1">
                    <span class="text-gray-500">Usage</span>
                    <span class="font-medium text-gray-900">
                      {{ fileCount }} / {{ network.entitlement.fileCountLimit }}
                    </span>
                  </div>
                  <div class="w-full bg-gray-100 rounded-full h-1.5">
                    <div class="bg-blue-500 h-1.5 rounded-full"
                      :style="{ width: Math.min((fileCount / network.entitlement.fileCountLimit) * 100, 100) + '%' }">
                    </div>
                  </div>
                </div>

                <div class="flex items-baseline gap-1">
                  <span class="text-2xl font-bold text-gray-900">
                    {{ (fileStorage / 1024).toFixed(1) }}
                  </span>
                  <span class="text-sm text-gray-500">
                    / {{ network.entitlement.fileStorageLimit }} KB
                  </span>
                </div>
              </div>
            </template>

            <div v-else class="py-4">
              <p class="text-sm text-gray-500 italic">Not available for this network
              </p>
            </div>
          </div>

          <div
            class="relative overflow-hidden border border-gray-100 p-5 rounded-xl transition-all duration-200"
            :class="{
              'bg-white shadow-sm hover:shadow-lg hover:-translate-y-1 cursor-pointer border-blue-100': network.entitlement.allowBlogs,
              'bg-gray-50 opacity-75 cursor-not-allowed': !network.entitlement.allowBlogs
            }"
            @click="network.entitlement.allowBlogs && router.push('manage/blogs')">

            <div class="flex items-center justify-between mb-4">
              <div class="flex items-center gap-3">
                <h3 class="font-bold text-gray-700 tracking-tight">Blogs</h3>
              </div>
              <span v-if="!network.entitlement.allowBlogs"
                class="px-2 py-1 text-[10px] font-bold uppercase tracking-wider text-gray-400 bg-gray-200 rounded-full">
                Locked
              </span>
            </div>

            <LoadingErrorComponent v-if="blogsLoading || blogsError"
              :loading="blogsLoading" :error="blogsError ?? undefined"
              :has-value="!!blogs" />

            <template v-else-if="network.entitlement.allowBlogs">
              <div class="space-y-4">
                <div>
                  <div class="flex justify-between text-xs mb-1">
                    <span class="text-gray-500">Usage</span>
                    <span class="font-medium text-gray-900">
                      {{ blogCount }} / {{ network.entitlement.blogCountLimit }}
                    </span>
                  </div>
                  <div class="w-full bg-gray-100 rounded-full h-1.5">
                    <div class="bg-blue-500 h-1.5 rounded-full"
                      :style="{ width: Math.min((blogCount / network.entitlement.blogCountLimit) * 100, 100) + '%' }">
                    </div>
                  </div>
                </div>
              </div>
            </template>

            <div v-else class="py-4">
              <p class="text-sm text-gray-500 italic">Not available for this network
              </p>
            </div>
          </div>

          <div
            class="relative overflow-hidden border border-gray-100 p-5 rounded-xl transition-all duration-200"
            :class="{
              'bg-white shadow-sm hover:shadow-lg hover:-translate-y-1 cursor-pointer border-blue-100': network.entitlement.allowConfigurations,
              'bg-gray-50 opacity-75 cursor-not-allowed': !network.entitlement.allowConfigurations
            }"
            @click="network.entitlement.allowConfigurations && router.push('manage/configurations')">

            <div class="flex items-center justify-between mb-4">
              <div class="flex items-center gap-3">
                <h3 class="font-bold text-gray-700 tracking-tight">Configurations
                </h3>
              </div>
              <span v-if="!network.entitlement.allowConfigurations"
                class="px-2 py-1 text-[10px] font-bold uppercase tracking-wider text-gray-400 bg-gray-200 rounded-full">
                Locked
              </span>
            </div>

            <LoadingErrorComponent v-if="configsLoading || configsError"
              :loading="configsLoading" :error="configsError ?? undefined"
              :has-value="!!configs" />

            <template v-else-if="network.entitlement.allowConfigurations">
              <div class="space-y-4">
                <div>
                  <div class="flex justify-between text-xs mb-1">
                    <span class="text-gray-500">Usage</span>
                    <span class="font-medium text-gray-900">
                      {{ configCount }} / {{
                        network.entitlement.configurationCountLimit }}
                    </span>
                  </div>
                  <div class="w-full bg-gray-100 rounded-full h-1.5">
                    <div class="bg-blue-500 h-1.5 rounded-full"
                      :style="{ width: Math.min((configCount / network.entitlement.configurationCountLimit) * 100, 100) + '%' }">
                    </div>
                  </div>
                </div>
              </div>
            </template>

            <div v-else class="py-4">
              <p class="text-sm text-gray-500 italic">Not available for this network
              </p>
            </div>
          </div>

          <div
            class="relative overflow-hidden border border-gray-100 p-5 rounded-xl transition-all duration-200"
            :class="{
              'bg-white shadow-sm hover:shadow-lg hover:-translate-y-1 cursor-pointer border-blue-100': network.entitlement.allowCustomPages,
              'bg-gray-50 opacity-75 cursor-not-allowed': !network.entitlement.allowCustomPages
            }"
            @click="network.entitlement.allowCustomPages && router.push('manage/custom-pages')">

            <div class="flex items-center justify-between mb-4">
              <div class="flex items-center gap-3">
                <h3 class="font-bold text-gray-700 tracking-tight">Custom Pages</h3>
              </div>
              <span v-if="!network.entitlement.allowCustomPages"
                class="px-2 py-1 text-[10px] font-bold uppercase tracking-wider text-gray-400 bg-gray-200 rounded-full">
                Locked
              </span>
            </div>

            <LoadingErrorComponent v-if="pagesLoading || pagesError"
              :loading="pagesLoading" :error="pagesError ?? undefined"
              :has-value="!!pages" />

            <template v-else-if="network.entitlement.allowCustomPages">
              <div class="space-y-4">
                <div>
                  <div class="flex justify-between text-xs mb-1">
                    <span class="text-gray-500">Usage</span>
                    <span class="font-medium text-gray-900">
                      {{ pageCount }} / {{ network.entitlement.customPageCountLimit
                      }}
                    </span>
                  </div>
                  <div class="w-full bg-gray-100 rounded-full h-1.5">
                    <div class="bg-blue-500 h-1.5 rounded-full"
                      :style="{ width: Math.min((pageCount / network.entitlement.customPageCountLimit) * 100, 100) + '%' }">
                    </div>
                  </div>
                </div>
              </div>
            </template>

            <div v-else class="py-4">
              <p class="text-sm text-gray-500 italic">Not available for this network
              </p>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import type { Network } from '@/types';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';

// Import our new composables
import useFiles from '@/composables/useFiles';
import useBlogs from '@/composables/useBlogs';
import useConfigurations from '@/composables/useConfigurations';
import useCustomPages from '@/composables/useCustomPages';

const props = defineProps<{
  network: Network;
}>();

const router = useRouter();

// --- 1. Files Setup ---
const { fetchNetworkFiles } = useFiles();
const {
  data: files,
  loading: filesLoading,
  error: filesError,
  execute: loadFiles
} = fetchNetworkFiles;

const fileCount = computed(() => files.value?.length ?? 0);
const fileStorage = computed(() =>
  files.value?.reduce((acc, file) => acc + (file.sizeBytes || 0), 0) ?? 0
);

// --- 2. Blogs Setup ---
const { data: blogs,
  loading: blogsLoading,
  error: blogsError,
  execute: loadBlogs
} = useBlogs().fetchBlogs;

const blogCount = computed(() => blogs.value?.length ?? 0);

// --- 3. Configurations Setup ---
const { fetchNetworkConfigurations } = useConfigurations();
const {
  data: configs,
  loading: configsLoading,
  error: configsError,
  execute: loadConfigs
} = fetchNetworkConfigurations;

const configCount = computed(() => configs.value?.length ?? 0);

// --- 4. Custom Pages Setup ---
const { data: pages,
  loading: pagesLoading,
  error: pagesError,
  execute: loadPages } = useCustomPages().fetchCustomPages;

const pageCount = computed(() => pages.value?.length ?? 0);

// --- Lifecycle ---
onMounted(async () => {
  const { id, entitlement } = props.network;
  if (!entitlement) return;

  // Fire requests in parallel (background) if allowed
  if (entitlement.allowFiles) loadFiles(id);
  if (entitlement.allowBlogs) loadBlogs(id);
  if (entitlement.allowConfigurations) loadConfigs(id);
  if (entitlement.allowCustomPages) loadPages(id);
});

defineEmits<{
  (e: 'navigateBack'): void;
  (e: 'editNetwork'): void;
}>();
</script>