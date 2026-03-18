<template>
  <Sidebar :nav-items="navItems" base-path="" sub-label-key="name">
    <template #header>
    </template>
  </Sidebar>
</template>

<script setup lang="ts">
import { computed, type ComputedRef } from 'vue';
import { useHistoryStore } from '@/stores/history';
import Sidebar from '@/components/sidebar/SideBar.vue';
import type { NavCategory, SubItem } from '@/types/sidebar';
import { defaultProxy, getNameDisplayUserProxy } from '@/lib/user';

const historyStore = useHistoryStore();

const navItems: ComputedRef<NavCategory[]> = computed(() => [
  {
    title: 'Admin',
    items: [
      { page: 'admin', label: 'Dashboard', subItems: [], getURI: () => `` },
      {
        page: 'admin/networks',
        label: 'Networks',
        subItems: historyStore.networks.map(cp => ({ page: cp.id, label: cp.name })).slice().reverse(),
        getURI: (sub: SubItem) => `networks/${sub.page}/manage`,
      },
      {
        page: 'admin/users',
        label: 'Users',
        subItems: historyStore.users.map(user => {
          const up = defaultProxy(user);
          return ({ page: user.id, label: getNameDisplayUserProxy(up) });
        }).slice().reverse(),
        getURI: (sub: SubItem) => `admin/networks/${sub.page}`,
      },
    ],
  },
]);
</script>
