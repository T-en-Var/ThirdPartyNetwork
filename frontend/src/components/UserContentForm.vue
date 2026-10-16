<template>
  <form
    @submit.prevent="handleSubmit"
    class="user-content-form"
  >
    <slot></slot>

    <div
      v-if="showNetworkSelector"
      class="form-field"
    >
      <label class="form-label"> Select a Network to store the image </label>
      <div class="user-content-form__network-options">
        <label
          v-for="networkId in props.networkIds"
          :key="networkId"
          class="user-content-form__network-option"
          :class="
            selectedNetwork?.id === networkId ? 'user-content-form__network-option--selected' : ''
          "
        >
          <input
            type="radio"
            class="user-content-form__network-option-radio"
            :value="networks[networkId]"
            v-model="selectedNetwork"
          />
          <span class="user-content-form__network-option-name">{{
            networks[networkId].name
          }}</span>
        </label>
      </div>
    </div>

    <p
      v-if="error"
      class="form-error"
    >
      {{ error }}
    </p>

    <div
      v-if="warningText"
      class="alert alert--warning"
    >
      <span>{{ warningText }}</span>
    </div>

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
        type="submit"
        class="btn btn--primary"
        :disabled="isSubmitting || !inputIsValid"
      >
        <span v-if="isSubmitting">Submitted...</span>
        <span v-else>{{ buttonText }}</span>
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import type { CreateUserContentForm, NetworkDto } from '@/types';
import { computed, onMounted, ref, watch } from 'vue';
import useNetworks from '@/composables/useNetworks';

const networkState = useNetworks();

const props = defineProps<{
  buttonText: string;
  isSubmitting: boolean;
  inputIsValid: boolean;
  networkId?: string;
  networkIds?: string[];
  error?: string | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', form: CreateUserContentForm): void;
}>();

const warningText = ref('');
let warningTextTimeout: ReturnType<typeof setTimeout> | null = null;

const showNetworkSelector = computed(() => !!props.networkIds);
const selectedNetwork = ref<NetworkDto | null>(null);
const networks = ref<Record<string, NetworkDto>>({});

onMounted(() => {
  if (props.networkId === undefined && props.networkIds === undefined)
    throw new Error(
      "'props.networkId === undefined && props.networkIds === undefined' Define either one or both",
    );
});

const handleSubmit = () => {
  if (!props.inputIsValid) showWarning('Input is invalid.');
  if (props.isSubmitting) return;

  const form: CreateUserContentForm = {
    networkId: props.networkId ?? selectedNetwork.value?.id ?? '',
  };

  emit('submit', form);
};

watch(
  () => props.networkIds,
  async (newIds) => {
    if (!showNetworkSelector.value || !newIds?.length) return;
    const uniqueNetworkIds = [...new Set(newIds)];

    await Promise.all(
      uniqueNetworkIds.map(async (id) => {
        if (networks.value[id]) return;

        const network = await getNetwork(id);
        if (network) networks.value[id] = network;
      }),
    );
  },
  { immediate: true },
);

const showWarning = (message: string) => {
  if (warningTextTimeout) clearTimeout(warningTextTimeout);
  warningTextTimeout = null;

  warningText.value = message;
  warningTextTimeout = setTimeout(() => (warningText.value = ''), 3000);
};

async function getNetwork(networkId: string) {
  const { execute: fetchNetworkDetails, data: network } = networkState.fetchNetworkDetails;
  await fetchNetworkDetails(networkId);
  return network.value;
}
</script>
