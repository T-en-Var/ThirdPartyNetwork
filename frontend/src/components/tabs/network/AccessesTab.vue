<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-medium text-gray-800">Network Accesses</h2>
      <button @click="$emit('addAccess')"
        class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
        Add Access
      </button>
    </div>

    <div class="space-y-4">
      <div v-for="networkAccess in network.networkAccesses"
        :key="networkAccess.accessId"
        class="bg-gray-50 p-4 rounded-lg border border-gray-200">
        <div class="flex justify-between items-center">
          <div>
            <h3 class="text-lg font-medium text-gray-900">{{
              networkAccess.access.name }}</h3>
            <p class="text-sm text-gray-600">{{ networkAccess.access.description }}
            </p>
          </div>
          <div>
            <button @click="$emit('toggleAccessRequired', networkAccess)"
              class="px-4 py-2 mr-3 text-sm border rounded-md"
              :class="networkAccess.isRequired ? 'bg-green-100 text-green-800 border-green-200' : 'bg-gray-100 text-gray-800 border-gray-200'">
              {{ networkAccess.isRequired ? 'Required' : 'Optional' }}
            </button>
            <button @click="$emit('removeAccess', networkAccess)"
              class="text-red-600 hover:text-red-900">
              Remove
            </button>
          </div>
        </div>
      </div>

      <div v-if="!network.networkAccesses?.length"
        class="p-6 text-center text-gray-500">
        No accesses defined in this network
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network, NetworkAccess } from '@/types';

defineProps<{
  network: Network;
}>();

defineEmits<{
  (e: 'addAccess'): void;
  (e: 'toggleAccessRequired', access: NetworkAccess): void;
  (e: 'removeAccess', access: NetworkAccess): void;
}>();
</script>