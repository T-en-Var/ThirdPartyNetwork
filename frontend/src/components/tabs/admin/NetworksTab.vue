<template>
  <div>
    <div class="flex justify-between items-center mb-8">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Manage Networks</h1>
        <p class="text-slate-500 mt-1">Overview of all networks in the system</p>
      </div>
      <button @click="refreshNetworks"
        class="p-2 text-slate-400 hover:text-purple-600 transition-colors"
        title="Refresh">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none"
          viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
      </button>
    </div>

    <LoadingErrorComponent :loading="loading" :error="error"
      :has-value="!!networks" />

    <div v-if="!!networks && !!mainNetwork && !loading" class="flex gap-4 flex-col">
      <div
        class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
        <div
          class="px-4 py-3 border-b border-slate-100 bg-slate-50/50 flex justify-between">
          <h1 class="text-xs font-bold uppercase text-slate-500 tracking-widest">
            Global
            Entitlement Limits</h1>
          <button @click="handleEditNetwork(mainNetwork)"
            class="text-sm font-medium text-purple-600 hover:text-purple-800 pr-4">Edit</button>
        </div>

        <div class="p-4 flex flex-row gap-8 justify-start flex-wrap">
          <div v-for="group in groupedEntitlements" :key="group.name"
            class="space-y-2">
            <h2
              class="text-[11px] font-semibold text-blue-600 uppercase tracking-tighter">
              {{ group.name }}
            </h2>

            <div class="flex gap-2 flex-wrap">
              <div v-for="item in group.items" :key="item.label"
                class="flex items-center rounded-lg border border-slate-200 bg-slate-50/50 px-2 py-1 text-[11px] transition-all hover:border-slate-300 hover:bg-white">
                <span class="text-slate-500 font-medium mr-2 text-nowrap">{{
                  item.label
                }}</span>
                <div class="text-slate-800">
                  <span class="border-l border-slate-200 pl-2 cursor-help" :title="`${totalMetrics[item.key.replace('Limit', '') as keyof
                    NetworkMetrics]?.toLocaleString()} used`" :class="{
                      'text-red-800': totalMetrics[item.key.replace('Limit', '') as keyof NetworkMetrics] as number > totalLimits[item.key],
                    }">{{
                      (totalMetrics[item.key.replace('Limit', '') as keyof
                        NetworkMetrics] as number | undefined)?.toLocaleString()
                    }}</span>
                  <span
                    :title="`${totalLimits[item.key].toLocaleString()} used limit`"
                    class="cursor-help" :class="{
                      'text-orange-400 font-bold': remainingLimits[item.key] < 10 && remainingLimits[item.key] > 0,
                      'text-red-800 font-bold': remainingLimits[item.key] <= 0,
                    }"> {{ totalMetrics[item.key.replace('Limit', '') as keyof
                      NetworkMetrics] !== undefined ? " / " : "" }} {{
                      totalLimits[item.key].toLocaleString() }}</span>
                  <span :title="`${item.value.toLocaleString()} limit`"
                    class="font-bold text-slate-800 text-nowrap cursor-help">
                    / {{ item.value.toLocaleString() }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="groupedEntitlements.length === 0"
            class="text-center py-4 text-slate-400 text-sm italic">
            No active limits found.
          </div>
        </div>
      </div>

      <div v-if="mainNetwork"
        class="bg-white rounded-xl border border-slate-200 shadow-sm mb-4">
        <div class="p-3 flex flex-wrap items-center justify-between gap-4">
          <div class="relative flex-1 min-w-[300px]">
            <span
              class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-4 w-4 text-slate-400" fill="none" viewBox="0 0 24 24"
                stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </span>
            <input v-model="searchQuery" type="text"
              placeholder="Search name or ID..."
              class="block w-full pl-10 pr-3 py-2 border border-slate-200 rounded-lg text-sm focus:ring-2 focus:ring-purple-500/20 focus:border-purple-500 outline-none" />
          </div>

          <div class="flex items-center gap-2">
            <button
              v-if="Object.keys(activeBooleanFilters).length || Object.keys(activeRangeFilters).length"
              @click="resetFilters"
              class="text-xs font-semibold text-slate-400 hover:text-red-500 px-2 transition-colors">Clear
              All</button>
            <button @click="showFilters = !showFilters"
              :class="showFilters ? 'bg-purple-50 text-purple-700 border-purple-200' : 'bg-white text-slate-600 border-slate-200'"
              class="relative flex items-center gap-2 px-3 py-2 border rounded-lg text-sm font-medium hover:bg-slate-50 transition-all">

              <span v-if="activeFilterCount > 0 && !showFilters"
                class="absolute -top-2 -right-2 flex h-5 w-5 items-center justify-center rounded-full bg-purple-600 text-[10px] text-white border-2 border-white shadow-sm">
                {{ activeFilterCount }}
              </span>

              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24"
                stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
              </svg>
              Advanced Filters
            </button>
          </div>
        </div>

        <div v-if="showFilters"
          class="px-4 py-4 border-t border-slate-100 bg-slate-50/30 space-y-6">

          <div>
            <p
              class="text-[10px] font-bold uppercase text-slate-400 tracking-widest mb-3">
              Feature Entitlements</p>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="key in Object.keys(entitlementKeys).filter(k => k.startsWith('allow'))"
                :key="key" @click="toggleBooleanFilter(key)" :class="{
                  'bg-green-600 text-white border-green-600': activeBooleanFilters[key] === true,
                  'bg-red-600 text-white border-red-600': activeBooleanFilters[key] === false,
                  'bg-white text-slate-600 border-slate-200': activeBooleanFilters[key] === null || activeBooleanFilters[key] === undefined
                }"
                class="px-3 py-1.5 border rounded-full text-xs font-medium transition-all shadow-sm flex items-center gap-2">
                <span v-if="activeBooleanFilters[key] === true">✓</span>
                <span v-if="activeBooleanFilters[key] === false">✕</span>
                {{ key.replace('allow', '') }}
              </button>
            </div>
          </div>

          <div>
            <p
              class="text-[10px] font-bold uppercase text-slate-400 tracking-widest mb-3">
              Capacity Thresholds</p>
            <div class="flex flex-row flex-wrap items-start gap-4">
              <div v-for="key in allEntitlementKeys.filter(k => k.includes('Limit'))"
                :key="key"
                class="bg-white p-3 rounded-lg border border-slate-200 shadow-sm"
                :class="[
                  // 'transition-all duration-300',
                  !!activeRangeFilters[key] ? 'basis-[calc(33.333%-1rem)]' : 'basis-[calc(16.666%-1rem)]'
                ]">
                <div class="flex justify-between items-center mb-2">
                  <span class="text-[11px] font-bold text-slate-700">
                    {{ capitalize(key.replace('Limit', '').replace(/([A-Z])/g,
                      ' $1')) }}
                  </span>
                  <input type="checkbox" :checked="!!activeRangeFilters[key]"
                    @change="(event) => (event.target as HTMLInputElement).checked ? activeRangeFilters[key] = [[0, mainNetwork!.entitlement![key]], [0, mainNetwork!.entitlement![key]]] : delete activeRangeFilters[key]"
                    class="rounded text-purple-600" />
                </div>

                <div v-if="activeRangeFilters[key]">
                  <span
                    class="text-[10px] font-bold uppercase text-slate-400 tracking-widest pt-2">Limit</span>
                  <div class="flex items-center gap-2">
                    <input type="number"
                      v-model.number="activeRangeFilters[key][0][0]"
                      class="w-full p-1 text-[11px] border rounded"
                      placeholder="Min" />
                    <span class="text-slate-400">-</span>
                    <input type="number"
                      v-model.number="activeRangeFilters[key][0][1]"
                      class="w-full p-1 text-[11px] border rounded"
                      placeholder="Max" />
                  </div>
                  <div class="flex flex-row flex-nowrap gap-6">
                    <DualSlider v-model="activeRangeFilters[key][0]" :min="0"
                      :max="mainNetwork!.entitlement![key]" />
                  </div>

                  <span
                    class="text-[10px] font-bold uppercase text-slate-400 tracking-widest pt-2">Used</span>
                  <div class="flex items-center gap-2">
                    <input type="number"
                      v-model.number="activeRangeFilters[key][1][0]"
                      class="w-full p-1 text-[11px] border rounded"
                      placeholder="Min" />
                    <span class="text-slate-400">-</span>
                    <input type="number"
                      v-model.number="activeRangeFilters[key][1][1]"
                      class="w-full p-1 text-[11px] border rounded"
                      placeholder="Max" />
                  </div>
                  <div class="flex flex-row flex-nowrap gap-6">
                    <DualSlider v-model="activeRangeFilters[key][1]" :min="0"
                      :max="mainNetwork!.entitlement![key]" />
                  </div>

                  <div class="flex justify-between text-[10px] text-slate-400">
                    <span>Min: 0</span>
                    <span>Global Max: {{ mainNetwork!.entitlement![key] }}</span>
                  </div>

                  <div
                    v-if="activeRangeFilters[key][0][0] > activeRangeFilters[key][0][1] || activeRangeFilters[key][1][0] > activeRangeFilters[key][1][1]">
                    <span class="text-xs font-bold text-orange-400 pt-2">
                      This filter is excluding all results.
                    </span>
                  </div>
                </div>
                <div v-else class="text-[10px] text-slate-400 italic">Filter inactive
                </div>
              </div>
            </div>
          </div>

          <div class="pt-4 flex flex-row flex-wrap lg:flex-nowrap gap-2">
            <div>
              <p
                class="text-[10px] font-bold uppercase text-slate-400 tracking-widest text-nowrap mb-3">
                Sort Results By</p>

              <div class="flex flex-wrap gap-2">
                <button v-for="key in ['name', 'id', 'users']" :key="key"
                  @click="toggleSort(key)"
                  :class="sortBy === key ? 'bg-purple-600 text-white border-purple-600' : 'bg-white text-slate-600 border-slate-200'"
                  class="px-3 py-1.5 border rounded-md text-xs font-medium capitalize flex items-center gap-1 shadow-sm">
                  {{ key }}
                  <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓'
                  }}</span>
                </button>
              </div>
            </div>
            <div>
              <p
                class="text-[10px] font-bold uppercase text-slate-400 tracking-widest mb-3">
                Sort Results By Max Limit</p>

              <div class="flex flex-wrap gap-2">
                <button v-for="key in allEntitlementKeys" :key="key"
                  @click="toggleSort(key)"
                  :class="sortBy === key ? 'bg-purple-600 text-white border-purple-600' : 'bg-white text-slate-600 border-slate-200'"
                  class="px-3 py-1.5 border rounded-md text-xs font-medium capitalize flex items-center gap-1 shadow-sm">
                  {{ key.replace('Limit', '').replace(/([A-Z])/g, ' $1') }}
                  <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓'
                  }}</span>
                </button>
              </div>
            </div>
            <div>
              <p
                class="text-[10px] font-bold uppercase text-slate-400 tracking-widest mb-3">
                Sort Results By Used</p>

              <div class="flex flex-wrap gap-2">
                <button v-for="key in allEntitlementKeys" :key="key"
                  @click="toggleSort(`used-${key}`)"
                  :class="sortBy === `used-${key}` ? 'bg-purple-600 text-white border-purple-600' : 'bg-white text-slate-600 border-slate-200'"
                  class="px-3 py-1.5 border rounded-md text-xs font-medium capitalize flex items-center gap-1 shadow-sm">
                  {{ key.replace('Limit', '').replace(/([A-Z])/g, ' $1') }}
                  <span v-if="sortBy === `used-${key}`">{{ sortOrder === 'asc' ? '↑'
                    : '↓'
                    }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <span
          class="block text-[10px] uppercase tracking-widest font-bold text-slate-400 leading-none pl-4 mb-4">
          Showing
          <span class="text-lg font-black text-slate-900 leading-none pl-1">
            {{ filteredNetworks.length }}
          </span>
          <span class="text-xs font-medium text-slate-400">
            / {{ allNetworks.length }}
          </span>
          Networks
        </span>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-50 border-b border-slate-200">
                <th
                  class="w-64 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  Network Name</th>
                <th
                  class="min-w-56 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  ID
                </th>
                <th
                  class="w-1/2 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  Entitlements</th>
                <th
                  class="w-0 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  Users</th>
                <th
                  class="w-32 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider text-right">
                  Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="network in filteredNetworks" :key="network.id"
                class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4">
                  <div class="flex gap-2">
                    <div class="font-medium text-slate-900">{{ network.name }}</div>
                  </div>
                  <div class="text-sm text-slate-500 truncate max-w-xs">
                    {{ network.description }}
                  </div>
                </td>

                <td class="px-6 py-4 text-sm text-slate-500 font-mono">
                  <SecurableField :value="network.id" :sensitive="true" />
                </td>
                <td class="px-6 py-4 max-w-xs">
                  <div class="flex flex-wrap gap-1.5">
                    <template
                      v-if="Object.keys(network.entitlement || {}).length > 0">
                      <span v-for="(val, key) in network.entitlement"
                        v-show="key != 'networkId'" :key="key"
                        class="inline-flex items-center rounded-md border border-slate-200 bg-white px-2 py-0.5 text-[11px] font-medium shadow-sm transition-hover hover:border-blue-300">
                        <span
                          class="text-slate-400 font-normal mr-1 uppercase tracking-tighter">{{
                            key }}</span>
                        <span
                          class="font-normal border-l border-slate-200 pl-1.5 ml-0.5"
                          :class="{
                            'text-green-500': val === true,
                            'text-red-500': val === false,
                            'text-slate-700 font-bold': val !== true && val !== false
                          }"><span
                            :class="{ 'text-red-500': (((network[key.replace('Limit', '') as keyof ComboNetwork] as number | undefined) ?? 0 as number) > (val as number)) }">{{
                              network[key.replace('Limit', '') as keyof
                                ComboNetwork]?.toLocaleString() }}</span>{{
                              network[key.replace('Limit', '') as keyof ComboNetwork] !==
                                undefined ?
                                "/" : "" }}<span class="font-bold">{{ val.toLocaleString()
                          }}</span></span>
                      </span>
                    </template>

                    <span v-else
                      class="text-slate-400 text-xs flex items-center italic">
                      <svg class="w-3 h-3 mr-1 opacity-50" fill="currentColor"
                        viewBox="0 0 20 20">
                        <path fill-rule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zM7 9a1 1 0 000 2h6a1 1 0 100-2H7z"
                          clip-rule="evenodd" />
                      </svg>
                      No active entitlements
                    </span>
                  </div>
                </td>
                <td class="px-6 py-4 text-sm text-slate-500 font-mono">{{
                  network.networkUsers.length
                }}
                </td>
                <td class="px-6 py-4 text-right space-x-2">
                  <button @click="handleEditNetwork(network)"
                    class="text-sm font-medium text-purple-600 hover:text-purple-800">Entitlements</button>
                  <button @click="handleManageNetwork(network)"
                    class="text-sm font-medium text-indigo-600 hover:text-indigo-800">Manage</button>
                  <button @click="$emit('delete-network', network)"
                    class="text-sm font-medium text-red-600 hover:text-red-800">Delete</button>
                </td>
              </tr>
              <tr v-if="networks.length === 0">
                <td colspan="4" class="px-6 py-12 text-center text-slate-500">No
                  networks
                  found.</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="mainNetwork">
          <EditNetworkEntitlementsModal
            v-if="showEditNetworkModal && selectedNetwork"
            :is-submitting="isSubmitting" :main-network="mainNetwork"
            :network="selectedNetwork" :total-limits="totalLimits"
            @close="showEditNetworkModal = false" @update="handleUpdateNetwork" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import SecurableField from '@/components/fields/SecurableField.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import EditNetworkEntitlementsModal from '@/components/modals/admin/EditNetworkEntitlementsModal.vue';
import { entitlementKeys } from '@/composables/useEntitlements';
import useNetworks from '@/composables/useNetworks';
import { useHistoryStore } from '@/stores/history';
import type { EntitlementLimits, Network, NetworkEntitlement, NetworkMetrics, SettableEntitlement } from '@/types';
import { capitalize, computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import DualSlider from '@/components/DualSlider.vue';

interface EntitlementItem {
  label: string;
  value: string | number | boolean;
  key: keyof EntitlementLimits;
}

interface EntitlementGroup {
  name: string;
  items: EntitlementItem[];
}

const router = useRouter();
const historyStore = useHistoryStore();

const { execute: fetchNetworks, error, loading, data: networks } = useNetworks().fetchNetworks;
const { execute: fetchNetworksMetrics, data: networksMetrics } = useNetworks().fetchNetworksMetrics;
const { execute: forceFetchNetworks } = useNetworks().forceFetchNetworks;
const { execute: fetchMainNetwork, data: mainNetwork } = useNetworks().fetchMainNetworkDetails;
const { execute: updateNetworkEntitlement } = useNetworks().updateNetworkEntitlement;

const showEditNetworkModal = ref(false);
const selectedNetwork = ref<Network | null>(null);

const searchQuery = ref('');
const sortBy = ref<string>('users');
const sortOrder = ref<'asc' | 'desc'>('desc');
const showFilters = ref(false);

// activeFilters.booleans: { allowFiles: true | false | null }
const activeBooleanFilters = ref<Record<string, boolean | null>>({});
// activeFilters.ranges: { fileCountLimit: [[limit-min, limit-max], [used-min, used-max]] }
const activeRangeFilters = ref<Record<string, [[number, number], [number, number]]>>({});

const isSubmitting = ref(false);

type ComboNetwork = Network & NetworkMetrics;

const allNetworks = computed<ComboNetwork[]>(() => {
  if (!networks.value || !networksMetrics.value) return [];

  return networks.value
    .filter(x => x.id !== mainNetwork.value?.id)
    .map(network => {
      const metrics = networksMetrics.value?.find(n => n.networkId === network.id);

      // If metrics aren't found, we can't make a ComboNetwork
      if (!metrics) return null;

      return { ...network, ...metrics };
    })
    .filter((n): n is ComboNetwork => n !== null); // Remove nulls and type-guard
});
const allEntitlementKeys = computed(() => Object.values(entitlementKeys).flat());

const groupedEntitlements = computed(() => {
  const entitlements = mainNetwork.value?.entitlement || {} as NetworkEntitlement;
  const groups: EntitlementGroup[] = [];

  Object.entries(entitlementKeys).forEach(([parentKey, childKeys]) => {
    if (entitlements[parentKey as keyof NetworkEntitlement] === true) {
      const items = childKeys
        .filter(key => entitlements[key] !== undefined)
        .map(key => ({
          label: capitalize(key.replace(/([A-Z])/g, ' $1').replace('Limit', '').trim()), // 'fileCountLimit' -> 'File Count'
          value: entitlements[key],
          key
        }));

      if (items.length > 0) {
        groups.push({ name: parentKey.replace('allow', ''), items });
      }
    }
  });

  return groups;
});

const totalMetrics = computed<NetworkMetrics>(() => {
  const initial: NetworkMetrics = {
    networkId: "all-networks",
    fileCount: 0,
    fileStorage: 0,
    blogCount: 0,
    configurationCount: 0,
    customPageCount: 0,
    customPageBlockCount: 0,
    customPageBlockSize: 0,
  };

  return (networksMetrics.value || []).reduce((acc, network) => {
    // 2. Iterate through the keys of our object (excluding the ID)
    (Object.keys(acc) as Array<keyof NetworkMetrics>).forEach((key) => {
      if (key === 'networkId') return;

      const val = network[key];
      if (typeof val === 'number') {
        (acc[key] as number) += val;
      }
    });

    return acc;
  }, { ...initial }); // Use a spread to avoid mutating the 'initial' object reference
});

const totalLimits = computed(() => {
  return (allNetworks.value || []).reduce((acc, network) => {
    const entitlement = network.entitlement;
    if (!entitlement) return acc;

    allEntitlementKeys.value.forEach((key) => {
      const value = entitlement[key];
      acc[key] = (acc[key] || 0) + value;
    });

    return acc;
  }, {} as EntitlementLimits);
});

const remainingLimits = computed(() => {
  const entitlements = mainNetwork.value?.entitlement || {} as NetworkEntitlement;
  const totals = totalLimits.value;
  const remaining: Record<string, number> = {};

  Object.keys(totals).forEach((key) => {
    const max = Number(entitlements[key as keyof NetworkEntitlement]) || 0;
    const used = totals[key as keyof EntitlementLimits] || 0;
    remaining[key] = max - used;
  });

  return remaining;
});

const activeFilterCount = computed(() => {
  let count = 0;
  // Count boolean filters that aren't null
  count += Object.values(activeBooleanFilters.value).filter(v => v !== null).length;
  // Count active range filters
  count += Object.keys(activeRangeFilters.value).length;
  // Count search if it's not empty
  if (searchQuery.value.trim()) count++;
  return count;
});

const filteredNetworks = computed(() => {
  return allNetworks.value.filter(n => {
    const matchesSearch = n.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      n.id.toLowerCase().includes(searchQuery.value.toLowerCase());
    if (!matchesSearch) return false;

    for (const [key, targetValue] of Object.entries(activeBooleanFilters.value)) {
      if (targetValue !== null && n.entitlement?.[key as keyof NetworkEntitlement] !== targetValue) return false;
    }

    for (const [key, type] of Object.entries(activeRangeFilters.value)) {
      let range = type[0];
      let val = (n.entitlement?.[key as keyof NetworkEntitlement] as number) || 0;
      if (val < range[0] || val > range[1]) return false;

      range = type[1];
      val = (n?.[key.replace('Limit', '') as keyof ComboNetwork] as number) || 0;
      if (val < range[0] || val > range[1]) return false;
    }

    return true;
  }).sort((a, b) => {
    let valA: string | number, valB: string | number;
    // Check if sorting by a top-level property or a nested entitlement
    if (sortBy.value === 'users') {
      valA = a.networkUsers.length;
      valB = b.networkUsers.length;
    } else if (sortBy.value === 'name' || sortBy.value === 'id') {
      valA = a[sortBy.value];
      valB = b[sortBy.value];
    } else if (sortBy.value.startsWith("used-")) {
      const key = sortBy.value.replace('Limit', '').split('-')[1] as keyof ComboNetwork;
      valA = a[key] as number;
      valB = b[key] as number;
    } else {
      valB = b.entitlement?.[sortBy.value as keyof NetworkEntitlement] as number ?? 0;
      valA = a.entitlement?.[sortBy.value as keyof NetworkEntitlement] as number ?? 0;
    }

    if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1;
    return 0;
  });
});


defineEmits<{
  (e: 'delete-network', network: Network): void;
}>();

onMounted(async () => {
  await fetchMainNetwork();
  await fetchNetworksMetrics();
  await fetchNetworks();
});

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

const handleEditNetwork = (network: Network) => {
  selectedNetwork.value = network;
  showEditNetworkModal.value = true;
};

const handleUpdateNetwork = async (networkEntitlement: SettableEntitlement) => {
  if (!selectedNetwork.value) return;

  const payload: SettableEntitlement = {
    ...networkEntitlement
  };

  try {
    await updateNetworkEntitlement(selectedNetwork.value.id, payload);
    showEditNetworkModal.value = false;
  } catch (err) {
    console.error('Error updating network entitlements:', err);
  } finally {
    isSubmitting.value = false;
  }
};

const handleManageNetwork = (network: Network) => {
  historyStore.networkVisit(network);
  router.push(`/networks/${network.id}/manage`);
}

</script>
