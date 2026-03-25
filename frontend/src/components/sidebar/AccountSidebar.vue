<template>
  <Sidebar :nav-items="navItems" base-path="/account" sub-label-key="username" sub-reverse />
</template>

<script setup lang="ts">
import { computed, type ComputedRef } from 'vue';
import { DEFAULT_STORES, useHistoryStore } from '@/stores/history';
import Sidebar from '@/components/sidebar/SideBar.vue';
import type { NavCategory, SubItem } from '@/types/sidebar';

const historyStore = useHistoryStore(DEFAULT_STORES.account);

const navItems: ComputedRef<NavCategory[]> = computed(() => [
  {
    title: 'Account',
    items: [
      { page: '', label: 'Home', subItems: [], getURI: () => `` },
      {
        page: 'proxies',
        label: 'Proxies',
        subItems: historyStore.data.userProxies.map(up => ({ page: up.id, label: up.username ?? 'Unnamed' })),
        getURI: (sub: SubItem) => `proxies/${sub.page}/edit`,
      },
    ],
  },
  {
    title: 'Content',
    items: [
      { page: 'files', label: 'Files', subItems: [], getURI: () => `` },
      {
        page: 'custom-pages',
        label: 'Custom Pages',
        subItems: historyStore.data.customPages.map(cp => ({ page: cp.id, label: cp.name })).slice().reverse(),
        getURI: (sub: SubItem) => `custom-pages/${sub.page}/edit`,
      },
      {
        page: 'configurations',
        label: 'Configurations',
        subItems: historyStore.data.configurations.map(cp => ({ page: cp.id, label: cp.key })).slice().reverse(),
        getURI: (sub: SubItem) => `configurations/${sub.page}/edit`,
      },
      {
        page: 'blogs',
        label: 'Blogs',
        subItems: historyStore.data.blogs.map(b => ({ page: b.id, label: b.title })).slice().reverse(),
        getURI: (sub: SubItem) => `blogs/${sub.page}/edit`,
      },
    ],
  },
]);
</script>
