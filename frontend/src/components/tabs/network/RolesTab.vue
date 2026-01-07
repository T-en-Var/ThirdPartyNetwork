<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-medium text-gray-800">Network Roles</h2>
      <button v-if="!loading && !error" @click="$emit('addRole')"
        class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
        Add Role
      </button>
    </div>

    <div v-if="loading || error">
      <LoadingErrorComponent :loading="loading" :error="error"
        :button-value="'Nothing will happen if you press me.'" />
    </div>

    <div v-else class="space-y-4">
      <div v-for="role in roles" :key="role.id"
        class="bg-gray-50 p-4 rounded-lg border border-gray-200">
        <div class="flex justify-between items-center">
          <div>
            <h3 class="text-lg font-medium text-gray-900">{{ role.name }}</h3>
            <p class="text-sm text-gray-600">{{ role.description }}</p>
          </div>
          <div class="mr-1">
            <button @click="$emit('manageRole', role)"
              class="text-blue-600 hover:text-blue-900 mr-3">
              Edit
            </button>
            <button @click="$emit('removeRole', role)"
              class="text-red-600 hover:text-red-900">
              Remove
            </button>
          </div>
        </div>

        <div class="mt-4 flex justify-between items-center">
          <div>
            <h4 class="text-sm font-medium text-gray-700 mb-2">Permissions:</h4>
            <div class="flex flex-wrap gap-2">
              <span v-for="rolePermission in role.rolePermissions"
                :key="rolePermission.permissionId"
                class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                {{ rolePermission.permission.name }}
              </span>
              <span v-if="!role.rolePermissions?.length"
                class="text-sm text-gray-500">No permissions assigned</span>
            </div>
          </div>

          <div v-if="role.isDefault">
            <span
              class="px-4 py-2 text-sm border rounded-md bg-gray-100 text-gray-800 border-gray-200">
              Default
            </span>
          </div>
        </div>

        <div class="mt-4">
          <h4 class="text-sm font-medium text-gray-700 mb-2">Users with this access:
          </h4>
          <div class="flex flex-wrap gap-2">
            <div v-for="networkUser in role.networkUserRoles.map(r => r.networkUser)"
              :key="networkUser.id"
              class="flex items-center px-2 py-1 bg-white rounded-md border border-gray-200">
              <div class="h-6 w-6 flex-shrink-0 mr-2">
                <ProfileAvatar :userProxy="networkUser.userProxy" :size="6" />
              </div>
              <span class="text-xs">{{ networkUser.userProxy.firstName }} {{
                networkUser.userProxy.lastName }}</span>
            </div>
            <span v-if="role.networkUserRoles.length === 0"
              class="text-sm text-gray-500">
              No users have this role assigned
            </span>
          </div>
        </div>
      </div>

      <div v-if="!network.roles.length" class="p-6 text-center text-gray-500">
        No roles in this network
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import useRoles from '@/composables/useRoles';
import type { Network, Role } from '@/types';
import { onMounted } from 'vue';

const { roles, loading, error, fetchRoles } = useRoles();

const props = defineProps<{
  network: Network;
}>();

onMounted(async () => {
  await fetchRoles(props.network.id);
});

defineEmits<{
  (e: 'addRole'): void;
  (e: 'manageRole', role: Role): void;
  (e: 'removeRole', role: Role): void;
}>();
</script>