<template>
  <Sidebar :nav-items="navItems" :base-path="`/networks/${networkId}/manage`" sub-label-key="name">
    <template #header>
      <RouterLink :to="`/networks/${networkId}/`"
        class="flex items-center gap-2 text-gray-700 hover:text-blue-700 transition">
        <!-- Back arrow icon -->
        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none">
          <path
            d="M4 10L3.64645 10.3536L3.29289 10L3.64645 9.64645L4 10ZM20.5 18C20.5 18.2761 20.2761 18.5 20 18.5C19.7239 18.5 19.5 18.2761 19.5 18L20.5 18ZM8.64645 15.3536L3.64645 10.3536L4.35355 9.64645L9.35355 14.6464L8.64645 15.3536ZM3.64645 9.64645L8.64645 4.64645L9.35355 5.35355L4.35355 10.3536L3.64645 9.64645ZM4 9.5L14 9.5L14 10.5L4 10.5L4 9.5ZM20.5 16L20.5 18L19.5 18L19.5 16L20.5 16ZM14 9.5C17.5898 9.5 20.5 12.4101 20.5 16L19.5 16C19.5 12.9624 17.0376 10.5 14 10.5L14 9.5Z"
            fill="#222222" />
        </svg>
        <span class="text-sm font-medium">Back to details</span>
      </RouterLink>
    </template>
  </Sidebar>
</template>

<script setup lang="ts">
import { computed, type ComputedRef } from 'vue';
import { useRoute } from 'vue-router';
import { useHistoryStore } from '@/stores/history';
import Sidebar from '@/components/sidebar/SideBar.vue';
import type { NavCategory, SubItem } from '@/types/sidebar';

const route = useRoute();
const historyStore = useHistoryStore();

const networkId = computed(() => route.params.networkId as string);

const navItems: ComputedRef<NavCategory[]> = computed(() => [
  {
    title: 'Overview',
    items: [
      { page: '', label: 'Dashboard', subItems: [], getURI: () => `` },
      { page: 'edit', label: 'Edit Details', subItems: [], getURI: () => `` },
    ],
  },
  {
    title: 'Management',
    items: [
      { page: 'users', label: 'Users', subItems: [], getURI: () => `` },
      { page: 'roles', label: 'Roles', subItems: [], getURI: () => `` },
      { page: 'access', label: 'Access Requirements', subItems: [], getURI: () => `` },
    ],
  },
  {
    title: 'Content',
    items: [
      { page: 'files', label: 'Files', subItems: [], getURI: () => `` },
      {
        page: 'custom-pages',
        label: 'Custom Pages',
        subItems: historyStore.customPages.map(cp => ({ page: cp.id, label: cp.name })),
        getURI: (sub: SubItem) => `custom-pages/${sub.page}/edit`,
      },
    ],
  },
]);
</script>
