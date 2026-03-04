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

        <div class="space-y-4 overflow-y-auto pr-1">
          <div v-for="group in entitlementGroups" :key="group.title">
            <section v-if="network.entitlement![group.allowKey]"
              class="border border-slate-200 rounded-xl overflow-hidden mb-4">
              <div class="bg-slate-50 px-4 py-2 border-b border-slate-200">
                <h3
                  class="text-[10px] font-black uppercase tracking-widest text-slate-500">
                  {{ group.title }}</h3>
              </div>
              <div class="p-4 space-y-6">
                <div v-for="limit in group.limits" :key="limit.key"
                  class="space-y-3">
                  <div class="flex items-center justify-between">
                    <span class="text-sm font-bold text-slate-700">{{ limit.label
                    }}</span>
                    <label
                      class="relative inline-flex items-center cursor-pointer group">
                      <input type="checkbox" v-model="entitlementsData[limit.setKey]"
                        class="sr-only peer">
                      <div class="
                        w-9 h-5 
                        bg-slate-200 
                        rounded-full 
                        peer 
                        peer-checked:bg-blue-600 
                        after:content-[''] 
                        after:absolute 
                        after:top-[2px] 
                        after:left-[2px] 
                        after:bg-white 
                        after:rounded-full 
                        after:h-4 
                        after:w-4 
                        after:transition-all 
                        after:shadow-sm
                        peer-checked:after:translate-x-[100%]
                        group-hover:after:scale-110
                      "></div>
                    </label>
                  </div>

                  <div
                    :class="entitlementsData[limit.setKey] ? 'opacity-100' : 'opacity-30 grayscale pointer-events-none transition-all duration-300'">
                    <div class="flex justify-between items-center mb-2">
                      <span class="text-[10px] text-slate-400 font-bold uppercase">
                        Range: 0 - {{ network.entitlement![limit.key] }}
                      </span>

                      <div
                        class="flex items-center gap-1 bg-white border border-slate-200 rounded px-2 py-0.5 shadow-sm focus-within:border-blue-400 transition-colors">
                        <input type="number"
                          v-model.number="entitlementsData[limit.key as keyof SettableEntitlementForm]"
                          @blur="clampValue(limit.key as keyof SettableEntitlementForm, network.entitlement![limit.key] as number)"
                          @change="clampValue(limit.key as keyof SettableEntitlementForm, network.entitlement![limit.key] as number)"
                          class="w-20 text-xs font-bold text-blue-600 outline-none text-right bg-transparent appearance-none" />
                        <span v-if="limit.unit"
                          class="text-[9px] font-black text-slate-400 uppercase">
                          {{ limit.unit }}
                        </span>
                      </div>
                    </div>

                    <input type="range"
                      v-model.number="entitlementsData[limit.key as keyof SettableEntitlementForm]"
                      :min="0" :max="(network.entitlement![limit.key] as number)"
                      class="w-full h-1 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-blue-600 hover:accent-blue-700 transition-all" />
                  </div>
                </div>
              </div>
            </section>
          </div>
        </div>
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
import type { Role, Network, RoleForm, SettableEntitlementForm, NetworkEntitlement } from '@/types';
import usePermissions from '@/composables/usePermissions';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';

const { data: permissions, loading, error, execute: fetchPermissions } = usePermissions().fetchPermissions;
const activeTab = ref<"permissions" | "entitlements">("permissions");

type EntitlementKey = keyof NetworkEntitlement;
type FormKey = keyof SettableEntitlementForm;

interface EntitlementLimit {
  label: string;
  key: EntitlementKey;
  setKey: FormKey;
  unit?: string;
}

interface EntitlementGroup {
  title: string;
  allowKey: EntitlementKey;
  limits: EntitlementLimit[];
}

const entitlementGroups: EntitlementGroup[] = [
  {
    title: 'File Management',
    allowKey: 'allowFiles',
    limits: [
      { label: 'File Count Limit', key: 'fileCountLimit', setKey: 'setFileCountLimit' },
      { label: 'File Size Limit', key: 'fileSizeLimit', setKey: 'setFileSizeLimit', unit: 'KB' },
      { label: 'Storage Total Limit', key: 'fileStorageLimit', setKey: 'setFileStorageLimit', unit: 'KB' },
    ]
  },
  {
    title: 'Blog Settings',
    allowKey: 'allowBlogs',
    limits: [
      { label: 'Blog Count Limit', key: 'blogCountLimit', setKey: 'setBlogCountLimit' },
    ]
  },
  {
    title: 'Configurations',
    allowKey: 'allowConfigurations',
    limits: [
      { label: 'Config Count Limit', key: 'configurationCountLimit', setKey: 'setConfigurationCountLimit' },
    ]
  },
  {
    title: 'Custom Pages',
    allowKey: 'allowCustomPages',
    limits: [
      { label: 'Custom Page Count Limit', key: 'customPageCountLimit', setKey: 'setCustomPageCountLimit' },
    ]
  }
];

const entitlementsData = ref<SettableEntitlementForm>({});

const isSubmitting = ref(false);

const props = defineProps<{
  network: Network,
  selectedRole: Role,
  manageRoleForm: RoleForm;
}>();

const emit = defineEmits(['close', 'update']);

onMounted(async () => {
  await fetchPermissions();

  initEntitlements();
});

function initEntitlements() {
  if (!props.network.entitlement) return;

  entitlementsData.value.allowBlogs = props.selectedRole.entitlements.allowBlogs;
  entitlementsData.value.allowConfigurations = props.network.entitlement.allowConfigurations;
  entitlementsData.value.allowCustomPages = props.network.entitlement.allowCustomPages;
  entitlementsData.value.allowFiles = props.network.entitlement.allowFiles;

  entitlementsData.value.blogCountLimit = props.network.entitlement.blogCountLimit;
  entitlementsData.value.configurationCountLimit = props.network.entitlement.configurationCountLimit;
  entitlementsData.value.customPageCountLimit = props.network.entitlement.customPageCountLimit;
  entitlementsData.value.fileCountLimit = props.network.entitlement.fileCountLimit;
  entitlementsData.value.fileSizeLimit = props.network.entitlement.fileSizeLimit;
  entitlementsData.value.fileStorageLimit = props.network.entitlement.fileStorageLimit;

  if (props.selectedRole.entitlements.blogCountLimit) {
    entitlementsData.value.blogCountLimit = props.selectedRole.entitlements.blogCountLimit;
    entitlementsData.value.setBlogCountLimit = true;
  }
  if (props.selectedRole.entitlements.configurationCountLimit) {
    entitlementsData.value.configurationCountLimit = props.selectedRole.entitlements.configurationCountLimit;
    entitlementsData.value.setConfigurationCountLimit = true;
  }
  if (props.selectedRole.entitlements.customPageCountLimit) {
    entitlementsData.value.customPageCountLimit = props.selectedRole.entitlements.customPageCountLimit;
    entitlementsData.value.setCustomPageCountLimit = true;
  }
  if (props.selectedRole.entitlements.fileCountLimit) {
    entitlementsData.value.fileCountLimit = props.selectedRole.entitlements.fileCountLimit;
    entitlementsData.value.setFileCountLimit = true;
  }
  if (props.selectedRole.entitlements.fileSizeLimit) {
    entitlementsData.value.fileSizeLimit = props.selectedRole.entitlements.fileSizeLimit;
    entitlementsData.value.setFileSizeLimit = true;
  }
  if (props.selectedRole.entitlements.fileStorageLimit) {
    entitlementsData.value.fileStorageLimit = props.selectedRole.entitlements.fileStorageLimit;
    entitlementsData.value.setFileStorageLimit = true;
  }
}

function handleSubmit() {
  isSubmitting.value = true;
  localForm.value.entitlements = {};

  if (entitlementsData.value.setAllowBlogs) localForm.value.entitlements.allowBlogs = entitlementsData.value.allowBlogs;
  if (entitlementsData.value.setAllowConfigurations) localForm.value.entitlements.allowConfigurations = entitlementsData.value.allowConfigurations;
  if (entitlementsData.value.setAllowCustomPages) localForm.value.entitlements.allowCustomPages = entitlementsData.value.allowCustomPages;
  if (entitlementsData.value.setAllowFiles) localForm.value.entitlements.allowFiles = entitlementsData.value.allowFiles;
  if (entitlementsData.value.setBlogCountLimit) localForm.value.entitlements.blogCountLimit = entitlementsData.value.blogCountLimit;
  if (entitlementsData.value.setConfigurationCountLimit) localForm.value.entitlements.configurationCountLimit = entitlementsData.value.configurationCountLimit;
  if (entitlementsData.value.setCustomPageCountLimit) localForm.value.entitlements.customPageCountLimit = entitlementsData.value.customPageCountLimit;
  if (entitlementsData.value.setFileCountLimit) localForm.value.entitlements.fileCountLimit = entitlementsData.value.fileCountLimit;
  if (entitlementsData.value.setFileSizeLimit) localForm.value.entitlements.fileSizeLimit = entitlementsData.value.fileSizeLimit;
  if (entitlementsData.value.setFileStorageLimit) localForm.value.entitlements.fileStorageLimit = entitlementsData.value.fileStorageLimit;

  emit('update', localForm.value);
}

function clampValue(key: keyof SettableEntitlementForm, max: number) {
  const val = entitlementsData.value[key];
  if (typeof val !== 'number') return;

  if (val < 0) {
    (entitlementsData.value[key] as number) = 0;
  } else if (val > max) {
    (entitlementsData.value[key] as number) = max;
  }
};

// Create a local reactive copy
const localForm = ref({ ...props.manageRoleForm });
</script>