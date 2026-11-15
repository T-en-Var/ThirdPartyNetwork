<template>
  <div>
    <h2 class="my-networks-tab__title">Networks You've Joined</h2>
    <div class="network-card-grid">
      <NetworkCard
        v-for="network in networks"
        :key="network.id"
        :network="network"
        :can-manage="checkCanManage(network)"
      />
      <div
        v-if="!networks.length"
        class="network-card-grid__empty"
      >
        You haven't joined any networks yet. Browse available networks to join.
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import NetworkCard from '@/components/NetworkCard.vue';
import { useAuthStore } from '@/stores/auth';
import type { NetworkDto } from '@/types';

const authStore = useAuthStore();

defineProps<{
  networks: NetworkDto[];
}>();

const checkCanManage = (network: NetworkDto) => authStore.canI.manageNetwork(network);
</script>
