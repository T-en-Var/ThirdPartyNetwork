<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <div class="flex flex-col gap-6">
      <!-- Toolbar -->
      <div class="flex flex-col justify-between items-start gap-4 w-full">
        <div class="flex flex-row justify-between items-start gap-4 w-full">
          <div class="flex justify-end gap-6">
            <button @click="showFilters = !showFilters"
              class="w-64 py-2 text-sm bg-gray-200 hover:bg-gray-300 rounded-md text-gray-800">
              {{ showFilters ? 'Hide Filters' : 'Show Filters' }}
            </button>
            <h2 class="text-xl font-medium text-gray-800">Blogs</h2>
          </div>

          <div class="flex items-center gap-4">
            <div class="relative w-full sm:w-64">
              <input type="text" v-model="searchQuery" placeholder="Filter blogs..."
                class="w-full pl-3 pr-10 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm" />
              <div class="absolute inset-y-0 right-2 flex items-center pointer-events-none">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"
                  class="text-gray-500">
                  <circle cx="10" cy="10" r="6" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round" />
                  <path d="M14.5 14.5L19 19" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round" />
                </svg>
              </div>
            </div>

            <button @click="$emit('openCreateBlogModal')"
              class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">Add
              Blog</button>
          </div>
        </div>
      </div>

      <div class="flex flex-col xl:flex-row gap-6">
        <aside v-if="showFilters"
          class="flex flex-row flex-wrap xl:flex-col items-start gap-4 w-full xl:w-64 xl:min-w-64 bg-white border rounded-lg p-4 shadow-sm">
          <div>
            <h3 class="font-semibold text-gray-700">Author</h3>
            <div v-for="author in uniqueAuthors" :key="author.username"
              class="flex items-center gap-2">
              <input type="checkbox" :id="`author-${author.username}`"
                v-model="filters.authors" :value="author.username"
                class="accent-blue-600" />
              <label :for="`author-${author.username}`"
                class="text-sm text-gray-800">{{ author.firstName }} {{
                  author.lastName }} ({{ author.username }})</label>
            </div>
          </div>

          <div>
            <h3 class="font-semibold text-gray-700">Status</h3>
            <select v-model="statusFilter"
              class="w-full px-2 py-1 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option value="all">All Status</option>
              <option value="published">Published</option>
              <option value="unpublished">Unpublished</option>
            </select>
          </div>

          <div>
            <h3 class="font-semibold text-gray-700">Date Range</h3>
            <div class="flex flex-col gap-2">
              <input v-model="startDate" type="date"
                class="w-full px-2 py-1 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              <input v-model="endDate" type="date"
                class="w-full px-2 py-1 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>

          <div>
            <h3 class="font-semibold text-gray-700">Visibility</h3>
            <div v-for="level in ACCESS_LEVELS" :key="level.value"
              class="flex items-center gap-2">
              <input type="checkbox" :id="`access-${level.value}`"
                v-model="filters.accessLevels" :value="level.value"
                class="accent-blue-600" />
              <label :for="`access-${level.value}`" class="text-sm text-gray-800">
                {{ level.label }}
              </label>
            </div>
          </div>

          <button @click="clearFilters"
            class="w-auto xl:w-full text-sm px-3 py-2 bg-gray-100 hover:bg-gray-200 rounded text-gray-800 border border-gray-300">Clear
            Filters</button>
        </aside>

        <div class="flex-1">

          <div>
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Title</th>
                    <th
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Slug</th>
                    <th
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Published At</th>
                    <th
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Visibility</th>
                    <th
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Author</th>
                    <th
                      class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr v-for="b in filteredBlogs" :key="b.id"
                    class="hover:bg-slate-100 cursor-pointer"
                    @click="$emit('editBlog', b)">
                    <td
                      class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-[300px]">
                      {{ b.title }}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{
                      b.slug }}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {{ b.publishedAt ? new Date(b.publishedAt).toLocaleDateString()
                        :
                      'Not Published' }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{
                      getAccessLevel(b.accessLevel).label }}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{
                      b.author?.userProxy?.firstName }} {{
                        b.author?.userProxy?.lastName
                      }}</td>
                    <td
                      class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        class="text-blue-600 hover:text-blue-900 mr-3">Open</button>
                      <button @click.prevent.stop="$emit('removeBlog', b)"
                        class="text-red-600 hover:text-red-900">Remove</button>
                    </td>
                  </tr>
                  <tr v-if="!filteredBlogs.length">
                    <td colspan="7" class="px-6 py-10 text-center text-gray-500">No
                      blogs
                      found.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import useBlogs from '@/composables/useBlogs';
import type { Blog, CreateBlog } from '@/types';
import { onMounted, ref, computed } from 'vue';
import { useRoute } from 'vue-router';
import { ACCESS_LEVELS, getAccessLevel } from '@/lib/accessLevels';

const blogsState = useBlogs();
const route = useRoute();

const showFilters = ref(false);
const searchQuery = ref('');
const statusFilter = ref('all');
const startDate = ref('');
const endDate = ref('');

const filters = ref({ authors: [] as string[], accessLevels: [] as number[] });

const filteredBlogs = computed(() => {
  let blogs = blogsState.blogs.value || [];

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    blogs = blogs.filter((b) =>
      b.title?.toLowerCase().includes(query) ||
      b.slug?.toLowerCase().includes(query) ||
      b.summary?.toLowerCase().includes(query) ||
      (b.author?.userProxy?.firstName + ' ' + b.author?.userProxy?.lastName).toLowerCase().includes(query)
    );
  }

  if (statusFilter.value === 'published') {
    blogs = blogs.filter((b) => b.publishedAt);
  } else if (statusFilter.value === 'unpublished') {
    blogs = blogs.filter((b) => !b.publishedAt);
  }

  // authors filter
  if (filters.value.authors.length) {
    blogs = blogs.filter(b => filters.value.authors.includes(b.author?.userProxy?.username ?? ''));
  }

  // access level filter
  if (filters.value.accessLevels.length) {
    blogs = blogs.filter(b => filters.value.accessLevels.includes(b.accessLevel));
  }

  if (startDate.value) {
    blogs = blogs.filter((b) => b.publishedAt && new Date(b.publishedAt) >= new Date(startDate.value));
  }

  if (endDate.value) {
    blogs = blogs.filter((b) => b.publishedAt && new Date(b.publishedAt) <= new Date(endDate.value));
  }

  return blogs;
});

onMounted(() => {
  const networkId = route.params.networkId as string;
  blogsState.fetchBlogs(networkId);
});

const uniqueAuthors = computed(() => {
  const seen = new Map<string, { username: string; firstName: string; lastName: string; }>();
  (blogsState.blogs.value || []).forEach(b => {
    const u = b.author?.userProxy;
    if (u && !seen.has(u.username ?? 'Unknown')) {
      seen.set(u.username ?? 'Unknown', { username: u.username ?? 'Unknown', firstName: u.firstName ?? 'Unknown', lastName: u.lastName ?? 'Unknown' });
    }
  });
  return Array.from(seen.values());
});

const clearFilters = () => {
  filters.value = { authors: [], accessLevels: [] };
};

defineEmits<{
  (e: 'openCreateBlogModal'): void;
  (e: 'editBlog', blog: CreateBlog): void;
  (e: 'removeBlog', blog: Blog): void;
}>();
</script>
