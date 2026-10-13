<template>
  <modal-container
    :title="`Manage Role: ${localForm.name}`"
    @close="$emit('close')"
    :close-on-outside-click="false"
  >
    <div class="edit-role-modal">
      <section class="edit-role-modal__details">
        <div class="edit-role-modal__fields">
          <div class="form-field">
            <label
              for="name"
              class="form-label"
            >
              Role Name
            </label>
            <input
              id="name"
              v-model="localForm.name"
              type="text"
              placeholder="e.g. Content Manager"
              class="form-input"
              required
            />
          </div>

          <div class="form-field">
            <label
              for="description"
              class="form-label"
            >
              Role Description
            </label>
            <textarea
              id="description"
              v-model="localForm.description"
              rows="2"
              placeholder="What can this role do?"
              class="form-textarea"
            ></textarea>
          </div>
        </div>

        <label class="edit-role-modal__toggle">
          <div class="edit-role-modal__toggle-control">
            <input
              type="checkbox"
              v-model="localForm.isDefault"
              class="sr-only"
            />
            <div class="edit-role-modal__toggle-track"></div>
          </div>
          <div class="edit-role-modal__toggle-copy">
            <span class="edit-role-modal__toggle-title">Auto-assign to new users</span>
            <span class="edit-role-modal__toggle-subtitle"
              >New network members will receive this role by default.</span
            >
          </div>
        </label>
      </section>

      <div class="edit-role-modal__subtabs">
        <button
          @click="activeTab = 'permissions'"
          :class="{ 'edit-role-modal__subtab--active': activeTab === 'permissions' }"
          class="edit-role-modal__subtab"
        >
          <span
            class="edit-role-modal__subtab-dot"
            :class="{ 'edit-role-modal__subtab-dot--active': activeTab === 'permissions' }"
          ></span>
          Permissions
        </button>
        <button
          @click="activeTab = 'entitlements'"
          :class="{ 'edit-role-modal__subtab--active': activeTab === 'entitlements' }"
          class="edit-role-modal__subtab"
        >
          <span
            class="edit-role-modal__subtab-dot"
            :class="{ 'edit-role-modal__subtab-dot--active': activeTab === 'entitlements' }"
          ></span>
          Entitlements
        </button>
      </div>

      <div
        v-if="activeTab === 'permissions'"
        class="edit-role-modal__panel"
      >
        <loading-error-component
          :loading="loading"
          :error="error"
          :has-value="!!permissions"
        />

        <div v-if="permissions">
          <checkbox-list
            :items="permissions"
            v-model="localForm.permissionIds"
            variant="card"
            empty-message="No permissions available"
            empty-class="edit-role-modal__permissions-empty"
            container-class="edit-role-modal__permissions-list"
            show-id
          />
        </div>
      </div>

      <div
        v-if="activeTab === 'entitlements'"
        class="edit-role-modal__panel"
      >
        <div class="network-modal__info-banner">
          <span class="network-modal__info-icon">i</span>
          <p class="network-modal__info-text">
            Unspecified entitlements will default to network-wide limits.
          </p>
        </div>

        <entitlements-form
          :network="network"
          v-model="entitlementsData"
        />
      </div>

      <modal-form-actions
        :is-submitting="isSubmitting"
        submit-type="button"
        submit-label="Save Changes"
        submitting-label="Saving..."
        @cancel="$emit('close')"
        @submit="handleSubmit"
      />
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ModalFormActions from '@/components/modals/ModalFormActions.vue';
import CheckboxList from '@/components/CheckboxList.vue';
import type { Role, Network, RoleForm } from '@/types';
import usePermissions from '@/composables/usePermissions';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import { useEntitlements } from '@/composables/useEntitlements';
import EntitlementsForm from '@/components/EntitlementsForm.vue';

const {
  data: permissions,
  loading,
  error,
  execute: fetchPermissions,
} = usePermissions().fetchPermissions;
const activeTab = ref<'permissions' | 'entitlements'>('permissions');

const isSubmitting = ref(false);

const props = defineProps<{
  network: Network;
  selectedRole: Role;
  manageRoleForm: RoleForm;
}>();

const { entitlementsData, initEntitlements, getSubmitData } = useEntitlements(props.network);

const emit = defineEmits(['close', 'update']);

onMounted(async () => {
  await fetchPermissions();

  initEntitlements(props.selectedRole.entitlements);
});

function handleSubmit() {
  isSubmitting.value = true;
  localForm.value.entitlements = getSubmitData();

  emit('update', localForm.value);
}

const localForm = ref({ ...props.manageRoleForm });
</script>
