<template>
  <div class="card">
    <div class="custom-pages-tab__content">
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
        :fetch-custom-pages="fetchCustomPages"
        @submit="handleCreateCustomPage"
        @close="showCreateModal = false"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CustomPageDto, ConfirmForm, CreateCustomPageDto } from '@/types';
import { onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddCustomPageModal from '@/components/modals/usercontent/AddCustomPageModal.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const customPages = ref<CustomPageDto[]>([]);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchCustomPages: () => Promise<Ref<CustomPageDto[]>>;
}>();

const emit = defineEmits<{
  (e: 'custom-pages-create', networkId: string, payload: CreateCustomPageDto): void;
  (e: 'custom-pages-edit', customPage: CustomPageDto): void;
  (e: 'custom-pages-delete', customPage: CustomPageDto): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  const remoteRef = await props.fetchCustomPages();

  watch(remoteRef, (newVal) => (customPages.value = newVal ?? []), { immediate: true });
});

async function handleCreateCustomPage(networkId: string, customPageCreate: CreateCustomPageDto) {
  events.listen.customPages.create((customPage) => {
    showCreateModal.value = false;
    handleEditCustomPage(customPage);
  }, true);
  emit('custom-pages-create', networkId, customPageCreate);
}

function handleEditCustomPage(customPage: CustomPageDto) {
  emit('custom-pages-edit', customPage);
}

function handleRemoveCustomPage(customPage: CustomPageDto) {
  emit('custom-pages-delete', customPage);
}
</script>
