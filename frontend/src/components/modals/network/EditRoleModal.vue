<template>
  <modal-container
    :title="`Manage Role: ${localForm.name}`"
    @close="$emit('close')"
    :close-on-outside-click="false"
  >
    <div class="space-y-8 overflow-y-auto px-1">
      <section class="space-y-4">
        <div class="grid grid-cols-1 gap-4">
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
              placeholder="e.g. Content Manager"
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
            <textarea
              id="description"
              v-model="localForm.description"
              rows="2"
              placeholder="What can this role do?"
              class="block w-full resize-none rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm outline-none transition-all focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
            ></textarea>
          </div>
        </div>

        <label
          class="group flex cursor-pointer items-center rounded-xl border border-slate-100 bg-slate-50/50 p-3 transition-colors hover:bg-slate-50"
        >
          <div class="relative inline-flex items-center">
            <input
              type="checkbox"
              v-model="localForm.isDefault"
              class="peer sr-only"
            />
            <div
              class="peer h-5 w-9 rounded-full bg-slate-200 after:absolute after:left-[2px] after:top-[2px] after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-all after:content-[''] peer-checked:bg-indigo-600 peer-checked:after:translate-x-full"
            ></div>
          </div>
          <div class="ml-3">
            <span class="block text-sm font-semibold text-slate-700">Auto-assign to new users</span>
            <span class="block text-xs text-slate-500"
              >New network members will receive this role by default.</span
            >
          </div>
        </label>
      </section>

      <div class="flex rounded-xl bg-slate-100 p-1">
        <button
          @click="activeTab = 'permissions'"
          :class="
            activeTab === 'permissions'
              ? 'bg-white text-blue-600 shadow-sm'
              : 'text-slate-500 hover:text-slate-700'
          "
          class="flex flex-1 items-center justify-center gap-2 rounded-lg py-2 text-xs font-bold uppercase tracking-wider transition-all"
        >
          <span
            class="h-2 w-2 rounded-full"
            :class="activeTab === 'permissions' ? 'bg-blue-600' : 'bg-slate-300'"
          ></span>
          Permissions
        </button>
        <button
          @click="activeTab = 'entitlements'"
          :class="
            activeTab === 'entitlements'
              ? 'bg-white text-blue-600 shadow-sm'
              : 'text-slate-500 hover:text-slate-700'
          "
          class="flex flex-1 items-center justify-center gap-2 rounded-lg py-2 text-xs font-bold uppercase tracking-wider transition-all"
        >
          <span
            class="h-2 w-2 rounded-full"
            :class="activeTab === 'entitlements' ? 'bg-blue-600' : 'bg-slate-300'"
          ></span>
          Entitlements
        </button>
      </div>

      <div
        v-if="activeTab === 'permissions'"
        class="space-y-4 duration-300 animate-in fade-in"
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
            empty-class="p-8 text-center text-sm italic text-slate-400"
            container-class="divide-y divide-slate-50 overflow-y-auto"
            show-id
          />
        </div>
      </div>

      <div
        v-if="activeTab === 'entitlements'"
        class="space-y-6 duration-300 animate-in fade-in"
      >
        <div class="flex items-center gap-3 rounded-xl border border-blue-100 bg-blue-50/50 p-3">
          <span
            class="flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full bg-blue-100 text-[10px] font-black italic text-blue-600"
            >i</span
          >
          <p class="text-[11px] leading-tight text-blue-800">
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
