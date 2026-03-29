<template>
  <form
    @submit.prevent="handleSubmit"
    class="space-y-4"
  >
    <slot></slot>

    <access-level-picker v-model="accessLevel" />

    <div
      v-if="showNetworkSelector"
      class="space-y-2"
    >
      <label class="block text-sm font-medium text-gray-700">
        Select a Network to store the image
      </label>
      <div class="space-y-2">
        <label
          v-for="networkId in props.networkIds"
          :key="networkId"
          class="flex cursor-pointer items-center space-x-2 rounded-lg border p-2 transition hover:bg-gray-50"
          :class="
            selectedNetwork?.id === networkId ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300'
          "
        >
          <input
            type="radio"
            class="text-indigo-600 focus:ring-indigo-500"
            :value="networks[networkId]"
            v-model="selectedNetwork"
          />
          <span class="text-sm text-gray-700">{{ networks[networkId].name }}</span>
        </label>
      </div>
    </div>

    <p
      v-if="error"
      class="mt-2 text-sm text-red-600"
    >
      {{ error }}
    </p>

    <div
      v-if="warningText"
      class="rounded-md border-[1px] border-yellow-500 bg-yellow-200 p-2 text-yellow-600"
    >
      <span>{{ warningText }}</span>
    </div>

    <div class="flex justify-end space-x-3 border-t border-gray-200 pt-4">
      <button
        type="button"
        class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
        @click="$emit('close')"
        :disabled="isSubmitting"
      >
        Cancel
      </button>
      <button
        type="submit"
        class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-indigo-600"
        :disabled="isSubmitting || !inputIsValid"
      >
        <span v-if="isSubmitting">Submitted...</span>
        <span v-else>{{ buttonText }}</span>
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import type { CreateUserContentForm, Network } from '@/types';
import { computed, onMounted, ref, watch } from 'vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
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

const accessLevel = ref(0);
const warningText = ref('');
let warningTextTimeout: ReturnType<typeof setTimeout> | null = null;

const showNetworkSelector = computed(() => !!props.networkIds);
const selectedNetwork = ref<Network | null>(null);
const networks = ref<Record<string, Network>>({});

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
    networkId: props.networkId ?? 'fakenews',
    accessLevel: accessLevel.value,
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
