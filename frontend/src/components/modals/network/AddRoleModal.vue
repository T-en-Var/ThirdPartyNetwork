<template>
  <modal-container
    title="Add Role to Network"
    @close="$emit('close')"
  >
    <form
      @submit.prevent="handleSubmit"
      class="space-y-4"
    >
      <div>
        <label
          for="name"
          class="mb-1.5 ml-1 block text-xs font-bold uppercase tracking-wider text-slate-500"
        >
          Role Name
        </label>
        <input
          id="name"
          v-model="localForm.name"
          type="text"
          class="block w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm outline-none transition-all focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
          required
        />
      </div>

      <div>
        <label
          for="description"
          class="mb-1.5 ml-1 block text-xs font-bold uppercase tracking-wider text-slate-500"
        >
          Role Description
        </label>
        <input
          id="description"
          v-model="localForm.description"
          type="text"
          class="block w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm outline-none transition-all focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
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
        container-class="mt-4 max-h-64 overflow-y-auto rounded-md border p-2"
        item-class="ml-2 flex items-center py-1"
        show-id
      />

      <div class="flex items-center">
        <input
          id="isRequired"
          v-model="localForm.isDefault"
          type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
        />
        <label
          for="isRequired"
          class="ml-2 block text-sm text-gray-700"
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
