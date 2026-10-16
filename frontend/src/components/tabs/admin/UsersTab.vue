<template>
  <div>
    <div class="admin-users-tab__header">
      <div>
        <h1 class="admin-users-tab__title">Manage Users</h1>
        <p class="admin-users-tab__subtitle">
          Global user registry, metrics, and effective entitlements
        </p>
      </div>
      <button
        @click="refreshUsers"
        class="admin-users-tab__refresh-btn"
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
      :has-value="!!users"
    />

    <div
      v-if="!!users && !loading"
      class="admin-users-tab__body"
    >
      <div class="card admin-users-tab__filters-card">
        <div class="admin-users-tab__filters-bar">
          <div class="admin-users-tab__search">
            <span class="admin-users-tab__search-icon">
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
              placeholder="Search user name or ID..."
              class="form-input admin-users-tab__search-input"
            />
          </div>

          <div class="admin-users-tab__filters-actions">
            <button
              v-if="
                Object.keys(activeBooleanFilters).length || Object.keys(activeRangeFilters).length
              "
              @click="resetFilters"
              class="admin-users-tab__clear-link"
            >
              Clear All
            </button>
            <button
              @click="showFilters = !showFilters"
              class="admin-users-tab__filters-toggle"
              :class="{ 'admin-users-tab__filters-toggle--active': showFilters }"
            >
              <span
                v-if="activeFilterCount > 0 && !showFilters"
                class="admin-users-tab__filters-count"
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
          class="admin-users-tab__advanced-filters"
        >
          <div class="admin-users-tab__calc-box">
            <div>
              <p class="admin-users-tab__calc-box-title">Calculate Limits Using</p>
              <p class="admin-users-tab__calc-box-text">
                Select which entitlement rules apply when calculating global limits.
              </p>
            </div>
            <div class="admin-users-tab__calc-box-options">
              <label class="admin-users-tab__checkbox-label">
                <input
                  type="checkbox"
                  v-model="includeRoleEntitlements"
                  class="checkbox"
                />
                Include Role Entitlements
              </label>
              <label class="admin-users-tab__checkbox-label">
                <input
                  type="checkbox"
                  v-model="includeUserEntitlements"
                  class="checkbox"
                />
                Include User Entitlements (Overrides)
              </label>
            </div>
          </div>

          <div>
            <p class="admin-users-tab__filters-label">Feature Entitlements</p>
            <div class="admin-users-tab__filter-chips">
              <button
                v-for="key in allEntitlementKeys.filter((k) => k.startsWith('allow'))"
                :key="key"
                @click="toggleBooleanFilter(key)"
                class="admin-users-tab__filter-chip"
                :class="{
                  'admin-users-tab__filter-chip--true': activeBooleanFilters[key] === true,
                  'admin-users-tab__filter-chip--false': activeBooleanFilters[key] === false,
                }"
              >
                <span v-if="activeBooleanFilters[key] === true">✓</span>
                <span v-if="activeBooleanFilters[key] === false">✕</span>
                {{ key.replace('allow', '') }}
              </button>
            </div>
          </div>

          <div>
            <p class="admin-users-tab__filters-label">Capacity Thresholds</p>
            <div class="admin-users-tab__threshold-list">
              <div
                v-for="key in userMetricKeys"
                :key="key"
                class="admin-users-tab__threshold-card"
                :class="{ 'admin-users-tab__threshold-card--expanded': !!activeRangeFilters[key] }"
              >
                <div class="admin-users-tab__threshold-card-header">
                  <span class="admin-users-tab__threshold-card-title">
                    {{ capitalize(key.replace(/([A-Z])/g, ' $1')) }}
                  </span>
                  <input
                    type="checkbox"
                    :checked="!!activeRangeFilters[key]"
                    @change="
                      (event) =>
                        (event.target as HTMLInputElement).checked
                          ? (activeRangeFilters[key] = [0, maxSliderLimits[key]])
                          : delete activeRangeFilters[key]
                    "
                    class="checkbox"
                  />
                </div>

                <div v-if="activeRangeFilters[key]">
                  <span class="admin-users-tab__threshold-sub-label">Used</span>
                  <div class="admin-users-tab__threshold-range-inputs">
                    <input
                      type="number"
                      v-model.number="activeRangeFilters[key][0]"
                      class="form-input admin-users-tab__threshold-number"
                      placeholder="Min"
                    />
                    <span class="admin-users-tab__threshold-range-sep">-</span>
                    <input
                      type="number"
                      v-model.number="activeRangeFilters[key][1]"
                      class="form-input admin-users-tab__threshold-number"
                      placeholder="Max"
                    />
                  </div>
                  <div class="admin-users-tab__threshold-slider">
                    <DualSlider
                      v-model="activeRangeFilters[key]"
                      :min="0"
                      :max="maxSliderLimits[key] || 100"
                    />
                  </div>
                </div>
                <div
                  v-else
                  class="admin-users-tab__threshold-inactive"
                >
                  Filter inactive
                </div>
              </div>
            </div>
          </div>

          <div class="admin-users-tab__sort-row">
            <div>
              <p class="admin-users-tab__filters-label admin-users-tab__filters-label--nowrap">
                Sort Results By
              </p>
              <div class="admin-users-tab__filter-chips">
                <button
                  v-for="key in ['name', 'id', 'networks']"
                  :key="key"
                  @click="toggleSort(key)"
                  class="admin-users-tab__sort-chip"
                  :class="{ 'admin-users-tab__sort-chip--active': sortBy === key }"
                >
                  {{ key }} <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓' }}</span>
                </button>
              </div>
            </div>
            <div>
              <p class="admin-users-tab__filters-label">Sort By Used</p>
              <div class="admin-users-tab__filter-chips">
                <button
                  v-for="key in userMetricKeys"
                  :key="key"
                  @click="toggleSort(key)"
                  class="admin-users-tab__sort-chip"
                  :class="{ 'admin-users-tab__sort-chip--active': sortBy === key }"
                >
                  {{ key.replace(/([A-Z])/g, ' $1') }}
                  <span v-if="sortBy === key">{{ sortOrder === 'asc' ? '↑' : '↓' }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <span class="admin-users-tab__results-count">
          Showing
          <span class="admin-users-tab__results-count-value">{{ filteredUsers.length }}</span>
          <span class="admin-users-tab__results-count-total"> / {{ allComboUsers.length }}</span>
          Users
        </span>
      </div>

      <div class="card admin-users-tab__table-card">
        <div class="admin-users-tab__table-scroll">
          <table class="table">
            <thead>
              <tr class="table__row">
                <th class="table__cell table__cell--head admin-users-tab__col-user">User</th>
                <th class="table__cell table__cell--head">ID</th>
                <th class="table__cell table__cell--head admin-users-tab__col-content">
                  Created Content
                </th>
                <th class="table__cell table__cell--head">Networks</th>
                <th class="table__cell table__cell--head admin-users-tab__col-actions">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="user in filteredUsers"
                :key="user.id"
                class="table__row"
              >
                <td class="table__cell">
                  <div class="admin-users-tab__user-name">
                    {{ defaultProxy(user).firstName }} {{ defaultProxy(user).lastName }}
                  </div>
                  <div class="admin-users-tab__user-handle">@{{ defaultProxy(user).username }}</div>
                </td>
                <td class="table__cell admin-users-tab__id-cell">
                  <SecurableField
                    :value="user.id"
                    :sensitive="true"
                  />
                </td>
                <td class="table__cell admin-users-tab__content-cell">
                  <div class="admin-users-tab__metric-chips">
                    <template v-if="userMetricKeys.some((k) => ((user[k] as number) || 0) > 0)">
                      <template
                        v-for="key in userMetricKeys"
                        :key="key"
                      >
                        <span
                          v-if="((user[key] as number) || 0) > 0"
                          class="admin-users-tab__metric-chip"
                        >
                          <span class="admin-users-tab__metric-chip-key">
                            {{
                              key
                                .replace('Count', '')
                                .replace('Size', '')
                                .replace(/([A-Z])/g, ' $1')
                                .trim()
                            }}
                          </span>

                          <span class="admin-users-tab__metric-chip-value">
                            <span class="admin-users-tab__metric-chip-value-number">{{
                              (user[key] || 0).toLocaleString('nl-NL')
                            }}</span>
                          </span>
                        </span>
                      </template>
                    </template>

                    <span
                      v-else
                      class="admin-users-tab__no-metrics"
                    >
                      <svg
                        class="admin-users-tab__no-metrics-icon"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fill-rule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zM7 9a1 1 0 000 2h6a1 1 0 100-2H7z"
                          clip-rule="evenodd"
                        />
                      </svg>
                      No metrics or limits
                    </span>
                  </div>
                </td>
                <td class="table__cell">
                  <div class="admin-users-tab__networks-list">
                    <div
                      v-for="network in getNetworksForUser(user)"
                      :key="network.id"
                      class="admin-users-tab__network-item"
                    >
                      <a
                        :href="`/networks/${network.id}`"
                        class="admin-users-tab__network-link"
                      >
                        <CdnFile
                          v-if="network.imageFile"
                          :file="network.imageFile"
                          :display-only="true"
                          class="avatar__image admin-users-tab__network-avatar"
                        />
                        <img
                          v-else
                          :src="`https://ui-avatars.com/api/?name=${encodeURIComponent(network.name)}&background=random`"
                          :alt="network.name"
                          class="avatar__image admin-users-tab__network-avatar"
                        />
                      </a>
                      <div class="admin-users-tab__network-hover-card">
                        <h6 class="admin-users-tab__network-hover-title">{{ network.name }}</h6>
                        <p class="admin-users-tab__network-hover-desc">{{ network.description }}</p>
                      </div>
                    </div>
                    <span
                      v-if="getNetworksForUser(user).length === 0"
                      class="admin-users-tab__no-networks"
                      >No networks</span
                    >
                  </div>
                </td>
                <td class="table__cell admin-users-tab__actions-cell">
                  <button
                    @click="showTempModal = true"
                    class="admin-users-tab__action-link"
                  >
                    Manage
                  </button>
                  <button
                    @click="openEntitlementsModal(user)"
                    class="admin-users-tab__action-link admin-users-tab__action-link--entitlements"
                  >
                    Entitlements
                  </button>
                </td>
              </tr>
              <tr v-if="filteredUsers.length === 0">
                <td
                  colspan="5"
                  class="table__cell admin-users-tab__empty-cell"
                >
                  No users matched the filters.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <ViewUserEntitlementsModal
      v-if="showEntitlementsModal && selectedUser"
      :user="selectedUser"
      @close="showEntitlementsModal = false"
      @edit-user-entitlement="openEditEntitlementModal"
    />

    <EditUserModal
      v-if="showEditNetworkUserEntitlementModal && selectedNetwork && selectedNetworkUser"
      :network="selectedNetwork"
      :selected-user="selectedNetworkUser"
      :is-submitting="isSubmitting"
      :manageUserForm="manageUserForm"
      @close="showEditNetworkUserEntitlementModal = false"
      @update="handleUpdateNetworkUser"
    />

    <TempModal
      v-if="showTempModal"
      @close="showTempModal = false"
    />
  </div>
</template>

<script lang="ts" setup>
import CdnFile from '@/components/cdn/CdnFile.vue';
import SecurableField from '@/components/fields/SecurableField.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import DualSlider from '@/components/DualSlider.vue';
import ViewUserEntitlementsModal from '@/components/modals/admin/ViewUserEntitlementsModal.vue';
import useUsers from '@/composables/useUsers';
import { entitlementKeys } from '@/composables/useEntitlements';
import { defaultProxy } from '@/lib/user';
import type {
  ManageUserForm,
  NetworkDto,
  NetworkUserDto,
  UpdateNetworkUserDto,
  UserWithNetworksDto,
  UserMetricsDto,
  SettableEntitlement,
} from '@/types';
import { capitalize, computed, onMounted, ref } from 'vue';
import useNetworks from '@/composables/useNetworks';
import EditUserModal from '@/components/modals/network/EditUserModal.vue';
import useNetworkUsers from '@/composables/useNetworkUsers';
import TempModal from '@/components/modals/admin/TempModal.vue';
import { extractApiErrorMessage } from '@/lib/utils';
import { useGlobalStore } from '@/stores/global';

const globalStore = useGlobalStore();

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
const selectedUser = ref<UserWithNetworksDto | null>(null);
const showEditNetworkUserEntitlementModal = ref(false);
const selectedNetwork = ref<NetworkDto | null>(null);
const selectedNetworkUser = ref<NetworkUserDto | null>(null);
const manageUserForm = ref<ManageUserForm>({ roleIds: [], entitlements: {} });

const openEntitlementsModal = (user: UserWithNetworksDto) => {
  selectedUser.value = user;
  showEntitlementsModal.value = true;
};

const openEditEntitlementModal = async (networkId: string) => {
  const network = networks.value?.find((n) => n.id == networkId);
  if (!network) throw new Error('Network not found.');

  const lightUserProxy = selectedUser.value?.userProxies.find((up) =>
    up.networkUsers.find((nu) => nu.networkId == networkId),
  );
  if (!lightUserProxy) throw new Error('UserProxy/NetworkUser not found.');

  // The light NetworkUser nested under the user doesn't carry `userProxy`/`networkUserRoles` -
  // fetch the full NetworkUserDto list for this network to get those.
  const { execute: fetchNetworkUsersFor, data: networkUsersForNetwork } =
    useNetworkUsers().fetchNetworkUsers;
  await fetchNetworkUsersFor(networkId);
  const networkUser = networkUsersForNetwork.value?.find(
    (nu) => nu.userProxyId === lightUserProxy.id,
  );

  if (!networkUser) throw new Error('NetworkUser not found.');

  manageUserForm.value.roleIds = networkUser.networkUserRoles?.map((nur) => nur.role.id) || [];
  manageUserForm.value.entitlements = networkUser.entitlements as SettableEntitlement;

  selectedNetwork.value = network;
  selectedNetworkUser.value = networkUser;
  showEditNetworkUserEntitlementModal.value = true;
};

async function handleUpdateNetworkUser(localForm: ManageUserForm) {
  if (!selectedUser.value || !selectedNetwork.value) return;

  const addedRoles = localForm.roleIds.filter(
    (roleId) => !manageUserForm.value.roleIds.includes(roleId),
  );
  const removedRoles = manageUserForm.value.roleIds.filter(
    (roleId) => !localForm.roleIds.includes(roleId),
  );

  const { execute: updateNetworkUser } = useNetworkUsers().updateNetworkUser;

  const payload: UpdateNetworkUserDto = {
    entitlements: localForm.entitlements,
  };

  try {
    await updateNetworkUser(
      selectedNetwork.value.id,
      selectedUser.value.id,
      payload,
      addedRoles,
      removedRoles,
    );

    manageUserForm.value.roleIds = localForm.roleIds;
    manageUserForm.value.entitlements = localForm.entitlements;

    showEditNetworkUserEntitlementModal.value = false;
  } catch (err) {
    console.error('Error updating user settings:', err);
    globalStore.addToast({
      message: extractApiErrorMessage(err, 'Failed to update user settings.'),
      type: 'error',
      duration: 5000,
    });
  } finally {
    isSubmitting.value = false;
  }
}

const includeRoleEntitlements = ref(true);
const includeUserEntitlements = ref(true);

const activeBooleanFilters = ref<Record<string, boolean | null>>({});
const activeRangeFilters = ref<Record<string, [number, number]>>({});

// UserMetricsDto has no per-user `customPageBlockSize` (that's a network-wide storage metric,
// only exposed on NetworkMetricsDto) - omitted here since the backend doesn't provide it per user.
const userMetricKeys = [
  'fileCount',
  'fileStorage',
  'blogCount',
  'configurationCount',
  'customPageCount',
  'customPageBlockCount',
] as const;

onMounted(async () => {
  await fetchUsersMetrics();
  await fetchNetworks();
  await fetchUsers();
});

const allEntitlementKeys = computed(() => Object.values(entitlementKeys).flat());

type ComboUser = UserWithNetworksDto & UserMetricsDto;

const allComboUsers = computed<ComboUser[]>(() => {
  if (!users.value) return [];

  return users.value.map((user) => {
    const metrics = usersMetrics.value?.find((m) => m.userId === user.id) || ({} as UserMetricsDto);

    return { ...user, ...metrics } as ComboUser;
  });
});

const highestUserMetrics = computed<Record<string, number>>(() => {
  const highest: Record<string, number> = {};

  userMetricKeys.forEach((metricKey) => {
    let topVal = 0;

    allComboUsers.value.forEach((user) => {
      const val = user[metricKey as keyof ComboUser];
      if (typeof val === 'number' && val > topVal) {
        topVal = val;
      }
    });

    highest[metricKey] = topVal;
  });

  return highest;
});

const maxSliderLimits = computed<Record<string, number>>(() => {
  const limits: Record<string, number> = {};

  userMetricKeys.forEach((key) => {
    limits[key] = highestUserMetrics.value[key] || 100;
  });

  return limits;
});

const activeFilterCount = computed(() => {
  let count = 0;
  count += Object.values(activeBooleanFilters.value).filter((v) => v !== null).length;
  count += Object.keys(activeRangeFilters.value).length;
  if (searchQuery.value.trim()) count++;
  return count;
});

const filteredUsers = computed(() => {
  return allComboUsers.value
    .filter((user) => {
      const proxy = defaultProxy(user);
      const searchStr =
        `${proxy.username} ${proxy.firstName} ${proxy.lastName} ${user.id}`.toLowerCase();

      if (searchQuery.value && !searchStr.includes(searchQuery.value.toLowerCase())) {
        return false;
      }

      for (const [key, range] of Object.entries(activeRangeFilters.value)) {
        const limitVal = (user[key as keyof UserMetricsDto] as number) || 0;
        if (limitVal < range[0] || limitVal > range[1]) return false;
      }

      return true;
    })
    .sort((a, b) => {
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
      } else if (sortBy.value.startsWith('used-')) {
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

const getNetworksForUser = (user: UserWithNetworksDto) =>
  defaultProxy(user).networkUsers?.map((nu) => nu.network) || [];

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
