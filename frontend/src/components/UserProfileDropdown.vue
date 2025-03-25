<template>
  <div class="user-profile-dropdown">
    <button 
      class="profile-button" 
      @click="toggleDropdown" 
      aria-haspopup="true"
      :aria-expanded="isOpen"
    >
      <div class="avatar">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="user-icon">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
          <circle cx="12" cy="7" r="4"></circle>
        </svg>
      </div>
      <span class="dropdown-arrow" :class="{ 'open': isOpen }">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="arrow-icon">
          <polyline points="6 9 12 15 18 9"></polyline>
        </svg>
      </span>
    </button>
    
    <div v-if="isOpen" class="dropdown-menu">
      <div class="dropdown-header">
        <div class="user-info">
          <p class="user-name">{{ user?.username }}</p>
          <p class="user-email">{{ `${user?.firstName} ${user?.lastName}` }}</p>
        </div>
      </div>
      <div class="dropdown-divider"></div>
      <ul class="dropdown-list">
        <li>
          <RouterLink to="/profile" class="dropdown-item" @click="closeDropdown">
            Your Profile
          </RouterLink>
        </li>
        <li>
          <RouterLink to="/account" class="dropdown-item" @click="closeDropdown">
            Account Settings
          </RouterLink>
        </li>
        <li class="dropdown-divider"></li>
        <li>
          <button class="dropdown-item logout-button" @click="handleLogout">
            Sign out
          </button>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';
import type { UserProxy } from '@/types';


const authStore = useAuthStore();
const router = useRouter();
const isOpen = ref(false);

const user = ref<UserProxy | null>(null);

const toggleDropdown = async () => {
  user.value = await authStore.getUser()
  isOpen.value = !isOpen.value;
};

const closeDropdown = () => {
  isOpen.value = false;
};

const handleLogout = () => {
  authStore.logout();
  closeDropdown();
  router.push('/');
};

// Close dropdown when clicking outside
const handleClickOutside = (event: MouseEvent) => {
  const dropdown = document.querySelector('.user-profile-dropdown');
  if (dropdown && !dropdown.contains(event.target as Node) && isOpen.value) {
    closeDropdown();
  }
};

// Listen for clicks outside the dropdown
onMounted(async () => {
  document.addEventListener('click', handleClickOutside);
});

// Clean up the event listener
onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>

<style scoped>
.user-profile-dropdown {
  position: relative;
}

.profile-button {
  display: flex;
  align-items: center;
  background: none;
  border: none;
  padding: 0.5rem;
  border-radius: 1rem;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.profile-button:hover {
  background-color: #f0f0f0;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #e1e4e8;
  color: #555;
}

.user-icon {
  width: 20px;
  height: 20px;
}

.dropdown-arrow {
  margin-left: 4px;
  display: flex;
  align-items: center;
  transform: rotate(0deg);
  transition: transform 0.2s ease;
}

.dropdown-arrow.open {
  transform: rotate(180deg);
}

.arrow-icon {
  width: 16px;
  height: 16px;
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  right: 0;
  width: 200px;
  background-color: white;
  border-radius: 6px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  margin-top: 8px;
  z-index: 1000;
  overflow: hidden;
  border: 1px solid #eaeaea;
}

.dropdown-header {
  padding: 1rem;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.user-name {
  font-weight: 600;
  margin: 0;
  font-size: 0.9rem;
}

.user-email {
  color: #666;
  margin: 0;
  font-size: 0.8rem;
}

.dropdown-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.dropdown-item {
  display: block;
  padding: 0.75rem 1rem;
  color: #333;
  text-decoration: none;
  font-size: 0.9rem;
  transition: background-color 0.2s ease;
  cursor: pointer;
  text-align: left;
  width: 100%;
  border: none;
  background: none;
}

.dropdown-item:hover {
  background-color: #f6f8fa;
  color: #0066cc;
}

.logout-button {
  color: #d73a49;
}

.logout-button:hover {
  background-color: #ffdce0;
  color: #d73a49;
}

.dropdown-divider {
  height: 1px;
  background-color: #eaeaea;
  margin: 4px 0;
}
</style>
