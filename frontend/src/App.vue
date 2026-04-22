<template>
  <header
    v-if="showNavbar"
    class="static"
  >
    <NavBar />
  </header>

  <main class="flex flex-col">
    <div class="flex-grow">
      <RouterView />
    </div>
    <AppFooter v-if="showNavbar" />
    <!-- <FetchingToast /> -->
    <ToastManager
      :toasts="global.activeToasts"
      :is-fetching="global.isFetching"
      @remove="global.removeToast"
    />

    <!-- <div class="fixed bg-green-500 w-10 h-10 z-50 cursor-pointer" @click="global.addToast({ message: 'hello world!!', duration: 2500 })"></div> -->
    <!-- <div class="fixed bg-red-500 w-10 h-10 z-50 cursor-pointer" @click="console.log((globalCache.get(`networks_019b722f-5d71-7631-812d-6646febcaef2`)))"></div> -->
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
// import { globalCache } from './composables/useApi';

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

<style>
main {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

#app {
  height: 100vh;
  background-color: #f8f8fb;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
</style>
