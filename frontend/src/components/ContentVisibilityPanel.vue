<template>
  <div class="edit-file-modal__section">
    <h3 class="edit-file-modal__section-title">Visibility</h3>

    <div class="edit-file-modal__checkbox-row">
      <input
        id="isPublished"
        :checked="!!publishedAt"
        @change="publishedAt ? $emit('unpublish') : $emit('publish')"
        type="checkbox"
        class="checkbox"
        :disabled="publishing"
      />
      <label
        for="isPublished"
        class="edit-file-modal__checkbox-label"
      >
        Published (required for anyone other than you to view this)
      </label>
    </div>

    <div class="form-field">
      <label class="form-label">Ownership</label>
      <select
        :value="ownershipMode"
        @change="
          $emit('set-ownership', Number(($event.target as HTMLSelectElement).value) as OwnershipMode)
        "
        class="form-select"
        :disabled="settingOwnership"
      >
        <option :value="OwnershipMode.AuthorOwned">Author-owned (you)</option>
        <option :value="OwnershipMode.NetworkOwned">Network-owned</option>
      </select>
      <p class="form-help">Network-owned content stays with the network if you ever leave it.</p>
    </div>
  </div>

  <div class="edit-file-modal__section">
    <h3 class="edit-file-modal__section-title">Shared with</h3>

    <ul
      v-if="grants?.length"
      class="edit-file-modal__grant-list"
    >
      <li
        v-for="grant in grants"
        :key="grant.id"
        class="edit-file-modal__grant-row"
      >
        <span>{{ describeGrant(grant) }}</span>
        <button
          type="button"
          class="btn btn--sm btn--danger"
          @click="$emit('revoke-grant', grant.id)"
          :disabled="revokingGrant"
        >
          Revoke
        </button>
      </li>
    </ul>
    <p
      v-else
      class="form-help"
    >
      Not shared with anyone beyond you (and anyone with manage permission).
    </p>

    <div class="edit-file-modal__grant-form">
      <select
        v-model="newGrant.type"
        class="form-select"
      >
        <option :value="GranteeType.Network">Everyone in this network</option>
        <option :value="GranteeType.NetworkRole">A specific role</option>
        <option :value="GranteeType.User">A specific person</option>
        <option :value="GranteeType.NetworkPermission">Anyone with a permission</option>
      </select>

      <select
        v-if="newGrant.type === GranteeType.NetworkRole"
        v-model="newGrant.targetId"
        class="form-select"
      >
        <option
          value=""
          disabled
        >
          Select a role
        </option>
        <option
          v-for="role in roles ?? []"
          :key="role.id"
          :value="role.id"
        >
          {{ role.name }}
        </option>
      </select>

      <select
        v-else-if="newGrant.type === GranteeType.NetworkPermission"
        v-model="newGrant.targetId"
        class="form-select"
      >
        <option
          value=""
          disabled
        >
          Select a permission
        </option>
        <option
          v-for="permission in networkPermissions"
          :key="permission.id"
          :value="permission.id"
        >
          {{ permission.name }}
        </option>
      </select>

      <SearchableSelect
        v-else-if="newGrant.type === GranteeType.User"
        v-model="newGrant.targetId"
        :options="userOptions"
      />

      <input
        v-model="newGrant.expiresAt"
        type="date"
        class="form-input"
        title="Expires on (optional)"
      />

      <button
        type="button"
        class="btn btn--sm btn--outline"
        @click="handleAddGrant"
        :disabled="!canAddGrant || creatingGrant"
      >
        Grant access
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import SearchableSelect from '@/components/SearchableSelect.vue';
import { getNameDisplayUserProxy } from '@/lib/user';
import { GranteeType, OwnershipMode, ContentCapabilities } from '@/types';
import type { ContentAccessGrantDto, CreateContentAccessGrantDto, NetworkUserDto, PermissionDto, RoleDto } from '@/types';

const props = defineProps<{
  networkId: string;
  publishedAt: string | null;
  ownershipMode: OwnershipMode;
  grants: ContentAccessGrantDto[] | null;
  roles: RoleDto[] | null;
  permissions: PermissionDto[] | null;
  networkUsers: NetworkUserDto[] | null;
  publishing?: boolean;
  settingOwnership?: boolean;
  creatingGrant?: boolean;
  revokingGrant?: boolean;
}>();

const emit = defineEmits<{
  (e: 'publish'): void;
  (e: 'unpublish'): void;
  (e: 'set-ownership', mode: OwnershipMode): void;
  (e: 'add-grant', payload: CreateContentAccessGrantDto): void;
  (e: 'revoke-grant', grantId: string): void;
}>();

const networkPermissions = computed(
  () => props.permissions?.filter((p) => !p.network || p.network.id === props.networkId) ?? [],
);

const userOptions = computed(
  () =>
    props.networkUsers?.map((nu) => ({
      id: nu.userProxyId,
      text: getNameDisplayUserProxy(nu.userProxy),
    })) ?? [],
);

const newGrant = ref<{ type: GranteeType; targetId: string; expiresAt: string }>({
  type: GranteeType.Network,
  targetId: '',
  expiresAt: '',
});

const canAddGrant = computed(() => {
  if (newGrant.value.type === GranteeType.Network) return true;
  return !!newGrant.value.targetId;
});

function handleAddGrant() {
  emit('add-grant', {
    granteeType: newGrant.value.type,
    granteeNetworkId: newGrant.value.type === GranteeType.Network ? props.networkId : null,
    granteeRoleId: newGrant.value.type === GranteeType.NetworkRole ? newGrant.value.targetId : null,
    granteePermissionId:
      newGrant.value.type === GranteeType.NetworkPermission ? newGrant.value.targetId : null,
    granteeUserProxyId: newGrant.value.type === GranteeType.User ? newGrant.value.targetId : null,
    capabilities: ContentCapabilities.View,
    expiresAt: newGrant.value.expiresAt ? new Date(newGrant.value.expiresAt).toISOString() : null,
  });

  newGrant.value = { type: GranteeType.Network, targetId: '', expiresAt: '' };
}

function describeGrant(grant: ContentAccessGrantDto) {
  let target = 'Everyone in this network';
  if (grant.granteeType === GranteeType.NetworkRole) {
    target = `Role: ${props.roles?.find((r) => r.id === grant.granteeRoleId)?.name ?? 'Unknown role'}`;
  } else if (grant.granteeType === GranteeType.NetworkPermission) {
    target = `Permission: ${props.permissions?.find((p) => p.id === grant.granteePermissionId)?.name ?? 'Unknown permission'}`;
  } else if (grant.granteeType === GranteeType.User) {
    const nu = props.networkUsers?.find((u) => u.userProxyId === grant.granteeUserProxyId);
    target = nu ? getNameDisplayUserProxy(nu.userProxy) : 'Unknown user';
  }

  return grant.expiresAt ? `${target} (until ${new Date(grant.expiresAt).toLocaleDateString()})` : target;
}
</script>
