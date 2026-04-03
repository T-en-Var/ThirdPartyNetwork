<template>
  <modal-container
    title="Add Page Block"
    @close="$emit('close')"
  >
    <UserContentForm
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="customPage.networkId"
      :hide-access-picker="true"
      button-text="Add Page Block"
      @submit="handleSubmit"
      @close="emit('close')"
    >
      <div>
        <label
          for="pageText"
          class="mb-2 block text-sm font-semibold text-gray-800"
        >
          Page Block Text
        </label>
        <input
          id="pageName"
          v-model="form.text"
          type="text"
          placeholder="Enter text"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
          required
        />
      </div>

      <div>
        <label
          for="pagePosition"
          class="mb-2 block text-sm font-semibold text-gray-800"
        >
          Page Block Posistion
        </label>
        <input
          id="pageSlug"
          v-model="form.position"
          type="number"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
          required
        />
        <p class="mt-1 text-xs text-gray-500">You can change all this later as well.</p>
      </div>
    </UserContentForm>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { CreatePageBlock, CreateUserContentForm, CustomPage } from '@/types';
import UserContentForm from '../../UserContentForm.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const props = defineProps<{
  isSubmitting: boolean;
  customPage: CustomPage;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, customPageId: string, pageBlock: CreatePageBlock): void;
}>();

const inputIsValid = computed(() => true);

const form = ref({
  text: '',
  position: 1,
  data: {},
  customPageId: props.customPage.id,
});

function handleSubmit(userContentFrom: CreateUserContentForm) {
  events.listen.pageBlocks.create(() => emit('close'), true);
  emit('submit', userContentFrom.networkId, props.customPage.id, { ...form.value });
}
</script>
