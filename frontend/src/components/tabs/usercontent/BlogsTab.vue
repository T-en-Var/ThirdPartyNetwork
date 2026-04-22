<template>
  <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <div class="flex flex-col gap-6">
      <UserContentViewer
        title="Blogs"
        :entries="blogs || []"
        :extra-columns="[
          {
            key: 'title',
            label: 'Title',
            type: 'string',
            filter: false,
          },
          {
            key: 'slug',
            label: 'Slug',
            type: 'string',
            filter: false,
          },
          {
            key: 'summary',
            label: 'Summary',
            type: 'string',
            filter: false,
          },
        ]"
        :show-network="false"
        @add-new="showCreateModal = true"
        @edit="handleEditBlog"
        @remove="handleRemoveBlog"
      >
      </UserContentViewer>

      <AddBlogModal
        v-if="showCreateModal"
        :is-submitting="isSubmitting"
        :network-id="networkId"
        :fetch-blogs="fetchBlogs"
        @create-blog="handleCreateBlog"
        @close="showCreateModal = false"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Blog, ConfirmForm, CreateBlog } from '@/types';
import { onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddBlogModal from '@/components/modals/usercontent/AddBlogModal.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const blogs = ref<Blog[]>([]);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchBlogs: () => Promise<Ref<Blog[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'blog-create', networkId: string, payload: CreateBlog): void;
  (e: 'blog-edit', blog: Blog): void;
  (e: 'blog-delete', blog: Blog): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  const remoteRef = await props.fetchBlogs();

  watch(remoteRef, (newVal) => (blogs.value = newVal ?? []), { immediate: true });
});

async function handleCreateBlog(networkId: string, blogCreate: CreateBlog) {
  events.listen.blogs.create((blog) => {
    showCreateModal.value = false;
    handleEditBlog(blog);
  }, true);

  emit('blog-create', networkId, blogCreate);
}

function handleEditBlog(blog: Blog) {
  emit('blog-edit', blog);
}

function handleRemoveBlog(blog: Blog) {
  emit('blog-delete', blog);
}
</script>
