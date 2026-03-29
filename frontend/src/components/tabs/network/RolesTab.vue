<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="mb-6 flex items-center justify-between">
      <h2 class="text-xl font-medium text-gray-800">Network Roles</h2>
      <button
        v-if="!loading && !error"
        @click="$emit('addRole')"
        class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
      >
        Add Role
      </button>
    </div>

    <div v-if="loading || error">
      <LoadingErrorComponent
        :loading="loading"
        :error="error ?? undefined"
        :button-value="'Nothing will happen if you press me.'"
        :has-value="!!roles"
      />
    </div>

    <div
      v-else
      class="space-y-4"
    >
      <div
        v-for="role in roles"
        :key="role.id"
        class="rounded-lg border border-gray-200 bg-gray-50 p-4"
      >
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-lg font-medium text-gray-900">{{ role.name }}</h3>
            <p class="text-sm text-gray-600">{{ role.description }}</p>
          </div>
          <div class="mr-1">
            <button
              @click="$emit('manageRole', role)"
              class="mr-3 text-blue-600 hover:text-blue-900"
            >
              Edit
            </button>
            <button
              @click="$emit('removeRole', role)"
              class="text-red-600 hover:text-red-900"
            >
              Remove
            </button>
          </div>
        </div>

        <template v-if="!isFetching">
          <div class="mt-4 flex items-center justify-between">
            <div>
              <h4 class="mb-2 text-sm font-medium text-gray-700">Permissions:</h4>
              <div class="flex flex-wrap gap-2">
                <span
                  v-for="rolePermission in role.rolePermissions"
                  :key="rolePermission.permissionId"
                  class="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-800"
                >
                  {{ rolePermission.permission.name }}
                </span>
                <span
                  v-if="!role.rolePermissions?.length"
                  class="text-sm text-gray-500"
                  >No permissions assigned</span
                >
              </div>
            </div>

            <div v-if="role.isDefault">
              <span
                class="rounded-md border border-gray-200 bg-gray-100 px-4 py-2 text-sm text-gray-800"
              >
                Default
              </span>
            </div>
          </div>

          <div class="mt-4">
            <h4 class="mb-2 text-sm font-medium text-gray-700">Users with this access:</h4>
            <div class="flex flex-wrap gap-2">
              <div
                v-for="networkUser in role.networkUserRoles.map((r) => r.networkUser)"
                :key="networkUser.id"
                class="flex items-center rounded-md border border-gray-200 bg-white px-2 py-1"
              >
                <div class="mr-2 h-6 w-6 flex-shrink-0">
                  <ProfileAvatar
                    :userProxy="networkUser.userProxy"
                    :size="6"
                  />
                </div>
                <span class="text-xs">{{ networkUser.userProxy.username ?? 'Unknown User' }}</span>
              </div>
              <span
                v-if="role.networkUserRoles.length === 0"
                class="text-sm text-gray-500"
              >
                No users have this role assigned
              </span>
            </div>
          </div>
        </template>

        <template v-else>
          <div class="pt-8 text-sm text-gray-500">
            <LoadingSpinner size="sm" /> Fetching more information...
          </div>
        </template>
      </div>

      <div
        v-if="!network.roles.length"
        class="p-6 text-center text-gray-500"
      >
        No roles in this network
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import useRoles from '@/composables/useRoles';
import type { Network, Role } from '@/types';
import { onMounted } from 'vue';

const { data: roles, isFetching, loading, error, execute: fetchRoles } = useRoles().fetchRoles;

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
