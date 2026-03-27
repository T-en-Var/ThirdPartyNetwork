<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <UserContentViewer title="Blogs" :entries="blogs || []" :extra-columns="[{
        key: 'title',
        label: 'Title',
        type: 'string',
        filter: false,
      }, {
        key: 'slug',
        label: 'Slug',
        type: 'string',
        filter: false,
      }, {
        key: 'summary',
        label: 'Summary',
        type: 'string',
        filter: false,
      }]" :show-network="false" @add-new="showCreateModal = true"
        @edit="handleEditBlog" @remove="handleRemoveBlog">
      </UserContentViewer>

      <AddBlogModal v-if="showCreateModal" :is-submitting="isSubmitting"
        :network-id="networkId" @create-blog="handleCreateBlog"
        @close="showCreateModal = false" />
    </div>
  </div>
</template>

<script setup lang="ts">
import useBlogs from '@/composables/useBlogs';
import type { Blog, ConfirmForm, CreateBlog } from '@/types';
import { onMounted, ref } from 'vue';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddBlogModal from '@/components/modals/usercontent/AddBlogModal.vue';

const { execute: fetchBlogs, data: blogs } = useBlogs().fetchBlogs;

const props = defineProps<{
  networkId: string;
}>();

const emit = defineEmits<{
  (e: 'blog-create', networkId: string, payload: CreateBlog): void;
  (e: 'blog-edit', blog: Blog): void;
  (e: 'blog-delete', blog: Blog): void;
  (e: 'confirm', form: ConfirmForm): void;
}>()

const showCreateModal = ref(false);
const isSubmitting = ref(false);

onMounted(async () => {
  await fetchBlogs(props.networkId);
});


async function handleCreateBlog(networkId: string, blogCreate: CreateBlog) {
  emit('blog-create', networkId, blogCreate);
}

function handleEditBlog(blog: Blog) {
  emit('blog-edit', blog);
}

function handleRemoveBlog(blog: Blog) {
  emit('blog-delete', blog);
}
</script>
