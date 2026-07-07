<template>
  <modal-container
    title="Add Custom Page"
    @close="$emit('close')"
  >
    <UserContentForm
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="networkId"
      :network-ids="networkIds"
      button-text="Add Custom Page"
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
        <transition name="fade">
          <p
            v-if="slugExists"
            class="mt-2 rounded-lg border border-amber-200 bg-amber-100/50 p-2 text-[11px] text-amber-700"
          >
            This slug is already taken. The system will append a unique ID to your link.
          </p>
        </transition>
      </div>
    </UserContentForm>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted, type Ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { CreateCustomPage, CreateUserContentForm, CustomPage } from '@/types';
import UserContentForm from '../../UserContentForm.vue';

const props = defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
  fetchCustomPages: () => Promise<Ref<CustomPage[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, payload: CreateCustomPage): void;
}>();

const inputIsValid = computed(() => true);

const name = ref('');
const slug = ref('');

const customPages = ref<CustomPage[]>([]);

onMounted(async () => {
  const remoteRef = await props.fetchCustomPages();

  watch(remoteRef, (newVal) => (customPages.value = newVal ?? []), { immediate: true });
});

const slugExists = computed(() =>
  customPages.value.find((p) => p.slug === slug.value.trim() && slug.value.trim() !== ''),
);

function handleSubmit(form: CreateUserContentForm) {
  emit('submit', form.networkId, {
    name: name.value.trim(),
    slug: slug.value.trim(),
    accessLevel: form.accessLevel,
  });
}
</script>
