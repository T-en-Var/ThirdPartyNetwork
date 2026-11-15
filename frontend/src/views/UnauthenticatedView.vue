<template>
  <div class="status-page">
    <div class="status-page__card">
      <div class="status-page__content">
        <div class="status-page__icon status-page__icon--warning">
          <svg
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
        <h1 class="status-page__code">401</h1>
        <p class="status-page__subtitle">Unauthenticated Access</p>
        <p class="status-page__description">
          You are not logged in. Thus you cannot access this resource.
        </p>
        <div class="status-page__actions">
          <button
            @click="handleLogin"
            class="btn btn--outline"
          >
            Login
          </button>
          <router-link
            to="/"
            class="btn btn--primary"
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

const authStore = useAuthStore();

onMounted(() => {
  authStore.setModalOpen(true);
  authStore.setModalMode('login');
});

function handleLogin() {
  authStore.setModalOpen(true);
  authStore.setModalMode('login');
}
</script>
