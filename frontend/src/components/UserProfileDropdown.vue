<template>
  <div
    class="user-dropdown"
    v-if="user != null"
  >
    <button
      class="user-dropdown__trigger"
      @click="toggleDropdown"
      aria-haspopup="true"
      :aria-expanded="isOpen"
    >
      <div class="user-dropdown__avatar">
        <ProfileAvatar :userProxy="user!" />
      </div>
      <span
        class="user-dropdown__arrow"
        :class="{ 'user-dropdown__arrow--open': isOpen }"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
          class="user-dropdown__arrow-icon"
        >
          <polyline points="6 9 12 15 18 9"></polyline>
        </svg>
      </span>
    </button>

    <div
      v-if="isOpen"
      class="user-dropdown__menu"
    >
      <div class="user-dropdown__header">
        <div class="user-dropdown__user-info">
          <p class="user-dropdown__user-name">@{{ user?.username }}</p>
          <p class="user-dropdown__user-email">{{ `${user?.firstName} ${user?.lastName}` }}</p>
        </div>
      </div>
      <div class="user-dropdown__divider"></div>
      <ul class="user-dropdown__list">
        <li
          v-for="link in links"
          :key="link.value"
        >
          <RouterLink
            :to="link.to"
            class="user-dropdown__item"
            @click="closeDropdown"
          >
            {{ link.value }}
          </RouterLink>
        </li>
        <li class="user-dropdown__divider"></li>
        <li>
          <button
            class="user-dropdown__item user-dropdown__item--logout"
            @click="handleLogout"
          >
            Sign out
          </button>
        </li>
      </ul>
    </div>
  </div>
  <div
    v-else
    class="user-dropdown__skeleton"
  >
    <svg
      class="user-dropdown__skeleton-icon"
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
    >
      <circle
        cx="12"
        cy="12"
        r="10"
        stroke-width="1"
      ></circle>
    </svg>
  </div>
</template>

<script setup lang="ts">
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';
import type { UserProxy } from '@/types';

const authStore = useAuthStore();
const router = useRouter();
const isOpen = ref(false);

const user = ref<UserProxy | null>(null);

const links = [
  {
    value: 'Account Settings',
    to: '/account',
  },
];

const toggleDropdown = async () => {
  isOpen.value = !isOpen.value;
};

const closeDropdown = () => {
  isOpen.value = false;
};

const handleLogout = () => {
  router.push('/logout');
  authStore.logout();
  closeDropdown();
};

const handleClickOutside = (event: MouseEvent) => {
  const dropdown = document.querySelector('.user-dropdown');
  if (dropdown && !dropdown.contains(event.target as Node) && isOpen.value) {
    closeDropdown();
  }
};

onMounted(async () => {
  document.addEventListener('click', handleClickOutside);
  user.value = await authStore.getUserProxy();
});

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>
