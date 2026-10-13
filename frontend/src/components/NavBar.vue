<template>
  <nav class="navbar">
    <div class="navbar__inner">
      <!-- Logo -->
      <div class="navbar__logo">
        <RouterLink
          to="/"
          class="navbar__logo-text"
          >TrojoNetworks</RouterLink
        >
      </div>

      <!-- Links -->
      <div class="navbar__links">
        <RouterLink
          to="/"
          class="navbar__link"
          active-class="navbar__link--active"
          >Home</RouterLink
        >
        <RouterLink
          to="/about"
          class="navbar__link"
          active-class="navbar__link--active"
          >About</RouterLink
        >
        <RouterLink
          v-if="authStore.isAuthenticated"
          to="/networks"
          class="navbar__link"
          active-class="navbar__link--active"
          >Networks
        </RouterLink>
        <RouterLink
          v-if="isSuperAdmin && !isLoadingAuth"
          to="/admin"
          class="navbar__link"
          active-class="navbar__link--active"
          >Admin
        </RouterLink>
      </div>

      <!-- Actions -->
      <div class="navbar__actions">
        <template v-if="!authStore.isAuthenticated">
          <button
            class="navbar__login-btn"
            @click="openLoginModal"
          >
            Log In
          </button>
          <button
            class="navbar__signup-btn"
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
