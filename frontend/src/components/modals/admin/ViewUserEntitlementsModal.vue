<template>
  <modal-container
    title="User Entitlements"
    @close="$emit('close')"
    size="md"
  >
    <div
      v-if="proxy"
      class="custom-scrollbar max-h-[75vh] space-y-6 overflow-y-auto pr-2"
    >
      <div class="mb-4">
        <h2 class="text-lg font-bold text-slate-900">{{ proxy.firstName }} {{ proxy.lastName }}</h2>
        <p class="text-sm text-slate-500">@{{ proxy.username }}</p>
      </div>

      <div>
        <div
          v-if="breakdown.length === 0"
          class="text-sm italic text-slate-500"
        >
          This user is not a member of any networks.
        </div>

        <div
          v-for="nw in breakdown"
          :key="nw.networkId"
          class="mb-6 overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm"
        >
          <div
            class="flex items-center justify-between border-b border-slate-200 bg-slate-50 px-4 py-3"
          >
            <div>
              <span class="font-semibold text-slate-700">{{ nw.networkName }}</span>
              <div class="flex gap-1">
                <span
                  v-for="(val, key) in nw.baseEnt"
                  v-show="key.startsWith('allow')"
                  :key="key"
                  class="inline-flex items-center rounded-md border border-slate-200 bg-white px-2 text-[11px] font-medium shadow-sm"
                >
                  <span class="mr-1 font-normal uppercase tracking-tighter text-slate-400">{{
                    key.replace('allow', '')
                  }}</span>
                  <span
                    class="ml-0.5 border-l border-slate-200 pl-1.5 font-normal"
                    :class="{ 'text-green-500': val === true, 'text-red-500': val === false }"
                  >
                    {{ val }}
                  </span>
                </span>
              </div>
            </div>
            <div class="flex gap-4">
              <RouterLink
                :to="`/networks/${nw.networkId}/manage`"
                class="text-sm font-medium text-indigo-600 hover:text-indigo-800"
                >Go to network</RouterLink
              >
              <button
                @click="$emit('edit-user-entitlement', nw.networkId)"
                class="text-sm font-medium text-purple-600 hover:text-purple-800"
              >
                Edit entitlements
              </button>
            </div>
          </div>

          <div class="overflow-x-auto">
            <table class="w-full border-collapse text-left text-sm">
              <thead>
                <tr
                  class="border-b border-slate-100 bg-slate-50/50 text-[11px] uppercase tracking-wider text-slate-500"
                >
                  <th class="px-4 py-2 font-semibold">Entitlement</th>
                  <th class="px-4 py-2 font-semibold text-slate-400">Network Base</th>
                  <th class="px-4 py-2 font-semibold text-blue-500">Role Overrides</th>
                  <th class="px-4 py-2 font-semibold text-purple-500">User Overrides</th>
                  <th class="bg-slate-50 px-4 py-2 font-bold text-slate-800">Effective</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr
                  v-for="key in nw.activeKeys"
                  :key="key"
                  class="transition-colors hover:bg-slate-50/30"
                >
                  <td class="px-4 py-2 font-medium text-slate-600">{{ formatKey(key) }}</td>

                  <td class="px-4 py-2 text-slate-400">
                    {{ formatValue(nw.baseEnt[key]) }}
                  </td>

                  <td class="px-4 py-2 font-medium text-blue-600">
                    <span
                      v-if="nw.roleEnt[key] !== undefined"
                      class="rounded bg-blue-50 px-1.5 py-0.5"
                    >
                      {{ formatValue(nw.roleEnt[key]) }}
                    </span>
                    <span
                      v-else
                      class="text-slate-300"
                      >-</span
                    >
                  </td>

                  <td class="px-4 py-2 font-medium text-purple-600">
                    <span
                      v-if="nw.userEnt[key] !== undefined"
                      class="rounded bg-purple-50 px-1.5 py-0.5"
                    >
                      {{ formatValue(nw.userEnt[key]) }}
                    </span>
                    <span
                      v-else
                      class="text-slate-300"
                      >-</span
                    >
                  </td>

                  <td
                    class="border-l border-slate-100 bg-slate-50 px-4 py-2 font-bold text-slate-800"
                  >
                    {{ formatValue(nw.effective[key]) }}
                  </td>
                </tr>
                <tr v-if="nw.activeKeys.length === 0">
                  <td
                    colspan="5"
                    class="px-4 py-4 text-center text-xs italic text-slate-400"
                  >
                    No limits configured for this network.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-4 flex justify-end border-t border-slate-200 pt-4">
      <button
        type="button"
        class="rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition-colors hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        @click="$emit('close')"
      >
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

const cleanObj = (obj: object) =>
  Object.fromEntries(
    Object.entries(obj || {}).filter(([_, v]) => {
      void _;
      return v !== undefined && v !== null;
    }),
  );

const formatKey = (key: string) => {
  return capitalize(key.replace(/([A-Z])/g, ' $1').trim());
};

const formatValue = (val?: boolean | number) => {
  if (val === undefined || val === null) return '-';
  if (val === true) return 'Yes';
  if (val === false) return 'No';
  return val.toLocaleString('nl-NL');
};

const breakdown = computed(() => {
  if (!proxy.value || !proxy.value.networkUsers) return [];

  return proxy.value.networkUsers.map((nu) => {
    const networkName = nu.network?.name || 'Unknown Network';
    const baseEnt = nu.network?.entitlement || ({} as Record<string, number | boolean>);

    const roleEnt: Record<string, number | boolean> = {};
    if (nu.networkUserRoles) {
      nu.networkUserRoles.forEach((nur) => {
        if (nur.role?.entitlements) {
          Object.entries(cleanObj(nur.role.entitlements)).forEach(([k, v]) => {
            if (typeof v === 'boolean') {
              roleEnt[k] = roleEnt[k] || v;
            } else if (typeof v === 'number') {
              roleEnt[k] = Math.max((roleEnt[k] as number) || 0, v);
            }
          });
        }
      });
    }

    const userEnt = cleanObj(nu.entitlements) || {};

    const effective = { ...baseEnt, ...roleEnt, ...userEnt };

    const activeKeys = allEntitlementKeys.value.filter(
      (key) =>
        baseEnt[key] !== undefined || roleEnt[key] !== undefined || userEnt[key] !== undefined,
    );

    return {
      networkId: nu.network?.id || Math.random().toString(),
      networkName,
      baseEnt,
      roleEnt,
      userEnt,
      effective,
      activeKeys,
    };
  });
});
</script>

<style scoped>
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
