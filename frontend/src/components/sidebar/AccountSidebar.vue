<template>
  <div class="relative z-30">
    <div :class="['transition-all duration-300 ease-in-out', isOpen ? 'w-64 p-4' : 'w-0 p-0',]"></div>
    <aside
      class="fixed left-0 top-0 bottom-0 z-30 bg-gray-50 border-r w-64 p-4 transition-transform duration-300 ease-in-out"
      :class="{ '-translate-x-full': !isOpen, 'translate-x-0': isOpen }">
      <div v-if="isOpen" class="flex items-center justify-between mt-20 mb-6">
        <div></div>

        <!-- Collapse Button -->
        <button @click="isOpen = false" class="p-1 rounded hover:bg-gray-200 transition" aria-label="Collapse sidebar">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
      </div>

      <!-- Nav Items -->
      <nav v-if="isOpen" class="flex flex-col gap-6 text-sm">
        <div v-for="category in navItems" :key="category.title" class="space-y-2">
          <h2 class="text-xs font-semibold text-gray-500 uppercase tracking-wide px-2">
            {{ category.title }}
          </h2>

          <div class="flex flex-col">
            <div v-for="item in category.items" :key="item.page">
              <!-- Main nav item: block + full width so it stacks cleanly -->
              <RouterLink :to="`/account/${item.page}`"
                class="block w-full px-4 py-2 rounded-md transition hover:bg-blue-100 hover:text-blue-800 text-gray-700"
                exact-active-class="bg-blue-200 text-blue-800 font-medium">
                {{ item.label }}
              </RouterLink>

              <!-- Sub-items: indented, with a little top margin and vertical gap -->
              <div v-if="item.subItems && item.subItems.length" class="ml-6 flex flex-col">
                <RouterLink v-for="sub in [...item.subItems].reverse()" :key="sub.id"
                  :to="`/account/${item.page}/${item.getURI(sub)}`"
                  class="block w-full px-3 py-1.5 text-xs rounded-md transition hover:bg-blue-50 hover:text-blue-700 text-gray-600"
                  exact-active-class="bg-blue-100 text-blue-800 font-medium">
                  {{ sub.username ?? 'No username' }}
                </RouterLink>
              </div>
            </div>
          </div>
        </div>
      </nav>
    </aside>

    <!-- Floating Expand Button (when sidebar closed) -->
    <button v-if="!isOpen" @click="isOpen = true"
      class="fixed top-4 z-50 mt-20 bg-white border rounded-r-full shadow pr-2 py-1 text-gray-700 hover:bg-blue-100 transition"
      aria-label="Expand sidebar">
      <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
import { useHistoryStore } from '@/stores/history';
import { computed, ref } from 'vue';

const isOpen = ref(true);
const historyStore = useHistoryStore();

const navItems = computed(() => [
  {
    title: 'Account',
    items: [
      { page: '', label: 'Home' },
      {
        page: 'proxies',
        label: 'Proxies',
        subItems: historyStore.userProxies,
        getURI: (sub: { id: string, username?: string }) => `${sub.id}/edit`
      },
    ],
  },
  {
    title: 'Content', items: [
      { page: 'files', label: 'Files' },
    ]
  }
]);
</script>
