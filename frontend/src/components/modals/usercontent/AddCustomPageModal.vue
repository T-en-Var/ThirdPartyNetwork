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
      <div class="form-field">
        <label
          for="pageName"
          class="form-label"
        >
          Page Name
        </label>
        <input
          id="pageName"
          v-model="name"
          type="text"
          placeholder="Enter page name"
          class="form-input"
          required
        />
      </div>

      <div class="form-field">
        <label
          for="pageSlug"
          class="form-label"
        >
          Page Slug
        </label>
        <input
          id="pageSlug"
          v-model="slug"
          type="text"
          placeholder="Enter page slug"
          class="form-input"
          required
        />
        <p class="form-help">Information which should make it easier to look up.</p>
        <transition name="fade">
          <p
            v-if="slugExists"
            class="add-custom-page-modal__slug-warning"
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
import type { CreateCustomPageDto, CreateUserContentForm, CustomPageDto } from '@/types';
import UserContentForm from '../../UserContentForm.vue';

const props = defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
  fetchCustomPages: () => Promise<Ref<CustomPageDto[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, payload: CreateCustomPageDto): void;
}>();

const inputIsValid = computed(() => true);

const name = ref('');
const slug = ref('');

const customPages = ref<CustomPageDto[]>([]);

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
  });
}
</script>
