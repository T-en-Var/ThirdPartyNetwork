<template>
  <header
    v-if="showNavbar"
    class="app-shell__header"
  >
    <NavBar />
  </header>

  <main class="app-main">
    <div class="app-main__content">
      <RouterView />
    </div>

    <AppFooter v-if="showNavbar" />
    <ToastManager
      :toasts="global.activeToasts"
      :is-fetching="global.isFetching"
      @remove="global.removeToast"
    />
  </main>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router';
import NavBar from '@/components/NavBar.vue';
import AppFooter from '@/components/AppFooter.vue';
import { computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { initializeApiClient } from './api/api';
import { useAuthStore } from './stores/auth';
import { useGlobalStore } from './stores/global';
import ToastManager from './components/toasts/ToastManager.vue';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const global = useGlobalStore();

onMounted(async () => {
  initializeApiClient(router, route);

  await authStore.initialize();
});

const showNavbar = computed(() => {
  return !(route.meta.showNavbar === false || route.query.hideNavbar !== undefined);
});
</script>
