<template>
  <modal-container title="Add Role to Network" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div class="mb-6">
        <label for="name" class="block text-sm font-semibold text-gray-800 mb-2">
          Role Name
        </label>
        <input 
          id="name" 
          v-model="localForm.name" 
          type="text"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
          required />
      </div>

      <div class="rounded-md bg-gray-50 p-3">
        <h4 class="text-sm font-medium text-gray-700">Role Description:</h4>
        <input 
          id="name" 
          v-model="localForm.description" 
          type="text"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
          required />
      </div>

      <div v-if="loading" class="py-4 text-center">
        <p class="text-sm text-gray-500">Loading permissions...</p>
      </div>

      <div v-else class="mt-4 max-h-64 overflow-y-auto border rounded-md p-2">
        <div v-if="permissions.length === 0" class="text-sm text-gray-500">
          No permissions available
        </div>

        <div v-else-if="error != null">
          {{ error }}
        </div>
        
        <div 
          v-for="permission in permissions" 
          :key="permission.id"
          class="flex items-center py-1 ml-2"
        >
          <input
            :id="`perm-${permission.id}`"
            type="checkbox"
            :value="permission.id"
            v-model="localForm.permissionIds"
            class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
          />
          <label :for="`perm-${permission.id}`" class="ml-2 block text-sm text-gray-700">
            {{ permission.name }}
            <span class="text-xs text-gray-500 ml-1">({{ permission.id }})</span>
          </label>
        </div>
      </div>

      <div class="flex items-center">
        <input id="isRequired" v-model="localForm.isDefault" type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500" />
        <label for="isRequired" class="ml-2 block text-sm text-gray-700">
          Automatically add this role to new users.
        </label>
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting || !localForm.name">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Role</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { RoleForm } from '@/types';
import usePermissions from '@/composables/usePermissions';

const emit = defineEmits(['close', 'add-role']);
const { permissions, loading, error, fetchPermissions } = usePermissions();

const isSubmitting = ref(false);

const localForm = ref<RoleForm>({
  name: '',
  description: '',
  permissionIds: [],
  isDefault: false,
});

onMounted(async () => {
  await fetchPermissions();
});

function handleSubmit() {
  isSubmitting.value = true;
  emit('add-role', localForm);
}
</script>