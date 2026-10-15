<template>
  <nav class="navbar">
    <div class="navbar__inner">
      <!-- Logo -->
      <div
        class="navbar__logo"
        :class="{ 'navbar__logo--hidden': isMobileOpen }"
      >
        <RouterLink
          to="/"
          class="navbar__logo-text"
          >TrojoNetworks</RouterLink
        >
      </div>

      <!-- Links -->
      <div
        class="navbar__links"
        :class="{ 'navbar__links--mobile-open': isMobileOpen }"
      >
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
      <div
        class="navbar__actions"
        :class="{ 'navbar__actions--mobile-open': isMobileOpen }"
      >
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

      <!-- Mobile -->
      <div class="navbar-mobile__menu-btn">
        <MenuIcon
          class="btn icon"
          v-if="!isMobileOpen"
          @click="isMobileOpen = true"
        >
        </MenuIcon>
        <CloseIcon
          class="btn icon"
          v-else
          @click="isMobileOpen = false"
        >
        </CloseIcon>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import UserProfileDropdown from './UserProfileDropdown.vue';
import { useAuthStore } from '@/stores/auth';
import { onMounted, ref } from 'vue';
import MenuIcon from './icons/MenuIcon.vue';
import CloseIcon from './icons/CloseIcon.vue';

const authStore = useAuthStore();

const isSuperAdmin = ref(false);
const isLoadingAuth = ref(true);

const isMobileOpen = ref(false);

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
  border-bottom: 1px solid var(--primary-accent-color);
  box-shadow: 0px 2px 10px rgba(0, 0, 0, 0.65);
  padding: 0 10%;
  background-color: white;

  @media (max-width: $layout-sm) {
    padding: 0 1rem;
  }
}
.navbar__inner {
  display: flex;
  align-items: center;
  justify-content: row;
  flex-wrap: wrap;
}
.navbar__logo {
  border-right: 1px solid var(--primary-accent-color);
  margin: 0.5rem 1rem;

  @media (max-width: $layout-sm) {
    &.navbar__logo--hidden {
      display: none;
    }
  }
}
.navbar__logo-text {
  padding: 0.5rem 1rem;
  cursor: pointer;
  display: inline-block;
  transition: 0.3s;

  &:hover {
    translate: -5px -5px;
    text-shadow: 10px 10px 1px rgba(0, 0, 0, 0.3);
    scale: 1.05;
  }

  @media (max-width: $layout-sm) {
    padding: 0;
  }
}
.navbar__links {
  display: flex;
  flex-wrap: wrap;

  @media (max-width: $layout-sm) {
    display: none;
    padding: 0.3rem 0;

    &.navbar__links--mobile-open {
      display: flex;
    }
  }
}
.navbar__link {
  padding: 0.2rem 0.7rem;

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

  @media (max-width: $layout-sm) {
    display: none;

    &.navbar__actions--mobile-open {
      display: flex;
    }
  }
}
.navbar__login-btn {
}
.navbar__signup-btn {
  border-left: 1px black solid;
  padding-left: 0.5rem;
}
.navbar-mobile__menu-btn {
  display: none;

  @media (max-width: $layout-sm) {
    display: block;
  }
}
</style>
