<template>
  <div class="space-y-6">
    <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
      <LoadingErrorComponent
        :loading="loading"
        :error="error"
        :has-value="!!customPage"
      />

      <div v-if="customPage">
        <h2 class="mb-4 text-xl font-semibold text-gray-800">Edit Custom Page</h2>
        <UserContentForm
          button-text="Update"
          :is-submitting="isSubmitting"
          :input-is-valid="inputIsValid"
          :network-id="networkId"
          @submit="handleUpdate"
          @close="handleReturn"
        >
          <div class="flex w-full gap-4">
            <div class="w-full">
              <label class="mb-1 block text-sm font-medium text-gray-700">Name</label>
              <input
                v-model="form.name"
                type="text"
                class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
              />
            </div>
            <div class="w-full">
              <label class="mb-1 block text-sm font-medium text-gray-700">Slug</label>
              <input
                v-model="form.slug"
                type="text"
                class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
              />
            </div>
          </div>
        </UserContentForm>
      </div>
    </div>

    <div class="overflow-hidden rounded-lg bg-white p-6 shadow-md">
      <div v-if="customPage">
        <div class="mb-4 flex items-center justify-between">
          <div class="flex gap-2 space-x-2">
            <h2 class="text-lg font-semibold text-gray-800">Page Blocks</h2>
            <button
              @click="viewMode = 'flat'"
              :class="viewMode === 'flat' ? activeTabClasses : inactiveTabClasses"
            >
              Flat View
            </button>
            <button
              @click="viewMode = 'grouped'"
              :class="viewMode === 'grouped' ? activeTabClasses : inactiveTabClasses"
            >
              Grouped View
            </button>
          </div>

          <div>
            <button
              @click="showCreatePageBlockModal = true"
              class="rounded-md bg-blue-600 px-3 py-1 text-sm text-white transition-colors hover:bg-blue-500"
            >
              Add page block
            </button>
          </div>
        </div>

        <div
          v-if="viewMode === 'flat'"
          class="space-y-4"
        >
          <div
            v-for="block in customPage!.pages || []"
            :key="block.id"
            class="rounded-md border bg-gray-50 p-4"
          >
            <div class="mb-2 flex items-center justify-between">
              <h3 class="text-sm font-medium text-gray-800">{{ block.text }}</h3>
              <div class="flex gap-2">
                <button
                  @click="handleEditPageBlock(block)"
                  class="text-sm text-blue-600 hover:text-blue-800"
                >
                  Edit
                </button>
                <button
                  @click="handleDeletePageBlock(block)"
                  class="text-sm text-red-600 hover:text-red-800"
                >
                  Delete
                </button>
              </div>
            </div>
            <div class="mb-2 text-xs text-gray-500">
              Position: {{ block.position }} | Parent: {{ getParentText(block.parentPageId) }}
            </div>
            <pre class="overflow-x-auto rounded-md border bg-white p-2 text-xs">{{
              formatJson(block.data)
            }}</pre>
          </div>
        </div>

        <div
          v-else
          class="space-y-6"
        >
          <div
            v-for="(blocks, parentId) in groupedBlocks"
            :key="parentId"
            class="space-y-2"
          >
            <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Parent: <span class="font-bold text-gray-800">{{ getParentText(parentId) }}</span>
            </h4>
            <div
              v-for="block in blocks"
              :key="block.id"
              class="rounded-md border bg-gray-50 p-4"
            >
              <div class="mb-2 flex items-center justify-between">
                <h3 class="text-sm font-medium text-gray-800">{{ block.text }}</h3>
                <button
                  @click="handleEditPageBlock(block)"
                  class="text-sm text-blue-600 hover:text-blue-800"
                >
                  Open
                </button>
              </div>
              <div class="mb-2 text-xs text-gray-500">Position: {{ block.position }}</div>
              <pre class="overflow-x-auto rounded-md border bg-white p-2 text-xs">{{
                formatJson(block.data)
              }}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>

    <AddPageBlockModal
      v-if="showCreatePageBlockModal && customPage"
      :custom-page="customPage"
      :is-submitting="isSubmitting"
      @submit="handleCreatePageBlock"
      @close="showCreatePageBlockModal = false"
    />
  </div>
</template>

<script setup lang="ts">
import type {
  CreateCustomPage,
  CreatePageBlock,
  CreateUserContentForm,
  CustomPage,
  PageBlock,
} from '@/types';
import { ref, computed, watch, type Ref } from 'vue';
import { useRoute } from 'vue-router';
import UserContentForm from '@/components/UserContentForm.vue';
import { useEventStore } from '@/stores/event';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AddPageBlockModal from '@/components/modals/usercontent/AddPageBlockModal.vue';

const route = useRoute();
const events = useEventStore();

const emit = defineEmits<{
  (e: 'custom-pages-update', networkId: string, customPageId: string, page: CreateCustomPage): void;
  (e: 'page-blocks-edit', pageBlock: PageBlock): void;
  (
    e: 'page-blocks-create',
    networkId: string,
    customPageId: string,
    payload: CreatePageBlock,
  ): void;
  (e: 'page-blocks-update', networkId: string, customPageId: string, block: PageBlock): void;
  (e: 'page-blocks-delete', pageBlock: PageBlock): void;
  (e: 'return', section: string): void;
}>();

const props = defineProps<{
  fetchCustomPage: (customPageId: string) => Promise<Ref<CustomPage | null>>;
}>();

const customPageId = computed(() => route.params.customPageId as string);
const networkId = computed(() => route.params.networkId as string);

const isSubmitting = ref(false);
const loading = ref(false);
const error = ref<string | null>();

const showCreatePageBlockModal = ref(false);

const inputIsValid = computed(() => true);
const customPage = ref<CustomPage | null>(null);

const form = ref({ name: '', slug: '' });
const viewMode = ref<'flat' | 'grouped'>('flat');

const activeTabClasses = 'px-3 py-1 bg-blue-600 text-white rounded-md text-sm cursor-default';
const inactiveTabClasses =
  'px-3 py-1 bg-gray-100 text-gray-700 rounded-md text-sm hover:bg-gray-200';

watch(
  customPageId,
  async (newId) => {
    customPage.value = null;
    loading.value = true;

    const data = await props.fetchCustomPage(newId);
    loading.value = false;

    if (!data.value) throw new Error('CustomPage not found');
    watch(data, (newEntry) => (customPage.value = newEntry), { immediate: true });
    form.value.name = customPage.value!.name;
    form.value.slug = customPage.value!.slug;
  },
  { immediate: true },
);

const groupedBlocks = computed(() => {
  if (!customPage.value?.pages) return {};
  return customPage.value.pages.reduce((acc: Record<string, PageBlock[]>, block: PageBlock) => {
    const parentId = block.parentPageId || 'none';
    if (!acc[parentId]) acc[parentId] = [];
    acc[parentId].push(block);
    return acc;
  }, {});
});

function handleUpdate(userContentform: CreateUserContentForm) {
  if (!customPage.value) return;

  events.listen.customPages.update(() => {
    isSubmitting.value = false;
  }, true);

  isSubmitting.value = true;
  emit('custom-pages-update', userContentform.networkId, customPage.value.id, {
    ...form.value,
    accessLevel: userContentform.accessLevel,
  });
}

function handleReturn() {
  emit('return', 'custom-pages');
}

function formatJson(data: object) {
  try {
    return JSON.stringify(data, null, 2);
  } catch {
    return String(data);
  }
}

function handleCreatePageBlock(
  networkId: string,
  customPageId: string,
  pageBlock: CreatePageBlock,
) {
  emit('page-blocks-create', networkId, customPageId, pageBlock);
}

function handleEditPageBlock(pageBlock: PageBlock) {
  emit('page-blocks-edit', pageBlock);
}

function handleDeletePageBlock(pageBlock: PageBlock) {
  emit('page-blocks-delete', pageBlock);
}

function getParentText(parentId?: string) {
  if (!parentId) return 'None';
  const parentBlock = customPage.value?.pages?.find((block) => block.id === parentId);
  return parentBlock?.text || parentId;
}
</script>
