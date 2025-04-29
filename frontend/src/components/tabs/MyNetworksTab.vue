<!-- MyNetworksTab.vue -->
<template>
  <div>
    <h2 class="text-xl font-medium text-gray-800 mb-4">Networks You've Joined</h2>
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <NetworkCard
        v-for="network in networks"
        :key="network.id"
        :network="network"
      />
      <div v-if="!networks.length" class="col-span-full text-center p-10 text-gray-500">
        You haven't joined any networks yet. Browse available networks to join.
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import NetworkCard from '@/components/NetworkCard.vue';
import type { Network } from '@/types';

defineProps<{
  networks: Network[];
}>();
</script>
