<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <UserContentViewer title="Configurations" :entries="configurations || []" :extra-columns="[{
        key: 'key',
        label: 'Key',
        type: 'string',
        filter: false,
      }, {
        key: 'value',
        label: 'Value',
        type: 'string',
        filter: false,
      }]" :show-network="false" @add-new="showCreateModal = true"
        @edit="handleEditConfiguration" @remove="handleRemoveConfiguration">
      </UserContentViewer>

      <AddConfigurationModal v-if="showCreateModal" :is-submitting="isSubmitting"
        :network-id="networkId" @submit="handleCreateConfiguration"
        @close="showCreateModal = false" />
    </div>
  </div>
</template>

<script setup lang="ts">
import useConfigurations from '@/composables/useConfigurations';
import type { Configuration, ConfirmForm, CreateConfiguration } from '@/types';
import { computed, onMounted, ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddConfigurationModal from '@/components/modals/usercontent/AddConfigurationModal.vue';

const { execute: fetchConfigurations, data: rawConfigurations } = useConfigurations().fetchNetworkConfigurations;

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const configurations = computed(() => (rawConfigurations.value ?? []).map(cfg => {
  return {...cfg, value: previewValue(cfg.value) as unknown as object}
}))

const props = defineProps<{
  networkId: string;
}>();

const emit = defineEmits<{
  (e: 'configuration-create', networkId: string, payload: CreateConfiguration): void;
  (e: 'configuration-edit', configuration: Configuration): void;
  (e: 'configuration-delete', configuration: Configuration): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  await fetchConfigurations(props.networkId);
});

function previewValue(v: object | string | number | boolean | null): string {
  try {
    if (typeof v === 'string') return v;
    const s = JSON.stringify(v);
    if (s.length > 80) return s.slice(0, 77) + '...';
    return s;
  } catch {
    return '';
  }
}

async function handleCreateConfiguration(networkId: string, configurationCreate: CreateConfiguration) {
  emit('configuration-create', networkId, configurationCreate)
}

function handleEditConfiguration(configuration: Configuration) {
  emit('configuration-edit', configuration);
}

function handleRemoveConfiguration(configuration: Configuration) {
  emit('configuration-delete', configuration);
}
</script>
