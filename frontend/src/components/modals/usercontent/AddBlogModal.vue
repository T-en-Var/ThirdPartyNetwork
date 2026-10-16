<template>
  <modal-container
    title="Create New Blog"
    @close="$emit('close')"
  >
    <UserContentForm
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="networkId"
      :network-ids="networkIds"
      button-text="Add Blog"
      @submit="handleSubmit"
      @selected-network-id="handleNetworkSelect"
    >
      <div class="form-field">
        <label
          for="title"
          class="form-label"
        >
          Blog Title
        </label>
        <input
          id="title"
          v-model="title"
          type="text"
          placeholder="My Awesome Journey..."
          class="form-input"
          required
        />
      </div>

      <div class="add-blog-modal__slug-preview">
        <label class="add-blog-modal__slug-preview-label"> Permanent URL Preview </label>

        <div class="add-blog-modal__slug-row">
          <span class="add-blog-modal__slug-prefix">.../blogs/</span>
          <span
            :class="[
              'add-blog-modal__slug-value',
              slugExists ? 'add-blog-modal__slug-value--taken' : 'add-blog-modal__slug-value--ok',
              !slugPreview ? 'add-blog-modal__slug-value--empty' : '',
            ]"
          >
            {{ slugPreview || 'your-slug-here' }}
          </span>

          <div class="add-blog-modal__slug-status">
            <div
              v-if="slugExists"
              class="add-blog-modal__slug-status-icon add-blog-modal__slug-status-icon--warning"
              title="Slug Taken"
            >
              ⚠️
            </div>
            <div
              v-else
              class="add-blog-modal__slug-status-icon add-blog-modal__slug-status-icon--success"
              title="Available"
            >
              ✓
            </div>
          </div>
        </div>

        <transition name="fade">
          <div
            v-if="slugExists"
            class="add-blog-modal__slug-warning"
          >
            This URL is already taken. The system will append a unique ID to your link.
          </div>
        </transition>
      </div>
    </UserContentForm>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, type Ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { BlogDto, CreateBlogDto } from '@/types';
import UserContentForm from '../../UserContentForm.vue';
import type { CreateUserContentForm } from '@/types';

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'create-blog', networkId: string, payload: CreateBlogDto): void;
}>();

const props = defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
  fetchBlogs: () => Promise<Ref<BlogDto[] | null>>;
}>();

const title = ref('');
const selectedNetworkId = ref<string | null>(null);

const blogs = ref<BlogDto[]>([]);

const slugPreview = computed(() => generateSlug(title.value));
const inputIsValid = computed(() => true);

onMounted(async () => {
  if (!!props.networkId && !props.networkIds) handleNetworkSelect(props.networkId);

  const remoteRef = await props.fetchBlogs();

  watch(remoteRef, (newVal) => (blogs.value = newVal ?? []), { immediate: true });
});

const slugExists = computed(() => blogs.value.find((b) => b.slug == slugPreview.value));

function generateSlug(title: string) {
  if (!title) return '';
  let str = title.toLowerCase();
  str = str.normalize('NFD').replace(/\p{Diacritic}/gu, '');
  str = str.replace(/[^a-z0-9\s-]/g, '');
  str = str.replace(/[\s-]+/g, '-');
  return str.replace(/^-+|-+$/g, '');
}

function handleNetworkSelect(networkId: string) {
  selectedNetworkId.value = networkId;
}

function handleSubmit(form: CreateUserContentForm) {
  emit('create-blog', form.networkId, {
    title: title.value.trim(),
    summary: '',
    coverImageId: null,
    body: {},
  });
}
</script>
