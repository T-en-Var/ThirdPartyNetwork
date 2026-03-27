<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <UserContentViewer title="CustomPages" :entries="customPages || []" :extra-columns="[{
        key: 'name',
        label: 'Name',
        type: 'string',
        filter: false,
      }, {
        key: 'value',
        label: 'Value',
        type: 'string',
        filter: false,
      }]" :show-network="false" @add-new="showCreateModal = true"
        @edit="handleEditCustomPage" @remove="handleRemoveCustomPage">
      </UserContentViewer>

      <AddCustomPageModal v-if="showCreateModal"
        :is-submitting="isSubmitting" @create-custom-page="handleCreateCustomPage"
        @close="showCreateModal = false" />
    </div>
  </div>
</template>

<script setup lang="ts">
import useCustomPages from '@/composables/useCustomPages';
import type { CustomPage, ConfirmForm, CreateCustomPage } from '@/types';
import { onMounted, ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddCustomPageModal from '@/components/modals/usercontent/AddCustomPageModal.vue';

const { execute: fetchCustomPages, data: customPages } = useCustomPages().fetchCustomPages;

const showCreateModal = ref(false);
const isSubmitting = ref(false);


const props = defineProps<{
  networkId: string;
}>();

const emit = defineEmits<{
  (e: 'custom-pages-create', networkId: string, payload: CreateCustomPage): void;
  (e: 'custom-pages-edit', customPage: CustomPage): void;
  (e: 'custom-pages-delete', customPage: CustomPage): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();


onMounted(async () => {
  await fetchCustomPages(props.networkId);
});


async function handleCreateCustomPage(networkId: string, customPageCreate: CreateCustomPage) {
  emit('custom-pages-create', networkId, customPageCreate);
}

function handleEditCustomPage(customPage: CustomPage) {
  emit('custom-pages-edit', customPage);
}

function handleRemoveCustomPage(customPage: CustomPage) {
  emit('custom-pages-delete', customPage);
}
</script>
