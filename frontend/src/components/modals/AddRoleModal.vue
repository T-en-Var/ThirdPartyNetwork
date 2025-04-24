<template>
  <modal-container title="Add Role to Network" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label for="role" class="block text-sm font-medium text-gray-700">Select Role</label>
        <div class="relative mt-1">
          <select id="role" v-model="form.roleId"
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            required>
            <option value="" disabled selected>Select a role</option>
            <option v-for="role in availableRoles" :key="role.id" :value="role.id">
              {{ role.name }}
            </option>
          </select>
        </div>
        <p v-if="loadingRoles" class="mt-1 text-sm text-gray-500">Loading roles...</p>
      </div>

      <!-- Role description if one is selected -->
      <div v-if="selectedRoleDescription" class="rounded-md bg-gray-50 p-3">
        <h4 class="text-sm font-medium text-gray-700">Role Description:</h4>
        <p class="text-sm text-gray-600">{{ selectedRoleDescription }}</p>
      </div>

      <div class="flex justify-end space-x-3 pt-4">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting || !form.roleId">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Role</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import api from '@/api/api';
import type { Role, Network } from '@/types';
import { useGlobalStore } from '@/stores/global';

const global = useGlobalStore();

const props = withDefaults(defineProps<{
  network: Network;
  isSubmitting?: boolean;
}>(), {
  isSubmitting: false
});

const emit = defineEmits(['close', 'add-role']);

const form = ref({
  roleId: ''
});

const allRoles = ref<Role[]>([]);
const loadingRoles = ref(true);

const availableRoles = computed(() => {
  // Filter out roles that are already in the network
  const networkRoleIds = props.network.roles.map(r => r.id);
  return allRoles.value.filter(role => !networkRoleIds.includes(role.id));
});

const selectedRoleDescription = computed(() => {
  if (!form.value.roleId) return '';
  const role = allRoles.value.find(r => r.id === form.value.roleId);
  return role?.description || '';
});

onMounted(async () => {
  global.startFetching();
  loadingRoles.value = true;

  try {
    // Get all roles
    const response = await api.get<Role[]>('/roles/');
    allRoles.value = response.data || [];
  } catch (error) {
    console.error('Error fetching roles:', error);
  } finally {
    global.stopFetching();
    loadingRoles.value = false;
  }
});

function handleSubmit() {
  emit('add-role', {
    roleId: form.value.roleId
  });
}
</script>