<template>
  <modal-container title="Add Access Requirement" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div class="mb-6">
        <label for="access" class="block text-sm font-semibold text-gray-800 mb-2">
          Select Access Requirement
        </label>
        <div class="relative">
          <select id="access" v-model="form.accessId"
            class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all"
            required>
            <option value="" disabled selected>Select an access requirement</option>
            <option v-for="access in availableAccesses" :key="access.id" :value="access.id">
              {{ access.name }}
            </option>
          </select>
        </div>
        <p v-if="accessesState.loading.value" class="mt-2 text-sm text-gray-500 italic">
          Loading access requirements...
        </p>
      </div>


      <!-- Access description if one is selected -->
      <div v-if="selectedAccessDescription" class="rounded-md bg-gray-50 p-3">
        <h4 class="text-sm font-medium text-gray-700">Access Description:</h4>
        <p class="text-sm text-gray-600">{{ selectedAccessDescription }}</p>
      </div>

      <div class="flex items-center">
        <input id="isRequired" v-model="form.isRequired" type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500" />
        <label for="isRequired" class="ml-2 block text-sm text-gray-700">
          Required for network access (Users must have this access approved)
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
          :disabled="isSubmitting || !form.accessId">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Access</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { Network } from '@/types';
import useAccesses from '@/composables/useAccesses';

const props = withDefaults(defineProps<{
  network: Network;
  isSubmitting?: boolean;
}>(), {
  isSubmitting: false
});

const emit = defineEmits(['close', 'add-access']);

const form = ref({
  accessId: '',
  isRequired: true
});

const accessesState = useAccesses();

const availableAccesses = computed(() => {
  // Filter out accesses that are already in the network
  const networkAccessIds = props.network.networkAccesses.map(na => na.access.id);
  return accessesState.accesses.value.filter(access => !networkAccessIds.includes(access.id));
});

const selectedAccessDescription = computed(() => {
  if (!form.value.accessId) return '';
  const access = accessesState.accesses.value.find(a => a.id === form.value.accessId);
  return access?.description || '';
});

onMounted(async () => {
  await accessesState.fetchAccesses();
});

function handleSubmit() {
  emit('add-access', {
    accessId: form.value.accessId,
    isRequired: form.value.isRequired
  });
}
</script>