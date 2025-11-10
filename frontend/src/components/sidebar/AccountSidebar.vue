<template>
  <Sidebar :nav-items="navItems" base-path="/account" sub-label-key="username" sub-reverse />
</template>

<script setup lang="ts">
import { computed, type ComputedRef } from 'vue';
import { useHistoryStore } from '@/stores/history';
import Sidebar from '@/components/sidebar/SideBar.vue';
import type { NavCategory, SubItem } from '@/types/sidebar';

const historyStore = useHistoryStore();

const navItems: ComputedRef<NavCategory[]> = computed(() => [
  {
    title: 'Account',
    items: [
      { page: '', label: 'Home', subItems: [], getURI: () => `` },
      {
        page: 'proxies',
        label: 'Proxies',
        subItems: historyStore.userProxies.map(up => ({ page: up.id, label: up.username ?? 'Unnamed' })),
        getURI: (sub: SubItem) => `proxies/${sub.page}/edit`,
      },
    ],
  },
  {
    title: 'Content',
    items: [
      { page: 'files', label: 'Files', subItems: [], getURI: () => `` }
    ],
  },
]);
</script>
