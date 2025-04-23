<template>
  <modal-container :title="`Manage Role: ${roleName}`" @close="$emit('close')">
    <div class="space-y-6">
      <!-- Role Info -->
      <div>
        <h3 class="text-lg font-medium leading-6 text-gray-900">Role Information</h3>
        <p class="mt-1 text-sm text-gray-500">{{ roleDescription }}</p>
      </div>

      <!-- Permissions Section -->
      <div>
        <h3 class="text-lg font-medium leading-6 text-gray-900">Role Permissions</h3>
        <p class="mt-1 text-sm text-gray-500">Manage permissions assigned to this role</p>
        
        <div v-if="loadingPermissions" class="py-4 text-center">
          <p class="text-sm text-gray-500">Loading permissions...</p>
        </div>
        
        <div v-else class="mt-4 max-h-64 overflow-y-auto border rounded-md p-2">
          <div v-if="availablePermissions.length === 0" class="text-sm text-gray-500">
            No permissions available
          </div>
          
          <!-- Group permissions by category -->
          <!-- <div v-for="category in permissionCategories" :key="category" class="mb-4"> -->
            <!-- <h4 class="font-medium text-gray-700 mb-2">{{ category }}</h4> -->
            <!-- v-for="permission in permissionsByCategory[category]"  -->
            <div 
              v-for="permission in availablePermissions" 
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
          <!-- </div> -->
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
          @click="$emit('update', localForm)"
        >
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>Save Changes</span>
        </button>
      </div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { Role, Permission } from '@/types';

const props = defineProps({
  network: {
    type: Object,
    required: true
  },
  selectedRole: {
    type: Object as () => Role | null,
    required: true
  },
  isSubmitting: {
    type: Boolean,
    default: false
  },
  availablePermissions: {
    type: Array as () => Permission[],
    required: true
  },
  loadingPermissions: {
    type: Boolean,
    default: false
  },
  manageRoleForm: {
    type: Object,
    required: true
  }
});

defineEmits(['close', 'update']);

// Create a local reactive copy
const localForm = ref({ ...props.manageRoleForm });

// Sync if prop changes externally (optional, depending on your use case)
watch(() => props.manageRoleForm, (newVal) => {
  localForm.value = { ...newVal };
});

const roleName = computed(() => {
  return props.selectedRole?.name || 'Unknown Role';
});

const roleDescription = computed(() => {
  return props.selectedRole?.description || '';
});

// Group permissions by category
// const permissionCategories = computed(() => {
//   const categories = props.availablePermissions.map(p => p.category);
//   return [...new Set(categories)].sort();
// });

// const permissionsByCategory = computed(() => {
//   const result: Record<string, Permission[]> = {};
  
//   props.availablePermissions.forEach(permission => {
//     if (!result[permission.category]) {
//       result[permission.category] = [];
//     }
//     result[permission.category].push(permission);
//   });
  
//   // Sort permissions within each category
//   Object.keys(result).forEach(category => {
//     result[category].sort((a, b) => a.name.localeCompare(b.name));
//   });
  
//   return result;
// });
</script>