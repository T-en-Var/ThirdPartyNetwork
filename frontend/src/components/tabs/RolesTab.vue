<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-medium text-gray-800">Network Roles</h2>
      <button 
        @click="$emit('addRole')" 
        class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Add Role
      </button>
    </div>
    
    <div class="space-y-4">
      <div 
        v-for="role in network.roles" 
        :key="role.id"
        class="bg-gray-50 p-4 rounded-lg border border-gray-200"
      >
        <div class="flex justify-between items-center">
          <div>
            <h3 class="text-lg font-medium text-gray-900">{{ role.name }}</h3>
            <p class="text-sm text-gray-600">{{ role.description }}</p>
          </div>
          <div>
            <button 
              @click="$emit('manageRole', role)" 
              class="text-blue-600 hover:text-blue-900 mr-3"
            >
              Permissions
            </button>
            <button 
              @click="$emit('removeRole', role)" 
              class="text-red-600 hover:text-red-900"
            >
              Remove
            </button>
          </div>
        </div>
        
        <div class="mt-4">
          <h4 class="text-sm font-medium text-gray-700 mb-2">Permissions:</h4>
          <div class="flex flex-wrap gap-2">
            <span 
              v-for="rolePermission in role.rolePermissions" 
              :key="rolePermission.permissionId"
              class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
            >
              {{ rolePermission.permission.name }}
            </span>
            <span v-if="!role.rolePermissions?.length" class="text-sm text-gray-500">No permissions assigned</span>
          </div>
        </div>
      </div>
      
      <div v-if="!network.roles.length" class="p-6 text-center text-gray-500">
        No roles in this network
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network, Role } from '@/types';

defineProps<{
  network: Network;
}>();

defineEmits<{
  (e: 'addRole'): void;
  (e: 'manageRole', role: Role): void;
  (e: 'removeRole', role: Role): void;
}>();
</script>