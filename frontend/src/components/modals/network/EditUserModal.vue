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

        <div class="mt-4 max-h-48 overflow-y-auto rounded-md border p-2">
          <div
            v-if="availableRoles.length === 0"
            class="text-sm text-gray-500"
          >
            No roles available in this network
          </div>
          <div
            v-for="role in availableRoles"
            :key="role.id"
            class="flex items-center py-1"
          >
            <input
              :id="`role-${role.id}`"
              type="checkbox"
              :value="role.id"
              v-model="localForm.roleIds"
              class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
            />
            <label
              :for="`role-${role.id}`"
              class="ml-2 block text-sm text-gray-700"
            >
              {{ role.name }}
            </label>
          </div>
        </div>
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

      <div class="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        <button
          type="button"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting"
          @click="handleSubmit"
        >
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>Save Changes</span>
        </button>
      </div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
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
