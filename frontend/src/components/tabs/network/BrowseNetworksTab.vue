<template>
  <div class="flex justify-between items-center mb-4">
    <h2 class="text-xl font-medium text-gray-800">Public Networks</h2>
    <div class="relative">
      <input type="text" v-model="searchQuery" placeholder="Search networks..."
        class="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
    </div>
  </div>

  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
    <NetworkCard v-for="network in filteredNetworks" :key="network.id"
      :network="network" :show-join="true" @join-network="joinNetwork(network)" />
    <div v-if="!filteredNetworks.length"
      class="col-span-full text-center p-10 text-gray-500">
      No networks found matching your search.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import NetworkCard from '@/components/NetworkCard.vue';
import type { Network } from '@/types';
import { useRouter } from 'vue-router';

const router = useRouter();

const props = defineProps<{
  networks: Network[];
}>();

const searchQuery = ref('');

const filteredNetworks = computed(() => {
  if (!searchQuery.value) return props.networks;

  const query = searchQuery.value.toLowerCase();
  return props.networks.filter(network => network.description.toLowerCase().includes(query) || network.name.toLowerCase().includes(query));
});

const joinNetwork = (network: Network) => {
  router.push(`/networks/${network.id}/join`);
};
</script>