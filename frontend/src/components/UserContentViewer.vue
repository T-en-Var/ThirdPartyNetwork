<template>
  <div class="user-content-viewer__toolbar">
    <div class="user-content-viewer__toolbar-left">
      <button
        @click="showFilters = !showFilters"
        class="btn btn--outline btn--sm"
      >
        {{ showFilters ? 'Hide Filters' : 'Show Filters' }}
      </button>
      <h2 class="user-content-viewer__title">{{ title }}</h2>
    </div>

    <div class="user-content-viewer__toolbar-right">
      <div class="user-content-viewer__search">
        <input
          type="text"
          v-model="filterKey"
          placeholder="Search entries..."
          class="form-input"
        />
      </div>

      <button
        @click="$emit('add-new')"
        v-if="!hideAddNew"
        class="btn btn--primary"
      >
        Add New
      </button>
    </div>
  </div>

  <div class="user-content-viewer__content">
    <aside
      v-if="showFilters"
      class="user-content-viewer__filters"
    >
      <div class="user-content-viewer__filters-header">
        <span class="user-content-viewer__filters-title">Filters</span>
        <button
          @click="clearFilters"
          class="user-content-viewer__filters-reset"
        >
          Reset All
        </button>
      </div>

      <div class="user-content-viewer__filter-groups">
        <div>
          <h3 class="user-content-viewer__filter-group-title">Authors</h3>
          <div
            v-for="author in uniqueAuthors"
            :key="author.username"
            class="user-content-viewer__filter-checkbox-row"
          >
            <input
              type="checkbox"
              :id="`author-${author.username}`"
              v-model="filters.authors"
              :value="author.username"
              class="checkbox"
            />
            <label
              :for="`author-${author.username}`"
              class="user-content-viewer__filter-checkbox-label"
            >
              {{ author.username }}
            </label>
          </div>
        </div>

        <div>
          <h3 class="user-content-viewer__filter-group-title">Visibility</h3>
          <div
            v-for="level in VISIBILITY_FILTERS"
            :key="level.value"
            class="user-content-viewer__filter-checkbox-row"
          >
            <input
              type="checkbox"
              :id="`visibility-${level.value}`"
              v-model="filters.visibility"
              :value="level.value"
              class="checkbox"
            />
            <label
              :for="`visibility-${level.value}`"
              class="user-content-viewer__filter-checkbox-label"
              >{{ level.label }}</label
            >
          </div>
        </div>

        <div
          v-for="col in extraColumns"
          :key="col.key"
          v-show="col.filter"
        >
          <h3 class="user-content-viewer__filter-group-title">{{ col.label }}</h3>

          <div
            v-if="col.type === 'string'"
            class="user-content-viewer__filter-options"
          >
            <div
              v-for="opt in getUniqueOptions(col.key)"
              :key="opt"
              class="user-content-viewer__filter-checkbox-row"
            >
              <input
                type="checkbox"
                v-model="filters.dynamic[col.key]"
                :value="opt"
                class="checkbox"
              />
              <label class="user-content-viewer__filter-checkbox-label">{{ opt }}</label>
            </div>
          </div>

          <div
            v-else-if="col.type === 'number'"
            class="user-content-viewer__filter-range"
          >
            <input
              type="number"
              v-model.number="filters.ranges[col.key].min"
              placeholder="Min"
              class="form-input"
            />
            <input
              type="number"
              v-model.number="filters.ranges[col.key].max"
              placeholder="Max"
              class="form-input"
            />
          </div>

          <div
            v-else-if="col.type === 'date'"
            class="user-content-viewer__filter-range"
          >
            <input
              type="date"
              v-model="filters.ranges[col.key].min"
              class="form-input"
            />
            <input
              type="date"
              v-model="filters.ranges[col.key].max"
              class="form-input"
            />
          </div>
        </div>

        <slot name="extra-filters"></slot>
      </div>
    </aside>

    <div class="user-content-viewer__table-col">
      <div class="user-content-viewer__table-wrap">
        <table class="table">
          <thead class="table__head">
            <tr>
              <th
                v-for="col in extraColumns"
                :key="col.key"
                class="table__cell"
                :class="{ 'user-content-viewer__cell--numeric': col.type === 'number' }"
              >
                {{ col.label }}
              </th>

              <th
                v-if="showNetwork"
                class="table__cell"
              >
                Network
              </th>
              <th class="table__cell">Author</th>
              <th class="table__cell">Visibility</th>

              <th class="table__cell user-content-viewer__cell--numeric">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="entry in filteredEntries"
              :key="entry.id"
              class="table__row"
            >
              <td
                v-for="col in extraColumns"
                :key="col.key"
                class="table__cell"
                :class="{ 'user-content-viewer__cell--numeric': col.type === 'number' }"
              >
                {{ formatValue(getEntryValue(entry, col.key), col.type) }}
              </td>

              <td
                v-if="showNetwork"
                class="table__cell"
              >
                {{ networks[entry.networkId]?.name || 'Loading...' }}
              </td>
              <td class="table__cell">
                <div
                  v-if="entry.authorUserProxy"
                  class="user-content-viewer__author-cell"
                >
                  <ProfileAvatar
                    :userProxy="entry.authorUserProxy"
                    :size="28"
                  />
                  <span class="user-content-viewer__author-name"
                    >@{{ entry.authorUserProxy.username }}</span
                  >
                </div>
                <span v-else>-</span>
              </td>
              <td class="table__cell">
                <span :class="['badge', entry.publishedAt ? 'badge--success' : 'badge--neutral']">
                  {{ entry.publishedAt ? 'Published' : 'Draft' }}
                </span>
              </td>

              <td
                class="table__cell user-content-viewer__cell--numeric"
                @click.stop
              >
                <button
                  @click="$emit('edit', entry)"
                  class="user-content-viewer__row-action"
                >
                  Edit
                </button>
                <button
                  @click="$emit('remove', entry)"
                  class="user-content-viewer__row-action user-content-viewer__row-action--danger"
                >
                  Remove
                </button>
              </td>
            </tr>
            <tr v-if="filteredEntries.length === 0">
              <td
                :colspan="extraColumns.length + 4"
                class="user-content-viewer__empty-row"
              >
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
  name: 'UserContentViewer',
};
</script>

<script setup lang="ts" generic="T extends UserContentBaseDto">
import { ref, computed, reactive, watch } from 'vue';
import type { NetworkDto, UserContentBaseDto } from '@/types';
import ProfileAvatar from './ProfileAvatar.vue';
import useNetworks from '@/composables/useNetworks';

const VISIBILITY_FILTERS = [
  { value: 'published', label: 'Published' },
  { value: 'draft', label: 'Draft' },
] as const;

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

const networks = ref<Record<string, NetworkDto>>({});

watch(
  () => props.entries,
  async (newEntries) => {
    if (!props.showNetwork || !newEntries.length) return;
    const uniqueNetworkIds = [...new Set(newEntries.map((e) => e.networkId))];

    await Promise.all(
      uniqueNetworkIds.map(async (id) => {
        if (networks.value[id]) return;

        const network = await getNetwork(id);
        if (network) networks.value[id] = network;
      }),
    );
  },
  { immediate: true },
);

const filters = reactive({
  authors: [] as string[],
  visibility: [] as Array<'published' | 'draft'>,
  dynamic: {} as Record<string, string[]>,
  ranges: {} as Record<string, { min: string | number | null; max: string | number | null }>,
});

const initFilters = () => {
  props.extraColumns.forEach((col) => {
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

const getEntryValue = (entry: T, key: string): unknown => {
  return (entry as Record<string, unknown>)[key];
};

const formatValue = (val: unknown, type: ExtraColumn['type']): string => {
  if (val === null || val === undefined) return '-';

  if (type === 'number') {
    return (val as number).toLocaleString('nl-NL', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 1,
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
}

const getUniqueOptions = (key: string): string[] => {
  const options = props.entries.map((e) => String(getEntryValue(e, key) ?? ''));
  return [...new Set(options)].filter(Boolean).sort();
};

const clearFilters = () => {
  filterKey.value = '';
  filters.authors = [];
  filters.visibility = [];
  initFilters();
};

const uniqueAuthors = computed(() => {
  const seen = new Set<string>();
  return props.entries
    .map((e) => e.authorUserProxy)
    .filter((u): u is NonNullable<typeof u> & { username: string } => {
      if (!u?.username) return false;
      if (seen.has(u.username)) return false;
      seen.add(u.username);
      return true;
    });
});

const filteredEntries = computed(() => {
  const search = filterKey.value.toLowerCase().trim();

  return props.entries.filter((entry) => {
    const dynamicValues = props.extraColumns.map((col) =>
      String(getEntryValue(entry, col.key) ?? ''),
    );
    const searchFields = [entry.authorUserProxy?.username, ...dynamicValues]
      .join(' ')
      .toLowerCase();

    if (search && !searchFields.includes(search)) return false;

    if (
      filters.authors.length > 0 &&
      entry.authorUserProxy?.username &&
      !filters.authors.includes(entry.authorUserProxy.username)
    )
      return false;
    if (filters.visibility.length > 0) {
      const status = entry.publishedAt ? 'published' : 'draft';
      if (!filters.visibility.includes(status)) return false;
    }

    for (const col of props.extraColumns) {
      if (!col.filter) continue;
      const val = getEntryValue(entry, col.key);

      if (col.type === 'string') {
        const selected = filters.dynamic[col.key];
        if (selected?.length > 0 && !selected.includes(String(val ?? ''))) return false;
      } else if (col.type === 'number' && typeof val === 'number') {
        const { min, max } = filters.ranges[col.key];
        if (min !== null && min !== '' && val < Number(min)) return false;
        if (max !== null && max !== '' && val > Number(max)) return false;
      } else if (col.type === 'date' && val) {
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
