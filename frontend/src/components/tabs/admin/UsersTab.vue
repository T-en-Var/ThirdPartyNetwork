<template>
  <div>
    <div class="flex justify-between items-center mb-8">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Manage Users</h1>
        <p class="text-slate-500 mt-1">Global user registry, metrics, and effective
          entitlements</p>
      </div>
      <button @click="refreshUsers"
        class="p-2 text-slate-400 hover:text-purple-600 transition-colors"
        title="Refresh">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none"
          viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
      </button>
    </div>

    <LoadingErrorComponent :loading="loading" :error="error" :has-value="!!users" />

    <div v-if="!!users && !loading" class="flex gap-4 flex-col">

      <div class="bg-white rounded-xl border border-slate-200 shadow-sm mb-4">
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
              placeholder="Search user name or ID..."
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

          <div
            class="bg-indigo-50/50 border border-indigo-100 p-3 rounded-lg flex flex-wrap gap-6">
            <div>
              <p
                class="text-[10px] font-bold uppercase text-indigo-400 tracking-widest mb-1">
                Calculate Limits Using</p>
              <p class="text-xs text-slate-500 mb-2">Select which entitlement rules
                apply when calculating global limits.</p>
            </div>
            <div class="flex items-center gap-4">
              <label
                class="flex items-center gap-2 cursor-pointer text-sm font-medium text-slate-700">
                <input type="checkbox" v-model="includeRoleEntitlements"
                  class="rounded text-indigo-600 focus:ring-indigo-500" />
                Include Role Entitlements
              </label>
              <label
                class="flex items-center gap-2 cursor-pointer text-sm font-medium text-slate-700">
                <input type="checkbox" v-model="includeUserEntitlements"
                  class="rounded text-indigo-600 focus:ring-indigo-500" />
                Include User Entitlements (Overrides)
              </label>
            </div>
          </div>

          <div>
            <p
              class="text-[10px] font-bold uppercase text-slate-400 tracking-widest mb-3">
              Feature Entitlements</p>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="key in allEntitlementKeys.filter(k => k.startsWith('allow'))"
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
              <div v-for="key in userMetricKeys" :key="key"
                class="bg-white p-3 rounded-lg border border-slate-200 shadow-sm"
                :class="[!!activeRangeFilters[key] ? 'basis-[calc(33.333%-1rem)]' : 'basis-[calc(16.666%-1rem)]']">
                <div class="flex justify-between items-center mb-2">
                  <span class="text-[11px] font-bold text-slate-700">
                    {{ capitalize(key.replace(/([A-Z])/g, ' $1')) }}
                  </span>
                  <input type="checkbox" :checked="!!activeRangeFilters[key]"
                    @change="(event) => (event.target as HTMLInputElement).checked ? activeRangeFilters[key] = [0, maxSliderLimits[key]] : delete activeRangeFilters[key]"
                    class="rounded text-purple-600" />
                </div>

                <div v-if="activeRangeFilters[key]">
                  <span
                    class="text-[10px] font-bold uppercase text-slate-400 tracking-widest pt-2">Used</span>
                  <div class="flex items-center gap-2">
                    <input type="number" v-model.number="activeRangeFilters[key][0]"
                      class="w-full p-1 text-[11px] border rounded"
                      placeholder="Min" />
                    <span class="text-slate-400">-</span>
                    <input type="number" v-model.number="activeRangeFilters[key][1]"
                      class="w-full p-1 text-[11px] border rounded"
                      placeholder="Max" />
                  </div>
                  <div class="flex flex-row flex-nowrap gap-6">
                    <DualSlider v-model="activeRangeFilters[key]" :min="0"
                      :max="maxSliderLimits[key] || 100" />
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
                <button v-for="key in ['name', 'id', 'networks']" :key="key"
                  @click="toggleSort(key)"
                  :class="sortBy === key ? 'bg-purple-600 text-white border-purple-600' : 'bg-white text-slate-600 border-slate-200'"
                  class="px-3 py-1.5 border rounded-md text-xs font-medium capitalize flex items-center gap-1 shadow-sm">
                  {{ key }} <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑'
                    : '↓' }}</span>
                </button>
              </div>
            </div>
            <div>
              <p
                class="text-[10px] font-bold uppercase text-slate-400 tracking-widest mb-3">
                Sort By Used</p>
              <div class="flex flex-wrap gap-2">
                <button v-for="key in userMetricKeys" :key="key"
                  @click="toggleSort(key)"
                  :class="sortBy === key ? 'bg-purple-600 text-white border-purple-600' : 'bg-white text-slate-600 border-slate-200'"
                  class="px-3 py-1.5 border rounded-md text-xs font-medium capitalize flex items-center gap-1 shadow-sm">
                  {{ key.replace(/([A-Z])/g, ' $1') }}
                  <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓'
                  }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <span
          class="block text-[10px] uppercase tracking-widest font-bold text-slate-400 leading-none pl-4 mb-4">
          Showing <span
            class="text-lg font-black text-slate-900 leading-none pl-1">{{
              filteredUsers.length }}</span>
          <span class="text-xs font-medium text-slate-400"> / {{ allComboUsers.length
          }}</span> Users
        </span>
      </div>

      <div
        class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-50 border-b border-slate-200">
                <th
                  class="w-48 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  User</th>
                <th
                  class="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  ID</th>
                <th
                  class="w-1/2 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  Created Content</th>
                <th
                  class="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                  Networks</th>
                <th
                  class="w-32 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider text-right">
                  Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="user in filteredUsers" :key="user.id"
                class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4">
                  <div class="font-medium text-slate-900">
                    {{ defaultProxy(user).firstName }} {{ defaultProxy(user).lastName
                    }}
                  </div>
                  <div class="text-sm text-slate-500">@{{ defaultProxy(user).username
                  }}</div>
                </td>
                <td class="px-6 py-4 text-sm text-slate-600 font-mono">
                  <SecurableField :value="user.id" :sensitive="true" />
                </td>
                <td class="px-6 py-4 max-w-md">
                  <div class="flex flex-wrap gap-1.5">
                    <template
                      v-if="userMetricKeys.some(k => (user[k] as number || 0) > 0)">
                      <template v-for="key in userMetricKeys" :key="key">
                        <span v-if="(user[key] as number || 0) > 0"
                          class="inline-flex items-center rounded-md border border-slate-200 bg-white px-2 py-0.5 text-[11px] font-medium shadow-sm transition-hover hover:border-blue-300">

                          <span
                            class="text-slate-400 font-normal mr-1 uppercase tracking-tighter">
                            {{ key.replace('Count', '').replace('Size',
                              '').replace(/([A-Z])/g, ' $1').trim() }}
                          </span>

                          <span
                            class="font-normal border-l border-slate-200 pl-1.5 ml-0.5 text-slate-700">
                            <span class="font-bold">{{ (user[key] ||
                              0).toLocaleString() }}</span>
                          </span>

                        </span>
                      </template>
                    </template>

                    <span v-else
                      class="text-slate-400 text-xs flex items-center italic">
                      <svg class="w-3 h-3 mr-1 opacity-50" fill="currentColor"
                        viewBox="0 0 20 20">
                        <path fill-rule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zM7 9a1 1 0 000 2h6a1 1 0 100-2H7z"
                          clip-rule="evenodd" />
                      </svg>
                      No metrics or limits
                    </span>
                  </div>
                </td>
                <td class="px-6 py-4">
                  <div class="flex flex-wrap gap-1">
                    <div v-for="network in getNetworksForUser(user)"
                      :key="network.id" class="relative group">
                      <a :href="`/networks/${network.id}`" class="block">
                        <CloudinaryFile v-if="network.imageFile"
                          :file="network.imageFile" :display-only="true"
                          class="h-8 w-8 rounded-full object-cover" />
                        <img v-else
                          :src="`https://ui-avatars.com/api/?name=${encodeURIComponent(network.name)}&background=random`"
                          :alt="network.name"
                          class="h-8 w-8 rounded-full object-cover" />
                      </a>
                      <div
                        class="absolute z-10 hidden group-hover:block bg-white border border-gray-200 rounded-md shadow-lg p-3 w-56 top-12 left-0">
                        <h6 class="font-medium text-gray-900">{{ network.name }}</h6>
                        <p class="text-xs text-gray-500 truncate">{{
                          network.description }}</p>
                      </div>
                    </div>
                    <span v-if="getNetworksForUser(user).length === 0"
                      class="text-slate-400 text-sm italic">No networks</span>
                  </div>
                </td>
                <td class="px-6 py-4 text-right">
                  <button @click="showTempModal = true"
                    class="text-sm font-medium text-indigo-600 hover:text-indigo-800">Manage</button>
                  <button @click="openEntitlementsModal(user)"
                    class="text-sm font-medium text-purple-600 hover:text-purple-800">Entitlements</button>
                </td>
              </tr>
              <tr v-if="filteredUsers.length === 0">
                <td colspan="5" class="px-6 py-12 text-center text-slate-500">No
                  users matched the
                  filters.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <ViewUserEntitlementsModal v-if="showEntitlementsModal && selectedUser"
      :user="selectedUser" @close="showEntitlementsModal = false"
      @edit-user-entitlement="openEditEntitlementModal" />

    <EditUserModal
      v-if="showEditNetworkUserEntitlementModal && selectedNetwork && selectedNetworkUser"
      :network="selectedNetwork" :selected-user="selectedNetworkUser"
      :is-submitting="isSubmitting" :manageUserForm="manageUserForm" @close="showEditNetworkUserEntitlementModal = false"
      @update="handleUpdateNetworkUser" />

      <TempModal v-if="showTempModal" @close="showTempModal = false" />
  </div>
</template>

<script lang="ts" setup>
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import SecurableField from '@/components/fields/SecurableField.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import DualSlider from '@/components/DualSlider.vue';
import ViewUserEntitlementsModal from '@/components/modals/admin/ViewUserEntitlementsModal.vue';
import useUsers from '@/composables/useUsers';
import { entitlementKeys } from '@/composables/useEntitlements';
import { defaultProxy } from '@/lib/user';
import type { ManageUserForm, Network, NetworkUser, UpdateNetworkUser, User, UserMetrics } from '@/types';
import { capitalize, computed, onMounted, ref } from 'vue';
import useNetworks from '@/composables/useNetworks';
import EditUserModal from '@/components/modals/network/EditUserModal.vue';
import useNetworkUsers from '@/composables/useNetworkUsers';
import TempModal from '@/components/modals/admin/TempModal.vue';

const { execute: fetchUsers, error, loading, data: users } = useUsers().fetchUsers;
const { execute: forceFetchUsers } = useUsers().forceFetchUsers;
const { execute: fetchUsersMetrics, data: usersMetrics } = useUsers().fetchUsersMetrics;
const { execute: fetchNetworks, data: networks } = useNetworks().fetchNetworks;

const searchQuery = ref('');
const sortBy = ref<string>('networks');
const sortOrder = ref<'asc' | 'desc'>('desc');
const showFilters = ref(false);

const isSubmitting = ref(false);

const showTempModal = ref(false);
const showEntitlementsModal = ref(false);
const selectedUser = ref<User | null>(null);
const showEditNetworkUserEntitlementModal = ref(false);
const selectedNetwork = ref<Network | null>(null);
const selectedNetworkUser = ref<NetworkUser | null>(null);
const manageUserForm = ref<ManageUserForm>({ roleIds: [], entitlements: {} });

const openEntitlementsModal = (user: User) => {
  selectedUser.value = user;
  showEntitlementsModal.value = true;
};

const openEditEntitlementModal = (networkId: string) => {
  const network = networks.value?.find(n => n.id == networkId);
  if (!network) throw new Error("Network not found.");

  const userProxy = selectedUser.value?.userProxies.find(up => up.networkUsers.find(nu => nu.networkId == networkId));
  const networkUser = userProxy?.networkUsers.find(nu => nu.networkId == networkId); 

  if (!userProxy) throw new Error("UserProxy/NetworkUser not found.");
  if (!networkUser) throw new Error("NetworkUser not found.");
  
  networkUser.userProxy = userProxy;
  
  manageUserForm.value.roleIds = networkUser.networkUserRoles?.map(nur => nur.role.id) || [];
  manageUserForm.value.entitlements = networkUser.entitlements;

  selectedNetwork.value = network;
  selectedNetworkUser.value = networkUser;
  showEditNetworkUserEntitlementModal.value = true;
};

async function handleUpdateNetworkUser(localForm: ManageUserForm) {
  if (!selectedUser.value || !selectedNetwork.value) return;

  const addedRoles = localForm.roleIds.filter(roleId => !manageUserForm.value.roleIds.includes(roleId));
  const removedRoles = manageUserForm.value.roleIds.filter(roleId => !localForm.roleIds.includes(roleId));

  const { execute: updateNetworkUser } = useNetworkUsers().updateNetworkUser;

  const payload: UpdateNetworkUser = {
    entitlements: localForm.entitlements,
  };

  try {
    await updateNetworkUser(selectedNetwork.value.id, selectedUser.value.id, payload, addedRoles, removedRoles);

    manageUserForm.value.roleIds = localForm.roleIds;
    manageUserForm.value.entitlements = localForm.entitlements;

    showEditNetworkUserEntitlementModal.value = false;
  } catch (err) {
    console.error('Error updating user settings:', err);
  } finally {
    isSubmitting.value = false;
  }
}

// Toggles for Entitlement Overrides
const includeRoleEntitlements = ref(true);
const includeUserEntitlements = ref(true);

const activeBooleanFilters = ref<Record<string, boolean | null>>({});
const activeRangeFilters = ref<Record<string, [number, number]>>({});

const userMetricKeys = [
  'fileCount',
  'fileStorage',
  'blogCount',
  'configurationCount',
  'customPageCount',
  'customPageBlockCount',
  'customPageBlockSize'
] as const;

onMounted(async () => {
  await fetchUsersMetrics();
  await fetchNetworks();
  await fetchUsers();
});

const allEntitlementKeys = computed(() => Object.values(entitlementKeys).flat());

// Create the unified data object
type ComboUser = User & UserMetrics;

const allComboUsers = computed<ComboUser[]>(() => {
  if (!users.value) return [];

  return users.value.map(user => {
    const metrics = usersMetrics.value?.find(m => m.userId === user.id) || {} as UserMetrics;

    return { ...user, ...metrics } as ComboUser;
  });
});

const highestUserMetrics = computed<Record<string, number>>(() => {
  const highest: Record<string, number> = {};

  userMetricKeys.forEach(metricKey => {
    let topVal = 0;

    allComboUsers.value.forEach(user => {
      const val = user[metricKey as keyof ComboUser];
      if (typeof val === 'number' && val > topVal) {
        topVal = val;
      }
    });

    highest[metricKey] = topVal;
  });

  return highest;
});

// Calculate global maximum bounds dynamically for the dual sliders
const maxSliderLimits = computed<Record<string, number>>(() => {
  const limits: Record<string, number> = {};

  userMetricKeys.forEach(key => {
    limits[key] = highestUserMetrics.value[key] || 100;
  });

  return limits;
});

const activeFilterCount = computed(() => {
  let count = 0;
  count += Object.values(activeBooleanFilters.value).filter(v => v !== null).length;
  count += Object.keys(activeRangeFilters.value).length;
  if (searchQuery.value.trim()) count++;
  return count;
});

const filteredUsers = computed(() => {
  return allComboUsers.value.filter(user => {
    const proxy = defaultProxy(user);
    const searchStr = `${proxy.username} ${proxy.firstName} ${proxy.lastName} ${user.id}`.toLowerCase();

    if (searchQuery.value && !searchStr.includes(searchQuery.value.toLowerCase())) {
      return false;
    }

    // Check Ranges
    for (const [key, range] of Object.entries(activeRangeFilters.value)) {
      const limitVal = (user[key as keyof UserMetrics] as number) || 0;
      if (limitVal < range[0] || limitVal > range[1]) return false;
    }

    return true;
  }).sort((a, b) => {
    let valA: string | number, valB: string | number;

    if (sortBy.value === 'networks') {
      valA = getNetworksForUser(a).length;
      valB = getNetworksForUser(b).length;
    } else if (sortBy.value === 'name') {
      valA = defaultProxy(a).username || '';
      valB = defaultProxy(b).username || '';
    } else if (sortBy.value === 'id') {
      valA = a.id;
      valB = b.id;
    } else if (sortBy.value.startsWith("used-")) {
      const metricKey = sortBy.value.replace('used-', '').replace('Limit', '') as keyof ComboUser;
      valA = (a[metricKey] as number) ?? 0;
      valB = (b[metricKey] as number) ?? 0;
    } else {
      valA = 0;
      valB = 0;
    }

    if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1;
    return 0;
  });
});

// Actions
const getNetworksForUser = (user: User): Network[] => defaultProxy(user).networkUsers?.map(nu => nu.network) || [];

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

const refreshUsers = async () => {
  await forceFetchUsers();
  await fetchUsersMetrics();
};
</script>