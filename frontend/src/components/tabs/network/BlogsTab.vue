<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <div class="flex flex-row justify-between items-center">
        <h2 class="text-xl font-medium text-gray-800">Blogs</h2>
        <div class="flex items-center gap-4">
          <button @click="$emit('openCreateBlogModal')" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">Add Blog</button>
        </div>
      </div>

      <div>
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Title</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Slug</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Author</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="b in blogsState.blogs.value" :key="b.id" class="hover:bg-slate-100 cursor-pointer" @click="$emit('editBlog', b)">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-[300px]">{{ b.title }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ b.slug }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ b.author?.userProxy?.firstName }} {{ b.author?.userProxy?.lastName }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button class="text-blue-600 hover:text-blue-900 mr-3">Open</button>
                  <button @click.prevent.stop="$emit('removeBlog', b)" class="text-red-600 hover:text-red-900">Remove</button>
                </td>
              </tr>
              <tr v-if="!blogsState.blogs.value.length">
                <td colspan="4" class="px-6 py-10 text-center text-gray-500">No blogs found.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import useBlogs from '@/composables/useBlogs';
import type { Blog, CreateBlog } from '@/types';
import { onMounted } from 'vue';
import { useRoute } from 'vue-router';

const blogsState = useBlogs();
const route = useRoute();

onMounted(() => {
  const networkId = route.params.networkId as string;
  blogsState.fetchBlogs(networkId);
});

defineEmits<{
  (e: 'openCreateBlogModal'): void;
  (e: 'editBlog', blog: CreateBlog): void;
  (e: 'removeBlog', blog: Blog): void;
}>();
</script>
