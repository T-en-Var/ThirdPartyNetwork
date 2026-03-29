<template>
  <div class="container mx-auto px-4 py-8">
    <div class="overflow-hidden rounded-lg bg-white py-16 text-center shadow-md">
      <div class="flex flex-col items-center justify-center">
        <div class="mb-6 rounded-full bg-yellow-50 p-6">
          <svg
            class="h-16 w-16 text-yellow-500"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M20.618 5.984A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01"
            />
          </svg>
        </div>
        <h1 class="mb-2 text-6xl font-bold text-gray-800">403</h1>
        <p class="mb-3 text-xl text-gray-600">Unauthorized Access</p>
        <p class="mx-auto mb-8 max-w-lg text-gray-500">
          You are not allowed to access this resource.<br />
          Sometimes something goes wrong and you have to log in again.<br />
          If there is another issue at hand, please contact your administrator.
        </p>
        <div class="flex space-x-4">
          <button
            @click="handleBack"
            class="rounded-md border border-gray-300 px-6 py-3 text-gray-700 transition-colors duration-200 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2"
          >
            Go back
          </button>
          <button
            @click="handleLogin"
            class="rounded-md border border-gray-300 px-6 py-3 text-gray-700 transition-colors duration-200 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2"
          >
            Login
          </button>
          <router-link
            to="/"
            class="rounded-md bg-blue-600 px-6 py-3 text-white transition-colors duration-200 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
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
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

onMounted(() => {
  authStore.setModalOpen(true);
  authStore.setModalMode('login');
});

function handleLogin() {
  authStore.setModalOpen(true);
  authStore.setModalMode('login');
}

function handleBack() {
  router.go(-1);
}
</script>
