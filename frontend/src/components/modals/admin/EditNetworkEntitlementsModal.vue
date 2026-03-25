<template>
  <modal-container :title="'Edit Network'" @close="$emit('close')">
    <div class="mb-6">
      <span class="block text-sm text-gray-800 mb-2">
        Network Name: <span class="font-semibold">{{ network.name }}</span>
      </span>
      <p v-if="props.network.isSystemProtected"
        class="mt-2 text-sm text-yellow-700 bg-yellow-50 border border-yellow-200 rounded-md p-2">
        You are changing the max limits throughout the whole application. Be
        cautious. This will be able to exceed backend limits. Limits that are set,
        will be applied per network the next time you set that network's limits,
        not in total (for now).
      </p>
    </div>

    <entitlements-form :network="fakeNetwork" v-model="entitlementsData"
      :auto-increase-max="mainNetwork.id == network.id" />

    <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
      <button type="button"
        class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
        @click="$emit('close')" :disabled="isSubmitting">
        Cancel
      </button>
      <button @click="handleSubmit" type="submit"
        class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        :disabled="isSubmitting">
        <span v-if="isSubmitting" class="flex items-center">
          <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
            xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor"
              stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
            </path>
          </svg>
          Saving...
        </span>
        <span v-else>Save Changes</span>
      </button>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, onMounted, type ComputedRef } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { EntitlementLimits, Network, SettableEntitlement } from '@/types';
import EntitlementsForm from '@/components/EntitlementsForm.vue';
import { useEntitlements } from '@/composables/useEntitlements';

const props = withDefaults(defineProps<{
  mainNetwork: Network,
  network: Network,
  totalLimits: EntitlementLimits,
  isSubmitting: boolean;
}>(), {
  isSubmitting: false
});

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'update', networkEntilement: SettableEntitlement): void;
}>();

const { entitlementsData, initEntitlements, getSubmitData } = useEntitlements(props.network);

const fakeNetwork: ComputedRef<Network> = computed(() => {
  return {
    ...props.mainNetwork,
    entitlement: {
      networkId: props.mainNetwork.id,
      allowFiles: props.mainNetwork.entitlement!.allowFiles,
      fileCountLimit: props.mainNetwork.entitlement!.fileCountLimit - props.totalLimits["fileCountLimit"] + props.network.entitlement!.fileCountLimit,
      fileSizeLimit: props.mainNetwork.entitlement!.fileSizeLimit - props.totalLimits["fileSizeLimit"],
      fileStorageLimit: props.mainNetwork.entitlement!.fileStorageLimit - props.totalLimits["fileStorageLimit"] + props.network.entitlement!.fileStorageLimit,
      allowBlogs: props.mainNetwork.entitlement!.allowBlogs,
      blogCountLimit: props.mainNetwork.entitlement!.blogCountLimit - props.totalLimits["blogCountLimit"] + props.network.entitlement!.blogCountLimit,
      allowConfigurations: props.mainNetwork.entitlement!.allowConfigurations,
      configurationCountLimit: props.mainNetwork.entitlement!.configurationCountLimit - props.totalLimits["configurationCountLimit"] + props.network.entitlement!.configurationCountLimit,
      allowCustomPages: props.mainNetwork.entitlement!.allowCustomPages,
      customPageCountLimit: props.mainNetwork.entitlement!.customPageCountLimit - props.totalLimits["customPageCountLimit"] + props.network.entitlement!.customPageCountLimit,
      customPageBlockCountLimit: props.mainNetwork.entitlement!.customPageBlockCountLimit - props.totalLimits["customPageBlockCountLimit"] + props.network.entitlement!.customPageBlockCountLimit,
      customPageBlockSizeLimit: props.mainNetwork.entitlement!.customPageBlockSizeLimit - props.totalLimits["customPageBlockSizeLimit"] + props.network.entitlement!.customPageBlockSizeLimit,
    }
  };
});

onMounted(() => {
  initEntitlements(props.network.entitlement!, false);
});

function handleSubmit() {
  const data = getSubmitData();
  emit('update', data);
}
</script>
