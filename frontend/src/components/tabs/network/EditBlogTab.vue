<template>
  <div class="max-w-4xl mx-auto py-8">
    <LoadingErrorComponent :loading="loading" :error="error" button-value="Go back"
      @button-action="router.go(-1)" />

    <div v-if="!loading && !error"
      class="bg-white shadow-lg rounded-xl overflow-hidden p-8 space-y-8">

      <div class="border-b border-gray-100 pb-4 flex justify-between items-end">
        <div class="flex items-center gap-3">
          <h2 class="text-2xl font-bold text-gray-800">
            {{ isPreviewing ? 'Preview Mode' : 'Edit Blog Post' }}
          </h2>
          <span class="text-xs text-gray-500 bg-gray-100 px-2 py-1 rounded">
            {{ published ? +form.publishedAt! > Date.now() ? 'Published' : 'Planned' : 'Draft' }}
          </span>
        </div>

        <button type="button" @click="isPreviewing = !isPreviewing"
          class="flex items-center gap-2 px-3 py-1.5 rounded-lg border text-sm font-medium transition-all"
          :class="isPreviewing
            ? 'bg-blue-50 text-blue-700 border-blue-200'
            : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'">
          <svg v-if="!isPreviewing" xmlns="http://www.w3.org/2000/svg" width="16"
            height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor"
            stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
            <circle cx="12" cy="12" r="3" />
          </svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" width="16" height="16"
            viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
            stroke-linecap="round" stroke-linejoin="round">
            <path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z" />
            <path d="m15 5 4 4" />
          </svg>

          <span>{{ isPreviewing ? 'Back to Edit' : 'Preview' }}</span>
        </button>
      </div>

      <form v-if="!isPreviewing" @submit.prevent="handleUpdate" class="space-y-6">

        <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div class="md:col-span-3">
            <label
              class="block text-sm font-semibold text-gray-700 mb-2">Title</label>
            <input v-model="form.title" type="text" @keydown.enter.prevent
              placeholder="Enter an engaging title..."
              class="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 shadow-sm focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all outline-none"
              required />
          </div>

          <div>
            <AccessLevelPicker v-model="form.accessLevel" label="Access Level" />
          </div>
        </div>

        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">Summary
            (SEO)</label>
          <textarea v-model="form.summary" rows="2"
            placeholder="A short description for search engines and card previews..."
            class="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 shadow-sm focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all outline-none resize-none"></textarea>
        </div>

        <div>
          <label
            class="block text-sm font-semibold text-gray-700 mb-2">Content</label>
          <RichTextEditor ref="editorRef" v-model="form.body" class="min-h-[400px]"
            @request-image="showImageModal = true" />
        </div>

        <div
          class="bg-gray-50 rounded-lg p-4 border border-gray-200 flex flex-wrap items-center gap-6">
          <label class="flex items-center gap-3 cursor-pointer">
            <input type="checkbox" v-model="published"
              class="w-5 h-5 text-blue-600 rounded border-gray-300 focus:ring-blue-500" />
            <span class="text-sm font-medium text-gray-700">Publish</span>
          </label>

          <div v-if="published" class="flex items-center gap-2">
            <span class="text-sm text-gray-500">Date:</span>
            <input v-model="publishedAtLocal" type="datetime-local" @keydown.enter.prevent
              class="rounded border-gray-300 text-sm py-1.5 px-3 focus:ring-blue-500 focus:border-blue-500" />
          </div>
        </div>

        <div class="flex justify-end pt-4 border-t border-gray-100">
          <button type="button" @click="router.back()"
            class="mr-4 px-6 py-2.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            Cancel
          </button>
          <button type="submit" :disabled="isSaving"
            class="px-8 py-2.5 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 shadow-sm transition-colors flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed">
            <span v-if="isSaving">Saving...</span>
            <span v-else>Save Changes</span>
          </button>
        </div>
      </form>

      <div v-if="isPreviewing" class="animate-in fade-in duration-300">
        <BlogRenderer :title="form.title" :content="form.body"
          :published-at="published ? (publishedAtLocal ? new Date(publishedAtLocal) : new Date()) : null" />
      </div>

    </div>

    <AddFileModal v-if="showImageModal" :network="network" media-type="image"
      @close="showImageModal = false" @uploaded="handleImageInserted" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import useBlogs from '@/composables/useBlogs';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import RichTextEditor from '@/components/fields/RichTextEditor.vue';
import BlogRenderer from '@/components/fields/BlogRenderer.vue';
import type { CreateBlog } from '@/types/userContent/blog';
import api from '@/api/api';
import AddFileModal from '@/components/modals/network/AddFileModal.vue'; // Import your modal
import type { Network, NetworkFile } from '@/types';
import { useHistoryStore } from '@/stores/history';

const router = useRouter();
const route = useRoute();
const historyStore = useHistoryStore();
const { blog, loading, error, fetchBlog } = useBlogs();

const blogId = route.params.blogId as string;
const networkId = route.params.networkId as string;

// State
const isPreviewing = ref(false); // Toggle state
const isSaving = ref(false);
const form = ref<Omit<CreateBlog, "body"> & { body?: object; }>({ title: '', summary: '', body: undefined, accessLevel: 0 });
const published = ref(false);
const publishedAtLocal = ref<string | null>(null);
const showImageModal = ref(false);
const editorRef = ref<InstanceType<typeof RichTextEditor> | null>(null);

defineProps<{
  network: Network;
}>();

onMounted(async () => {
  await fetchBlog(networkId, blogId);
  if (!blog.value) return;

  historyStore.blogVisit(blog.value);

  // Hydrate Form
  form.value.title = blog.value.title;
  form.value.summary = blog.value.summary ?? '';
  form.value.accessLevel = blog.value.accessLevel ?? 0;

  // Handle Body
  form.value.body = blog.value.body ? JSON.parse(JSON.stringify(blog.value.body)) : undefined;

  // Handle Dates
  console.log('blog publishedAt', blog.value.publishedAt, !!blog.value.publishedAt);
  published.value = !!blog.value.publishedAt;
  publishedAtLocal.value = blog.value.publishedAt ? toLocalDatetimeInput(blog.value.publishedAt) : null;
});

function handleImageInserted(file: NetworkFile) {
  if (editorRef.value && file.url) {
    editorRef.value.insertImage(file.url);
  }
  showImageModal.value = false;
}

// Helper for Datetime Local Input
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
      // Handle the date logic
      publishedAt: published.value
        ? (publishedAtLocal.value ? new Date(publishedAtLocal.value) : new Date())
        : undefined
    };

    await api.put(`/networks/${networkId}/blogs/${blog.value.id}`, payload);

    alert("Blog saved successfully!");
    // Optionally flip back to edit mode or redirect
    // isPreviewing.value = false; 

  } catch (err) {
    console.error('Error updating blog', err);
    alert("Failed to save blog.");
  } finally {
    isSaving.value = false;
  }
}
</script>