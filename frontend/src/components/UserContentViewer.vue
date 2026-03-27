<template>
  <div
    class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4 w-full">
    <div class="flex items-center gap-4">
      <button @click="showFilters = !showFilters"
        class="px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 border rounded-md text-gray-800 transition-colors">
        {{ showFilters ? 'Hide Filters' : 'Show Filters' }}
      </button>
      <h2 class="text-xl font-semibold text-gray-800">{{ title }}</h2>
    </div>

    <div class="flex flex-col sm:flex-row items-center gap-4 w-full sm:w-auto">
      <div class="relative w-full sm:w-64">
        <input type="text" v-model="filterKey" placeholder="Search entries..."
          class="w-full pl-3 pr-10 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-2 focus:ring-blue-500 text-sm" />
      </div>

      <button @click="$emit('add-new')" v-if="!hideAddNew"
        class="w-full sm:w-auto px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 font-medium">
        Add New
      </button>
    </div>
  </div>

  <div class="flex flex-col gap-6">
    <aside v-if="showFilters"
      class="w-full bg-gray-50 p-5 rounded-xl border border-gray-200 h-fit">
      <div class="mb-2">
        <div class="flex items-center justify-between">
          <span
            class="text-sm font-bold text-gray-500 uppercase tracking-wider">Filters</span>
          <button @click="clearFilters"
            class="text-xs text-blue-600 hover:underline">Reset All</button>
        </div>
      </div>

      <div class="flex flex-row gap-6">
        <div>
          <h3 class="font-semibold text-gray-700 mb-2">Authors</h3>
          <div v-for="author in uniqueAuthors" :key="author.username"
            class="flex items-center gap-2 mb-1">
            <input type="checkbox" :id="`author-${author.username}`"
              v-model="filters.authors" :value="author.username"
              class="rounded text-blue-600" />
            <label :for="`author-${author.username}`"
              class="text-sm text-gray-600 cursor-pointer">
              {{ author.username }}
            </label>
          </div>
        </div>

        <div>
          <h3 class="font-semibold text-gray-700 mb-2">Visibility</h3>
          <div v-for="level in ACCESS_LEVELS" :key="level.value"
            class="flex items-center gap-2 mb-1">
            <input type="checkbox" :id="`access-${level.value}`"
              v-model="filters.accessLevels" :value="level.value"
              class="rounded text-blue-600" />
            <label :for="`access-${level.value}`"
              class="text-sm text-gray-600 cursor-pointer">{{ level.label
              }}</label>
          </div>
        </div>

        <div v-for="col in extraColumns" :key="col.key" class="" v-show="col.filter">
          <h3 class="font-semibold text-gray-700 mb-2">{{ col.label }}</h3>

          <div v-if="col.type === 'string'" class="max-h-40 overflow-y-auto pr-2">
            <div v-for="opt in getUniqueOptions(col.key)" :key="opt"
              class="flex items-center gap-2 mb-1">
              <input type="checkbox" v-model="filters.dynamic[col.key]" :value="opt"
                class="rounded" />
              <label class="text-sm text-gray-600">{{ opt }}</label>
            </div>
          </div>

          <div v-else-if="col.type === 'number'" class="flex flex-col gap-2">
            <input type="number" v-model.number="filters.ranges[col.key].min"
              placeholder="Min" class="w-1/2 p-1 text-sm border rounded" />
            <input type="number" v-model.number="filters.ranges[col.key].max"
              placeholder="Max" class="w-1/2 p-1 text-sm border rounded" />
          </div>

          <div v-else-if="col.type === 'date'" class="flex flex-col gap-2">
            <input type="date" v-model="filters.ranges[col.key].min"
              class="w-full p-1 text-sm border rounded" />
            <input type="date" v-model="filters.ranges[col.key].max"
              class="w-full p-1 text-sm border rounded" />
          </div>
        </div>

        <slot name="extra-filters"></slot>
      </div>
    </aside>

    <div class="flex-1 min-w-0">
      <div class="overflow-x-auto border rounded-lg">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th v-for="col in extraColumns" :key="col.key"
                class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase"
                :class="{ 'w-min': col.type === 'number' }">
                {{ col.label }}
              </th>

              <th v-if="showNetwork"
                class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">
                Network</th>
              <th
                class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">
                Author</th>
              <th
                class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">
                Visibility</th>

              <th
                class="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase">
                Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="entry in filteredEntries" :key="entry.id"
              class="hover:bg-blue-50/50 transition-colors">
              <td v-for="col in extraColumns" :key="col.key"
                class="px-6 py-4 whitespace-nowrap text-sm text-gray-600 truncate max-w-[200px]"
                :class="{ 'text-right': col.type === 'number' }">
                {{ formatValue(getEntryValue(entry, col.key), col.type) }}
              </td>

              <td v-if="showNetwork"
                class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{{
                  networks[entry.networkId]?.name || 'Loading...' }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex flex-row gap-2 items-center">
                  <ProfileAvatar :userProxy="entry.author.userProxy" :size="7" />
                  <span class="text-xs text-gray-500">@{{
                    entry.author.userProxy.username }}</span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 py-1 text-xs rounded-full bg-gray-100 text-gray-700 border">
                  {{ getAccessLevel(entry.accessLevel).label }}
                </span>
              </td>

              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium"
                @click.stop>
                <button @click="$emit('edit', entry)"
                  class="text-blue-600 hover:text-blue-900 mr-4">Edit</button>
                <button @click="$emit('remove', entry)"
                  class="text-red-600 hover:text-red-900">Remove</button>
              </td>
            </tr>
            <tr v-if="filteredEntries.length === 0">
              <td :colspan="extraColumns.length + 4"
                class="px-6 py-12 text-center text-gray-500">
                No results found matching your criteria.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
export interface ExtraColumn {
  key: string;
  label: string;
  type: 'string' | 'number' | 'date';
  filter: boolean;
}

export default {
  name: 'UserContentViewer'
};
</script>

<script setup lang="ts" generic="T extends UserContentBase">
import { ref, computed, reactive, watch } from 'vue';
import { ACCESS_LEVELS, getAccessLevel } from '@/lib/accessLevels';
import type { Network, UserContentBase } from '@/types';
import ProfileAvatar from './ProfileAvatar.vue';
import useNetworks from '@/composables/useNetworks';

const networkState = useNetworks();

const props = defineProps<{
  title: string;
  entries: T[];
  extraColumns: ExtraColumn[];
  showNetwork?: boolean;
  hideAddNew?: boolean;
}>();

defineEmits<{
  (e: 'add-new'): void;
  (e: 'edit', entry: T): void;
  (e: 'remove', entry: T): void;
}>();

const showFilters = ref(false);
const filterKey = ref('');

const networks = ref<Record<string, Network>>({});

watch(
  () => props.entries,
  async (newEntries) => {
    if (!props.showNetwork || !newEntries.length) return;
    const uniqueNetworkIds = [...new Set(newEntries.map(e => e.networkId))];

    await Promise.all(
      uniqueNetworkIds.map(async (id) => {
        if (networks.value[id]) return;

        const network = await getNetwork(id);
        if (network) networks.value[id] = network;
      })
    );
  },
  { immediate: true }
);

const filters = reactive({
  authors: [] as string[],
  accessLevels: [] as number[],
  dynamic: {} as Record<string, string[]>,
  ranges: {} as Record<string, { min: string | number | null; max: string | number | null; }>
});

const initFilters = () => {
  props.extraColumns.forEach(col => {
    if (!col.filter) return;

    if (col.type === 'string') {
      filters.dynamic[col.key] = [];
    } else {
      filters.ranges[col.key] = { min: null, max: null };
    }
  });
};

initFilters();
watch(() => props.extraColumns, initFilters, { deep: true });

/**
 * HELPER: The "No-Any" Indexer
 * We cast the entry to a Record where keys are strings and values are unknown.
 */
const getEntryValue = (entry: T, key: string): unknown => {
  return (entry as Record<string, unknown>)[key];
};

const formatValue = (val: unknown, type: ExtraColumn['type']): string => {
  if (val === null || val === undefined) return '-';

  if (type === 'number') {
    return (val as number).toLocaleString("nl-NL", {
      minimumFractionDigits: 0,
      maximumFractionDigits: 1
    });
  }

  if (type === 'date') {
    const d = new Date(val as string | number | Date);
    return isNaN(d.getTime()) ? '-' : d.toLocaleDateString();
  }

  return String(val);
};

async function getNetwork(networkId: string) {
  const { execute: fetchNetworkDetails, data: network } = networkState.fetchNetworkDetails;
  await fetchNetworkDetails(networkId);
  return network.value;
};

const getUniqueOptions = (key: string): string[] => {
  const options = props.entries.map(e => String(getEntryValue(e, key) ?? ''));
  return [...new Set(options)].filter(Boolean).sort();
};

const clearFilters = () => {
  filterKey.value = '';
  filters.authors = [];
  filters.accessLevels = [];
  initFilters();
};

const uniqueAuthors = computed(() => {
  const seen = new Set<string>();
  return props.entries
    .map(e => e.author.userProxy)
    .filter(u => {
      if (!u.username) return false;
      if (seen.has(u.username)) return false;
      seen.add(u.username);
      return true;
    });
});

const filteredEntries = computed(() => {
  const search = filterKey.value.toLowerCase().trim();

  return props.entries.filter(entry => {
    // 1. Search Logic
    const dynamicValues = props.extraColumns.map(col => String(getEntryValue(entry, col.key) ?? ''));
    const searchFields = [
      entry.author.userProxy.username,
      ...dynamicValues
    ].join(' ').toLowerCase();

    if (search && !searchFields.includes(search)) return false;

    // 2. Standard Filters
    if (filters.authors.length > 0 && entry.author.userProxy.username && !filters.authors.includes(entry.author.userProxy.username)) return false;
    if (filters.accessLevels.length > 0 && !filters.accessLevels.includes(entry.accessLevel)) return false;

    // 3. Dynamic Filters
    for (const col of props.extraColumns) {
      if (!col.filter) continue;
      const val = getEntryValue(entry, col.key);

      if (col.type === 'string') {
        const selected = filters.dynamic[col.key];
        if (selected?.length > 0 && !selected.includes(String(val ?? ''))) return false;
      }
      else if (col.type === 'number' && typeof val === 'number') {
        const { min, max } = filters.ranges[col.key];
        if (min !== null && min !== '' && val < Number(min)) return false;
        if (max !== null && max !== '' && val > Number(max)) return false;
      }
      else if (col.type === 'date' && val) {
        const { min, max } = filters.ranges[col.key];
        const time = new Date(val as string | number | Date).getTime();
        if (min && time < new Date(min as string).getTime()) return false;
        if (max && time > new Date(max as string).getTime()) return false;
      }
    }
    return true;
  });
});
</script>
