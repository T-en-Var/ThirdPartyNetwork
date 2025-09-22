<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div>
      <h1 class="text-2xl font-semibold text-gray-800">
        {{ network.name }}
        <span class="ml-2 px-3 py-1 text-xs font-medium rounded-full"
          :class="network.isSystemProtected ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'">
          {{ network.isSystemProtected ? 'Protected' : 'Standard' }}
        </span>
      </h1>
    </div>

    <div class="p-6 pb-0">
      <!-- Network Stats -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-sm font-medium text-gray-500 uppercase">Users</h3>
          <p class="mt-2 text-3xl font-semibold">{{ network.networkUsers.length }}</p>
        </div>
        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-sm font-medium text-gray-500 uppercase">Roles</h3>
          <p class="mt-2 text-3xl font-semibold">{{ network.roles.length }}</p>
        </div>
        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-sm font-medium text-gray-500 uppercase">Access Requirements</h3>
          <p class="mt-2 text-3xl font-semibold">{{ network.networkAccesses.length }}</p>
        </div>
      </div>
    </div>

    <div class="p-6 pt-0">
      <span class="bg-gray-50 px-2 py-1 text-gray-500 normal-case font-medium black rounded-lg font-mono">ID: {{
        network.id }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network } from '@/types';

defineProps<{
  network: Network;
}>();

defineEmits<{
  (e: 'navigateBack'): void;
  (e: 'editNetwork'): void;
}>();
</script>