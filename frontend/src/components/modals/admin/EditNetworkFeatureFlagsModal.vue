<template>
  <modal-container
    :title="'Edit Network'"
    @close="$emit('close')"
  >
    <div class="edit-network-feature-flags-modal__intro">
      <span class="edit-network-feature-flags-modal__network-label">
        Network Name: <span class="edit-network-feature-flags-modal__network-name">{{ network.name }}</span>
      </span>
      <p
        v-if="props.network.isSystemProtected"
        class="alert alert--warning"
      >
        You are changing the max limits throughout the whole application. Be cautious. This will be
        able to exceed backend limits. Limits that are set, will be applied per network the next
        time you set that network's limits, not in total (for now).
      </p>
    </div>

    <feature-flags-form
      :network="fakeNetwork"
      v-model="featureFlagsData"
      :auto-increase-max="mainNetwork.id == network.id"
    />

    <div class="form__actions">
      <button
        type="button"
        class="btn btn--outline"
        @click="$emit('close')"
        :disabled="isSubmitting"
      >
        Cancel
      </button>
      <button
        @click="handleSubmit"
        type="submit"
        class="btn btn--primary"
        :disabled="isSubmitting"
      >
        <span
          v-if="isSubmitting"
          class="edit-network-feature-flags-modal__saving"
        >
          <svg
            class="edit-network-feature-flags-modal__spinner-icon"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              class="edit-network-feature-flags-modal__spinner-track"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              stroke-width="4"
            ></circle>
            <path
              class="edit-network-feature-flags-modal__spinner-head"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
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
import type { FeatureFlagLimits, NetworkDto, SettableFeatureFlag } from '@/types';
import FeatureFlagsForm from '@/components/FeatureFlagsForm.vue';
import { useFeatureFlags } from '@/composables/useFeatureFlags';

const props = withDefaults(
  defineProps<{
    mainNetwork: NetworkDto;
    network: NetworkDto;
    totalLimits: FeatureFlagLimits;
    isSubmitting: boolean;
  }>(),
  {
    isSubmitting: false,
  },
);

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'update', networkFeatureFlag: SettableFeatureFlag): void;
}>();

const { featureFlagsData, initFeatureFlags, getSubmitData } = useFeatureFlags(props.network);

const fakeNetwork: ComputedRef<NetworkDto> = computed(() => {
  return {
    ...props.mainNetwork,
    featureFlag: {
      networkId: props.mainNetwork.id,
      allowFiles: props.mainNetwork.featureFlag!.allowFiles,
      fileCountLimit:
        props.mainNetwork.featureFlag!.fileCountLimit -
        props.totalLimits['fileCountLimit'] +
        props.network.featureFlag!.fileCountLimit,
      fileSizeLimit: props.mainNetwork.featureFlag!.fileSizeLimit,
      fileStorageLimit:
        props.mainNetwork.featureFlag!.fileStorageLimit -
        props.totalLimits['fileStorageLimit'] +
        props.network.featureFlag!.fileStorageLimit,
      allowBlogs: props.mainNetwork.featureFlag!.allowBlogs,
      blogCountLimit:
        props.mainNetwork.featureFlag!.blogCountLimit -
        props.totalLimits['blogCountLimit'] +
        props.network.featureFlag!.blogCountLimit,
      allowConfigurations: props.mainNetwork.featureFlag!.allowConfigurations,
      configurationCountLimit:
        props.mainNetwork.featureFlag!.configurationCountLimit -
        props.totalLimits['configurationCountLimit'] +
        props.network.featureFlag!.configurationCountLimit,
      allowCustomPages: props.mainNetwork.featureFlag!.allowCustomPages,
      customPageCountLimit:
        props.mainNetwork.featureFlag!.customPageCountLimit -
        props.totalLimits['customPageCountLimit'] +
        props.network.featureFlag!.customPageCountLimit,
      customPageBlockCountLimit:
        props.mainNetwork.featureFlag!.customPageBlockCountLimit -
        props.totalLimits['customPageBlockCountLimit'] +
        props.network.featureFlag!.customPageBlockCountLimit,
      customPageBlockSizeLimit:
        props.mainNetwork.featureFlag!.customPageBlockSizeLimit -
        props.totalLimits['customPageBlockSizeLimit'] +
        props.network.featureFlag!.customPageBlockSizeLimit,
    },
  };
});

onMounted(() => {
  initFeatureFlags(props.network.featureFlag!, false);
});

function handleSubmit() {
  const data = getSubmitData();
  emit('update', data);
}
</script>
