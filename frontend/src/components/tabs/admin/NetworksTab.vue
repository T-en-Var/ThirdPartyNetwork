<template>
  <div>
    <div class="admin-networks-tab__header">
      <div>
        <h1 class="admin-networks-tab__title">Manage Networks</h1>
        <p class="admin-networks-tab__subtitle">Overview of all networks in the system</p>
      </div>
      <button
        @click="refreshNetworks"
        class="admin-networks-tab__refresh-btn"
        title="Refresh"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
          />
        </svg>
      </button>
    </div>

    <LoadingErrorComponent
      :loading="loading"
      :error="error"
      :has-value="!!networks"
    />

    <div
      v-if="!!networks && !!mainNetwork && !loading"
      class="admin-networks-tab__body"
    >
      <div class="card">
        <div class="admin-networks-tab__limits-header">
          <h1 class="admin-networks-tab__limits-title">Global Feature Flag Limits</h1>
          <button
            @click="handleEditNetwork(mainNetwork)"
            class="admin-networks-tab__edit-link"
          >
            Edit
          </button>
        </div>

        <div class="admin-networks-tab__limits-groups">
          <div
            v-for="group in groupedFeatureFlags"
            :key="group.name"
            class="admin-networks-tab__limits-group"
          >
            <h2 class="admin-networks-tab__limits-group-title">
              {{ group.name }}
            </h2>

            <div class="admin-networks-tab__limits-items">
              <div
                v-for="item in group.items"
                :key="item.label"
                class="admin-networks-tab__limit-item"
              >
                <span class="admin-networks-tab__limit-item-label">{{ item.label }}</span>
                <div class="admin-networks-tab__limit-item-value">
                  <span
                    class="admin-networks-tab__limit-item-used"
                    :title="`${getFeatureFlagNumber(totalMetrics, item.key)?.toLocaleString()} used`"
                    :class="{
                      'admin-networks-tab__limit-item-used--over':
                        (getFeatureFlagNumber(totalMetrics, item.key) as number) >
                        totalLimits[item.key],
                    }"
                    >{{
                      getFeatureFlagNumber(totalMetrics, item.key)?.toLocaleString('nl-NL')
                    }}</span
                  >
                  <span
                    :title="`${totalLimits[item.key].toLocaleString('nl-NL')} used limit`"
                    class="admin-networks-tab__limit-item-total"
                    :class="{
                      'admin-networks-tab__limit-item-total--warning':
                        remainingLimits[item.key] < 10 && remainingLimits[item.key] > 0,
                      'admin-networks-tab__limit-item-total--over': remainingLimits[item.key] <= 0,
                    }"
                  >
                    {{
                      getFeatureFlagNumber(totalMetrics, item.key) !== undefined
                        ? ` / ${totalLimits[item.key].toLocaleString('nl-NL')} / `
                        : ''
                    }}</span
                  >
                  <span
                    :title="`${item.value.toLocaleString('nl-NL')} limit`"
                    class="admin-networks-tab__limit-item-max"
                  >
                    {{ item.value.toLocaleString('nl-NL') }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div
            v-if="groupedFeatureFlags.length === 0"
            class="admin-networks-tab__limits-empty"
          >
            No active limits found.
          </div>
        </div>
      </div>

      <div
        v-if="mainNetwork"
        class="card admin-networks-tab__filters-card"
      >
        <div class="admin-networks-tab__filters-bar">
          <div class="admin-networks-tab__search">
            <span class="admin-networks-tab__search-icon">
              <svg
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
            </span>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search name or ID..."
              class="form-input admin-networks-tab__search-input"
            />
          </div>

          <div class="admin-networks-tab__filters-actions">
            <button
              v-if="
                Object.keys(activeBooleanFilters).length || Object.keys(activeRangeFilters).length
              "
              @click="resetFilters"
              class="admin-networks-tab__clear-link"
            >
              Clear All
            </button>
            <button
              @click="showFilters = !showFilters"
              class="admin-networks-tab__filters-toggle"
              :class="{ 'admin-networks-tab__filters-toggle--active': showFilters }"
            >
              <span
                v-if="activeFilterCount > 0 && !showFilters"
                class="admin-networks-tab__filters-count"
              >
                {{ activeFilterCount }}
              </span>

              <svg
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"
                />
              </svg>
              Advanced Filters
            </button>
          </div>
        </div>

        <div
          v-if="showFilters"
          class="admin-networks-tab__advanced-filters"
        >
          <div>
            <p class="admin-networks-tab__filters-label">Feature Flags</p>
            <div class="admin-networks-tab__filter-chips">
              <button
                v-for="key in Object.keys(featureFlagKeys).filter((k) => k.startsWith('allow'))"
                :key="key"
                @click="toggleBooleanFilter(key)"
                class="admin-networks-tab__filter-chip"
                :class="{
                  'admin-networks-tab__filter-chip--true': activeBooleanFilters[key] === true,
                  'admin-networks-tab__filter-chip--false': activeBooleanFilters[key] === false,
                }"
              >
                <span v-if="activeBooleanFilters[key] === true">✓</span>
                <span v-if="activeBooleanFilters[key] === false">✕</span>
                {{ key.replace('allow', '') }}
              </button>
            </div>
          </div>

          <div>
            <p class="admin-networks-tab__filters-label">Capacity Thresholds</p>
            <div class="admin-networks-tab__threshold-list">
              <div
                v-for="key in allFeatureFlagKeys.filter((k) => k.includes('Limit'))"
                :key="key"
                class="admin-networks-tab__threshold-card"
                :class="{ 'admin-networks-tab__threshold-card--expanded': !!activeRangeFilters[key] }"
              >
                <div class="admin-networks-tab__threshold-card-header">
                  <span class="admin-networks-tab__threshold-card-title">
                    {{ capitalize(key.replace('Limit', '').replace(/([A-Z])/g, ' $1')) }}
                  </span>
                  <input
                    type="checkbox"
                    :checked="!!activeRangeFilters[key]"
                    @change="
                      (event) =>
                        (event.target as HTMLInputElement).checked
                          ? (activeRangeFilters[key] = [
                              [0, mainNetwork!.featureFlag![key]],
                              [0, mainNetwork!.featureFlag![key]],
                            ])
                          : delete activeRangeFilters[key]
                    "
                    class="checkbox"
                  />
                </div>

                <div v-if="activeRangeFilters[key]">
                  <span class="admin-networks-tab__threshold-sub-label">Limit</span>
                  <div class="admin-networks-tab__threshold-range-inputs">
                    <input
                      type="number"
                      v-model.number="activeRangeFilters[key][0][0]"
                      class="form-input admin-networks-tab__threshold-number"
                      placeholder="Min"
                    />
                    <span class="admin-networks-tab__threshold-range-sep">-</span>
                    <input
                      type="number"
                      v-model.number="activeRangeFilters[key][0][1]"
                      class="form-input admin-networks-tab__threshold-number"
                      placeholder="Max"
                    />
                  </div>
                  <div class="admin-networks-tab__threshold-slider">
                    <DualSlider
                      v-model="activeRangeFilters[key][0]"
                      :min="0"
                      :max="mainNetwork!.featureFlag![key]"
                    />
                  </div>

                  <span class="admin-networks-tab__threshold-sub-label">Used</span>
                  <div class="admin-networks-tab__threshold-range-inputs">
                    <input
                      type="number"
                      v-model.number="activeRangeFilters[key][1][0]"
                      class="form-input admin-networks-tab__threshold-number"
                      placeholder="Min"
                    />
                    <span class="admin-networks-tab__threshold-range-sep">-</span>
                    <input
                      type="number"
                      v-model.number="activeRangeFilters[key][1][1]"
                      class="form-input admin-networks-tab__threshold-number"
                      placeholder="Max"
                    />
                  </div>
                  <div class="admin-networks-tab__threshold-slider">
                    <DualSlider
                      v-model="activeRangeFilters[key][1]"
                      :min="0"
                      :max="mainNetwork!.featureFlag![key]"
                    />
                  </div>

                  <div class="admin-networks-tab__threshold-minmax">
                    <span>Min: 0</span>
                    <span>Global Max: {{ mainNetwork!.featureFlag![key] }}</span>
                  </div>

                  <div
                    v-if="
                      activeRangeFilters[key][0][0] > activeRangeFilters[key][0][1] ||
                      activeRangeFilters[key][1][0] > activeRangeFilters[key][1][1]
                    "
                  >
                    <span class="admin-networks-tab__threshold-warning">
                      This filter is excluding all results.
                    </span>
                  </div>
                </div>
                <div
                  v-else
                  class="admin-networks-tab__threshold-inactive"
                >
                  Filter inactive
                </div>
              </div>
            </div>
          </div>

          <div class="admin-networks-tab__sort-row">
            <div>
              <p class="admin-networks-tab__filters-label admin-networks-tab__filters-label--nowrap">
                Sort Results By
              </p>

              <div class="admin-networks-tab__filter-chips">
                <button
                  v-for="key in ['name', 'id', 'users']"
                  :key="key"
                  @click="toggleSort(key)"
                  class="admin-networks-tab__sort-chip"
                  :class="{ 'admin-networks-tab__sort-chip--active': sortBy === key }"
                >
                  {{ key }}
                  <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓' }}</span>
                </button>
              </div>
            </div>
            <div>
              <p class="admin-networks-tab__filters-label">Sort Results By Max Limit</p>

              <div class="admin-networks-tab__filter-chips">
                <button
                  v-for="key in allFeatureFlagKeys"
                  :key="key"
                  @click="toggleSort(key)"
                  class="admin-networks-tab__sort-chip"
                  :class="{ 'admin-networks-tab__sort-chip--active': sortBy === key }"
                >
                  {{ key.replace('Limit', '').replace(/([A-Z])/g, ' $1') }}
                  <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓' }}</span>
                </button>
              </div>
            </div>
            <div>
              <p class="admin-networks-tab__filters-label">Sort Results By Used</p>

              <div class="admin-networks-tab__filter-chips">
                <button
                  v-for="key in allFeatureFlagKeys"
                  :key="key"
                  @click="toggleSort(`used-${key}`)"
                  class="admin-networks-tab__sort-chip"
                  :class="{ 'admin-networks-tab__sort-chip--active': sortBy === `used-${key}` }"
                >
                  {{ key.replace('Limit', '').replace(/([A-Z])/g, ' $1') }}
                  <span v-if="sortBy === `used-${key}`">{{ sortOrder === 'asc' ? '↑' : '↓' }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <span class="admin-networks-tab__results-count">
          Showing
          <span class="admin-networks-tab__results-count-value">
            {{ filteredNetworks.length }}
          </span>
          <span class="admin-networks-tab__results-count-total"> / {{ allNetworks.length }} </span>
          Networks
        </span>
      </div>

      <div class="card admin-networks-tab__table-card">
        <div class="admin-networks-tab__table-scroll">
          <table class="table">
            <thead>
              <tr class="table__row">
                <th class="table__cell table__cell--head admin-networks-tab__col-name">
                  Network Name
                </th>
                <th class="table__cell table__cell--head admin-networks-tab__col-id">ID</th>
                <th class="table__cell table__cell--head admin-networks-tab__col-feature-flags">
                  Feature Flags
                </th>
                <th class="table__cell table__cell--head admin-networks-tab__col-users">Users</th>
                <th
                  class="table__cell table__cell--head admin-networks-tab__col-actions"
                >
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="network in filteredNetworks"
                :key="network.id"
                class="table__row"
              >
                <td class="table__cell">
                  <div class="admin-networks-tab__name-row">
                    <div class="admin-networks-tab__name">{{ network.name }}</div>
                  </div>
                  <div class="admin-networks-tab__description">
                    {{ network.description }}
                  </div>
                </td>

                <td class="table__cell admin-networks-tab__id-cell">
                  <SecurableField
                    :value="network.id"
                    :sensitive="true"
                  />
                </td>
                <td class="table__cell admin-networks-tab__feature-flags-cell">
                  <div class="admin-networks-tab__feature-flag-chips">
                    <template v-if="Object.keys(network.featureFlag || {}).length > 0">
                      <span
                        v-for="(val, key) in network.featureFlag || ({} as Partial<NetworkFeatureFlagDto>)"
                        v-show="key != 'networkId'"
                        :key="key"
                        class="admin-networks-tab__feature-flag-chip"
                      >
                        <span class="admin-networks-tab__feature-flag-chip-key">{{ key }}</span>
                        <span
                          class="admin-networks-tab__feature-flag-chip-value"
                          :class="{
                            'admin-networks-tab__feature-flag-chip-value--true': val === true,
                            'admin-networks-tab__feature-flag-chip-value--false': val === false,
                            'admin-networks-tab__feature-flag-chip-value--number':
                              val !== true && val !== false,
                          }"
                          ><span
                            :class="{
                              'admin-networks-tab__feature-flag-chip-used--over':
                                (getFeatureFlagNumber(network, key) ?? 0) > (val as number),
                            }"
                            >{{ getFeatureFlagNumber(network, key)?.toLocaleString('nl-NL') }}</span
                          >{{ getFeatureFlagNumber(network, key) !== undefined ? '/' : ''
                          }}<span class="admin-networks-tab__feature-flag-chip-max">{{
                            (val as number)?.toLocaleString('nl-NL')
                          }}</span></span
                        >
                      </span>
                    </template>

                    <span
                      v-else
                      class="admin-networks-tab__no-feature-flags"
                    >
                      <svg
                        class="admin-networks-tab__no-feature-flags-icon"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fill-rule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zM7 9a1 1 0 000 2h6a1 1 0 100-2H7z"
                          clip-rule="evenodd"
                        />
                      </svg>
                      No active feature flags
                    </span>
                  </div>
                </td>
                <td class="table__cell admin-networks-tab__users-cell">
                  {{ network.networkUsers.length }}
                </td>
                <td class="table__cell admin-networks-tab__actions-cell">
                  <button
                    @click="handleEditNetwork(network)"
                    class="admin-networks-tab__action-link"
                  >
                    Feature Flags
                  </button>
                  <button
                    @click="handleManageNetwork(network)"
                    class="admin-networks-tab__action-link admin-networks-tab__action-link--manage"
                  >
                    Manage
                  </button>
                  <button
                    @click="$emit('delete-network', network)"
                    class="admin-networks-tab__action-link admin-networks-tab__action-link--danger"
                  >
                    Delete
                  </button>
                </td>
              </tr>
              <tr v-if="networks.length === 0">
                <td
                  colspan="4"
                  class="table__cell admin-networks-tab__empty-cell"
                >
                  No networks found.
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="mainNetwork">
          <EditNetworkFeatureFlagsModal
            v-if="showEditNetworkModal && selectedNetwork"
            :is-submitting="isSubmitting"
            :main-network="mainNetwork"
            :network="selectedNetwork"
            :total-limits="totalLimits"
            @close="showEditNetworkModal = false"
            @update="handleUpdateNetwork"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import SecurableField from '@/components/fields/SecurableField.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import EditNetworkFeatureFlagsModal from '@/components/modals/admin/EditNetworkFeatureFlagsModal.vue';
import { featureFlagKeys } from '@/composables/useFeatureFlags';
import useNetworks from '@/composables/useNetworks';
import { DEFAULT_STORES, useHistoryStore } from '@/stores/history';
import type {
  FeatureFlagLimits,
  NetworkDto,
  NetworkFeatureFlagDto,
  NetworkMetricsDto,
  SettableFeatureFlag,
} from '@/types';
import { capitalize, computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import DualSlider from '@/components/DualSlider.vue';
import { extractApiErrorMessage } from '@/lib/utils';
import { useGlobalStore } from '@/stores/global';

interface FeatureFlagItem {
  label: string;
  value: string | number | boolean;
  key: keyof FeatureFlagLimits;
}

interface FeatureFlagGroup {
  name: string;
  items: FeatureFlagItem[];
}

const router = useRouter();
const historyStore = useHistoryStore(DEFAULT_STORES.adminPage);
const globalStore = useGlobalStore();

const { execute: fetchNetworks, error, loading, data: networks } = useNetworks().fetchNetworks;
const { execute: fetchNetworksMetrics, data: networksMetrics } = useNetworks().fetchNetworksMetrics;
const { execute: forceFetchNetworks } = useNetworks().forceFetchNetworks;
const { execute: fetchMainNetwork, data: mainNetwork } = useNetworks().fetchMainNetworkDetails;
const { execute: updateNetworkFeatureFlag } = useNetworks().updateNetworkFeatureFlag;

const showEditNetworkModal = ref(false);
const selectedNetwork = ref<NetworkDto | null>(null);

const searchQuery = ref('');
const sortBy = ref<string>('users');
const sortOrder = ref<'asc' | 'desc'>('desc');
const showFilters = ref(false);

const activeBooleanFilters = ref<Record<string, boolean | null>>({});
const activeRangeFilters = ref<Record<string, [[number, number], [number, number]]>>({});

const isSubmitting = ref(false);

type ComboNetwork = NetworkDto & NetworkMetricsDto;

const allNetworks = computed<ComboNetwork[]>(() => {
  if (!networks.value || !networksMetrics.value) return [];

  return networks.value
    .filter((x) => x.id !== mainNetwork.value?.id)
    .map((network) => {
      const metrics = networksMetrics.value?.find((n) => n.networkId === network.id);

      if (!metrics) return null;

      return { ...network, ...metrics };
    })
    .filter((n): n is ComboNetwork => n !== null);
});
const allFeatureFlagKeys = computed(() => Object.values(featureFlagKeys).flat());

const groupedFeatureFlags = computed(() => {
  const featureFlags = mainNetwork.value?.featureFlag || ({} as NetworkFeatureFlagDto);
  const groups: FeatureFlagGroup[] = [];

  Object.entries(featureFlagKeys).forEach(([parentKey, childKeys]) => {
    if (featureFlags[parentKey as keyof NetworkFeatureFlagDto] === true) {
      const items = childKeys
        .filter((key) => featureFlags[key] !== undefined)
        .map((key) => {
          return {
            label: capitalize(
              key
                .replace(/([A-Z])/g, ' $1')
                .replace('Limit', '')
                .trim(),
            ), // 'fileCountLimit' -> 'File Count'
            value: featureFlags[key],
            key,
          };
        });

      if (items.length > 0) {
        groups.push({ name: parentKey.replace('allow', ''), items });
      }
    }
  });

  return groups;
});

const totalMetrics = computed<NetworkMetricsDto>(() => {
  const initial: NetworkMetricsDto = {
    networkId: 'all-networks',
    fileCount: 0,
    fileStorage: 0,
    blogCount: 0,
    configurationCount: 0,
    customPageCount: 0,
    customPageBlockCount: 0,
    customPageBlockSize: 0,
  };

  return (networksMetrics.value || []).reduce(
    (acc, network) => {
      (Object.keys(acc) as Array<keyof NetworkMetricsDto>).forEach((key) => {
        if (key === 'networkId') return;

        const val = network[key];
        if (typeof val === 'number') {
          (acc[key] as number) += val;
        }
      });

      return acc;
    },
    { ...initial },
  ); // Use a spread to avoid mutating the 'initial' object reference
});

const totalLimits = computed(() => {
  return (allNetworks.value || []).reduce((acc, network) => {
    const featureFlag = network.featureFlag;
    if (!featureFlag) return acc;

    allFeatureFlagKeys.value.forEach((key) => {
      const value = featureFlag[key];
      acc[key] = (acc[key] || 0) + value;
    });

    return acc;
  }, {} as FeatureFlagLimits);
});

const remainingLimits = computed(() => {
  const featureFlags = mainNetwork.value?.featureFlag || ({} as NetworkFeatureFlagDto);
  const totals = totalLimits.value;
  const remaining: Record<string, number> = {};

  Object.keys(totals).forEach((key) => {
    const max = Number(featureFlags[key as keyof NetworkFeatureFlagDto]) || 0;
    const used = totals[key as keyof FeatureFlagLimits] || 0;
    remaining[key] = max - used;
  });

  return remaining;
});

const activeFilterCount = computed(() => {
  let count = 0;
  count += Object.values(activeBooleanFilters.value).filter((v) => v !== null).length;
  count += Object.keys(activeRangeFilters.value).length;
  if (searchQuery.value.trim()) count++;
  return count;
});

const filteredNetworks = computed(() => {
  return allNetworks.value
    .filter((n) => {
      const matchesSearch =
        n.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
        n.id.toLowerCase().includes(searchQuery.value.toLowerCase());
      if (!matchesSearch) return false;

      for (const [key, targetValue] of Object.entries(activeBooleanFilters.value)) {
        if (
          targetValue !== null &&
          n.featureFlag?.[key as keyof NetworkFeatureFlagDto] !== targetValue
        )
          return false;
      }

      for (const [key, type] of Object.entries(activeRangeFilters.value)) {
        let range = type[0];
        let val = (n.featureFlag?.[key as keyof NetworkFeatureFlagDto] as number) || 0;
        if (val < range[0] || val > range[1]) return false;

        range = type[1];
        val = (n?.[key.replace('Limit', '') as keyof ComboNetwork] as number) || 0;
        if (val < range[0] || val > range[1]) return false;
      }

      return true;
    })
    .sort((a, b) => {
      let valA: string | number, valB: string | number;

      if (sortBy.value === 'users') {
        valA = a.networkUsers.length;
        valB = b.networkUsers.length;
      } else if (sortBy.value === 'name' || sortBy.value === 'id') {
        valA = a[sortBy.value];
        valB = b[sortBy.value];
      } else if (sortBy.value.startsWith('used-')) {
        const key = sortBy.value.replace('Limit', '').split('-')[1] as keyof ComboNetwork;
        valA = a[key] as number;
        valB = b[key] as number;
      } else {
        valB = (b.featureFlag?.[sortBy.value as keyof NetworkFeatureFlagDto] as number) ?? 0;
        valA = (a.featureFlag?.[sortBy.value as keyof NetworkFeatureFlagDto] as number) ?? 0;
      }

      if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1;
      if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1;
      return 0;
    });
});

defineEmits<{
  (e: 'delete-network', network: NetworkDto): void;
}>();

onMounted(async () => {
  await fetchMainNetwork();
  await fetchNetworksMetrics();
  await fetchNetworks();
});

const getFeatureFlagNumber = (
  input: NetworkMetricsDto,
  key: keyof NetworkFeatureFlagDto,
): number | undefined => {
  const comboKey = key.replace('Limit', '') as keyof NetworkMetricsDto;
  let value = input[comboKey] as number;

  if (value && key == 'fileStorageLimit') value = Math.round((value as number) / 1024);

  return value;
};

const toggleSort = (key: string) => {
  if (sortBy.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortBy.value = key;
    sortOrder.value = 'desc';
  }
};

const toggleBooleanFilter = (key: string) => {
  const current = activeBooleanFilters.value[key];
  if (current === true) activeBooleanFilters.value[key] = false;
  else if (current === false) activeBooleanFilters.value[key] = null;
  else activeBooleanFilters.value[key] = true;
};

const resetFilters = () => {
  activeBooleanFilters.value = {};
  activeRangeFilters.value = {};
  searchQuery.value = '';
};

const refreshNetworks = async () => {
  await forceFetchNetworks();
};

const handleEditNetwork = (network: NetworkDto) => {
  selectedNetwork.value = network;
  showEditNetworkModal.value = true;
};

const handleUpdateNetwork = async (networkFeatureFlag: SettableFeatureFlag) => {
  if (!selectedNetwork.value) return;

  const payload: SettableFeatureFlag = {
    ...networkFeatureFlag,
  };

  try {
    await updateNetworkFeatureFlag(selectedNetwork.value.id, payload);
    showEditNetworkModal.value = false;
  } catch (err) {
    console.error('Error updating network feature flags:', err);
    globalStore.addToast({
      message: extractApiErrorMessage(err, 'Failed to update network feature flags.'),
      type: 'error',
      duration: 5000,
    });
  } finally {
    isSubmitting.value = false;
  }
};

const handleManageNetwork = (network: NetworkDto) => {
  historyStore.visit.networks(network);
  router.push(`/networks/${network.id}/manage`);
};
</script>
