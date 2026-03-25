<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <UserContentViewer title="Configurations" :entries="configurations || []"
        :extra-columns="[{
          key: 'key',
          label: 'Key',
          type: 'string',
          filter: false,
        }, {
          key: 'value',
          label: 'Value',
          type: 'string',
          filter: false,
        }]" :show-network="true" @add-new="showCreateModal = true"
        @edit="handleEditConfiguration" @remove="handleRemoveConfiguration">
      </UserContentViewer>

      <AddConfigurationModal v-if="showCreateModal" :is-submitting="isSubmitting"
        :network-id="networkId" :network-ids="networkIds"
        @submit="handleCreateConfiguration"
        @close="showCreateModal = false" />
    </div>
  </div>
</template>

<script setup lang="ts">
import useConfigurations from '@/composables/useConfigurations';
import type { Configuration, ConfirmForm, CreateConfiguration } from '@/types';
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddConfigurationModal from '@/components/modals/network/AddConfigurationModal.vue';

const route = useRoute();
const router = useRouter();

const { execute: fetchConfigurations, data: rawConfigurations } = useConfigurations().fetchNetworkConfigurations;

const networkId = route.params.networkId as string;
const networkIds = computed(() => (rawConfigurations.value ?? []).map(cfg => cfg.networkId));

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const configurations = computed(() => (rawConfigurations.value ?? []).map(cfg => {
  return { ...cfg, value: previewValue(cfg.value) as unknown as object };
}));

onMounted(async () => {
  await fetchConfigurations(networkId);
});

const emit = defineEmits<{
  (e: 'confirm', form: ConfirmForm): void;
}>();

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

async function handleCreateConfiguration(payload: CreateConfiguration) {
  const { execute: createConfiguration } = useConfigurations().createConfiguration;
  isSubmitting.value = true;

  try {
    await createConfiguration(networkId, payload);
    showCreateModal.value = false;
  } catch (err) {
    console.error('Error creating configuration:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function handleEditConfiguration(configuration: Configuration) {
  router.push(`/networks/${networkId}/manage/configurations/${configuration.id}/edit`);
}

function handleRemoveConfiguration(configuration: Configuration) {
  const form: ConfirmForm = {
    title: 'Remove Configuration',
    message: `Are you sure you want to remove ${configuration.key} from this network?`,
    buttonText: 'Remove',
    buttonColor: 'red',

    action: async () => {
      isSubmitting.value = true;
      const { execute: deleteConfiguration } = useConfigurations().deleteConfiguration;

      try {
        await deleteConfiguration(networkId, configuration.id);
      } catch (err) {
        console.error('Error removing configuration:', err);
      } finally {
        isSubmitting.value = false;
      }
    }
  };

  emit('confirm', form);
}
</script>
