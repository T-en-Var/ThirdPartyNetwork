<template>
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

  <LoadingErrorComponent :loading="loading" :error="error" :has-value="!!networks" />

  <div v-if="!!networks && !loading"
    class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="bg-slate-50 border-b border-slate-200">
            <th
              class="w-1/4 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Network Name</th>
            <th
              class="w-0 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              ID
            </th>
            <th
              class="w-1/2 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Entitlements</th>
            <th
              class="w-32 px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider text-right">
              Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100">
          <tr v-for="network in networks" :key="network.id"
            class="hover:bg-slate-50/50 transition-colors">
            <td class="px-6 py-4">
              <div class="flex gap-2">
                <div class="font-medium text-slate-900">{{ network.name }}</div>

                <span class="px-3 py-1 text-xs h-min font-medium rounded-full"
                  :class="network.isSystemProtected ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'">
                  {{ network.isSystemProtected ? 'Protected' : 'Standard' }}
                </span>
              </div>
              <div class="text-sm text-slate-500 truncate max-w-xs">{{
                network.description }}</div>

            </td>
            <td class="px-6 py-4 text-sm text-slate-500 font-mono">{{ network.id }}
            </td>
            <td class="px-6 py-4 max-w-xs">
              <div class="flex flex-wrap gap-1.5">
                <template v-if="Object.keys(network.entitlement || {}).length > 0">
                  <span v-for="(val, key) in network.entitlement"
                    v-show="key != 'networkId'" :key="key"
                    class="inline-flex items-center rounded-md border border-slate-200 bg-white px-2 py-0.5 text-[11px] font-medium shadow-sm transition-hover hover:border-blue-300">
                    <span
                      class="text-slate-400 font-normal mr-1 uppercase tracking-tighter">{{
                        key }}</span>
                    <span class="font-bold border-l border-slate-200 pl-1.5 ml-0.5"
                      :class="{
                        'text-green-500': val === true,
                        'text-red-500': val === false,
                        'text-slate-700 font-bold': val !== true && val !== false
                      }">{{ val }}</span>
                  </span>
                </template>

                <span v-else class="text-slate-400 text-xs flex items-center italic">
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
            <td colspan="4" class="px-6 py-12 text-center text-slate-500">No networks
              found.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="mainNetwork">
      <EditNetworkEntitlementsModal v-if="showEditNetworkModal && selectedNetwork"
        :is-submitting="isSubmitting" :main-network="mainNetwork"
        :network="selectedNetwork" @close="showEditNetworkModal = false"
        @update="handleUpdateNetwork" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import EditNetworkEntitlementsModal from '@/components/modals/admin/EditNetworkEntitlementsModal.vue';
import useNetworks from '@/composables/useNetworks';
import { useHistoryStore } from '@/stores/history';
import type { Network, SettableEntitlement } from '@/types';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const historyStore = useHistoryStore();

const { execute: fetchNetworks, error, loading, data: networks } = useNetworks().fetchNetworks;
const { execute: forceFetchNetworks } = useNetworks().forceFetchNetworks;
const { execute: fetchMainNetwork, data: mainNetwork } = useNetworks().fetchMainNetworkDetails;
const { execute: updateNetworkEntitlement } = useNetworks().updateNetworkEntitlement;

const showEditNetworkModal = ref(false);
const selectedNetwork = ref<Network | null>(null);

const isSubmitting = ref(false);

defineEmits<{
  (e: 'delete-network', network: Network): void;
}>();

onMounted(async () => {
  await fetchMainNetwork();
  await fetchNetworks();
});

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
