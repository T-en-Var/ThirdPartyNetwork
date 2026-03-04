<template>
  <modal-container :title="`Manage User: ${getNameDisplayNetworkUser(selectedUser)}`" @close="$emit('close')"
    :close-on-outside-click="false">
    <div>

      <div class="flex items-center gap-2">
        <span
          class="text-[10px] font-black uppercase tracking-widest text-slate-400">Network
          User
          ID</span>
        <span
          class="px-2 py-0.5 font-mono text-xs font-medium text-slate-600 bg-slate-100 border border-slate-200 rounded-md shadow-sm">
          {{ selectedUser.id }}
        </span>
      </div>
      <div class="flex items-center gap-2">
        <span
          class="text-[10px] font-black uppercase tracking-widest text-slate-400">User
          Proxy
          ID</span>
        <span
          class="px-2 py-0.5 font-mono text-xs font-medium text-slate-600 bg-slate-100 border border-slate-200 rounded-md shadow-sm">
          {{ selectedUser.userProxyId }}
        </span>
      </div>
    </div>

    <div class="space-y-6">
      <!-- Roles Section -->
      <div>
        <h3 class="text-lg font-medium leading-6 text-gray-900">User Roles</h3>
        <p class="mt-1 text-sm text-gray-500">Manage roles assigned to this user</p>

        <div class="mt-4 max-h-48 overflow-y-auto border rounded-md p-2">
          <div v-if="availableRoles.length === 0" class="text-sm text-gray-500">
            No roles available in this network
          </div>
          <div v-for="role in availableRoles" :key="role.id"
            class="flex items-center py-1">
            <input :id="`role-${role.id}`" type="checkbox" :value="role.id"
              v-model="localForm.roleIds"
              class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500" />
            <label :for="`role-${role.id}`" class="ml-2 block text-sm text-gray-700">
              {{ role.name }}
            </label>
          </div>
        </div>
      </div>

      <div
        class="flex items-center gap-3 p-3 bg-blue-50/50 border border-blue-100 rounded-xl">
        <span
          class="flex-shrink-0 w-5 h-5 flex items-center justify-center bg-blue-100 text-blue-600 text-[10px] font-black rounded-full italic">i</span>
        <p class="text-[11px] text-blue-800 leading-tight">
          Unspecified entitlements will default to role-specific/network-wide limits.
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
              <div v-for="limit in group.limits" :key="limit.key" class="space-y-3">
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

      <div class="flex justify-end space-x-3 pt-4">
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
import { ref, watch, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { Network, NetworkUser, ManageUserForm, SettableEntitlementForm, NetworkEntitlement } from '@/types';
import { getNameDisplayNetworkUser } from '@/lib/user';

const props = defineProps<{
  network: Network,
  selectedUser: NetworkUser,
  isSubmitting: boolean,
  manageUserForm: ManageUserForm;
}>();

const emit = defineEmits(['close', 'update']);


onMounted(() => {
  initEntitlements();
});

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

function clampValue(key: keyof SettableEntitlementForm, max: number) {
  const val = entitlementsData.value[key];
  if (typeof val !== 'number') return;

  if (val < 0) {
    (entitlementsData.value[key] as number) = 0;
  } else if (val > max) {
    (entitlementsData.value[key] as number) = max;
  }
};


const localForm = ref({ ...props.manageUserForm });

watch(() => props.manageUserForm, (newVal) => {
  localForm.value = { ...newVal };
});

const availableRoles = computed(() => {
  return props.network.roles || [];
});

function initEntitlements() {
  if (!props.network.entitlement) return;

  entitlementsData.value.allowBlogs = props.selectedUser.entitlements.allowBlogs;
  entitlementsData.value.allowConfigurations = props.network.entitlement.allowConfigurations;
  entitlementsData.value.allowCustomPages = props.network.entitlement.allowCustomPages;
  entitlementsData.value.allowFiles = props.network.entitlement.allowFiles;

  entitlementsData.value.blogCountLimit = props.network.entitlement.blogCountLimit;
  entitlementsData.value.configurationCountLimit = props.network.entitlement.configurationCountLimit;
  entitlementsData.value.customPageCountLimit = props.network.entitlement.customPageCountLimit;
  entitlementsData.value.fileCountLimit = props.network.entitlement.fileCountLimit;
  entitlementsData.value.fileSizeLimit = props.network.entitlement.fileSizeLimit;
  entitlementsData.value.fileStorageLimit = props.network.entitlement.fileStorageLimit;

  if (props.selectedUser.entitlements.blogCountLimit) {
    entitlementsData.value.blogCountLimit = props.selectedUser.entitlements.blogCountLimit;
    entitlementsData.value.setBlogCountLimit = true;
  }
  if (props.selectedUser.entitlements.configurationCountLimit) {
    entitlementsData.value.configurationCountLimit = props.selectedUser.entitlements.configurationCountLimit;
    entitlementsData.value.setConfigurationCountLimit = true;
  }
  if (props.selectedUser.entitlements.customPageCountLimit) {
    entitlementsData.value.customPageCountLimit = props.selectedUser.entitlements.customPageCountLimit;
    entitlementsData.value.setCustomPageCountLimit = true;
  }
  if (props.selectedUser.entitlements.fileCountLimit) {
    entitlementsData.value.fileCountLimit = props.selectedUser.entitlements.fileCountLimit;
    entitlementsData.value.setFileCountLimit = true;
  }
  if (props.selectedUser.entitlements.fileSizeLimit) {
    entitlementsData.value.fileSizeLimit = props.selectedUser.entitlements.fileSizeLimit;
    entitlementsData.value.setFileSizeLimit = true;
  }
  if (props.selectedUser.entitlements.fileStorageLimit) {
    entitlementsData.value.fileStorageLimit = props.selectedUser.entitlements.fileStorageLimit;
    entitlementsData.value.setFileStorageLimit = true;
  }
}

function handleSubmit() {
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
</script>