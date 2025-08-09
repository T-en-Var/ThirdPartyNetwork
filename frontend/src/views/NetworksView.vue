<!-- NetworksPage.vue -->
<template>
  <div class="container mx-auto px-4 py-8 mt-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-2xl font-semibold text-gray-800">Networks</h1>
        <RouterLink to="/networks/create" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition-colors">
          + Create Network
        </RouterLink>
      </div>

      <div class="border-b border-gray-200">
        <nav class="flex">
          <button
            @click="activeTab = 'my-networks'"
            class="px-6 py-4 font-medium text-sm"
            :class="activeTab === 'my-networks' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-600 hover:text-gray-800'"
          >
            My Networks
          </button>
          <button
            @click="activeTab = 'browse-networks'"
            class="px-6 py-4 font-medium text-sm"
            :class="activeTab === 'browse-networks' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-600 hover:text-gray-800'"
          >
            Browse Networks
          </button>
        </nav>
      </div>

      <div v-if="networksState.loading.value || userNetworksState.loading.value" class="p-6 text-center">
        <span>Loading...</span>
      </div>

      <div v-else-if="networksState.error.value || userNetworksState.error.value" class="p-6 bg-red-50 text-red-600">
        <p class="font-medium">Error: {{ networksState.error || userNetworksState.error }}</p>
      </div>

      <div v-else class="p-6">
        <!-- My Networks Tab -->
        <my-networks-tab
          v-if="activeTab === 'my-networks'"
          :networks="userNetworksState.networks.value"
        />
        
        <!-- Browse Networks Tab -->
        <browse-networks-tab
          v-if="activeTab === 'browse-networks'"
          :networks="filteredNetworks"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import useNetworks from '@/composables/useNetworks';
import MyNetworksTab from '@/components/tabs/MyNetworksTab.vue';
import BrowseNetworksTab from '@/components/tabs/BrowseNetworksTab.vue';
import useUsersNetworks from '@/composables/useUserNetworks';

const activeTab = ref('my-networks');
const networksState = useNetworks();
const userNetworksState = useUsersNetworks()

onMounted(async () => {
  await networksState.fetchNetworks();
  await userNetworksState.fetchUserNetworks();
});

const filteredNetworks = computed(() => {
  const userNetworkIds = new Set(userNetworksState.networks.value.map(n => n.id));
  return networksState.networks.value.filter(n => !userNetworkIds.has(n.id));
});
</script>

<style scoped>
.spinner-border {
  width: 3rem;
  height: 3rem;
  border: 3px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}
@keyframes spinner-border {
  100% {
    transform: rotate(360deg);
  }
}
</style>
