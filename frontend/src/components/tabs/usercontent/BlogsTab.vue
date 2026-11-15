<template>
  <div class="card">
    <div class="blogs-tab__content">
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
import type { BlogDto, ConfirmForm, CreateBlogDto } from '@/types';
import { onMounted, ref, watch, type Ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddBlogModal from '@/components/modals/usercontent/AddBlogModal.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const showCreateModal = ref(false);
const isSubmitting = ref(false);

const blogs = ref<BlogDto[]>([]);

const props = defineProps<{
  networkId?: string;
  networkIds?: string[];
  fetchBlogs: () => Promise<Ref<BlogDto[] | null>>;
}>();

const emit = defineEmits<{
  (e: 'blog-create', networkId: string, payload: CreateBlogDto): void;
  (e: 'blog-edit', blog: BlogDto): void;
  (e: 'blog-delete', blog: BlogDto): void;
  (e: 'confirm', form: ConfirmForm): void;
}>();

onMounted(async () => {
  const remoteRef = await props.fetchBlogs();

  watch(remoteRef, (newVal) => (blogs.value = newVal ?? []), { immediate: true });
});

async function handleCreateBlog(networkId: string, blogCreate: CreateBlogDto) {
  events.listen.blogs.create((blog) => {
    showCreateModal.value = false;
    handleEditBlog(blog);
  }, true);

  emit('blog-create', networkId, blogCreate);
}

function handleEditBlog(blog: BlogDto) {
  emit('blog-edit', blog);
}

function handleRemoveBlog(blog: BlogDto) {
  emit('blog-delete', blog);
}
</script>
