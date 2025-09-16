<template>
  <modal-container
    title="Create New Proxy"
    @close="$emit('close')"
    :close-on-escape="!isSubmitting"
  >
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Info message -->
      <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3 text-sm text-yellow-800">
        Any empty fields will be automatically filled with values from your <strong>default proxy</strong>.
      </div>

      <!-- Username -->
      <div>
        <label for="username" class="block text-sm font-semibold text-gray-800 mb-2">Username</label>
        <input
          id="username"
          v-model="form.username"
          type="text"
          placeholder="e.g. johndoe"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
        />
      </div>

      <!-- First Name -->
      <div>
        <label for="firstName" class="block text-sm font-semibold text-gray-800 mb-2">First Name</label>
        <input
          id="firstName"
          v-model="form.firstName"
          type="text"
          placeholder="e.g. John"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
        />
      </div>

      <!-- Last Name -->
      <div>
        <label for="lastName" class="block text-sm font-semibold text-gray-800 mb-2">Last Name</label>
        <input
          id="lastName"
          v-model="form.lastName"
          type="text"
          placeholder="e.g. Doe"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
        />
      </div>

      <!-- Email -->
      <div>
        <label for="email" class="block text-sm font-semibold text-gray-800 mb-2">Email</label>
        <input
          id="email"
          v-model="form.email"
          type="email"
          placeholder="e.g. john@example.com"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
        />
      </div>

      <!-- Image URL -->
      <!-- <div>
        <label for="imageUrl" class="block text-sm font-semibold text-gray-800 mb-2">Profile Image URL</label>
        <input
          id="imageUrl"
          v-model="form.imageUrl"
          type="url"
          placeholder="https://example.com/image.jpg"
          class="block w-full rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 transition-all"
        />
      </div> -->

      <!-- Actions -->
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
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          :disabled="isSubmitting"
        >
          <span v-if="isSubmitting">Creating...</span>
          <span v-else>Create Proxy</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { UserProxy } from '@/types';

const props = defineProps<{
  isSubmitting?: boolean;
  defaultProxy: UserProxy;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'create-proxy', payload: Partial<UserProxy>): void;
}>();

const form = ref<Partial<UserProxy>>({
  username: '',
  firstName: '',
  lastName: '',
  email: '',
  // imageUrl: ''
});

function handleSubmit() {
  // Fill missing fields with default proxy values
  const newProxy: Partial<UserProxy> = {
    username: form.value.username || props.defaultProxy.username,
    firstName: form.value.firstName || props.defaultProxy.firstName,
    lastName: form.value.lastName || props.defaultProxy.lastName,
    email: form.value.email || props.defaultProxy.email,
    // imageUrl: form.value.imageUrl || props.defaultProxy.imageUrl
  };

  emit('create-proxy', newProxy);
}
</script>
