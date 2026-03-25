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
      }]" :show-network="true" :hide-add-new="true" @add-new="() => {}"
        @edit="handleEditBlog" @remove="handleRemoveBlog">
      </UserContentViewer>
    </div>
  </div>
</template>

<script setup lang="ts">
import useBlogs from '@/composables/useBlogs';
import type { Blog, ConfirmForm } from '@/types';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import UserContentViewer from '@/components/UserContentViewer.vue';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const { execute: fetchUserBlogs, data: blogs } = useBlogs().fetchUserBlogs;
const isSubmitting = ref(false);

onMounted(async () => {
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error("Userproxy not found");
  await fetchUserBlogs(currentUser.user.id, currentUser.id);
});

const emit = defineEmits<{
  (e: 'confirm', form: ConfirmForm): void;
}>();

function handleEditBlog(blog: Blog) {
  router.push(`/account/networks/${blog.networkId}/blogs/${blog.id}/edit`);
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
        await deleteBlog(blog.networkId, blog.id);
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
