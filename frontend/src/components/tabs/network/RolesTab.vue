<template>
  <div class="network-list-panel">
    <div class="network-list-panel__header">
      <h2 class="network-list-panel__title">Network Roles</h2>
      <button
        v-if="!loading && !error"
        @click="$emit('addRole')"
        class="btn btn--primary"
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
      class="network-list-panel__items"
    >
      <div
        v-for="role in roles"
        :key="role.id"
        class="network-list-panel__item"
      >
        <div class="network-list-panel__item-header">
          <div>
            <h3 class="network-list-panel__item-title">{{ role.name }}</h3>
            <p class="network-list-panel__item-description">{{ role.description }}</p>
          </div>
          <div class="network-list-panel__item-actions">
            <button
              @click="$emit('manageRole', role)"
              class="roles-tab__edit-btn"
            >
              Edit
            </button>
            <button
              @click="$emit('removeRole', role)"
              class="network-list-panel__remove-btn"
            >
              Remove
            </button>
          </div>
        </div>

        <template v-if="!isFetching">
          <div class="roles-tab__detail-row">
            <div>
              <h4 class="roles-tab__detail-label">Permissions:</h4>
              <div class="roles-tab__permissions">
                <span
                  v-for="rolePermission in role.rolePermissions"
                  :key="rolePermission.permission.id"
                  class="badge badge--neutral"
                >
                  {{ rolePermission.permission.name }}
                </span>
                <span
                  v-if="!role.rolePermissions?.length"
                  class="roles-tab__empty-note"
                  >No permissions assigned</span
                >
              </div>
            </div>

            <div v-if="role.isDefault">
              <span class="badge badge--neutral">
                Default
              </span>
            </div>
          </div>

          <div class="roles-tab__users-section">
            <h4 class="roles-tab__detail-label">Users with this access:</h4>
            <div class="roles-tab__users">
              <div
                v-for="networkUser in role.networkUserRoles.map((r) => r.networkUser)"
                :key="networkUser.id"
                class="roles-tab__user-chip"
              >
                <div class="roles-tab__user-avatar">
                  <ProfileAvatar
                    :userProxy="networkUser.userProxy"
                    :size="24"
                  />
                </div>
                <span class="roles-tab__user-name">{{
                  networkUser.userProxy.username ?? 'Unknown User'
                }}</span>
              </div>
              <span
                v-if="role.networkUserRoles.length === 0"
                class="roles-tab__empty-note"
              >
                No users have this role assigned
              </span>
            </div>
          </div>
        </template>

        <template v-else>
          <div class="roles-tab__loading-row">
            <LoadingSpinner size="sm" /> Fetching more information...
          </div>
        </template>
      </div>

      <div
        v-if="!network.roles.length"
        class="network-list-panel__empty"
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
import type { NetworkDto, RoleDto } from '@/types';
import { onMounted } from 'vue';

const { data: roles, isFetching, loading, error, execute: fetchRoles } = useRoles().fetchRoles;

const props = defineProps<{
  network: NetworkDto;
}>();

onMounted(async () => {
  await fetchRoles(props.network.id);
});

defineEmits<{
  (e: 'addRole'): void;
  (e: 'manageRole', role: RoleDto): void;
  (e: 'removeRole', role: RoleDto): void;
}>();
</script>
