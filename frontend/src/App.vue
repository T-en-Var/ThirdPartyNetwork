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

    <div
      style="position: fixed; z-index: 100000; width: 100px; height: 100px; background-color: red"
      @click="debug"
    ></div>
    <div
      style="
        position: fixed;
        z-index: 100000;
        left: 100px;
        width: 100px;
        height: 100px;
        background-color: green;
        color: white;
        padding: 1rem;
      "
      @click="start"
    >
      <span>{{ timeLeft }}</span>
    </div>
  </main>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router';
import NavBar from '@/components/NavBar.vue';
import AppFooter from '@/components/AppFooter.vue';
import { computed, onMounted, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { initializeApiClient } from './api/api';
import { useAuthStore } from './stores/auth';
import { useGlobalStore } from './stores/global';
import ToastManager from './components/toasts/ToastManager.vue';
import AuthModal from '@/components/modals/AuthModal.vue';
import UnauthorizedModal from '@/components/modals/UnauthorizedModal.vue';
import { decodeJWT } from './lib/utils.ts';
import type { AccessTokenClaims } from './types/token.ts';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const global = useGlobalStore();

const timeLeft = ref(0.0);
let loop: ReturnType<typeof setInterval> | null = null;
let decoded: AccessTokenClaims = {} as AccessTokenClaims;

onMounted(async () => {
  initializeApiClient(router, route);

  await authStore.initialize();
  start();
});

async function debug() {
  await authStore.refreshTokens();
  start();
}

function start() {
  const token = authStore.accessToken;
  if (!token) return;

  decoded = decodeJWT<AccessTokenClaims>(token);

  loop = setInterval(() => {
    if (loop && (!decoded.exp || timeLeft.value < 0)) clearInterval(loop);

    timeLeft.value = decoded.exp - Date.now() / 1000;
  }, 50);
}

const showNavbar = computed(() => {
  return !(route.meta.showNavbar === false || route.query.hideNavbar !== undefined);
});
</script>
