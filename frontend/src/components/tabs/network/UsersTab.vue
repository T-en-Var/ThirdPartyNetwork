<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="mb-6 flex items-center justify-between">
      <h2 class="text-xl font-medium text-gray-800">Network Users</h2>
      <button
        v-if="!loading && !error"
        @click="$emit('addUser')"
        class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
      >
        Add User
      </button>
    </div>

    <div v-if="loading || error">
      <LoadingErrorComponent
        :loading="loading"
        :error="error ?? undefined"
        :button-value="'Nothing will happen if you press me.'"
        :has-value="!!networkUsers"
      />
    </div>

    <div
      v-else
      class="overflow-x-auto"
    >
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th
              scope="col"
              class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
            >
              User
            </th>
            <th
              scope="col"
              class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
            >
              Roles
            </th>
            <th
              scope="col"
              class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
            >
              Access Status
            </th>
            <th
              scope="col"
              class="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider text-gray-500"
            >
              Actions
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
          <tr
            v-for="networkUser in networkUsers"
            :key="networkUser.id"
          >
            <td class="whitespace-nowrap px-6 py-4">
              <div class="flex items-center">
                <div class="h-10 w-10 flex-shrink-0">
                  <ProfileAvatar
                    :userProxy="networkUser.userProxy"
                    :size="10"
                  />
                </div>
                <div class="ml-4">
                  <div
                    class="max-w-xs truncate text-sm font-medium text-gray-900"
                    :title="getNameDisplayUserProxy(networkUser.userProxy)"
                  >
                    {{ getNameDisplayUserProxy(networkUser.userProxy) }}
                  </div>
                  <SecurableField
                    class="text-sm text-gray-500"
                    :value="networkUser.userProxy.email"
                    :sensitive="!!networkUser.userProxy.email"
                    button-text="View email"
                  />
                </div>
              </div>
            </td>
            <td class="whitespace-nowrap px-6 py-4">
              <div class="flex flex-wrap gap-2">
                <span
                  v-for="userRole in networkUser.networkUserRoles"
                  :key="userRole.role.id"
                  class="inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-800"
                >
                  {{ userRole.role.name }}
                </span>
                <span
                  v-if="!networkUser.networkUserRoles?.length"
                  class="text-sm text-gray-500"
                  >No roles assigned</span
                >
              </div>
            </td>
            <td class="whitespace-nowrap px-6 py-4">
              <div
                class="flex flex-wrap gap-2"
                v-if="!networkUser.accessIncomplete"
              >
                <span
                  v-for="userAccess in networkUser.networkUserAccesses"
                  :key="userAccess.access.id"
                  class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium"
                  :class="
                    userAccess.isAccepted
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  "
                >
                  {{ userAccess.access.name }}
                </span>
                <span
                  v-if="!networkUser.networkUserAccesses?.length"
                  class="text-sm text-gray-500"
                >
                  No access requirements
                </span>
              </div>
              <div v-else>
                <span
                  class="inline-flex items-center rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-800"
                >
                  Pending: User has not updated the new changes.
                </span>
              </div>
            </td>
            <td class="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
              <button
                @click="$emit('manageUser', networkUser)"
                class="mr-3 text-blue-600 hover:text-blue-900"
              >
                Manage
              </button>
              <button
                @click="$emit('removeUser', networkUser)"
                class="text-red-600 hover:text-red-900"
              >
                Remove
              </button>
            </td>
          </tr>
          <tr v-if="!network.networkUsers?.length">
            <td
              colspan="4"
              class="px-6 py-10 text-center text-gray-500"
            >
              No users in this network
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import SecurableField from '@/components/fields/SecurableField.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import useNetworkUsers from '@/composables/useNetworkUsers';
import { getNameDisplayUserProxy } from '@/lib/user';
import type { Network, NetworkUser } from '@/types';
import { onMounted } from 'vue';

const networkUsersState = useNetworkUsers();
const {
  execute: fetchNetworkUsers,
  loading,
  error,
  data: networkUsers,
} = networkUsersState.fetchNetworkUsers;

const props = defineProps<{
  network: Network;
}>();

onMounted(async () => {
  await fetchNetworkUsers(props.network.id);
});

defineEmits<{
  (e: 'addUser'): void;
  (e: 'manageUser', user: NetworkUser): void;
  (e: 'removeUser', user: NetworkUser): void;
}>();
</script>
