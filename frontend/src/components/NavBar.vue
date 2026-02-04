<template>
  <nav class="fixed top-0 w-full z-50 bg-white/80 backdrop-blur-md border-b border-slate-200/50 transition-all duration-300">
    <div class="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
      <!-- Logo -->
      <div class="logo-container">
        <RouterLink to="/" class="logo-text">TrojoNetworks</RouterLink>
      </div>
      <!-- <RouterLink to="/" class="font-bold text-xl tracking-tight flex items-center gap-2 text-slate-900 hover:opacity-80 transition">
        <div class="w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-indigo-600 shadow-lg shadow-purple-500/30"></div>
        TrojoNetworks
      </RouterLink> -->

      <!-- Links -->
      <div class="hidden md:flex gap-8 text-sm font-medium text-slate-600">
        <RouterLink to="/" class="hover:text-purple-600 transition-colors" active-class="text-purple-600">Home</RouterLink>
        <RouterLink to="/about" class="hover:text-purple-600 transition-colors" active-class="text-purple-600">About</RouterLink>
        <RouterLink v-if="authStore.isAuthenticated" to="/networks" class="hover:text-purple-600 transition-colors" active-class="text-purple-600">Networks
        </RouterLink>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-4">
        <template v-if="!authStore.isAuthenticated">
          <button class="text-sm font-medium text-slate-600 hover:text-purple-600 transition px-3 py-2" @click="openLoginModal">Log In</button>
          <button class="px-5 py-2 rounded-full bg-slate-900 text-white text-sm font-medium hover:bg-slate-800 transition shadow-lg shadow-slate-900/20" @click="openSignupModal">Sign Up</button>
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

const authStore = useAuthStore();

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
  font-family: "Rufina", serif;
  padding: 1rem 2rem;
  cursor: pointer;
  display: inline-block;
  transition: .3s;
}

.logo-text:hover {
  translate: -5px -5px;
  text-shadow: 10px 10px 1px rgba(0, 0, 0, 0.3);
  scale: 1.05;
}</style>
