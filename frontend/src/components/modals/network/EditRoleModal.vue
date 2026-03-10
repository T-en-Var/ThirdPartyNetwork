<template>
  <modal-container :title="`Manage Role: ${localForm.name}`" @close="$emit('close')" :close-on-outside-click="false">
    <div class="space-y-8 overflow-y-auto px-1">

      <section class="space-y-4">
        <div class="grid grid-cols-1 gap-4">
          <div>
            <label for="name"
              class="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-1.5 ml-1">
              Role Name
            </label>
            <input id="name" v-model="localForm.name" type="text"
              placeholder="e.g. Content Manager"
              class="block w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none"
              required />
          </div>

          <div>
            <label for="description"
              class="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-1.5 ml-1">
              Role Description
            </label>
            <textarea id="description" v-model="localForm.description" rows="2"
              placeholder="What can this role do?"
              class="block w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-900 shadow-sm focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none resize-none"></textarea>
          </div>
        </div>

        <label
          class="flex items-center group cursor-pointer p-3 rounded-xl border border-slate-100 bg-slate-50/50 hover:bg-slate-50 transition-colors">
          <div class="relative inline-flex items-center">
            <input type="checkbox" v-model="localForm.isDefault"
              class="sr-only peer">
            <div
              class="w-9 h-5 bg-slate-200 rounded-full peer peer-checked:bg-indigo-600 peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all">
            </div>
          </div>
          <div class="ml-3">
            <span class="block text-sm font-semibold text-slate-700">Auto-assign to
              new users</span>
            <span class="block text-xs text-slate-500">New network members will
              receive this role by default.</span>
          </div>
        </label>
      </section>

      <div class="flex p-1 bg-slate-100 rounded-xl">
        <button @click="activeTab = 'permissions'"
          :class="activeTab === 'permissions' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500 hover:text-slate-700'"
          class="flex-1 flex items-center justify-center gap-2 py-2 text-xs font-bold uppercase tracking-wider rounded-lg transition-all">
          <span class="w-2 h-2 rounded-full"
            :class="activeTab === 'permissions' ? 'bg-blue-600' : 'bg-slate-300'"></span>
          Permissions
        </button>
        <button @click="activeTab = 'entitlements'"
          :class="activeTab === 'entitlements' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500 hover:text-slate-700'"
          class="flex-1 flex items-center justify-center gap-2 py-2 text-xs font-bold uppercase tracking-wider rounded-lg transition-all">
          <span class="w-2 h-2 rounded-full"
            :class="activeTab === 'entitlements' ? 'bg-blue-600' : 'bg-slate-300'"></span>
          Entitlements
        </button>
      </div>

      <div v-if="activeTab === 'permissions'"
        class="space-y-4 animate-in fade-in duration-300">
        <loading-error-component :loading="loading" :error="error"
          :has-value="!!permissions" />

        <div v-if="permissions">
          <div class="overflow-y-auto divide-y divide-slate-50">
            <div v-if="permissions?.length === 0"
              class="p-8 text-center text-sm text-slate-400 italic">
              No permissions available
            </div>
            <label v-for="permission in permissions" :key="permission.id"
              class="flex items-center px-4 py-3 hover:bg-slate-50 rounded-md cursor-pointer group transition-colors">
              <input type="checkbox" :value="permission.id"
                v-model="localForm.permissionIds"
                class="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500" />
              <div class="ml-3">
                <span
                  class="block text-sm font-semibold text-slate-700 group-hover:text-blue-600">{{
                    permission.name }}</span>
                <span
                  class="block text-[10px] font-mono text-slate-400 uppercase tracking-tighter">{{
                    permission.id }}</span>
              </div>
            </label>
          </div>
        </div>
      </div>

      <div v-if="activeTab === 'entitlements'"
        class="space-y-6 animate-in fade-in duration-300">
        <div
          class="flex items-center gap-3 p-3 bg-blue-50/50 border border-blue-100 rounded-xl">
          <span
            class="flex-shrink-0 w-5 h-5 flex items-center justify-center bg-blue-100 text-blue-600 text-[10px] font-black rounded-full italic">i</span>
          <p class="text-[11px] text-blue-800 leading-tight">
            Unspecified entitlements will default to network-wide limits.
          </p>
        </div>

        <entitlements-form :network="network" v-model="entitlementsData" />
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="button"
          class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting" @click="handleSubmit">
          <span v-if="isSubmitting">Saving...</span>
          <span v-else>Save Changes</span>
        </button>
      </div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { Role, Network, RoleForm } from '@/types';
import usePermissions from '@/composables/usePermissions';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import { useEntitlements } from '@/composables/useEntitlements';
import EntitlementsForm from '@/components/EntitlementsForm.vue';

const { data: permissions, loading, error, execute: fetchPermissions } = usePermissions().fetchPermissions;
const activeTab = ref<"permissions" | "entitlements">("permissions");

const isSubmitting = ref(false);

const props = defineProps<{
  network: Network,
  selectedRole: Role,
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

// Create a local reactive copy
const localForm = ref({ ...props.manageRoleForm });
</script>