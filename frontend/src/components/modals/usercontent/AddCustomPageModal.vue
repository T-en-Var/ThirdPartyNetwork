<template>
  <modal-container
    title="Add Custom Page"
    @close="$emit('close')"
  >
    <CreateUserContentContainer
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="networkId"
      :network-ids="networkIds"
      button-text="Add Blog"
      @submit="handleSubmit"
    >
      <div>
        <label
          for="pageName"
          class="mb-2 block text-sm font-semibold text-gray-800"
        >
          Page Name
        </label>
        <input
          id="pageName"
          v-model="name"
          type="text"
          placeholder="Enter page name"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
          required
        />
      </div>

      <!-- TODO: 'unique ensure' -->
      <div>
        <label
          for="pageSlug"
          class="mb-2 block text-sm font-semibold text-gray-800"
        >
          Page Slug
        </label>
        <input
          id="pageSlug"
          v-model="slug"
          type="text"
          placeholder="Enter page slug"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
          required
        />
        <p class="mt-1 text-xs text-gray-500">
          Information which should make it easier to look up.
        </p>
      </div>
    </CreateUserContentContainer>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { CreateCustomPage, CreateUserContentForm } from '@/types';
import CreateUserContentContainer from '../CreateUserContentContainer.vue';

defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, payload: CreateCustomPage): void;
}>();

const inputIsValid = computed(() => true);

const name = ref('');
const slug = ref('');

function handleSubmit(form: CreateUserContentForm) {
  emit('submit', form.networkId, {
    name: name.value.trim(),
    slug: slug.value.trim(),
    accessLevel: form.accessLevel,
  });
}
</script>
