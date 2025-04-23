<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-medium text-gray-800">Network Accesses</h2>
      <button 
        @click="$emit('addAccess')" 
        class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Add Access
      </button>
    </div>
    
    <div class="space-y-4">
      <div
        v-for="networkAccess in network.networkAccesses"
        :key="networkAccess.accessId"
        class="bg-gray-50 p-4 rounded-lg border border-gray-200"
      >
        <div class="flex justify-between items-center">
          <div>
            <h3 class="text-lg font-medium text-gray-900">{{ networkAccess.access.name }}</h3>
            <p class="text-sm text-gray-600">{{ networkAccess.access.description }}</p>
          </div>
          <div>
            <button
              @click="$emit('toggleAccessRequired', networkAccess)"
              class="px-4 py-2 mr-3 text-sm border rounded-md"
              :class="networkAccess.isRequired ? 'bg-green-100 text-green-800 border-green-200' : 'bg-gray-100 text-gray-800 border-gray-200'"
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
        
        <div class="mt-4">
          <h4 class="text-sm font-medium text-gray-700 mb-2">Users with this access:</h4>
          <div class="flex flex-wrap gap-2">
            <div 
              v-for="networkUser in getUsersWithAccess(networkAccess.accessId)" 
              :key="networkUser.id"
              class="flex items-center px-2 py-1 bg-white rounded-md border border-gray-200"
            >
              <div class="h-6 w-6 flex-shrink-0 mr-2">
                <img
                  :src="isValidHttpUrl(networkUser.userProxy.profilePicture) ? networkUser.userProxy.profilePicture : `https://ui-avatars.com/api/?name=${networkUser.userProxy.firstName}+${networkUser.userProxy.lastName}&size=24&background=random`"
                  :alt="networkUser.userProxy.username"
                  class="h-6 w-6 rounded-full"
                />
              </div>
              <span class="text-xs">{{ networkUser.userProxy.firstName }} {{ networkUser.userProxy.lastName }}</span>
              <span 
                v-for="userAccess in networkAccess.access.networkUserAccesses.filter(uA => uA.networkUser.id == networkUser.id)"
                :key="userAccess.networkUser.id"
                class="ml-2 inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium"
                :class="getUserAccessForAccessId(networkUser, networkAccess.accessId)?.isAccepted ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'"
              >
                {{ getUserAccessForAccessId(networkUser, networkAccess.accessId)?.isAccepted ? 'Accepted' : 'Pending' }}
              </span>
            </div>
            <span v-if="getUsersWithAccess(networkAccess.accessId).length === 0" class="text-sm text-gray-500">
              No users have this access
            </span>
          </div>
        </div>
      </div>
      
      <div v-if="!network.networkAccesses?.length" class="p-6 text-center text-gray-500">
        No accesses defined in this network
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { isValidHttpUrl } from '@/lib/utils';
import type { Network, NetworkAccess, NetworkUser, NetworkUserAccess } from '@/types';

const props = defineProps<{
  network: Network;
}>();

defineEmits<{
  (e: 'addAccess'): void;
  (e: 'toggleAccessRequired', access: NetworkAccess): void;
  (e: 'removeAccess', access: NetworkAccess): void;
}>();

// Helper function to find users with a specific access
const getUsersWithAccess = (accessId: string): NetworkUser[] => {
  if (!props.network.networkUsers) return [];
  
  return props.network.networkUsers.filter(user => 
    user.networkUserAccesses?.some(userAccess => 
      userAccess.access.id === accessId
    )
  );
};

// Helper function to get specific user access for an access ID
const getUserAccessForAccessId = (networkUser: NetworkUser, accessId: string): NetworkUserAccess | undefined => {
  return networkUser.networkUserAccesses?.find(userAccess => 
    userAccess.access.id === accessId
  );
};
</script>