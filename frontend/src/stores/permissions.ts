import type { PermissionDto } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const usePermissionsStore = defineStore('permissions', () => {
  const permissions = ref<PermissionDto[]>([]);

  function setPermissions(newPermissions: PermissionDto[]) {
    permissions.value = newPermissions;
  }

  return {
    permissions,
    setPermissions,
  };
});
