<template>
  <div v-if="loading || networkAccesses.length === 0" class="mb-8">
    <LoadingSpinner v-if="loading" />
    <p v-else-if="networkAccesses.length === 0" class="text-gray-600">
      This network does not require any special permissions.
    </p>
  </div>

  <div v-else>
    <h3 class="text-lg font-medium text-gray-800 mb-4">Required Data Access</h3>
    <p class="text-gray-600 mb-4">Update your consent for the following data accesses:</p>

    <div class="space-y-4 mb-6">
      <div v-for="access in networkAccesses" :key="access.accessId" class="p-4 border border-gray-200 rounded-md">
        <div class="flex items-start">
          <div class="flex items-center h-6">
            <input :id="access.accessId" v-model="internalAccesses[access.accessId].value" type="checkbox"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              :disabled="access.isRequired && isAlreadyAccepted(access.accessId)"
              @change="handleAccessChange" :aria-required="access.isRequired ? 'true' : 'false'" />
          </div>
          <div class="ml-3">
            <label :for="access.accessId" class="block text-sm font-medium text-gray-700">
              {{ access.access.name }}
              <span v-if="access.isRequired" class="text-red-500 ml-1">(Required)</span>
              <span v-if="isAlreadyAccepted(access.accessId)" class="text-green-500 ml-1 text-xs"> (Accepted)</span>
            </label>
            <p class="text-sm text-gray-500">{{ access.access.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { NetworkAccess, NetworkUserAccess } from '@/types';

// Define types for component props
interface UserAccessState {
  value: boolean;
  userChecked: boolean;
}

const props = defineProps<{
  networkAccesses: NetworkAccess[];
  initialUserAccesses: Record<string, UserAccessState>;
  networkUserAccesses: NetworkUserAccess[] | undefined;
  loading: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'access-change']);

// Internal state for checkboxes
const internalAccesses = ref<Record<string, UserAccessState>>({ ...props.initialUserAccesses });

// Watch for initial data load or changes from the parent
watch(() => props.initialUserAccesses, (newAccesses) => {
  internalAccesses.value = { ...newAccesses };
}, { deep: true });

// Function to check if an access was already accepted by the network user
const isAlreadyAccepted = (accessId: string) => {
  return props.networkUserAccesses?.some(nua => nua.accessId === accessId && nua.isAccepted);
};

// Handle local change and emit event for validation/final submission
const handleAccessChange = (e: Event) => {
  const currentElementId = (e.target as HTMLInputElement).id;
  const isChecked = (e.target as HTMLInputElement).checked;
  
  // Find the corresponding access definition
  const accessDefinition = props.networkAccesses.find(a => a.accessId === currentElementId);
  
  if (accessDefinition) {
    // Update internal state and mark as user-checked
    internalAccesses.value[currentElementId] = { value: isChecked, userChecked: true };

    // Emit the change for parent validation (especially for required fields)
    emit('access-change', currentElementId, isChecked, accessDefinition.isRequired);
  }
};

// Expose the final state for the parent to read on submission
defineExpose({
  userAccesses: internalAccesses,
});
</script>
