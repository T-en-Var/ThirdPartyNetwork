<template>
  <nav
    class="fixed top-0 z-50 w-full border-b border-slate-200/50 bg-white/80 backdrop-blur-md transition-all duration-300"
  >
    <div class="mx-auto flex h-16 max-w-7xl items-center justify-between px-6">
      <!-- Logo -->
      <div class="logo-container">
        <RouterLink
          to="/"
          class="logo-text"
          >TrojoNetworks</RouterLink
        >
      </div>
      <!-- <RouterLink to="/" class="font-bold text-xl tracking-tight flex items-center gap-2 text-slate-900 hover:opacity-80 transition">
        <div class="w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-indigo-600 shadow-lg shadow-purple-500/30"></div>
        TrojoNetworks
      </RouterLink> -->

      <!-- Links -->
      <div class="hidden gap-8 text-sm font-medium text-slate-600 md:flex">
        <RouterLink
          to="/"
          class="transition-colors hover:text-purple-600"
          active-class="text-purple-600"
          >Home</RouterLink
        >
        <RouterLink
          to="/about"
          class="transition-colors hover:text-purple-600"
          active-class="text-purple-600"
          >About</RouterLink
        >
        <RouterLink
          v-if="authStore.isAuthenticated"
          to="/networks"
          class="transition-colors hover:text-purple-600"
          active-class="text-purple-600"
          >Networks
        </RouterLink>
        <RouterLink
          v-if="isSuperAdmin && !isLoadingAuth"
          to="/admin"
          class="transition-colors hover:text-purple-600"
          active-class="text-purple-600"
          >Admin
        </RouterLink>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-4">
        <template v-if="!authStore.isAuthenticated">
          <button
            class="px-3 py-2 text-sm font-medium text-slate-600 transition hover:text-purple-600"
            @click="openLoginModal"
          >
            Log In
          </button>
          <button
            class="rounded-full bg-slate-900 px-5 py-2 text-sm font-medium text-white shadow-lg shadow-slate-900/20 transition hover:bg-slate-800"
            @click="openSignupModal"
          >
            Sign Up
          </button>
        </template>
        <UserProfileDropdown v-else />
      </div>
    </div>

    <AuthModal />

    <UnauthorizedModal />
  </nav>
</template>

<script setup lang="ts">
import AuthModal from '@/components/modals/AuthModal.vue';
import UnauthorizedModal from '@/components/modals/UnauthorizedModal.vue';
import UserProfileDropdown from './UserProfileDropdown.vue';
import { useAuthStore } from '@/stores/auth';
import { onMounted, ref } from 'vue';

const authStore = useAuthStore();

const isSuperAdmin = ref(false);
const isLoadingAuth = ref(true);

onMounted(async () => {
  isSuperAdmin.value = await authStore.isSuperAdmin();
  isLoadingAuth.value = false;
});

const openLoginModal = () => {
  authStore.setModalOpen(true);
  authStore.setModalMode('login');
};

const openSignupModal = () => {
  authStore.setModalOpen(true);
  authStore.setModalMode('signup');
};
</script>

<style scoped>
.logo-container {
  flex: 0 0 auto;
}

.logo-text {
  font-family: 'Rufina', serif;
  padding: 1rem 2rem;
  cursor: pointer;
  display: inline-block;
  transition: 0.3s;
}

.logo-text:hover {
  translate: -5px -5px;
  text-shadow: 10px 10px 1px rgba(0, 0, 0, 0.3);
  scale: 1.05;
}
</style>
