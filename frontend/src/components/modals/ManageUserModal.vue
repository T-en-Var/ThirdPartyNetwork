<template>
  <modal-container :title="`Manage User: ${userName}`" @close="$emit('close')">
    <div class="space-y-6">
      <!-- Roles Section -->
      <div>
        <h3 class="text-lg font-medium leading-6 text-gray-900">User Roles</h3>
        <p class="mt-1 text-sm text-gray-500">Manage roles assigned to this user</p>
        
        <div class="mt-4 max-h-48 overflow-y-auto border rounded-md p-2">
          <div v-if="availableRoles.length === 0" class="text-sm text-gray-500">
            No roles available in this network
          </div>
          <div 
            v-for="role in availableRoles" 
            :key="role.roleId"
            class="flex items-center py-1"
          >
            <input
              :id="`role-${role.roleId}`"
              type="checkbox"
              :value="role.roleId"
              v-model="localForm.roleIds"
              class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
            />
            <label :for="`role-${role.roleId}`" class="ml-2 block text-sm text-gray-700">
              {{ role.role.name }}
            </label>
          </div>
        </div>
      </div>

      <!-- Access Requirements Section -->
      <div v-if="userAccesses.length > 0">
        <h3 class="text-lg font-medium leading-6 text-gray-900">Access Status</h3>
        <p class="mt-1 text-sm text-gray-500">Manage user's access requirements status</p>
        
        <div class="mt-4 space-y-3">
          <div 
            v-for="access in userAccesses" 
            :key="access.access.id"
            class="flex items-center justify-between py-2 border-b"
          >
            <div>
              <p class="font-medium">{{ access.access.name }}</p>
              <p class="text-sm text-gray-500">{{ access.access.description }}</p>
            </div>
            <div class="flex items-center space-x-2">
              <select
                v-model="localForm.accessStatuses[access.access.id]"
                class="rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              >
                <option value="pending">Pending</option>
                <option value="accepted">Accepted</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <div class="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        <button
          type="button"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting"
          @click="$emit('update', localForm.value)"
        >
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>Save Changes</span>
        </button>
      </div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { NetworkUser } from '@/types';

const props = defineProps({
  network: {
    type: Object,
    required: true
  },
  selectedUser: {
    type: Object as () => NetworkUser | null,
    required: true
  },
  isSubmitting: {
    type: Boolean,
    default: false
  },
  manageUserForm: {
    type: Object,
    required: true
  }
});

defineEmits(['close', 'update']);

// Create a local reactive copy
const localForm = ref({ ...props.manageUserForm });

// Sync if prop changes externally (optional, depending on your use case)
watch(() => props.manageUserForm, (newVal) => {
  localForm.value = { ...newVal };
});

const userName = computed(() => {
  if (!props.selectedUser) return 'Unknown User';
  return `${props.selectedUser.userProxy.firstName} ${props.selectedUser.userProxy.lastName}`;
});

const availableRoles = computed(() => {
  return props.network.networkRoles || [];
});

const userAccesses = computed(() => {
  if (!props.selectedUser) return [];
  return props.selectedUser.networkUserAccesses || [];
});
</script>