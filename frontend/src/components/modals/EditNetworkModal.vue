<template>
  <modal-container :title="'Edit Network'" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label for="name" class="block text-sm font-medium text-gray-700">Network Name</label>
        <input
          id="name"
          v-model="form.name"
          type="text"
          class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          required
        />
      </div>
      
      <div class="flex items-center">
        <input
          id="isSystemProtected"
          v-model="form.isSystemProtected"
          type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
        />
        <label for="isSystemProtected" class="ml-2 block text-sm text-gray-700">
          System Protected (Prevents accidental deletion)
        </label>
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
          type="submit"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting"
        >
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>Save Changes</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';

const props = defineProps({
  network: {
    type: Object,
    required: true
  },
  isSubmitting: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['close', 'update']);

const form = ref({
  name: '',
  isSystemProtected: false
});

onMounted(() => {
  // Initialize the form with network data
  if (props.network) {
    form.value.name = props.network.name;
    form.value.isSystemProtected = props.network.isSystemProtected;
  }
});

function handleSubmit() {
  emit('update', {
    name: form.value.name,
    isSystemProtected: form.value.isSystemProtected
  });
}
</script>