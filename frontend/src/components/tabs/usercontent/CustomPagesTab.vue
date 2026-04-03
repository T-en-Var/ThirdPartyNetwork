<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="flex flex-col gap-6">
      <UserContentViewer
        title="Custom Pages"
        :entries="customPages || []"
        :extra-columns="[
          {
            key: 'name',
            label: 'Name',
            type: 'string',
            filter: false,
          },
          {
            key: 'slug',
            label: 'Slug',
            type: 'string',
            filter: false,
          },
        ]"
        :show-network="false"
        @add-new="showCreateModal = true"
        @edit="handleEditCustomPage"
        @remove="handleRemoveCustomPage"
      >
      </UserContentViewer>

      <AddCustomPageModal
        v-if="showCreateModal"
        :is-submitting="isSubmitting"
        :network-id="networkId"
        @submit="handleCreateCustomPage"
        @close="showCreateModal = false"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CustomPage, ConfirmForm, CreateCustomPage } from '@/types';
import { onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddCustomPageModal from '@/components/modals/usercontent/AddCustomPageModal.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const customPages = ref<CustomPage[]>([]);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchCustomPages: () => Promise<Ref<CustomPage[]>>;
}>();

const emit = defineEmits<{
  (e: 'custom-pages-create', networkId: string, payload: CreateCustomPage): void;
  (e: 'custom-pages-edit', customPage: CustomPage): void;
  (e: 'custom-pages-delete', customPage: CustomPage): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  const remoteRef = await props.fetchCustomPages();

  watch(remoteRef, (newVal) => (customPages.value = newVal ?? []), { immediate: true });
});

async function handleCreateCustomPage(networkId: string, customPageCreate: CreateCustomPage) {
  events.listen.customPages.create((customPage) => {
    showCreateModal.value = false;
    handleEditCustomPage(customPage);
  }, true);
  emit('custom-pages-create', networkId, customPageCreate);
}

function handleEditCustomPage(customPage: CustomPage) {
  emit('custom-pages-edit', customPage);
}

function handleRemoveCustomPage(customPage: CustomPage) {
  emit('custom-pages-delete', customPage);
}
</script>
