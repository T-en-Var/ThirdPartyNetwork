<template>
  <modal-container
    :title="`Manage User: ${getNameDisplayUserProxy(selectedUser.userProxy)}`"
    @close="$emit('close')"
    :close-on-outside-click="false"
  >
    <div>
      <div class="flex items-center gap-2">
        <span class="text-[10px] font-black uppercase tracking-widest text-slate-400"
          >Network User ID</span
        >
        <span
          class="rounded-md border border-slate-200 bg-slate-100 px-2 py-0.5 font-mono text-xs font-medium text-slate-600 shadow-sm"
        >
          {{ selectedUser.id }}
        </span>
      </div>
      <div class="flex items-center gap-2">
        <span class="text-[10px] font-black uppercase tracking-widest text-slate-400"
          >User Proxy ID</span
        >
        <span
          class="rounded-md border border-slate-200 bg-slate-100 px-2 py-0.5 font-mono text-xs font-medium text-slate-600 shadow-sm"
        >
          {{ selectedUser.userProxyId }}
        </span>
      </div>
    </div>

    <div class="space-y-6">
      <!-- Roles Section -->
      <div>
        <h3 class="text-lg font-medium leading-6 text-gray-900">User Roles</h3>
        <p class="mt-1 text-sm text-gray-500">Manage roles assigned to this user</p>

        <checkbox-list
          :items="availableRoles"
          v-model="localForm.roleIds"
          id-prefix="role"
          empty-message="No roles available in this network"
          container-class="mt-4 max-h-48 overflow-y-auto rounded-md border p-2"
        />
      </div>

      <div class="flex items-center gap-3 rounded-xl border border-blue-100 bg-blue-50/50 p-3">
        <span
          class="flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full bg-blue-100 text-[10px] font-black italic text-blue-600"
          >i</span
        >
        <p class="text-[11px] leading-tight text-blue-800">
          Unspecified entitlements will default to role-specific/network-wide limits.
        </p>
      </div>

      <entitlements-form
        :network="network"
        v-model="entitlementsData"
      />

      <modal-form-actions
        :is-submitting="isSubmitting"
        :bordered="false"
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
import { ref, watch, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ModalFormActions from '@/components/modals/ModalFormActions.vue';
import CheckboxList from '@/components/CheckboxList.vue';
import type { Network, NetworkUser, ManageUserForm } from '@/types';
import { getNameDisplayUserProxy } from '@/lib/user';
import EntitlementsForm from '@/components/EntitlementsForm.vue';
import { useEntitlements } from '@/composables/useEntitlements';

const props = defineProps<{
  network: Network;
  selectedUser: NetworkUser;
  isSubmitting: boolean;
  manageUserForm: ManageUserForm;
}>();

const emit = defineEmits(['close', 'update']);

const localForm = ref({ ...props.manageUserForm });

const { entitlementsData, initEntitlements, getSubmitData } = useEntitlements(props.network);

onMounted(() => {
  initEntitlements(props.selectedUser.entitlements);
});

watch(
  () => props.manageUserForm,
  (newVal) => {
    localForm.value = { ...newVal };
  },
);

const availableRoles = computed(() => {
  return props.network.roles || [];
});

function handleSubmit() {
  localForm.value.entitlements = getSubmitData();

  emit('update', localForm.value);
}
</script>
