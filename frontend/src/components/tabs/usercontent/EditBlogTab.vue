<template>
  <div class="mx-auto max-w-4xl py-8">
    <LoadingErrorComponent
      :loading="loading"
      :error="error ?? undefined"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!blog"
    />

    <div
      v-if="!loading && !error"
      class="space-y-8 overflow-hidden rounded-xl bg-white p-8 shadow-lg"
    >
      <div class="flex items-end justify-between border-b border-gray-100 pb-4">
        <div class="flex items-center gap-3">
          <h2 class="text-2xl font-bold text-gray-800">
            {{ isPreviewing ? 'Preview Mode' : 'Edit Blog Post' }}
          </h2>
          <span class="rounded bg-gray-100 px-2 py-1 text-xs text-gray-500">
            {{ published ? (+form.publishedAt! > Date.now() ? 'Published' : 'Planned') : 'Draft' }}
          </span>
        </div>

        <button
          type="button"
          @click="isPreviewing = !isPreviewing"
          class="flex items-center gap-2 rounded-lg border px-3 py-1.5 text-sm font-medium transition-all"
          :class="
            isPreviewing
              ? 'border-blue-200 bg-blue-50 text-blue-700'
              : 'border-gray-300 bg-white text-gray-600 hover:bg-gray-50'
          "
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
        class="space-y-6"
      >
        <div class="grid grid-cols-1 gap-6 md:grid-cols-4">
          <div class="md:col-span-3">
            <label class="mb-2 block text-sm font-semibold text-gray-700">Title</label>
            <input
              v-model="form.title"
              type="text"
              @keydown.enter.prevent
              placeholder="Enter an engaging title..."
              class="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 shadow-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
              required
            />
          </div>

          <div>
            <AccessLevelPicker
              v-model="form.accessLevel"
              label="Access Level"
            />
          </div>
        </div>

        <div>
          <label class="mb-2 block text-sm font-semibold text-gray-700">Summary (SEO)</label>
          <textarea
            v-model="form.summary"
            rows="2"
            placeholder="A short description for search engines and card previews..."
            class="block w-full resize-none rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 shadow-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
          ></textarea>
        </div>

        <div>
          <label class="mb-2 block text-sm font-semibold text-gray-700">Content</label>
          <RichTextEditor
            ref="editorRef"
            v-model="form.body"
            class="min-h-[400px]"
            @request-image="showImageModal = true"
          />
        </div>

        <div
          class="flex flex-wrap items-center gap-6 rounded-lg border border-gray-200 bg-gray-50 p-4"
        >
          <label class="flex cursor-pointer items-center gap-3">
            <input
              type="checkbox"
              v-model="published"
              class="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span class="text-sm font-medium text-gray-700">Publish</span>
          </label>

          <div
            v-if="published"
            class="flex items-center gap-2"
          >
            <span class="text-sm text-gray-500">Date:</span>
            <input
              v-model="publishedAtLocal"
              type="datetime-local"
              @keydown.enter.prevent
              class="rounded border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:ring-blue-500"
            />
          </div>
        </div>

        <div class="flex justify-end border-t border-gray-100 pt-4">
          <button
            type="button"
            @click="router.back()"
            class="mr-4 rounded-lg border border-gray-300 bg-white px-6 py-2.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="isSaving"
            class="flex items-center gap-2 rounded-lg bg-blue-600 px-8 py-2.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-70"
          >
            <span v-if="isSaving">Saving...</span>
            <span v-else>Save Changes</span>
          </button>
        </div>
      </form>

      <div
        v-if="isPreviewing"
        class="duration-300 animate-in fade-in"
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
import { ref, watch, computed, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import RichTextEditor from '@/components/fields/RichTextEditor.vue';
import BlogRenderer from '@/components/fields/BlogRenderer.vue';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import api from '@/api/api';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import type { Network, NetworkFile } from '@/types';

const router = useRouter();
const route = useRoute();

const props = defineProps<{
  network: Network;
  fetchBlog: (blogId: string) => Promise<Ref<Blog | null>>;
}>();

const blogId = computed(() => route.params.blogId as string);
const networkId = route.params.networkId as string;

const blog = ref<Blog | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

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

watch(
  blogId,
  async (newId) => {
    blog.value = null;
    loading.value = true;

    const data = await props.fetchBlog(newId);
    loading.value = false;

    if (!data.value) throw new Error('Blog not found');
    watch(data, (newEntry) => (blog.value = newEntry), { immediate: true });

    form.value.title = blog.value!.title;
    form.value.summary = blog.value!.summary ?? '';
    form.value.accessLevel = blog.value!.accessLevel ?? 0;

    form.value.body = blog.value!.body ? JSON.parse(JSON.stringify(blog.value!.body)) : undefined;

    published.value = !!blog.value!.publishedAt;
    publishedAtLocal.value = blog.value!.publishedAt
      ? toLocalDatetimeInput(blog.value!.publishedAt)
      : null;
  },
  { immediate: true },
);

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
    console.error('Error updating blog', err);
    alert('Failed to save blog.');
  } finally {
    isSaving.value = false;
  }
}
</script>
