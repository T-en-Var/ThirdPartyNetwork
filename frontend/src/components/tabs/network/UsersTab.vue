<template>
  <div class="network-users-tab">
    <div class="network-users-tab__header">
      <h2 class="network-users-tab__title">Network Users</h2>
      <button
        v-if="!loading && !error"
        @click="$emit('addUser')"
        class="btn btn--primary"
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
      class="network-users-tab__table-wrap"
    >
      <table class="table">
        <thead class="table__head">
          <tr>
            <th scope="col">User</th>
            <th scope="col">Roles</th>
            <th scope="col">Access Status</th>
            <th scope="col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="networkUser in networkUsers"
            :key="networkUser.id"
            class="table__row"
          >
            <td class="table__cell">
              <div class="network-users-tab__user-cell">
                <div class="network-users-tab__avatar">
                  <ProfileAvatar
                    :userProxy="networkUser.userProxy"
                    :size="40"
                  />
                </div>
                <div class="network-users-tab__user-info">
                  <div
                    class="network-users-tab__user-name"
                    :title="getNameDisplayUserProxy(networkUser.userProxy)"
                  >
                    {{ getNameDisplayUserProxy(networkUser.userProxy) }}
                  </div>
                  <SecurableField
                    class="network-users-tab__user-email"
                    :value="networkUser.userProxy.email"
                    :sensitive="!!networkUser.userProxy.email"
                    button-text="View email"
                  />
                </div>
              </div>
            </td>
            <td class="table__cell">
              <div class="network-users-tab__tags">
                <span
                  v-for="userRole in networkUser.networkUserRoles"
                  :key="userRole.role.id"
                  class="badge badge--info"
                >
                  {{ userRole.role.name }}
                </span>
                <span
                  v-if="!networkUser.networkUserRoles?.length"
                  class="network-users-tab__empty-note"
                  >No roles assigned</span
                >
              </div>
            </td>
            <td class="table__cell">
              <div
                class="network-users-tab__tags"
                v-if="!networkUser.accessIncomplete"
              >
                <span
                  v-for="userAccess in networkUser.networkUserAccesses"
                  :key="userAccess.access.id"
                  class="badge"
                  :class="userAccess.isAccepted ? 'badge--success' : 'badge--danger'"
                >
                  {{ userAccess.access.name }}
                </span>
                <span
                  v-if="!networkUser.networkUserAccesses?.length"
                  class="network-users-tab__empty-note"
                >
                  No access requirements
                </span>
              </div>
              <div v-else>
                <span class="badge badge--warning">
                  Pending: User has not updated the new changes.
                </span>
              </div>
            </td>
            <td class="table__cell network-users-tab__actions">
              <button
                @click="$emit('manageUser', networkUser)"
                class="network-users-tab__manage-btn"
              >
                Manage
              </button>
              <button
                @click="$emit('removeUser', networkUser)"
                class="network-list-panel__remove-btn"
              >
                Remove
              </button>
            </td>
          </tr>
          <tr v-if="!network.networkUsers?.length">
            <td
              colspan="4"
              class="network-users-tab__empty-row"
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
