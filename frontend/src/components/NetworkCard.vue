<template>
  <div class="bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow">
    <div class="p-6">
      <div class="flex justify-between items-center mb-4">
        <h2 class="text-lg font-semibold text-gray-800">{{ network.name }}</h2>
        <span 
          class="px-3 py-1 rounded-full text-xs font-medium"
          :class="network.isSystemProtected ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'"
        >
          {{ network.isSystemProtected ? 'Protected' : 'Standard' }}
        </span>
      </div>
      
      <div class="mb-4">
        <p class="text-sm text-gray-600 mb-2">Network Users</p>
        <div class="flex -space-x-2">
          <template v-if="network.networkUsers && network.networkUsers.length">
            <div 
              v-for="user in network.networkUsers.slice(0, 3)" 
              :key="user.id" 
              class="inline-block h-8 w-8 rounded-full border-2 border-white"
            >
              <img 
                :src="isValidHttpUrl(user.userProxy.profilePicture) ? user.userProxy.profilePicture : `https://ui-avatars.com/api/?name=${user.userProxy.firstName}+${user.userProxy.lastName}&background=random`" 
                :alt="user.userProxy.username" 
                class="h-full w-full rounded-full object-cover"
              />
            </div>
            <div 
              v-if="network.networkUsers.length > 3" 
              class="h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-xs text-gray-600 border-2 border-white"
            >
              +{{ network.networkUsers.length - 3 }}
            </div>
          </template>
          <p v-else class="text-sm text-gray-500">No users</p>
        </div>
      </div>
      
      <div class="flex justify-between items-center border-t border-gray-200 pt-4 mt-4">
        <div class="text-sm text-gray-500">
          <span>Total Users: {{ network.networkUsers?.length || 0 }}</span>
        </div>
        <div class="flex space-x-2">
          <button class="text-blue-600 hover:text-blue-800 text-sm">
            Details
          </button>
          <RouterLink :to="`networks/${network.id}/manage`" class="text-gray-600 hover:text-gray-800 text-sm">
            Manage
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { isValidHttpUrl } from '@/lib/utils';
import type { Network } from '@/types';
import { defineProps } from 'vue';
import { RouterLink } from 'vue-router';

defineProps<{
  network: Network;
}>();
</script>