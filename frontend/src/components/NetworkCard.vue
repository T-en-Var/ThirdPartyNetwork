<template>
  <div
    class="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md"
    v-if="network"
  >
    <div class="p-4">
      <div class="mb-3 flex items-center">
        <div class="logo-container">
          <CloudinaryFile
            v-if="network.imageFile"
            :display-only="true"
            :file="network.imageFile"
            class="h-10 w-10 object-cover"
          />
          <div
            v-else
            class="logo h-10 w-10"
          >
            <img
              :src="`https://ui-avatars.com/api/?name=${network?.name}&size=24&background=random`"
              :alt="network.name"
            />
          </div>
        </div>
        <div class="flex w-full items-center justify-between">
          <div class="ml-3">
            <h2 class="text-lg font-semibold text-gray-800">{{ network.name }}</h2>
            <p
              v-if="network.networkUsers?.length > 0"
              class="text-sm text-gray-600"
            >
              {{ network.networkUsers?.length || 0 }} members
            </p>
          </div>

          <div class="flex gap-1">
            <span
              v-if="network.isSystemProtected"
              class="rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-800"
            >
              Protected
            </span>
            <span
              v-if="!network.isPublic"
              class="rounded-full bg-red-100 px-3 py-1 text-xs font-medium text-red-800"
            >
              Private
            </span>
          </div>
        </div>
      </div>

      <p class="mb-4 line-clamp-2 text-sm text-gray-600">
        {{ network.description || 'No description available' }}
      </p>

      <div class="mt-4 flex items-center justify-between border-t border-gray-200 pt-4">
        <RouterLink
          :to="`/networks/${network.id}`"
          class="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          View Details
        </RouterLink>

        <RouterLink
          v-if="canManage"
          :to="`/networks/${network.id}/manage`"
          class="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          Manage
        </RouterLink>

        <button
          v-if="showJoin"
          @click="$emit('joinNetwork', network)"
          class="rounded-md bg-blue-100 px-3 py-1 text-sm font-medium text-blue-700 hover:bg-blue-200"
        >
          Join Network
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network } from '@/types';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

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
