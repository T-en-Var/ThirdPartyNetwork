<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-2xl font-semibold text-gray-800">Your Networks</h1>
        <RouterLink to="/networks/create" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition-colors">
          + Add Network
        </RouterLink>
      </div>
      
      <div v-if="loading" class="p-6 text-center">
        <div class="spinner-border text-blue-500" role="status">
          <span class="sr-only">Loading...</span>
        </div>
      </div>
      
      <div v-else-if="error" class="p-6 bg-red-50 text-red-600">
        <p class="font-medium">Error: {{ error }}</p>
      </div>
      
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 p-6">
        <NetworkCard 
          v-for="network in networks" 
          :key="network.id" 
          :network="network"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import useUsersNetworks from '@/composables/useUsersNetworks';
import NetworkCard from '@/components/NetworkCard.vue';

const { networks, loading, error, fetchUsersNetworks } = useUsersNetworks();

onMounted(() => {
  fetchUsersNetworks();
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
