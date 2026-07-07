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
        v-for="networkAccess in networkAccesses"
        :key="networkAccess.access.id"
        class="rounded-md border border-gray-200 p-4"
      >
        <div class="flex items-start">
          <div class="flex h-6 items-center">
            <input
              :id="networkAccess.access.id"
              v-model="internalAccesses[networkAccess.access.id].value"
              type="checkbox"
              class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              :disabled="networkAccess.isRequired && isAlreadyAccepted(networkAccess.access.id)"
              @change="handleAccessChange"
              :aria-required="networkAccess.isRequired ? 'true' : 'false'"
            />
          </div>
          <div class="ml-3">
            <label
              :for="networkAccess.access.id"
              class="block text-sm font-medium text-gray-700"
            >
              {{ networkAccess.access.name }}
              <span
                v-if="networkAccess.isRequired"
                class="ml-1 text-red-500"
                >(Required)</span
              >
              <span
                v-if="isAlreadyAccepted(networkAccess.access.id)"
                class="ml-1 text-xs text-green-500"
              >
                (Accepted)</span
              >
            </label>
            <p class="text-sm text-gray-500">{{ networkAccess.access.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { NetworkAccess, NetworkUserAccess } from '@/types';

onMounted(() => {
  console.log('Hello?');
});

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

const internalAccesses = ref<Record<string, UserAccessState>>({ ...props.initialUserAccesses });
console.log(internalAccesses);
console.log(internalAccesses);
console.log(internalAccesses);

watch(
  () => props.initialUserAccesses,
  (newAccesses) => {
    internalAccesses.value = { ...newAccesses };
  },
  { deep: true },
);

const isAlreadyAccepted = (accessId: string) => {
  return props.networkUserAccesses?.some((nua) => nua.accessId === accessId && nua.isAccepted);
};

const handleAccessChange = (e: Event) => {
  const currentElementId = (e.target as HTMLInputElement).id;
  const isChecked = (e.target as HTMLInputElement).checked;

  const accessDefinition = props.networkAccesses.find((a) => a.accessId === currentElementId);

  if (accessDefinition) {
    internalAccesses.value[currentElementId] = { value: isChecked, userChecked: true };

    emit('access-change', currentElementId, isChecked, accessDefinition.isRequired);
  }
};

defineExpose({
  userAccesses: internalAccesses,
});
</script>
