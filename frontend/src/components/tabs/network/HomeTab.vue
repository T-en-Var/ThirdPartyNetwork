<template>
  <div class="network-home-tab">
    <div class="network-home-tab__summary-card">
      <div>
        <h1 class="network-home-tab__title">
          {{ network.name }}
          <span
            class="badge"
            :class="network.isSystemProtected ? 'badge--success' : 'badge--neutral'"
          >
            {{ network.isSystemProtected ? 'Protected' : 'Standard' }}
          </span>
        </h1>
      </div>

      <div class="network-home-tab__stats">
        <div class="network-home-tab__stat">
          <h3 class="network-home-tab__stat-label">Users</h3>
          <p class="network-home-tab__stat-value">{{ network.networkUsers.length }}</p>
        </div>
        <div class="network-home-tab__stat">
          <h3 class="network-home-tab__stat-label">Roles</h3>
          <p class="network-home-tab__stat-value">{{ network.roles.length }}</p>
        </div>
        <div class="network-home-tab__stat">
          <h3 class="network-home-tab__stat-label">Access Requirements</h3>
          <p class="network-home-tab__stat-value">{{ network.networkAccesses.length }}</p>
        </div>
      </div>

      <div class="network-home-tab__id-row">
        <span class="network-home-tab__id">
          ID: {{ network.id }}
        </span>
      </div>
    </div>

    <div class="network-home-tab__stats-card">
      <div>
        <h1 class="network-home-tab__title">Statistics</h1>
      </div>

      <div
        v-if="network.entitlement"
        class="network-home-tab__entitlements"
      >
        <div
          class="network-home-tab__entitlement-card"
          :class="{ 'network-home-tab__entitlement-card--locked': !network.entitlement.allowFiles }"
        >
          <div class="network-home-tab__entitlement-header">
            <h3 class="network-home-tab__entitlement-title">Files</h3>
            <span
              v-if="!network.entitlement.allowFiles"
              class="badge badge--neutral"
            >
              Locked
            </span>
          </div>

          <LoadingErrorComponent
            v-if="filesLoading || filesError"
            :loading="filesLoading"
            :error="filesError ?? undefined"
            :has-value="!!files"
          />

          <template v-else-if="network.entitlement.allowFiles">
            <div class="network-home-tab__usage">
              <div>
                <div class="network-home-tab__usage-row">
                  <span class="network-home-tab__usage-label">Usage</span>
                  <span class="network-home-tab__usage-value">
                    {{ fileCount }} / {{ network.entitlement.fileCountLimit }}
                  </span>
                </div>
                <div class="network-home-tab__progress">
                  <div
                    class="network-home-tab__progress-fill"
                    :style="{
                      width:
                        Math.min((fileCount / network.entitlement.fileCountLimit) * 100, 100) +
                        '%',
                    }"
                  ></div>
                </div>
              </div>

              <div class="network-home-tab__storage">
                <span class="network-home-tab__storage-value">
                  {{ (fileStorage / 1024).toFixed(1) }}
                </span>
                <span class="network-home-tab__storage-limit">
                  / {{ network.entitlement.fileStorageLimit }} KB
                </span>
              </div>
            </div>
          </template>

          <div
            v-else
            class="network-home-tab__unavailable"
          >
            <p class="network-home-tab__unavailable-text">Not available for this network</p>
          </div>
        </div>

        <div
          class="network-home-tab__entitlement-card"
          :class="{ 'network-home-tab__entitlement-card--locked': !network.entitlement.allowBlogs }"
        >
          <div class="network-home-tab__entitlement-header">
            <h3 class="network-home-tab__entitlement-title">Blogs</h3>
            <span
              v-if="!network.entitlement.allowBlogs"
              class="badge badge--neutral"
            >
              Locked
            </span>
          </div>

          <LoadingErrorComponent
            v-if="blogsLoading || blogsError"
            :loading="blogsLoading"
            :error="blogsError ?? undefined"
            :has-value="!!blogs"
          />

          <template v-else-if="network.entitlement.allowBlogs">
            <div class="network-home-tab__usage">
              <div>
                <div class="network-home-tab__usage-row">
                  <span class="network-home-tab__usage-label">Usage</span>
                  <span class="network-home-tab__usage-value">
                    {{ blogCount }} / {{ network.entitlement.blogCountLimit }}
                  </span>
                </div>
                <div class="network-home-tab__progress">
                  <div
                    class="network-home-tab__progress-fill"
                    :style="{
                      width:
                        Math.min((blogCount / network.entitlement.blogCountLimit) * 100, 100) +
                        '%',
                    }"
                  ></div>
                </div>
              </div>
            </div>
          </template>

          <div
            v-else
            class="network-home-tab__unavailable"
          >
            <p class="network-home-tab__unavailable-text">Not available for this network</p>
          </div>
        </div>

        <div
          class="network-home-tab__entitlement-card"
          :class="{
            'network-home-tab__entitlement-card--locked': !network.entitlement.allowConfigurations,
          }"
        >
          <div class="network-home-tab__entitlement-header">
            <h3 class="network-home-tab__entitlement-title">Configurations</h3>
            <span
              v-if="!network.entitlement.allowConfigurations"
              class="badge badge--neutral"
            >
              Locked
            </span>
          </div>

          <LoadingErrorComponent
            v-if="configsLoading || configsError"
            :loading="configsLoading"
            :error="configsError ?? undefined"
            :has-value="!!configs"
          />

          <template v-else-if="network.entitlement.allowConfigurations">
            <div class="network-home-tab__usage">
              <div>
                <div class="network-home-tab__usage-row">
                  <span class="network-home-tab__usage-label">Usage</span>
                  <span class="network-home-tab__usage-value">
                    {{ configCount }} / {{ network.entitlement.configurationCountLimit }}
                  </span>
                </div>
                <div class="network-home-tab__progress">
                  <div
                    class="network-home-tab__progress-fill"
                    :style="{
                      width:
                        Math.min(
                          (configCount / network.entitlement.configurationCountLimit) * 100,
                          100,
                        ) + '%',
                    }"
                  ></div>
                </div>
              </div>
            </div>
          </template>

          <div
            v-else
            class="network-home-tab__unavailable"
          >
            <p class="network-home-tab__unavailable-text">Not available for this network</p>
          </div>
        </div>

        <div
          class="network-home-tab__entitlement-card"
          :class="{
            'network-home-tab__entitlement-card--locked': !network.entitlement.allowCustomPages,
          }"
        >
          <div class="network-home-tab__entitlement-header">
            <h3 class="network-home-tab__entitlement-title">Custom Pages</h3>
            <span
              v-if="!network.entitlement.allowCustomPages"
              class="badge badge--neutral"
            >
              Locked
            </span>
          </div>

          <LoadingErrorComponent
            v-if="pagesLoading || pagesError"
            :loading="pagesLoading"
            :error="pagesError ?? undefined"
            :has-value="!!pages"
          />

          <template v-else-if="network.entitlement.allowCustomPages">
            <div class="network-home-tab__usage">
              <div>
                <div class="network-home-tab__usage-row">
                  <span class="network-home-tab__usage-label">Usage</span>
                  <span class="network-home-tab__usage-value">
                    {{ pageCount }} / {{ network.entitlement.customPageCountLimit }}
                  </span>
                </div>
                <div class="network-home-tab__progress">
                  <div
                    class="network-home-tab__progress-fill"
                    :style="{
                      width:
                        Math.min(
                          (pageCount / network.entitlement.customPageCountLimit) * 100,
                          100,
                        ) + '%',
                    }"
                  ></div>
                </div>
              </div>
            </div>
          </template>

          <div
            v-else
            class="network-home-tab__unavailable"
          >
            <p class="network-home-tab__unavailable-text">Not available for this network</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import type { Network } from '@/types';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';

import useFiles from '@/composables/network/useFiles';
import useBlogs from '@/composables/network/useBlogs';
import useConfigurations from '@/composables/network/useConfigurations';
import useCustomPages from '@/composables/network/useCustomPages';

const props = defineProps<{
  network: Network;
}>();

// --- Files Setup ---
const { fetchFiles } = useFiles();
const { data: files, loading: filesLoading, error: filesError, execute: loadFiles } = fetchFiles;

const fileCount = computed(() => files.value?.length ?? 0);
const fileStorage = computed(
  () => files.value?.reduce((acc, file) => acc + (file.sizeBytes || 0), 0) ?? 0,
);

// --- Blogs Setup ---
const {
  data: blogs,
  loading: blogsLoading,
  error: blogsError,
  execute: loadBlogs,
} = useBlogs().fetchBlogs;

const blogCount = computed(() => blogs.value?.length ?? 0);

// --- Configurations Setup ---
const { fetchConfigurations } = useConfigurations();
const {
  data: configs,
  loading: configsLoading,
  error: configsError,
  execute: loadConfigs,
} = fetchConfigurations;

const configCount = computed(() => configs.value?.length ?? 0);

// --- Custom Pages Setup ---
const {
  data: pages,
  loading: pagesLoading,
  error: pagesError,
  execute: loadPages,
} = useCustomPages().fetchCustomPages;

const pageCount = computed(() => pages.value?.length ?? 0);

onMounted(async () => {
  const { id, entitlement } = props.network;
  if (!entitlement) return;

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
