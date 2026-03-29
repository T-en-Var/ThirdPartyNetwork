<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="mb-6 flex items-center justify-between">
      <h2 class="text-xl font-medium text-gray-800">Network Accesses</h2>
      <button
        @click="$emit('addAccess')"
        class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
      >
        Add Access
      </button>
    </div>

    <div class="space-y-4">
      <div
        v-for="networkAccess in network.networkAccesses"
        :key="networkAccess.accessId"
        class="rounded-lg border border-gray-200 bg-gray-50 p-4"
      >
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-lg font-medium text-gray-900">{{ networkAccess.access.name }}</h3>
            <p class="text-sm text-gray-600">{{ networkAccess.access.description }}</p>
          </div>
          <div>
            <button
              @click="$emit('toggleAccessRequired', networkAccess)"
              class="mr-3 rounded-md border px-4 py-2 text-sm"
              :class="
                networkAccess.isRequired
                  ? 'border-green-200 bg-green-100 text-green-800'
                  : 'border-gray-200 bg-gray-100 text-gray-800'
              "
            >
              {{ networkAccess.isRequired ? 'Required' : 'Optional' }}
            </button>
            <button
              @click="$emit('removeAccess', networkAccess)"
              class="text-red-600 hover:text-red-900"
            >
              Remove
            </button>
          </div>
        </div>
      </div>

      <div
        v-if="!network.networkAccesses?.length"
        class="p-6 text-center text-gray-500"
      >
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
