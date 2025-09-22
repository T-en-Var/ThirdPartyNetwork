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
            <h2 class="text-xl font-medium text-gray-800">Network Files</h2>
          </div>

          <div class="flex flex-col sm:flex-row sm:items-center gap-4 w-full sm:w-auto">
            <div class="relative w-full sm:w-64">
              <input type="text" v-model="filterSlug" placeholder="Filter files..."
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

            <button @click="$emit('addFile')" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
              Add File
            </button>
          </div>
        </div>
      </div>

      <div class="flex flex-col xl:flex-row gap-6">
        <!-- Sidebar Filters -->
        <aside v-if="showFilters"
          class="flex flex-row flex-wrap xl:flex-col items-start gap-4 w-full xl:w-64 xl:min-w-64  bg-white border rounded-lg p-4 shadow-sm">
          <div>
            <h3 class="font-semibold text-gray-700">Visibility</h3>
            <div v-for="opt in ['public', 'private']" :key="opt" class="flex items-center gap-2">
              <input type="checkbox" :id="`vis-${opt}`" v-model="filters.visibility" :value="opt"
                class="accent-blue-600" />
              <label :for="`vis-${opt}`" class="text-sm text-gray-800 capitalize">{{ opt }}</label>
            </div>
          </div>

          <div>
            <h3 class="font-semibold text-gray-700">Format</h3>
            <div v-for="type in formats" :key="type" class="flex items-center gap-2">
              <input type="checkbox" :id="`type-${type}`" v-model="filters.formats" :value="type"
                class="accent-blue-600" />
              <label :for="`type-${type}`" class="text-sm text-gray-800">{{ type }}</label>
            </div>
          </div>

          <div>
            <h3 class="font-semibold text-gray-700">File Size (MB)</h3>
            <div class="flex items-center gap-2">
              <input type="number" v-model.number="filters.minSize" min="0"
                class="w-full border px-2 py-1 rounded text-sm" placeholder="Min" />
              <span>-</span>
              <input type="number" v-model.number="filters.maxSize" min="0"
                class="w-full border px-2 py-1 rounded text-sm" placeholder="Max" />
            </div>
          </div>

          <button @click="clearFilters"
            class="w-auto xl:w-full text-sm px-3 py-2 bg-gray-100 hover:bg-gray-200 rounded text-gray-800 border border-gray-300">
            Clear Filters
          </button>
        </aside>

        <!-- Main Content -->
        <div class="flex-1">
          <!-- File Table -->
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Extension
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Size</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Creator
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Visibility
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="file in filteredFiles" :key="file.id" :title="file.name">
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-[200px]">{{ file.name }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ file.mediaType }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ file.format }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ readableSize(file.sizeBytes) }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 truncate max-w-[250px]"
                    :title="`${file.author.userProxy.firstName} ${file.author.userProxy.lastName} (${file.author.userProxy.username})`">
                    {{ file.author.userProxy.firstName }} {{ file.author.userProxy.lastName }}
                    <span class="text-gray-400">({{ file.author.userProxy.username }})</span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span :class="file.isPublic ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                      class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium">
                      {{ file.isPublic ? 'Public' : 'Private' }}
                    </span>
                  </td>


                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button @click="$emit('editFile', file)" class="text-blue-600 hover:text-blue-900 mr-3">
                      Manage
                    </button>
                    <button @click="$emit('removeFile', file)" class="text-red-600 hover:text-red-900">
                      Remove
                    </button>
                  </td>
                </tr>

                <tr v-if="!filteredFiles.length">
                  <td colspan="7" class="px-6 py-10 text-center text-gray-500">
                    No files found.
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
import { readableSize } from '@/lib/utils';
import type { Network, NetworkFile } from '@/types';
import { computed, ref, watch } from 'vue';

const props = defineProps<{
  network: Network;
}>();

const showFilters = ref<boolean>(false);
const filterSlug = ref<string>("");

const filters = ref({
  visibility: [] as string[],
  formats: [] as string[],
  minSize: 0,
  maxSize: 0,
})

const clearFilters = () => {
  filters.value = {
    visibility: [],
    formats: [],
    minSize: 0,
    maxSize: 0,
  }
  filterSlug.value = ""
}

// Dynamically extract all media types from files
const formats = computed(() => {
  const types = new Set<string>()
  props.network.files.forEach(file => {
    if (file.format) types.add(file.format.toLowerCase())
  })
  return Array.from(types).sort()
})

watch(() => props.network.files, (newFiles: NetworkFile[]) => {
  console.log('Files updated:', newFiles)
})

const filteredFiles = computed(() => {
  const query = filterSlug.value.trim().toLowerCase()

  return props.network.files.filter(file => {
    const name = file.name.toLowerCase();
    const author = `${file.author?.userProxy?.username ?? ''} ${file.author?.userProxy?.firstName ?? ''} ${file.author?.userProxy?.lastName ?? ''}`.toLowerCase();
    const format = file.format?.toLowerCase();
    const visibility = file.isPublic ? 'public' : 'private';
    const sizeMb = file.sizeBytes / 1024 / 1024;

    const matchesSearch = query === "" || name.includes(query) || author.includes(query) || format?.includes(query);
    const matchesMediaType = filters.value.formats.length === 0 || (format && filters.value.formats.includes(format));
    const matchesVisibility = filters.value.visibility.length === 0 || filters.value.visibility.includes(visibility);
    const matchesMin = !filters.value.minSize || sizeMb >= filters.value.minSize;
    const matchesMax = !filters.value.maxSize || sizeMb <= filters.value.maxSize;

    return matchesSearch && matchesMediaType && matchesVisibility && matchesMin && matchesMax;
  })
})

defineEmits<{
  (e: 'addFile'): void;
  (e: 'editFile', file: NetworkFile): void;
  (e: 'removeFile', file: NetworkFile): void;
  (e: 'toggleFileVisibility', file: NetworkFile): void;
  (e: 'previewFile', file: NetworkFile): void;
}>();
</script>
