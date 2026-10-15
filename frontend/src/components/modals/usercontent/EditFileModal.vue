<template>
  <modal-container
    title="Edit File"
    @close="$emit('close')"
    :close-on-escape="!isSubmitting"
  >
    <form
      @submit.prevent="handleSubmit"
      class="form"
    >
      <!-- File Name -->
      <div class="form-field">
        <label
          for="fileName"
          class="form-label"
          >File Name</label
        >
        <input
          id="fileName"
          v-model="form.name"
          type="text"
          required
          class="form-input"
        />
      </div>

      <div>
        <CdnFile
          :file="file"
          :allow-download="true"
        />
      </div>

      <!-- Visibility -->
      <div class="edit-file-modal__section">
        <h3 class="edit-file-modal__section-title">Visibility</h3>

        <div class="edit-file-modal__checkbox-row">
          <input
            id="isPublished"
            :checked="!!file.publishedAt"
            @change="togglePublished"
            type="checkbox"
            class="checkbox"
            :disabled="publishFile.loading.value || unpublishFile.loading.value"
          />
          <label
            for="isPublished"
            class="edit-file-modal__checkbox-label"
          >
            Published (required for anyone other than you to view this file)
          </label>
        </div>

        <div class="form-field">
          <label class="form-label">Ownership</label>
          <select
            :value="file.ownershipMode"
            @change="handleOwnershipChange"
            class="form-select"
            :disabled="setFileOwnership.loading.value"
          >
            <option :value="OwnershipMode.AuthorOwned">Author-owned (you)</option>
            <option :value="OwnershipMode.NetworkOwned">Network-owned</option>
          </select>
          <p class="form-help">
            Network-owned files stay with the network if you ever leave it.
          </p>
        </div>
      </div>

      <!-- Access grants -->
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
              @click="handleRevokeGrant(grant.id)"
              :disabled="revokeFileAccessGrant.loading.value"
            >
              Revoke
            </button>
          </li>
        </ul>
        <p
          v-else
          class="form-help"
        >
          Not shared with anyone beyond you (and anyone with manage-file permission).
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
            :disabled="!canAddGrant || createFileAccessGrant.loading.value"
          >
            Grant access
          </button>
        </div>
      </div>

      <!-- Actions -->
      <div class="edit-file-modal__actions">
        <div class="edit-file-modal__actions-start">
          <button
            type="button"
            class="btn btn--danger"
            @click="$emit('delete-file', file)"
            :disabled="isSubmitting"
          >
            <span>Delete</span>
          </button>
        </div>

        <div class="edit-file-modal__actions-end">
          <button
            type="button"
            class="btn btn--outline"
            @click="$emit('close')"
            :disabled="isSubmitting"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="btn btn--primary"
            :disabled="isSubmitting || !form.name"
          >
            <span v-if="isSubmitting">Saving...</span>
            <span v-else>Update</span>
          </button>
        </div>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import SearchableSelect from '@/components/SearchableSelect.vue';
import CdnFile from '@/components/cdn/CdnFile.vue';
import useFiles from '@/composables/network/useFiles';
import useRoles from '@/composables/useRoles';
import usePermissions from '@/composables/usePermissions';
import useNetworkUsers from '@/composables/useNetworkUsers';
import { getNameDisplayUserProxy } from '@/lib/user';
import { GranteeType, OwnershipMode, ContentCapabilities } from '@/types';
import type { FileDto, UpdateFileDto, ContentAccessGrantDto } from '@/types';

const props = defineProps<{
  file: FileDto;
  isSubmitting?: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'update-file', id: string, networkId: string, updated: UpdateFileDto): void;
  (e: 'delete-file', file: FileDto): void;
}>();

const form = ref({ name: props.file.name });

watch(
  () => props.file,
  (newFile) => {
    form.value.name = newFile.name;
  },
);

function handleSubmit() {
  emit('update-file', props.file.id, props.file.networkId, { name: form.value.name });
}

const { execute: publishFileExec, ...publishFile } = useFiles().publishFile;
const { execute: unpublishFileExec, ...unpublishFile } = useFiles().unpublishFile;
const { execute: setOwnershipExec, ...setFileOwnership } = useFiles().setFileOwnership;
const { execute: fetchGrants, data: grants } = useFiles().fetchFileAccessGrants;
const { execute: createGrantExec, ...createFileAccessGrant } = useFiles().createFileAccessGrant;
const { execute: revokeGrantExec, ...revokeFileAccessGrant } = useFiles().revokeFileAccessGrant;

onMounted(async () => {
  await fetchGrants(props.file.networkId, props.file.id);
  await rolesExec(props.file.networkId);
  await permissionsExec();
  await networkUsersExec(props.file.networkId);
});

async function togglePublished() {
  if (props.file.publishedAt) {
    await unpublishFileExec(props.file.networkId, props.file.id);
  } else {
    await publishFileExec(props.file.networkId, props.file.id);
  }
}

function handleOwnershipChange(e: Event) {
  const mode = Number((e.target as HTMLSelectElement).value) as OwnershipMode;
  setOwnershipExec(props.file.networkId, props.file.id, mode);
}

// --- Access grants ---------------------------------------------------------
const { execute: rolesExec, data: roles } = useRoles().fetchRoles;
const { execute: permissionsExec, data: permissions } = usePermissions().fetchPermissions;
const { execute: networkUsersExec, data: networkUsers } = useNetworkUsers().fetchNetworkUsers;

const networkPermissions = computed(
  () => permissions.value?.filter((p) => !p.network || p.network.id === props.file.networkId) ?? [],
);

const userOptions = computed(
  () =>
    networkUsers.value?.map((nu) => ({
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

async function handleAddGrant() {
  await createGrantExec(props.file.networkId, props.file.id, {
    granteeType: newGrant.value.type,
    granteeNetworkId: newGrant.value.type === GranteeType.Network ? props.file.networkId : null,
    granteeRoleId: newGrant.value.type === GranteeType.NetworkRole ? newGrant.value.targetId : null,
    granteePermissionId:
      newGrant.value.type === GranteeType.NetworkPermission ? newGrant.value.targetId : null,
    granteeUserProxyId: newGrant.value.type === GranteeType.User ? newGrant.value.targetId : null,
    capabilities: ContentCapabilities.View,
    expiresAt: newGrant.value.expiresAt ? new Date(newGrant.value.expiresAt).toISOString() : null,
  });

  newGrant.value = { type: GranteeType.Network, targetId: '', expiresAt: '' };
}

async function handleRevokeGrant(grantId: string) {
  await revokeGrantExec(props.file.networkId, props.file.id, grantId);
}

function describeGrant(grant: ContentAccessGrantDto) {
  let target = 'Everyone in this network';
  if (grant.granteeType === GranteeType.NetworkRole) {
    target = `Role: ${roles.value?.find((r) => r.id === grant.granteeRoleId)?.name ?? 'Unknown role'}`;
  } else if (grant.granteeType === GranteeType.NetworkPermission) {
    target = `Permission: ${permissions.value?.find((p) => p.id === grant.granteePermissionId)?.name ?? 'Unknown permission'}`;
  } else if (grant.granteeType === GranteeType.User) {
    const nu = networkUsers.value?.find((u) => u.userProxyId === grant.granteeUserProxyId);
    target = nu ? getNameDisplayUserProxy(nu.userProxy) : 'Unknown user';
  }

  return grant.expiresAt ? `${target} (until ${new Date(grant.expiresAt).toLocaleDateString()})` : target;
}
</script>
