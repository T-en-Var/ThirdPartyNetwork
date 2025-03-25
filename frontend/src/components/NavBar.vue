<template>
  <div class="wrapper">
    <nav class="corporate-navbar">
      <div class="logo-container">
        <span class="logo-text">Thirdpartynetwork</span>
      </div>
      <div class="nav-links">
        <RouterLink to="/" class="nav-link" active-class="active">Home</RouterLink>
        <RouterLink to="/about" class="nav-link" active-class="active">About</RouterLink>
        <RouterLink v-if="authStore.isAuthenticated()" to="/networks" class="nav-link" active-class="active">Networks
        </RouterLink>
      </div>
      <div class="nav-actions">
        <button v-if="!authStore.isAuthenticated()" class="btn-login" @click="openLoginModal">Log In</button>
        <button v-if="!authStore.isAuthenticated()" class="btn-signup" @click="openSignupModal">Sign Up</button>
        <UserProfileDropdown v-if="authStore.isAuthenticated()" />
      </div>
    </nav>

    <AuthModal />
  </div>
</template>

<script setup lang="ts">
import AuthModal from '@/components/AuthModal.vue';
import UserProfileDropdown from './UserProfileDropdown.vue';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();

const openLoginModal = () => {
  authStore.setModelOpen(true);
  authStore.setModelMode('login');
};

const openSignupModal = () => {
  authStore.setModelOpen(true);
  authStore.setModelMode('signup');
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Rufina:wght@400;700&display=swap');

.wrapper {
  width: 100%;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  z-index: 100;
  background-color: #ffffff;
}

.corporate-navbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 70px;
  padding: 0 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

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
}

.company-logo {
  height: 40px;
}

.nav-links {
  display: flex;
  gap: 2rem;
  flex: 1;
  justify-content: center;
}

.nav-link {
  text-decoration: none;
  color: #555;
  font-weight: 500;
  font-size: 1rem;
  padding: 0.5rem 0;
  position: relative;
  transition: color 0.3s ease;
}

.nav-link:hover,
.nav-link.active {
  color: #0066cc;
}

.nav-link:after {
  content: '';
  position: absolute;
  width: 0;
  height: 2px;
  bottom: 0;
  left: 0;
  background-color: #0066cc;
  transition: width 0.3s ease;
}

.nav-link:hover:after,
.nav-link.active:after {
  width: 100%;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.btn-login {
  background: none;
  border: none;
  color: #555;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: .3rem;
  transition: background-color 0.3s ease;
}

.btn-login:hover {
  background-color: #f0f0f0;
  color: #0066cc;
}

.btn-signup {
  background-color: #0066cc;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.btn-signup:hover {
  background-color: #0052a3;
}

@media (max-width: 768px) {
  .corporate-navbar {
    flex-direction: column;
    height: auto;
    padding: 1rem;
  }

  .nav-links {
    margin: 1rem 0;
  }

  .nav-actions {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
