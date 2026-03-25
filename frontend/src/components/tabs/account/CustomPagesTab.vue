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
import { useRoute, useRouter } from 'vue-router';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddCustomPageModal from '@/components/modals/network/AddCustomPageModal.vue';

const route = useRoute();
const router = useRouter();

const { execute: fetchCustomPages, data: customPages } = useCustomPages().fetchCustomPages;

const networkId = route.params.networkId as string;

const showCreateModal = ref(false);
const isSubmitting = ref(false);

onMounted(async () => {
  await fetchCustomPages(networkId);
});

const emit = defineEmits<{
  (e: 'confirm', form: ConfirmForm): void;
}>();


async function handleCreateCustomPage(customPageCreate: CreateCustomPage) {
  const { execute: createCustomPage } = useCustomPages().createCustomPage;
  isSubmitting.value = true;

  try {
    await createCustomPage(networkId, customPageCreate);
    showCreateModal.value = false;
  } catch (err) {
    console.error('Error creating customPage:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function handleEditCustomPage(customPage: CustomPage) {
  router.push(`/networks/${networkId}/manage/custom-pages/${customPage.id}/edit`);
}

function handleRemoveCustomPage(customPage: CustomPage) {
  const form: ConfirmForm = {
    title: 'Remove CustomPage',
    message: `Are you sure you want to remove ${customPage.name} from this network?`,
    buttonText: 'Remove',
    buttonColor: 'red',

    action: async () => {
      isSubmitting.value = true;
      const { execute: deleteCustomPage } = useCustomPages().deleteCustomPage;

      try {
        await deleteCustomPage(networkId, customPage.id);
      } catch (err) {
        console.error('Error removing customPage:', err);
      } finally {
        isSubmitting.value = false;
      }
    }
  };

  emit('confirm', form);
}
</script>
