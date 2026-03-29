<template>
  <div
    v-if="loading || networkAccesses.length === 0"
    class="mb-8"
  >
    <LoadingSpinner v-if="loading" />
    <p
      v-else-if="networkAccesses.length === 0"
      class="text-gray-600"
    >
      This network does not require any special permissions.
    </p>
  </div>

  <div v-else>
    <h3 class="mb-4 text-lg font-medium text-gray-800">Required Data Access</h3>
    <p class="mb-4 text-gray-600">Update your consent for the following data accesses:</p>

    <div class="mb-6 space-y-4">
      <div
        v-for="access in networkAccesses"
        :key="access.accessId"
        class="rounded-md border border-gray-200 p-4"
      >
        <div class="flex items-start">
          <div class="flex h-6 items-center">
            <input
              :id="access.accessId"
              v-model="internalAccesses[access.accessId].value"
              type="checkbox"
              class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              :disabled="access.isRequired && isAlreadyAccepted(access.accessId)"
              @change="handleAccessChange"
              :aria-required="access.isRequired ? 'true' : 'false'"
            />
          </div>
          <div class="ml-3">
            <label
              :for="access.accessId"
              class="block text-sm font-medium text-gray-700"
            >
              {{ access.access.name }}
              <span
                v-if="access.isRequired"
                class="ml-1 text-red-500"
                >(Required)</span
              >
              <span
                v-if="isAlreadyAccepted(access.accessId)"
                class="ml-1 text-xs text-green-500"
              >
                (Accepted)</span
              >
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
watch(
  () => props.initialUserAccesses,
  (newAccesses) => {
    internalAccesses.value = { ...newAccesses };
  },
  { deep: true },
);

// Function to check if an access was already accepted by the network user
const isAlreadyAccepted = (accessId: string) => {
  return props.networkUserAccesses?.some((nua) => nua.accessId === accessId && nua.isAccepted);
};

// Handle local change and emit event for validation/final submission
const handleAccessChange = (e: Event) => {
  const currentElementId = (e.target as HTMLInputElement).id;
  const isChecked = (e.target as HTMLInputElement).checked;

  // Find the corresponding access definition
  const accessDefinition = props.networkAccesses.find((a) => a.accessId === currentElementId);

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
