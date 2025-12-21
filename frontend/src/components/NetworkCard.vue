<template>
  <div class="bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden hover:shadow-md transition-shadow" v-if="network">
    <div class="p-4">
      <div class="flex items-center mb-3">
        <div class="logo-container">
          <CloudinaryFile v-if="network.imageFile" :display-only="true" :file="network.imageFile" class="w-10 max-h-10 object-cover" />
          <div v-else class="logo">
            <img 
              :src="`https://ui-avatars.com/api/?name=${network?.name}&size=24&background=random`"
              :alt="network.name" />
          </div>
        </div>
        <div class="flex justify-between items-center w-full">
          <div class="ml-3">
            <h2 class="text-lg font-semibold text-gray-800">{{ network.name }}</h2>
            <p v-if="network.networkUsers?.length > 0" class="text-sm text-gray-600">{{ network.networkUsers?.length || 0 }} members</p>
          </div>

          <div class="flex gap-1">
            <span v-if="network.isSystemProtected"
              class="px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
              Protected
            </span>
            <span v-if="!network.isPublic"
              class="px-3 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
              Private
            </span>
          </div>
        </div>
      </div>

      <p class="text-gray-600 text-sm mb-4 line-clamp-2">{{ network.description || 'No description available' }}</p>

      <div class="flex justify-between items-center border-t border-gray-200 pt-4 mt-4">
        <RouterLink :to="`/networks/${network.id}`" class="text-blue-600 hover:text-blue-800 text-sm font-medium">
          View Details
        </RouterLink>

        <RouterLink v-if="canManage" :to="`/networks/${network.id}/manage`" class="text-blue-600 hover:text-blue-800 text-sm font-medium">
          Manage
        </RouterLink>

        <button v-if="showJoin" @click="$emit('joinNetwork', network)"
          class="bg-blue-100 text-blue-700 hover:bg-blue-200 px-3 py-1 rounded-md text-sm font-medium">
          Join Network
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network } from '@/types';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
// import { isValidHttpUrl } from '@/lib/utils';

defineProps<{
  network: Network;
  showJoin?: boolean;
  canManage?: boolean;
}>();

defineEmits<{
  (e: 'joinNetwork', network: Network): void;
}>();
</script>

<style scoped>
.logo-container {
  top: 16px;
  left: 16px;
  z-index: 10;
}

.logo {
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 6px;
  font-weight: 600;
  font-size: 12px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.logo img {
  max-width: 100%;
  max-height: 100%;
  border-radius: 6px;
  object-fit: contain;
}
</style>
