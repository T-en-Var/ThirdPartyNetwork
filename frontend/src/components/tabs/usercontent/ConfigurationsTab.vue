<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="flex flex-col gap-6">
      <UserContentViewer
        title="Configurations"
        :entries="configurations || []"
        :extra-columns="[
          {
            key: 'key',
            label: 'Key',
            type: 'string',
            filter: false,
          },
          {
            key: 'value',
            label: 'Value',
            type: 'string',
            filter: false,
          },
        ]"
        :show-network="false"
        @add-new="showCreateModal = true"
        @edit="handleEditConfiguration"
        @remove="handleRemoveConfiguration"
      >
      </UserContentViewer>

      <AddConfigurationModal
        v-if="showCreateModal"
        :is-submitting="isSubmitting"
        :network-id="networkId"
        @submit="handleCreateConfiguration"
        @close="showCreateModal = false"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Configuration, ConfirmForm, CreateConfiguration } from '@/types';
import { computed, onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddConfigurationModal from '@/components/modals/usercontent/AddConfigurationModal.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const rawConfigurations = ref<Configuration[]>([]);
const configurations = computed(() =>
  (rawConfigurations.value ?? []).map((cfg) => {
    return { ...cfg, value: previewValue(cfg.value) as unknown as object };
  }),
);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchConfigurations: () => Promise<Ref<Configuration[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'configurations-create', networkId: string, payload: CreateConfiguration): void;
  (e: 'configurations-edit', configuration: Configuration): void;
  (e: 'configurations-delete', configuration: Configuration): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  const remoteRef = await props.fetchConfigurations();

  watch(remoteRef, (newVal) => (rawConfigurations.value = newVal ?? []), { immediate: true });
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

async function handleCreateConfiguration(
  networkId: string,
  configurationCreate: CreateConfiguration,
) {
  events.listen.configurations.create((configuration) => {
    showCreateModal.value = false;
    handleEditConfiguration(configuration);
  }, true);

  emit('configurations-create', networkId, configurationCreate);
}

function handleEditConfiguration(configuration: Configuration) {
  emit('configurations-edit', configuration);
}

function handleRemoveConfiguration(configuration: Configuration) {
  emit('configurations-delete', configuration);
}
</script>
