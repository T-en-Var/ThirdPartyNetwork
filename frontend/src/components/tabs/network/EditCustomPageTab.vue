<template>
  <div class="bg-white shadow-md rounded-lg overflow-hidden p-6">
    <LoadingErrorComponent :loading="loading" :error="error" button-value="Go back" @button-action="router.go(-1)" />

    <div v-if="!loading && !error && customPage" class="space-y-6">
      <!-- Top segment: Edit custom page -->
      <h2 class="text-xl font-semibold text-gray-800 mb-4">Edit Custom Page</h2>
      <form @submit.prevent="handleUpdate" class="space-y-4 border-b border-gray-200 pb-4">
        <div class="flex gap-4 w-full">
          <div class="w-full">
            <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input v-model="form.name" type="text"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all" />
          </div>
          <div class="w-full">
            <label class="block text-sm font-medium text-gray-700 mb-1">Slug</label>
            <input v-model="form.slug" type="text"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all" />
          </div>
        </div>
        <AccessLevelPicker v-model="form.accessLevel" />
        <div class="flex justify-end">
          <button type="submit" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
            Update
          </button>
        </div>
      </form>

      <!-- Bottom segment: Page blocks -->
      <div class="p-4">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-lg font-semibold text-gray-800">Page Blocks</h2>
          <div class="space-x-2">
            <button @click="$emit('createPageBlock', customPage)"
              class="mr-6 px-3 py-1 bg-blue-600 hover:bg-blue-500 transition-colors text-white rounded-md text-sm">
              Add page block
            </button>
            <button @click="viewMode = 'flat'" :class="viewMode === 'flat' ? activeTabClasses : inactiveTabClasses">
              Flat View
            </button>
            <button @click="viewMode = 'grouped'"
              :class="viewMode === 'grouped' ? activeTabClasses : inactiveTabClasses">
              Grouped View
            </button>
          </div>
        </div>

        <!-- Flat view -->
        <div v-if="viewMode === 'flat'" class="space-y-4">
          <div v-for="block in customPage!.pages || []" :key="block.id" class="bg-gray-50 rounded-md border p-4">
            <div class="flex justify-between items-center mb-2">
              <h3 class="text-sm font-medium text-gray-800">{{ block.text }}</h3>
              <button @click="$emit('editPageBlock', customPage, block)"
                class="text-blue-600 hover:text-blue-800 text-sm">
                Open
              </button>
            </div>
            <div class="text-xs text-gray-500 mb-2">
              Position: {{ block.position }} | Parent: {{ getParentText(block.parentPageId) }}
            </div>
            <pre class="bg-white border rounded-md p-2 text-xs overflow-x-auto">{{ formatJson(block.data) }}</pre>
          </div>
        </div>

        <!-- Grouped view -->
        <div v-else class="space-y-6">
          <div v-for="(blocks, parentId) in groupedBlocks" :key="parentId" class="space-y-2">
            <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
              Parent: <span class="text-gray-800 font-bold">{{ getParentText(parentId) }}</span>
            </h4>
            <div v-for="block in blocks" :key="block.id" class="bg-gray-50 rounded-md border p-4">
              <div class="flex justify-between items-center mb-2">
                <h3 class="text-sm font-medium text-gray-800">{{ block.text }}</h3>
                <button @click="$emit('editPageBlock', customPage, block)"
                  class="text-blue-600 hover:text-blue-800 text-sm">
                  Open
                </button>
              </div>
              <div class="text-xs text-gray-500 mb-2">
                Position: {{ block.position }}
              </div>
              <pre class="bg-white border rounded-md p-2 text-xs overflow-x-auto">{{ formatJson(block.data) }}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import useCustomPage from '@/composables/useCustomPage';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import type { CreateCustomPage, CustomPage, Network, PageBlock } from '@/types';
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useHistoryStore } from '@/stores/history';

defineProps<{
  network: Network;
}>();

const emit = defineEmits<{
  (e: 'updateCustomPage', id: string, page: CreateCustomPage): void,
  (e: 'editPageBlock', page: CustomPage, block: PageBlock): void,
  (e: 'createPageBlock', page: CreateCustomPage): void,
}>()

const router = useRouter()
const route = useRoute()
const historyStore = useHistoryStore();
const { customPage, loading, error, fetchCustomPage } = useCustomPage();

const customPageId = route.params.customPageId as string;
const networkId = route.params.networkId as string;

const form = ref<CreateCustomPage>({ name: '', slug: '', accessLevel: 0 })
const viewMode = ref<'flat' | 'grouped'>('flat')

const activeTabClasses = 'px-3 py-1 bg-blue-600 text-white rounded-md text-sm cursor-default'
const inactiveTabClasses = 'px-3 py-1 bg-gray-100 text-gray-700 rounded-md text-sm hover:bg-gray-200'

onMounted(async () => {
  await fetchCustomPage(networkId, customPageId);

  if (!customPage.value) throw new Error("CustomPage not found");
  form.value.name = customPage.value.name;
  form.value.slug = customPage.value.slug;
  form.value.accessLevel = customPage.value.accessLevel ?? 0;

  historyStore.customPageVisit(customPage.value)
})

const groupedBlocks = computed(() => {
  if (!customPage.value?.pages) return {}
  return customPage.value.pages.reduce((acc: Record<string, PageBlock[]>, block: PageBlock) => {
    const parentId = block.parentPageId || 'none'
    if (!acc[parentId]) acc[parentId] = []
    acc[parentId].push(block)
    return acc
  }, {})
})

function handleUpdate() {
  if (!customPage.value) return;
  emit('updateCustomPage', customPage.value.id, { ...form.value })
}

function formatJson(data: object) {
  try {
    return JSON.stringify(data, null, 2)
  } catch {
    return String(data)
  }
}

function getParentText(parentId?: string) {
  if (!parentId) return 'None';
  const parentBlock = customPage.value?.pages?.find(block => block.id === parentId);
  return parentBlock?.text || parentId;
}
</script>
