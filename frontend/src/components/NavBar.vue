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

      <div class="flex__spacer" />

      <!-- Actions -->
      <div class="navbar__actions">
        <template v-if="!authStore.isAuthenticated">
          <button
            class="btn navbar__login-btn"
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

<style scoped lang="scss">
.navbar {
  background: linear-gradient(
    to right,
    color-mix(in srgb, var(--primary-color) 50%, transparent),
    var(--primary-color) 10%,
    var(--primary-color) 90%,
    color-mix(in srgb, var(--primary-color) 50%, transparent)
  );
  border-bottom: 1px solid black;
  box-shadow: 0px 2px 10px rgba(0, 0, 0, 0.65);
  padding: 0 10%;
}
.navbar__inner {
  display: flex;
  align-items: center;
  justify-content: row;
}
.navbar__logo {
}
.navbar__logo-text {
  padding: 1rem 2rem;
  cursor: pointer;
  display: inline-block;
  transition: 0.3s;

  &:hover {
    translate: -5px -5px;
    text-shadow: 10px 10px 1px rgba(0, 0, 0, 0.3);
    scale: 1.05;
  }
}
.navbar__links {
  display: flex;

}
.navbar__link {
  padding: .2rem .7rem;
  border-left: 1px solid black;

  &:first-child {
    border-left: none;
  }

  &:hover {
    text-decoration: underline;
    color: color-mix(in srgb, var(--secondary-color) 50%, transparent);
  }
}
.navbar__link--active {
  color: white;
}
.navbar__actions {
  display: flex;
  gap: 0.7rem;
  white-space: nowrap;
}
.navbar__login-btn {
}
.navbar__signup-btn {
  border-left: 1px black solid;
  padding-left: .5rem;
}
</style>
