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

      <ContentVisibilityPanel
        :network-id="file.networkId"
        :published-at="file.publishedAt"
        :ownership-mode="file.ownershipMode"
        :grants="grants"
        :roles="roles"
        :permissions="permissions"
        :network-users="networkUsers"
        :publishing="publishFile.loading.value || unpublishFile.loading.value"
        :setting-ownership="setFileOwnership.loading.value"
        :creating-grant="createFileAccessGrant.loading.value"
        :revoking-grant="revokeFileAccessGrant.loading.value"
        @publish="publishFileExec(file.networkId, file.id)"
        @unpublish="unpublishFileExec(file.networkId, file.id)"
        @set-ownership="(mode) => setOwnershipExec(file.networkId, file.id, mode)"
        @add-grant="(payload) => createGrantExec(file.networkId, file.id, payload)"
        @revoke-grant="(grantId) => revokeGrantExec(file.networkId, file.id, grantId)"
      />

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
import { onMounted, ref, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ContentVisibilityPanel from '@/components/ContentVisibilityPanel.vue';
import CdnFile from '@/components/cdn/CdnFile.vue';
import useFiles from '@/composables/network/useFiles';
import useRoles from '@/composables/useRoles';
import usePermissions from '@/composables/usePermissions';
import useNetworkUsers from '@/composables/useNetworkUsers';
import type { FileDto, UpdateFileDto } from '@/types';

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

const { execute: rolesExec, data: roles } = useRoles().fetchRoles;
const { execute: permissionsExec, data: permissions } = usePermissions().fetchPermissions;
const { execute: networkUsersExec, data: networkUsers } = useNetworkUsers().fetchNetworkUsers;

onMounted(async () => {
  await fetchGrants(props.file.networkId, props.file.id);
  await rolesExec(props.file.networkId);
  await permissionsExec();
  await networkUsersExec(props.file.networkId);
});
</script>
