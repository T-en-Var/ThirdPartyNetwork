<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <div class="flex flex-row justify-between items-start gap-4 w-full">
        <div class="flex justify-end gap-6">
          <button @click="showFilters = !showFilters"
            class="w-64 py-2 text-sm bg-gray-200 hover:bg-gray-300 rounded-md text-gray-800">
            {{ showFilters ? 'Hide Filters' : 'Show Filters' }}
          </button>
          <h2 class="text-xl font-medium text-gray-800">Configurations</h2>
        </div>

        <div
          class="flex flex-col sm:flex-row sm:items-center gap-4 w-full sm:w-auto">
          <div class="relative w-full sm:w-64">
            <input type="text" v-model="filterKey" placeholder="Filter configs..."
              class="w-full pl-3 pr-10 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm" />
          </div>

          <button @click="$emit('openCreateConfigurationModal')"
            class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
            Add Configuration
          </button>
        </div>
      </div>

      <div class="flex flex-col xl:flex-row gap-6">
        <aside v-if="showFilters"
          class="flex flex-row flex-wrap xl:flex-col items-start gap-4 w-full xl:w-64 xl:min-w-64 bg-white border rounded-lg p-4 shadow-sm">
          <div>
            <h3 class="font-semibold text-gray-700">Author</h3>
            <div v-for="author in uniqueAuthors" :key="author.username"
              class="flex items-center gap-2">
              <input type="checkbox" :id="`author-${author.username}`"
                v-model="filters.authors" :value="author.username"
                class="accent-blue-600" />
              <label :for="`author-${author.username}`"
                class="text-sm text-gray-800">
                {{ author.firstName }} {{ author.lastName }} ({{ author.username }})
              </label>
            </div>
          </div>

          <div>
            <h3 class="font-semibold text-gray-700">Access Level</h3>
            <div v-for="level in ACCESS_LEVELS" :key="level.value"
              class="flex items-center gap-2">
              <input type="checkbox" :id="`access-${level.value}`"
                v-model="filters.accessLevels" :value="level.value"
                class="accent-blue-600" />
              <label :for="`access-${level.value}`" class="text-sm text-gray-800">
                {{ level.label }}
              </label>
            </div>
          </div>

          <button @click="clearFilters"
            class="w-auto xl:w-full text-sm px-3 py-2 bg-gray-100 hover:bg-gray-200 rounded text-gray-800 border border-gray-300">
            Clear Filters
          </button>
        </aside>

        <div class="flex-1">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th
                    class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Key</th>
                  <th
                    class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Value</th>
                  <th
                    class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Access</th>
                  <th
                    class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Author</th>
                  <th
                    class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="cfg in filteredConfigs" :key="cfg.id" :title="cfg.key"
                  class="hover:bg-slate-100 cursor-pointer"
                  @click.prevent.stop="$emit('editConfiguration', cfg)">
                  <td
                    class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-[200px]">
                    {{ cfg.key }}</td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 truncate max-w-[300px]">
                    {{ previewValue(cfg.value) }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{
                    getAccessLevel(cfg.accessLevel).label }}</td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 truncate max-w-[250px]">
                    {{ cfg.author.userProxy.firstName }} {{
                      cfg.author.userProxy.lastName }} <span class="text-gray-400">({{
                      cfg.author.userProxy.username }})</span></td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button @click.prevent.stop="$emit('editConfiguration', cfg)"
                      class="text-blue-600 hover:text-blue-900 mr-3">Edit</button>
                    <button @click.prevent.stop="$emit('removeConfiguration', cfg)"
                      class="text-red-600 hover:text-red-900">Remove</button>
                  </td>
                </tr>

                <tr v-if="!filteredConfigs.length">
                  <td colspan="5" class="px-6 py-10 text-center text-gray-500">No
                    configurations found.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import useConfigurations from '@/composables/useConfigurations';
import { ACCESS_LEVELS, getAccessLevel } from '@/lib/accessLevels';
import type { Configuration } from '@/types';
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';

const showFilters = ref(false);
const filterKey = ref('');

const route = useRoute();
const configsState = useConfigurations();

const filters = ref({ authors: [] as string[], accessLevels: [] as number[] });

onMounted(() => {
  const networkId = route.params.networkId as string;
  configsState.fetchNetworkConfigurations(networkId);
});

const clearFilters = () => {
  filters.value = { authors: [], accessLevels: [] };
  filterKey.value = '';
};

const uniqueAuthors = computed(() => {
  const seen = new Map<string, { username: string; firstName: string; lastName: string; }>();
  configsState.configurations.value.forEach(p => {
    const u = p.author.userProxy;
    if (u && !seen.has(u.username ?? 'Unknown')) {
      seen.set(u.username ?? 'Unknown', { username: u.username ?? 'Unknown', firstName: u.firstName ?? 'Unknown', lastName: u.lastName ?? 'Unknown' });
    }
  });
  return Array.from(seen.values());
});

const filteredConfigs = computed(() => {
  const query = filterKey.value.trim().toLowerCase();
  return configsState.configurations.value.filter(cfg => {
    const key = cfg.key.toLowerCase();
    const author = `${cfg.author?.userProxy?.username ?? ''} ${cfg.author?.userProxy?.firstName ?? ''} ${cfg.author?.userProxy?.lastName ?? ''}`.toLowerCase();
    const value = JSON.stringify(cfg.value).toLowerCase();

    const matchesSearch = query === '' || key.includes(query) || author.includes(query) || value.includes(query);
    const matchesAuthor = filters.value.authors.length === 0 || filters.value.authors.includes(cfg.author?.userProxy?.username ?? '');
    const matchesAccessLevel = filters.value.accessLevels.length === 0 || filters.value.accessLevels.includes(cfg.accessLevel);

    return matchesSearch && matchesAuthor && matchesAccessLevel;
  });
});

function previewValue(v: object | string | number | boolean | null): string {
  try {
    if (typeof v === 'string') return v;
    const s = JSON.stringify(v);
    if (s.length > 80) return s.slice(0, 77) + '...';
    return s;
  } catch {
    return '';
  }
}

defineEmits<{
  (e: 'openCreateConfigurationModal'): void;
  (e: 'editConfiguration', cfg: Configuration): void;
  (e: 'removeConfiguration', cfg: Configuration): void;
}>();
</script>
