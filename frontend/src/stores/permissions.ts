import type { Permission } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const usePermissionsStore = defineStore('permissions', () => {
  const permissions = ref<Permission[]>([]);

  function setPermissions(newPermissions: Permission[]) {
    permissions.value = newPermissions;
  }

  return {
    permissions,
    setPermissions,
  };
});
