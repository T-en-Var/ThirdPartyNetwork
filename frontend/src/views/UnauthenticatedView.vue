<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden text-center py-16">
      <div class="flex flex-col items-center justify-center">
        <div class="bg-yellow-50 rounded-full p-6 mb-6">
          <svg class="h-16 w-16 text-yellow-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
            stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M20.618 5.984A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01" />
          </svg>
        </div>
        <h1 class="text-6xl font-bold text-gray-800 mb-2">401</h1>
        <p class="text-xl text-gray-600 mb-3">Unauthenticated Access</p>
        <p class="text-gray-500 max-w-md mx-auto mb-8">You are not logged in. Thus you cannot access this resource.</p>
        <div class="flex space-x-4">
          <button @click="handleLogin"
            class="px-6 py-3 border border-gray-300 text-gray-700 rounded-md hover:bg-gray-50 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2">
            Login
          </button>
          <router-link to="/"
            class="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2">
            Return Home
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import { onMounted } from 'vue';

const authStore = useAuthStore();

onMounted(() => authStore.setModelOpen(true))

function handleLogin() {
  authStore.setModelOpen(true);
  authStore.setModelMode('login');
}
</script>
