<template>
  <div class="mb-4 flex items-center justify-between">
    <h2 class="text-xl font-medium text-gray-800">Public Networks</h2>
    <div class="relative">
      <input
        type="text"
        v-model="searchQuery"
        placeholder="Search networks..."
        class="rounded-md border border-gray-300 px-4 py-2 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
    </div>
  </div>

  <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
    <NetworkCard
      v-for="network in filteredNetworks"
      :key="network.id"
      :network="network"
      :show-join="true"
      :can-manage="isSuperAdmin"
      @join-network="joinNetwork(network)"
    />
    <div
      v-if="!filteredNetworks.length"
      class="col-span-full p-10 text-center text-gray-500"
    >
      No networks found matching your search.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import NetworkCard from '@/components/NetworkCard.vue';
import type { Network } from '@/types';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const props = defineProps<{
  networks: Network[];
}>();

const searchQuery = ref('');
const isSuperAdmin = ref(false);

const filteredNetworks = computed(() => {
  if (!searchQuery.value) return props.networks;

  const query = searchQuery.value.toLowerCase();
  return props.networks.filter(
    (network) =>
      network.description.toLowerCase().includes(query) ||
      network.name.toLowerCase().includes(query),
  );
});

onMounted(async () => {
  isSuperAdmin.value = await authStore.isSuperAdmin();
});

const joinNetwork = (network: Network) => {
  router.push(`/networks/${network.id}/join`);
};
</script>
