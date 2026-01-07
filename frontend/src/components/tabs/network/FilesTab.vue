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
          class="flex flex-row flex-wrap xl:flex-col items-start gap-4 w-full xl:w-64 xl:min-w-64 bg-white border rounded-lg p-4 shadow-sm">
          
          <div>
            <h3 class="font-semibold text-gray-700">Access Level</h3>
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

          <div>
            <h3 class="font-semibold text-gray-700">Visibility</h3>
            <div v-for="level in ACCESS_LEVELS" :key="level.value" class="flex items-center gap-2">
              <input type="checkbox" :id="`access-${level.value}`" v-model="filters.accessLevels" :value="level.value" class="accent-blue-600" />
              <label :for="`access-${level.value}`" class="text-sm text-gray-800">{{ level.label }}</label>
            </div>
          </div>

          <button @click="clearFilters"
            class="w-auto xl:w-full text-sm px-3 py-2 bg-gray-100 hover:bg-gray-200 rounded text-gray-800 border border-gray-300">
            Clear Filters
          </button>
        </aside>

        <!-- Main Content -->
        <div class="flex-1">
          <!-- Image Grid (only when filtering for images) -->
          <div v-if="isImageGrid" class="flex flex-wrap gap-4">
            <div v-for="file in filteredFiles" :key="file.id" @click="$emit('editFile', file)"
              class="relative group border rounded-lg overflow-hidden shadow-sm cursor-pointer">
              <img :src="file.url" class="w-full h-40 object-cover" />
              <div
                class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition flex flex-col justify-end text-white p-2 text-xs">
                <div class="truncate">{{ file.name }}</div>
                <div>{{ readableSize(file.sizeBytes) }}</div>
                <div>{{ file.author.userProxy.username }}</div>
              </div>
            </div>
            <p v-if="!filteredFiles.length" class="col-span-full text-center text-gray-500">No images found.</p>
          </div>

          <!-- File Table -->
          <div v-else class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Extension
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Size</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Visibility
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Author
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <template v-for="file in filteredFiles" :key="file.id">
                  <!-- Main file row -->
                  <tr :title="file.name">
                    <td
                      class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-[200px] flex items-center gap-2">
                      <img v-if="file.mediaType === 'image'" :src="file.url" class="w-8 h-8 object-cover rounded" />

                      <div v-else-if="isVideo(file)" class="relative w-8 h-8">
                        <img :src="videoThumbnail(file.url)" class="w-full h-full object-cover rounded" />
                        <button @click="togglePlay(file)"
                          class="absolute inset-0 flex items-center justify-center bg-black/40 text-white rounded">
                          <svg v-if="playingFile?.id === file.id" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
                            width="18" height="18" class="text-blue-600 shadow-md">
                            <rect x="6" y="6" width="12" height="12" rx="1" fill="currentColor" />
                          </svg>
                          <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18"
                            class="text-blue-600 shadow-md">
                            <path d="M5 3v18l15-9L5 3z" fill="currentColor" />
                          </svg>
                        </button>
                      </div>

                      <button v-else-if="isAudio(file)" @click="togglePlay(file)"
                        :aria-pressed="playingFile?.id === file.id"
                        :title="playingFile?.id === file.id ? 'Stop' : 'Play'"
                        class="inline-flex items-center justify-center w-8 h-8 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 hover:bg-blue-50">
                        <span class="sr-only">{{ playingFile?.id === file.id ? 'Stop audio' : 'Play audio' }}</span>
                        <svg v-if="playingFile?.id === file.id" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
                          width="18" height="18" class="text-blue-600">
                          <rect x="6" y="6" width="12" height="12" rx="1" fill="currentColor" />
                        </svg>
                        <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18"
                          class="text-blue-600">
                          <path d="M5 3v18l15-9L5 3z" fill="currentColor" />
                        </svg>
                      </button>
                      <span>{{ file.name }}</span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ file.mediaType }}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ file.format }}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ readableSize(file.sizeBytes) }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 truncate max-w-[250px]"
                      :title="`${file.author.userProxy.firstName} ${file.author.userProxy.lastName} (${file.author.userProxy.username})`">
                      {{ file.author.userProxy.firstName }} {{ file.author.userProxy.lastName }}
                      <span class="text-gray-400">({{ file.author.userProxy.username }})</span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ getAccessLevel(file.accessLevel).label }}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div class="flex items-center justify-end gap-3 h-full">
                        <button @click="$emit('editFile', file)" class="text-blue-600 hover:text-blue-900 mr-3">
                          Manage
                        </button>
                        <button @click="$emit('removeFile', file)" class="text-red-600 hover:text-red-900">
                          Remove
                        </button>
                      </div>
                    </td>
                  </tr>

                  <!-- Expansion row -->
                  <tr v-if="playingFile?.id === file.id">
                    <td colspan="7" class="px-6 pb-4">
                      <div v-if="isAudio(file)">
                        <audio controls autoplay class="mt-2 w-full max-w-md">
                          <source :src="file.url" :type="`audio/${file.format}`" />
                        </audio>
                      </div>
                      <div v-else-if="isVideo(file)">
                        <video v-if="playingFile?.id === file.id" controls autoplay class="mt-2 w-64 rounded shadow">
                          <source :src="file.url" :type="`video/${file.format}`" />
                        </video>
                      </div>
                    </td>
                  </tr>
                </template>


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
import useFiles from '@/composables/useFiles';
import { readableSize } from '@/lib/utils';
import type { NetworkFile } from '@/types';
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { ACCESS_LEVELS, getAccessLevel } from '@/lib/accessLevels';

const showFilters = ref(false);
const filterSlug = ref("");

const route = useRoute();
const filesState = useFiles();

onMounted(async () => {
  const networkId = route.params.networkId as string;
  filesState.fetchNetworkFiles(networkId);
});

const filters = ref({
  visibility: [] as string[],
  formats: [] as string[],
  minSize: 0,
  maxSize: 0,
  accessLevels: [] as number[],
});

const clearFilters = () => {
  filters.value = { visibility: [], formats: [], minSize: 0, maxSize: 0, accessLevels: [] };
  filterSlug.value = "";
};

// --- Media helpers ---
function isAudio(file: NetworkFile) {
  return file.mediaType === 'video' && ['mp3', 'wav', 'ogg'].includes(file.format);
}
function isVideo(file: NetworkFile) {
  return file.mediaType === 'video' && !isAudio(file);
}
function videoThumbnail(url: string) {
  // Cloudinary trick: swap to jpg preview
  return url.replace('/upload/', '/upload/w_200,h_120,c_fill/').replace(/\.[^.]+$/, '.jpg');
}

const playingFile = ref<NetworkFile | null>(null);
function togglePlay(file: NetworkFile) {
  if (playingFile.value?.id === file.id) {
    playingFile.value = null;
  } else {
    playingFile.value = file;
  }
}

// Extract formats
const formats = computed(() => {
  const types = new Set<string>();
  filesState.files.value.forEach(f => { if (f.format) types.add(f.format.toLowerCase()); });
  return Array.from(types).sort();
});

// Filtered files
const filteredFiles = computed(() => {
  const query = filterSlug.value.trim().toLowerCase();
  return filesState.files.value.filter(file => {
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
  });
});

// Switch to grid when only images are shown
const isImageGrid = computed(() => {
  if (!filteredFiles.value.length) return false;
  return filteredFiles.value.every(f => f.mediaType === 'image');
});

defineEmits<{
  (e: 'addFile'): void;
  (e: 'editFile', file: NetworkFile): void;
  (e: 'removeFile', file: NetworkFile): void;
  (e: 'toggleFileVisibility', file: NetworkFile): void;
  (e: 'previewFile', file: NetworkFile): void;
}>();
</script>
