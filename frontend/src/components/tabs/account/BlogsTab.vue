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
import { useRoute, useRouter } from 'vue-router';
import UserContentViewer from '@/components/UserContentViewer.vue';
import AddBlogModal from '@/components/modals/network/AddBlogModal.vue';

const route = useRoute();
const router = useRouter();

const { execute: fetchBlogs, data: blogs } = useBlogs().fetchBlogs;

const networkId = route.params.networkId as string;

const showCreateModal = ref(false);
const isSubmitting = ref(false);

onMounted(async () => {
  await fetchBlogs(networkId);
});

const emit = defineEmits<{
  (e: 'confirm', form: ConfirmForm): void;
}>();


async function handleCreateBlog(blogCreate: CreateBlog) {
  const { execute: createBlog } = useBlogs().createBlog;
  isSubmitting.value = true;

  try {
    await createBlog(networkId, blogCreate);
    showCreateModal.value = false;
  } catch (err) {
    console.error('Error creating blog:', err);
  } finally {
    isSubmitting.value = false;
  }
}

function handleEditBlog(blog: Blog) {
  router.push(`/networks/${networkId}/manage/blogs/${blog.id}/edit`);
}

function handleRemoveBlog(blog: Blog) {
  const form: ConfirmForm = {
    title: 'Remove Blog',
    message: `Are you sure you want to remove ${blog.title} from this network?`,
    buttonText: 'Remove',
    buttonColor: 'red',

    action: async () => {
      isSubmitting.value = true;
      const { execute: deleteBlog } = useBlogs().deleteBlog;

      try {
        await deleteBlog(networkId, blog.id);
      } catch (err) {
        console.error('Error removing blog:', err);
      } finally {
        isSubmitting.value = false;
      }
    }
  };

  emit('confirm', form);
}
</script>
