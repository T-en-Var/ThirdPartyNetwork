<template>
  <div class="edit-blog-tab">
    <LoadingErrorComponent
      :loading="loading"
      :error="error ?? undefined"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!blog"
    />

    <div
      v-if="!loading && !error"
      class="edit-blog-tab__card"
    >
      <div class="edit-blog-tab__header">
        <div class="edit-blog-tab__heading">
          <h2 class="edit-blog-tab__title">
            {{ isPreviewing ? 'Preview Mode' : 'Edit Blog Post' }}
          </h2>
          <span class="badge badge--neutral">
            {{ published ? (+form.publishedAt! > Date.now() ? 'Published' : 'Planned') : 'Draft' }}
          </span>
        </div>

        <button
          type="button"
          @click="isPreviewing = !isPreviewing"
          class="edit-blog-tab__preview-toggle"
          :class="isPreviewing ? 'edit-blog-tab__preview-toggle--active' : ''"
        >
          <svg
            v-if="!isPreviewing"
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
            <circle
              cx="12"
              cy="12"
              r="3"
            />
          </svg>
          <svg
            v-else
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z" />
            <path d="m15 5 4 4" />
          </svg>

          <span>{{ isPreviewing ? 'Back to Edit' : 'Preview' }}</span>
        </button>
      </div>

      <form
        v-if="!isPreviewing"
        @submit.prevent="handleUpdate"
        class="form"
      >
        <div class="edit-blog-tab__row">
          <div class="form-field edit-blog-tab__field--title">
            <label class="form-label">Title</label>
            <input
              v-model="form.title"
              type="text"
              @keydown.enter.prevent
              placeholder="Enter an engaging title..."
              class="form-input"
              required
            />
          </div>

          <div class="form-field">
            <AccessLevelPicker
              v-model="form.accessLevel"
              label="Access Level"
            />
          </div>
        </div>

        <div class="form-field">
          <label class="form-label">Summary (SEO)</label>
          <textarea
            v-model="form.summary"
            rows="2"
            placeholder="A short description for search engines and card previews..."
            class="form-textarea"
          ></textarea>
        </div>

        <div class="form-field">
          <label class="form-label">Content</label>
          <RichTextEditor
            ref="editorRef"
            v-model="form.body"
            class="edit-blog-tab__editor"
            @request-image="showImageModal = true"
          />
        </div>

        <div class="edit-blog-tab__publish-row">
          <label class="edit-blog-tab__publish-checkbox-label">
            <input
              type="checkbox"
              v-model="published"
              class="checkbox"
            />
            <span>Publish</span>
          </label>

          <div
            v-if="published"
            class="edit-blog-tab__publish-date"
          >
            <span class="edit-blog-tab__publish-date-label">Date:</span>
            <input
              v-model="publishedAtLocal"
              type="datetime-local"
              @keydown.enter.prevent
              class="form-input"
            />
          </div>
        </div>

        <div class="form__actions">
          <button
            type="button"
            @click="router.back()"
            class="btn btn--outline"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="isSaving"
            class="btn btn--primary"
          >
            <span v-if="isSaving">Saving...</span>
            <span v-else>Save Changes</span>
          </button>
        </div>
      </form>

      <div
        v-if="isPreviewing"
        class="edit-blog-tab__preview"
      >
        <BlogRenderer
          :title="form.title"
          :content="form.body"
          :published-at="
            published ? (publishedAtLocal ? new Date(publishedAtLocal) : new Date()) : null
          "
        />
      </div>
    </div>

    <AddFileModal
      v-if="showImageModal"
      :network="network"
      media-type="image"
      @close="showImageModal = false"
      @uploaded="handleImageInserted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import RichTextEditor from '@/components/fields/RichTextEditor.vue';
import BlogRenderer from '@/components/fields/BlogRenderer.vue';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import api from '@/api/api';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import type { Network, NetworkFile } from '@/types';
import { useEditableEntity } from '@/composables/useEditableEntity';
import { useGlobalStore } from '@/stores/global';
import { extractApiErrorMessage } from '@/lib/utils';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();

const props = defineProps<{
  network: Network;
  fetchBlog: (blogId: string) => Promise<Ref<Blog | null>>;
}>();

const blogId = computed(() => route.params.blogId as string);
const networkId = route.params.networkId as string;

const isPreviewing = ref(false);
const isSaving = ref(false);
const form = ref<Omit<CreateBlog, 'body'> & { body?: object }>({
  title: '',
  summary: '',
  body: undefined,
  accessLevel: 0,
});
const published = ref(false);
const publishedAtLocal = ref<string | null>(null);
const showImageModal = ref(false);
const editorRef = ref<InstanceType<typeof RichTextEditor> | null>(null);

const {
  entity: blog,
  loading,
  error,
} = useEditableEntity<Blog>({
  id: blogId,
  fetch: props.fetchBlog,
  notFoundMessage: 'Blog not found.',
  onNotFound: () => router.back(),
  onLoaded: (loadedBlog) => {
    form.value.title = loadedBlog.title;
    form.value.summary = loadedBlog.summary ?? '';
    form.value.accessLevel = loadedBlog.accessLevel ?? 0;

    form.value.body = loadedBlog.body ? JSON.parse(JSON.stringify(loadedBlog.body)) : undefined;

    published.value = !!loadedBlog.publishedAt;
    publishedAtLocal.value = loadedBlog.publishedAt
      ? toLocalDatetimeInput(loadedBlog.publishedAt)
      : null;
  },
});

function handleImageInserted(file: NetworkFile) {
  if (editorRef.value && file.url) {
    editorRef.value.insertImage(file.url);
  }
  showImageModal.value = false;
}

function toLocalDatetimeInput(d: Date | string) {
  const dt = new Date(d);
  const off = dt.getTimezoneOffset();
  const local = new Date(dt.getTime() - off * 60 * 1000);
  return local.toISOString().slice(0, 16);
}

async function handleUpdate() {
  if (!blog.value) return;
  isSaving.value = true;

  try {
    const payload: CreateBlog = {
      ...form.value,
      body: form.value.body ? form.value.body : {},
      publishedAt: published.value
        ? publishedAtLocal.value
          ? new Date(publishedAtLocal.value)
          : new Date()
        : undefined,
    };

    await api.put(`/networks/${networkId}/blogs/${blog.value.id}`, payload);

    alert('Blog saved successfully!');
  } catch (err) {
    global.addToast({
      message: extractApiErrorMessage(err, 'Failed to save blog.'),
      type: 'error',
      duration: 5000,
    });
  } finally {
    isSaving.value = false;
  }
}
</script>
