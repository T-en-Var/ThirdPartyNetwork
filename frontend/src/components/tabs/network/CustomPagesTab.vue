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
            <h2 class="text-xl font-medium text-gray-800">Custom Pages</h2>
          </div>

          <div class="flex flex-col sm:flex-row sm:items-center gap-4 w-full sm:w-auto">
            <div class="relative w-full sm:w-64">
              <input type="text" v-model="filterSlug" placeholder="Filter pages..."
                class="w-full pl-3 pr-10 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm" />
              <div class="absolute inset-y-0 right-2 flex items-center pointer-events-none">
                <!-- search icon -->
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"
                  class="text-gray-500">
                  <circle cx="10" cy="10" r="6" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round" />
                  <path d="M14.5 14.5L19 19" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round" />
                </svg>
              </div>
            </div>

            <button @click="$emit('openCreateCustomPageModal')"
              class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
              Add Page
            </button>
          </div>
        </div>
      </div>

      <div class="flex flex-col xl:flex-row gap-6">
        <!-- Sidebar Filters -->
        <aside v-if="showFilters"
          class="flex flex-row flex-wrap xl:flex-col items-start gap-4 w-full xl:w-64 xl:min-w-64 bg-white border rounded-lg p-4 shadow-sm">
          <div>
            <h3 class="font-semibold text-gray-700">Author</h3>
            <div v-for="author in uniqueAuthors" :key="author.username" class="flex items-center gap-2">
              <input type="checkbox" :id="`author-${author.username}`" v-model="filters.authors"
                :value="author.username" class="accent-blue-600" />
              <label :for="`author-${author.username}`" class="text-sm text-gray-800">
                {{ author.firstName }} {{ author.lastName }} ({{ author.username }})
              </label>
            </div>
          </div>

          <button @click="clearFilters"
            class="w-auto xl:w-full text-sm px-3 py-2 bg-gray-100 hover:bg-gray-200 rounded text-gray-800 border border-gray-300">
            Clear Filters
          </button>
        </aside>

        <!-- Main Content -->
        <div class="flex-1">
          <!-- Pages Table -->
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Slug</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Blocks</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Author</th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="page in filteredPages" :key="page.id" :title="page.name"
                  class="hover:bg-slate-100 cursor-pointer" @click="$emit('editCustomPage', page)">
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-[200px]">
                    {{ page.name }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ page.slug }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ page.pages.length }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 truncate max-w-[250px]"
                    :title="`${page.author.userProxy.firstName} ${page.author.userProxy.lastName} (${page.author.userProxy.username})`">
                    {{ page.author.userProxy.firstName }} {{ page.author.userProxy.lastName }}
                    <span class="text-gray-400">({{ page.author.userProxy.username }})</span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button class="text-blue-600 hover:text-blue-900 mr-3">
                      Open
                    </button>
                    <button @click.prevent.stop="$emit('removeCustomPage', page)"
                      class="text-red-600 hover:text-red-900">
                      Remove
                    </button>
                  </td>
                </tr>

                <tr v-if="!filteredPages.length">
                  <td colspan="5" class="px-6 py-10 text-center text-gray-500">
                    No pages found.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import useCustomPages from '@/composables/useCustomPages';
import type { CustomPage } from '@/types';
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';

const showFilters = ref<boolean>(false);
const filterSlug = ref<string>("");

const route = useRoute();
const customPagesState = useCustomPages();

const filters = ref({
  authors: [] as string[],
})

onMounted(async () => {
  const networkId = route.params.networkId as string;
  customPagesState.fetchCustomPages(networkId);
})

const clearFilters = () => {
  filters.value = {
    authors: [],
  }
  filterSlug.value = ""
}

const uniqueAuthors = computed(() => {
  const seen = new Map<string, { username: string; firstName: string; lastName: string }>()
  customPagesState.customPages.value.forEach(p => {
    const u = p.author.userProxy
    if (u && !seen.has(u.username ?? 'Unknown')) {
      seen.set(u.username ?? 'Unknown', { username: u.username ?? 'Unknown', firstName: u.firstName ?? 'Unknown', lastName: u.lastName ?? 'Unknown' })
    }
  })
  return Array.from(seen.values())
})

const filteredPages = computed(() => {
  const query = filterSlug.value.trim().toLowerCase()

  return customPagesState.customPages.value.filter(page => {
    const name = page.name.toLowerCase();
    const slug = page.slug.toLowerCase();
    const author = `${page.author?.userProxy?.username ?? ''} ${page.author?.userProxy?.firstName ?? ''} ${page.author?.userProxy?.lastName ?? ''}`.toLowerCase();

    const matchesSearch = query === "" || name.includes(query) || slug.includes(query) || author.includes(query);
    const matchesAuthor = filters.value.authors.length === 0 || filters.value.authors.includes(page.author?.userProxy?.username ?? '');

    return matchesSearch && matchesAuthor;
  })
})

defineEmits<{
  (e: 'openCreateCustomPageModal'): void;
  (e: 'editCustomPage', page: CustomPage): void;
  (e: 'removeCustomPage', page: CustomPage): void;
}>();
</script>
