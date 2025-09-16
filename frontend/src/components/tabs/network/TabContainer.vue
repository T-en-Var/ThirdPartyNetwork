<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden">
    <div class="border-b border-gray-200">
      <nav class="flex">
        <button 
          @click="$emit('tabChanged', 'users')" 
          class="px-6 py-4 font-medium text-sm"
          :class="activeTab === 'users' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-600 hover:text-gray-800'"
        >
          Users
        </button>
        <button 
          @click="$emit('tabChanged', 'roles')" 
          class="px-6 py-4 font-medium text-sm"
          :class="activeTab === 'roles' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-600 hover:text-gray-800'"
        >
          Roles
        </button>
        <button 
          @click="$emit('tabChanged', 'accesses')" 
          class="px-6 py-4 font-medium text-sm"
          :class="activeTab === 'accesses' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-600 hover:text-gray-800'"
        >
          Accesses
        </button>
        <button 
          @click="$emit('tabChanged', 'files')" 
          class="px-6 py-4 font-medium text-sm"
          :class="activeTab === 'files' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-gray-600 hover:text-gray-800'"
        >
          Files
        </button>
      </nav>
    </div>
    
    <div class="p-6">
      <!-- Users Tab -->
      <users-tab
        v-if="activeTab === 'users'"
        :network="network"
        @add-user="$emit('addUser')"
        @manage-user="$emit('manageUser', $event)"
        @remove-user="$emit('removeUser', $event)"
      />
      
      <!-- Roles Tab -->
      <roles-tab
        v-if="activeTab === 'roles'"
        :network="network"
        @add-role="$emit('addRole')"
        @manage-role="$emit('manageRole', $event)"
        @remove-role="$emit('removeRole', $event)"
      />
      
      <!-- Accesses Tab -->
      <accesses-tab
        v-if="activeTab === 'accesses'"
        :network="network"
        @add-access="$emit('addAccess')"
        @toggle-access-required="$emit('toggleAccessRequired', $event)"
        @remove-access="$emit('removeAccess', $event)"
      />
      
      <!-- Accesses Tab -->
      <files-tab
        v-if="activeTab === 'files'"
        :network="network"
        @add-file="$emit('addFile')"
        @edit-file="$emit('editFile')"
        @remove-file="$emit('removeFile', $event)"
        @toggle-visibility="$emit('toggleFileVisibility', $event)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network, NetworkUser, Role, NetworkAccess, NetworkFile } from '@/types';
import UsersTab from './UsersTab.vue';
import RolesTab from './RolesTab.vue';
import AccessesTab from './AccessesTab.vue';
import FilesTab from './FilesTab.vue';

defineProps<{
  network: Network;
  activeTab: string;
}>();

defineEmits<{
  (e: 'tabChanged', tab: string): void;
  (e: 'addUser'): void;
  (e: 'manageUser', user: NetworkUser): void;
  (e: 'removeUser', user: NetworkUser): void;
  (e: 'addRole'): void;
  (e: 'manageRole', role: Role): void;
  (e: 'removeRole', role: Role): void;
  (e: 'addAccess'): void;
  (e: 'toggleAccessRequired', access: NetworkAccess): void;
  (e: 'removeAccess', access: NetworkAccess): void;
  (e: 'addFile'): void;
  (e: 'editFile', file: NetworkFile): void;
  (e: 'removeFile', file: NetworkFile): void;
  (e: 'toggleFileVisibility', file: NetworkFile): void;
}>();
</script>