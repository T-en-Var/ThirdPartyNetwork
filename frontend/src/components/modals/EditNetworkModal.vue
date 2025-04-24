<template>
  <modal-container :title="'Edit Network'" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-6">
      <div class="mb-6">
        <label for="name" class="block text-sm font-semibold text-gray-800 mb-2">
          Network Name
        </label>
        <input id="name" v-model="localForm.name" type="text"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 disabled:cursor-not-allowed transition-all"
          :disabled="props.network.isSystemProtected" required />
        <p v-if="props.network.isSystemProtected"
          class="mt-2 text-sm text-yellow-700 bg-yellow-50 border border-yellow-200 rounded-md p-2">
          This is a system-protected network and cannot be modified.
        </p>
      </div>

      <div class="flex items-center">
        <input id="isRequired" v-model="localForm.isPublic" type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500" />
        <label for="isRequired" class="ml-2 block text-sm text-gray-700">
          Network is public.
        </label>
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button v-if="!props.network.isSystemProtected" type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          :disabled="isSubmitting">
          <span v-if="isSubmitting" class="flex items-center">
            <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none"
              viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
              </path>
            </svg>
            Saving...
          </span>
          <span v-else>Save Changes</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { Network, NetworkForm } from '@/types';

const props = withDefaults(defineProps<{
  network: Network,
  isSubmitting: boolean
}>(), {
  isSubmitting: false
});

const emit = defineEmits(['close', 'update']);

const localForm = ref<NetworkForm>({
  name: '',
  isPublic: false,
});

onMounted(() => {
  localForm.value.name = props.network.name;
  localForm.value.isPublic = props.network.isPublic;
});

function handleSubmit() {
  // Only proceed if not system protected
  if (!props.network.isSystemProtected) {
    emit('update', localForm);
  }
}
</script>
