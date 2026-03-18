<template>
  <modal-container title="User Entitlements" @close="$emit('close')" size="md">
    <div v-if="proxy"
      class="space-y-6 max-h-[75vh] overflow-y-auto pr-2 custom-scrollbar">

      <div class="mb-4">
        <h2 class="text-lg font-bold text-slate-900">{{ proxy.firstName }} {{
          proxy.lastName }}</h2>
        <p class="text-sm text-slate-500">@{{ proxy.username }}</p>
      </div>

      <div>
        <div v-if="breakdown.length === 0" class="text-slate-500 text-sm italic">
          This user is not a member of any networks.
        </div>

        <div v-for="nw in breakdown" :key="nw.networkId"
          class="mb-6 bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
          <div
            class="bg-slate-50 px-4 py-3 border-b border-slate-200 flex justify-between items-center">
            <div>
              <span class="font-semibold text-slate-700">{{ nw.networkName }}</span>
              <div class="flex gap-1">
                <span v-for="(val, key) in nw.baseEnt" v-show="key.startsWith('allow')"
                  :key="key"
                  class="inline-flex items-center rounded-md border border-slate-200 bg-white px-2 text-[11px] font-medium shadow-sm">
                  <span
                    class="text-slate-400 font-normal mr-1 uppercase tracking-tighter">{{
                      key.replace('allow', '') }}</span>
                  <span class="font-normal border-l border-slate-200 pl-1.5 ml-0.5"
                    :class="{ 'text-green-500': val === true, 'text-red-500': val === false, }">
                    {{ val }}
                  </span>
                </span>
              </div>
            </div>
            <div class="flex gap-4">
              <RouterLink :to="`/networks/${nw.networkId}/manage`"
                class="text-sm font-medium text-indigo-600 hover:text-indigo-800">Go
                to network</RouterLink>
              <button @click="$emit('edit-user-entitlement', nw.networkId)"
                class="text-sm font-medium text-purple-600 hover:text-purple-800">Edit
                entitlements</button>
            </div>
          </div>

          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr
                  class="bg-slate-50/50 text-[11px] uppercase tracking-wider text-slate-500 border-b border-slate-100">
                  <th class="px-4 py-2 font-semibold">Entitlement</th>
                  <th class="px-4 py-2 font-semibold text-slate-400">Network Base
                  </th>
                  <th class="px-4 py-2 font-semibold text-blue-500">Role Overrides
                  </th>
                  <th class="px-4 py-2 font-semibold text-purple-500">User Overrides
                  </th>
                  <th class="px-4 py-2 font-bold text-slate-800 bg-slate-50">
                    Effective</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-for="key in nw.activeKeys" :key="key"
                  class="hover:bg-slate-50/30 transition-colors">
                  <td class="px-4 py-2 font-medium text-slate-600">{{ formatKey(key)
                  }}
                  </td>

                  <td class="px-4 py-2 text-slate-400">
                    {{ formatValue(nw.baseEnt[key]) }}
                  </td>

                  <td class="px-4 py-2 text-blue-600 font-medium">
                    <span v-if="nw.roleEnt[key] !== undefined"
                      class="bg-blue-50 px-1.5 py-0.5 rounded">
                      {{ formatValue(nw.roleEnt[key]) }}
                    </span>
                    <span v-else class="text-slate-300">-</span>
                  </td>

                  <td class="px-4 py-2 text-purple-600 font-medium">
                    <span v-if="nw.userEnt[key] !== undefined"
                      class="bg-purple-50 px-1.5 py-0.5 rounded">
                      {{ formatValue(nw.userEnt[key]) }}
                    </span>
                    <span v-else class="text-slate-300">-</span>
                  </td>

                  <td
                    class="px-4 py-2 font-bold bg-slate-50 text-slate-800 border-l border-slate-100">
                    {{ formatValue(nw.effective[key]) }}
                  </td>
                </tr>
                <tr v-if="nw.activeKeys.length === 0">
                  <td colspan="5"
                    class="px-4 py-4 text-center text-xs text-slate-400 italic">No
                    limits
                    configured for this network.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

    </div>

    <div class="flex justify-end pt-4 border-t border-slate-200 mt-4">
      <button type="button"
        class="rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors"
        @click="$emit('close')">
        Close
      </button>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { capitalize, computed } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { User } from '@/types';
import { defaultProxy } from '@/lib/user';
import { entitlementKeys } from '@/composables/useEntitlements';

const props = defineProps<{
  user: User;
}>();

defineEmits<{
  (e: 'close'): void;
  (e: 'edit-user-entitlement', networkId: string): void;
}>();

const proxy = computed(() => defaultProxy(props.user));
const allEntitlementKeys = computed(() => Object.values(entitlementKeys).flat());

// Helper to clean out undefined/null values for accurate merging
const cleanObj = (obj: object) => Object.fromEntries(Object.entries(obj || {}).filter(([_, v]) => {
  void _;
  return v !== undefined && v !== null;
}));

// Formats 'fileCountLimit' into 'File Count'
const formatKey = (key: string) => {
  return capitalize(key.replace(/([A-Z])/g, ' $1').trim());
};

const formatValue = (val?: boolean | number) => {
  if (val === undefined || val === null) return '-';
  if (val === true) return 'Yes';
  if (val === false) return 'No';
  return val.toLocaleString();
};

// Calculates the breakdown per network
const breakdown = computed(() => {
  if (!proxy.value || !proxy.value.networkUsers) return [];

  return proxy.value.networkUsers.map(nu => {
    const networkName = nu.network?.name || 'Unknown Network';
    const baseEnt = nu.network?.entitlement || {} as Record<string, number | boolean>;

    // 1. Calculate Role Aggregate (Max of all roles)
    const roleEnt: Record<string, number | boolean> = {};
    if (nu.networkUserRoles) {
      nu.networkUserRoles.forEach(nur => {
        if (nur.role?.entitlements) {
          Object.entries(cleanObj(nur.role.entitlements)).forEach(([k, v]) => {
            if (typeof v === 'boolean') {
              roleEnt[k] = roleEnt[k] || v; // True wins
            } else if (typeof v === 'number') {
              roleEnt[k] = Math.max((roleEnt[k] as number) || 0, v); // Max wins
            }
          });
        }
      });
    }

    // 2. User Override
    const userEnt = cleanObj(nu.entitlements) || {};

    // 3. Merge: Base < Role < User
    const effective = { ...baseEnt, ...roleEnt, ...userEnt };

    // Find all keys that have *any* value in this network to show in the table
    const activeKeys = allEntitlementKeys.value.filter(key =>
      baseEnt[key] !== undefined || roleEnt[key] !== undefined || userEnt[key] !== undefined
    );

    return {
      networkId: nu.network?.id || Math.random().toString(),
      networkName,
      baseEnt,
      roleEnt,
      userEnt,
      effective,
      activeKeys
    };
  });
});
</script>

<style scoped>
/* Optional styling to make the modal scrollbar subtle */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 10px;
}
</style>