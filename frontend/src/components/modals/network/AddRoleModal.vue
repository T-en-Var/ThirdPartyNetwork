<template>
  <modal-container
    title="Add Role to Network"
    @close="$emit('close')"
  >
    <form
      @submit.prevent="handleSubmit"
      class="form"
    >
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
        <input
          id="description"
          v-model="localForm.description"
          type="text"
          class="form-input"
          required
        />
      </div>

      <loading-error-component
        :loading="loading"
        :error="error"
        :has-value="!!permissions"
      />

      <checkbox-list
        v-if="permissions"
        :items="permissions"
        v-model="localForm.permissionIds"
        id-prefix="perm"
        empty-message="No permissions available"
        container-class="add-role-modal__permissions-list"
        item-class="add-role-modal__permission-item"
        show-id
      />

      <div class="add-role-modal__default-row">
        <input
          id="isRequired"
          v-model="localForm.isDefault"
          type="checkbox"
          class="checkbox"
        />
        <label
          for="isRequired"
          class="form-label"
        >
          Automatically add this role to new users.
        </label>
      </div>

      <modal-form-actions
        :is-submitting="isSubmitting"
        :disable-submit="!localForm.name"
        submit-label="Add Role"
        submitting-label="Adding..."
        @cancel="$emit('close')"
      />
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ModalFormActions from '@/components/modals/ModalFormActions.vue';
import CheckboxList from '@/components/CheckboxList.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import type { RoleForm } from '@/types';
import usePermissions from '@/composables/usePermissions';

const emit = defineEmits(['close', 'add-role']);
const {
  data: permissions,
  loading,
  error,
  execute: fetchPermissions,
} = usePermissions().fetchPermissions;

const isSubmitting = ref(false);

const localForm = ref<RoleForm>({
  name: '',
  description: '',
  permissionIds: [],
  isDefault: false,
  entitlements: {},
});

onMounted(async () => {
  await fetchPermissions();
});

function handleSubmit() {
  isSubmitting.value = true;
  emit('add-role', localForm.value);
}
</script>
