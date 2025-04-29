<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-medium text-gray-800">Network Users</h2>
      <button 
        @click="$emit('addUser')" 
        class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Add User
      </button>
    </div>
    
    <div class="overflow-x-auto">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              User
            </th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Roles
            </th>
            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Access Status
            </th>
            <th scope="col" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="user in network.networkUsers" :key="user.id">
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex items-center">
                <div class="h-10 w-10 flex-shrink-0">
                  <ProfileAvatar :userProxy="user.userProxy" :size="10" />
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">
                    {{ user.userProxy.firstName }} {{ user.userProxy.lastName }}
                  </div>
                  <div class="text-sm text-gray-500">{{ user.userProxy.email }}</div>
                </div>
              </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex flex-wrap gap-2">
                <span 
                  v-for="userRole in user.networkUserRoles" 
                  :key="userRole.role.id"
                  class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                >
                  {{ userRole.role.name }}
                </span>
                <span v-if="!user.networkUserRoles?.length" class="text-sm text-gray-500">No roles assigned</span>
              </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex flex-wrap gap-2">
                <span 
                  v-for="userAccess in user.networkUserAccesses" 
                  :key="userAccess.access.id"
                  class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
                  :class="userAccess.isAccepted ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'"
                >
                  {{ userAccess.access.name }}: {{ userAccess.isAccepted ? 'Accepted' : 'Pending' }}
                </span>
                <span v-if="!user.networkUserAccesses?.length" class="text-sm text-gray-500">No access requirements</span>
              </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
              <button 
                @click="$emit('manageUser', user)" 
                class="text-blue-600 hover:text-blue-900 mr-3"
              >
                Manage
              </button>
              <button 
                @click="$emit('removeUser', user)" 
                class="text-red-600 hover:text-red-900"
              >
                Remove
              </button>
            </td>
          </tr>
          <tr v-if="!network.networkUsers?.length">
            <td colspan="4" class="px-6 py-10 text-center text-gray-500">
              No users in this network
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import ProfileAvatar from '@/components/ProfileAvatar.vue'
import type { Network, NetworkUser } from '@/types';

defineProps<{
  network: Network;
}>();

defineEmits<{
  (e: 'addUser'): void;
  (e: 'manageUser', user: NetworkUser): void;
  (e: 'removeUser', user: NetworkUser): void;
}>();
</script>
