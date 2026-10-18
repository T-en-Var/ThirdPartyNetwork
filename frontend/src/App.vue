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
    <AuthModal />
    <UnauthorizedModal />

    <!-- <div style="position: fixed; z-index: 100000; width: 100px; height: 100px; background-color: red;" @click="debug"></div> -->
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
import AuthModal from '@/components/modals/AuthModal.vue';
import UnauthorizedModal from '@/components/modals/UnauthorizedModal.vue';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const global = useGlobalStore();

onMounted(async () => {
  initializeApiClient(router, route);

  await authStore.initialize();
});

// function debug() {
//   global.addToast({
//     message: "testt",
//     type: "error",
//     duration: 5000,
//   })
// }

const showNavbar = computed(() => {
  return !(route.meta.showNavbar === false || route.query.hideNavbar !== undefined);
});
</script>
