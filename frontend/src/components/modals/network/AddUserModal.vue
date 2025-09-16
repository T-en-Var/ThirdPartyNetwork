<template>
  <modal-container title="Add User to Network" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label for="user" class="block text-sm font-medium text-gray-700">Select User</label>
        <div class="relative mt-1">
          <select
            id="user"
            v-model="form.userId"
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            required
          >
            <option value="" disabled selected>Select a user</option>
            <option v-for="user in availableUsers" :key="user.id" :value="user.id">
              {{ user.firstName }} {{ user.lastName }} ({{ user.email }})
            </option>
          </select>
        </div>
        <p v-if="loadingUsers" class="mt-1 text-sm text-gray-500">Loading users...</p>
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700">Assign Roles</label>
        <div class="mt-2 max-h-48 overflow-y-auto border rounded-md p-2">
          <div v-if="availableRoles.length === 0" class="text-sm text-gray-500">
            No roles available in this network
          </div>
          <div 
            v-for="role in availableRoles" 
            :key="role.id"
            class="flex items-center py-1"
          >
            <input
              :id="`role-${role.id}`"
              type="checkbox"
              :value="role.id"
              v-model="form.roleIds"
              class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
            />
            <label :for="`role-${role.id}`" class="ml-2 block text-sm text-gray-700">
              {{ role.name }}
            </label>
          </div>
        </div>
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button
          type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        <button
          type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting || !form.userId"
        >
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add User</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import api from '@/api/api';
import type { UserProxy, Network, Role } from '@/types';
import { useGlobalStore } from '@/stores/global';

const global = useGlobalStore();

const props = defineProps<{
  network: Network;
  isSubmitting?: boolean;
}>();

const emit = defineEmits(['close', 'add-user']);

const form = ref({
  userId: '',
  roleIds: [] as string[]
});

const availableUsers = ref<UserProxy[]>([]);
const availableRoles = ref<Role[]>([]);
const loadingUsers = ref(true);

onMounted(async () => {
  global.startFetching();
  loadingUsers.value = true;
  
  try {
    // Get all users that aren't already in the network
    const response = await api.get<UserProxy[]>('/users/');
    const allUsers = response.data || [];
    
    // Filter out users that are already in the network
    const networkUserIds = props.network.networkUsers.map(nu => nu.userProxy.id);
    availableUsers.value = allUsers.filter(user => !networkUserIds.includes(user.id));
    
    // Get roles from the network
    availableRoles.value = props.network.roles;
  } catch (error) {
    console.error('Error fetching users:', error);
  } finally {
    loadingUsers.value = false;
  global.stopFetching();
}
});

function handleSubmit() {
  emit('add-user', {
    userId: form.value.userId,
    roleIds: form.value.roleIds
  });
}
</script>