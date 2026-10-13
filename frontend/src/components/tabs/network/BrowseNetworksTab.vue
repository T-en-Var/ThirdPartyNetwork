<template>
  <div class="browse-networks-tab__header">
    <h2 class="browse-networks-tab__title">Public Networks</h2>
    <input
      type="text"
      v-model="searchQuery"
      placeholder="Search networks..."
      class="form-input"
    />
  </div>

  <div class="network-card-grid">
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
      class="network-card-grid__empty"
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
