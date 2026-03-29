<template>
  <div class="space-y-6">
    <!-- Back button -->
    <div class="flex justify-start">
      <RouterLink
        :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/edit`"
        class="inline-flex items-center gap-2 rounded-md bg-gray-100 px-3 py-1.5 text-sm text-gray-700 transition hover:bg-gray-200"
      >
        ← Back to Custom Page
      </RouterLink>
    </div>

    <div class="rounded-lg bg-white p-6 shadow-md">
      <LoadingErrorComponent
        :loading="loading"
        :error="error ?? undefined"
        button-value="Go back"
        @button-action="router.go(-1)"
        :has-value="!!currentPageBlock"
      />

      <div
        v-if="!loading && !error && currentPageBlock"
        class="space-y-6"
      >
        <h2 class="mb-4 text-xl font-semibold text-gray-800">Edit Page Block</h2>
        <form
          @submit.prevent="handleUpdate"
          class="space-y-4"
        >
          <!-- Parent block selector -->
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Parent Block</label>
            <SearchableSelect
              v-model="form.parentPageId"
              :options="parentOptions"
              label="Parent Block"
              :nullable="true"
            />
          </div>

          <!-- Text -->
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Text</label>
            <input
              v-model="form.text"
              type="text"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
            />
          </div>

          <!-- Position -->
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Position</label>
            <input
              v-model.number="form.position"
              type="number"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
            />
          </div>

          <!-- JSON Block Data Editor -->
          <div>
            <label class="mb-2 block text-sm font-medium text-gray-700">Block Data</label>

            <div v-if="!editDataMode">
              <pre class="overflow-x-auto rounded-md border bg-gray-50 p-2 text-xs">{{
                formattedJson
              }}</pre>
              <button
                type="button"
                class="mt-2 rounded-md bg-blue-600 px-3 py-1 text-sm text-white hover:bg-blue-700"
                @click="editDataMode = true"
              >
                Edit JSON
              </button>
            </div>

            <div
              v-else
              class="relative"
            >
              <JsonEditorVue
                v-model="jsonEditorValue"
                :stringified="false"
                class="h-64 rounded-md border"
              />
              <div class="mt-2 flex justify-end space-x-2">
                <button
                  type="button"
                  class="rounded-md bg-gray-200 px-3 py-1 text-sm text-gray-700 hover:bg-gray-300"
                  @click="cancelEditData"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  class="rounded-md bg-green-600 px-3 py-1 text-sm text-white hover:bg-green-700"
                  @click="saveJsonData"
                >
                  Save JSON
                </button>
              </div>
            </div>
          </div>

          <div class="flex justify-end">
            <button
              type="submit"
              class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
            >
              Update Block
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Footer navigation -->
    <div class="rounded-lg bg-white p-4 shadow">
      <h3 class="mb-3 text-sm font-semibold text-gray-700">Navigate Page Blocks</h3>

      <!-- Previous / Next -->
      <div class="flex items-center justify-between">
        <!-- Previous -->
        <RouterLink
          v-if="previousBlock"
          :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${previousBlock.id}/edit`"
          class="rounded-md bg-gray-100 px-3 py-1 text-sm text-gray-700 transition hover:bg-gray-200"
        >
          ← Previous (#{{ previousBlock.position }})
        </RouterLink>
        <span
          v-else
          class="text-xs text-gray-400"
          >No previous block</span
        >

        <!-- Next -->
        <RouterLink
          v-if="nextBlock"
          :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${nextBlock.id}/edit`"
          class="rounded-md bg-gray-100 px-3 py-1 text-sm text-gray-700 transition hover:bg-gray-200"
        >
          Next (#{{ nextBlock.position }}) →
        </RouterLink>
        <span
          v-else
          class="text-xs text-gray-400"
          >No next block</span
        >
      </div>

      <div
        v-if="parentBlock || childBlocks.length"
        class="mt-6 flex gap-6"
      >
        <!-- Parent -->
        <div v-if="parentBlock">
          <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
            Parent Block
          </h4>
          <RouterLink
            :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${parentBlock.id}/edit`"
            class="flex flex-col space-y-1 rounded-md border bg-gray-50 px-5 py-3 transition hover:bg-gray-100"
          >
            <span class="truncate text-sm font-medium text-gray-800">↑ {{ parentBlock.text }}</span>
            <span class="text-xs text-gray-500"> Position: {{ parentBlock.position }} </span>
          </RouterLink>
        </div>

        <!-- Children -->
        <div v-if="childBlocks.length">
          <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
            Child Blocks
          </h4>
          <div class="flex justify-center gap-3">
            <RouterLink
              v-for="child in childBlocks"
              :key="child.id"
              :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${child.id}/edit`"
              class="flex flex-col space-y-1 rounded-md border bg-gray-50 px-5 py-3 transition hover:bg-gray-100"
            >
              <span class="truncate text-sm font-medium text-gray-800">
                {{ child.text }}
              </span>
              <span class="text-xs text-gray-500"> Position: {{ child.position }} </span>
            </RouterLink>
          </div>
        </div>
      </div>

      <div class="mt-6 flex gap-6">
        <!-- Tree display -->
        <div v-if="tree.length">
          <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
            Tree Display
          </h4>
          <div class="text-xs">
            <TreeNodeComponent
              v-for="root in tree"
              :key="root.id"
              :node="root"
              :current-id="pageBlockId"
              :network-id="networkId"
              :custom-page-id="customPageId"
            />
          </div>
        </div>
      </div>

      <!-- History -->
      <div
        v-if="historyStore.data.pageBlocks.length"
        class="mt-4"
      >
        <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
          Recently Visited
        </h4>
        <div class="flex flex-wrap gap-2">
          <RouterLink
            v-for="pb in [...historyStore.data.pageBlocks].reverse()"
            :key="pb.id"
            :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${pb.id}/edit`"
            class="rounded bg-blue-50 px-2 py-1 text-xs text-blue-700 transition hover:bg-blue-100"
          >
            {{ pb.text }}
          </RouterLink>
        </div>
      </div>
    </div>

    <div class="h-96"></div>
  </div>
</template>

<script setup lang="ts">
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import type { PageBlock, TreeNode } from '@/types';
import { ref, computed, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import JsonEditorVue from 'json-editor-vue';
import { useHistoryStore } from '@/stores/history';
import TreeNodeComponent from '@/components/TreeNodeComponent.vue';
import SearchableSelect from '@/components/SearchableSelect.vue';
import useCustomPages from '@/composables/network/useCustomPages';

const emit = defineEmits<{
  (e: 'updatePageBlock', customPageId: string, block: PageBlock): void;
}>();

const router = useRouter();
const route = useRoute();

const networkId = route.params.networkId as string;
const customPageId = route.params.customPageId as string;
const pageBlockId = computed(() => route.params.pageBlockId as string);

const historyStore = useHistoryStore(networkId);

const form = ref<Partial<PageBlock>>({
  parentPageId: '',
  text: '',
  position: 0,
  data: {},
});

const editDataMode = ref(false);
const jsonEditorValue = ref<object>({});

const {
  data: customPage,
  loading,
  error,
  execute: fetchCustomPage,
} = useCustomPages().fetchCustomPage;

const handleMounted = async () => {
  await fetchCustomPage(networkId, customPageId);

  if (!currentPageBlock.value) throw new Error('PageBlock not found');

  form.value = {
    parentPageId: currentPageBlock.value.parentPageId || '',
    text: currentPageBlock.value.text,
    position: currentPageBlock.value.position,
    data: currentPageBlock.value.data,
  };

  try {
    jsonEditorValue.value = currentPageBlock.value.data;
  } catch {
    jsonEditorValue.value = {};
  }

  historyStore.visit.pageBlocks(currentPageBlock.value);
};

watch(
  () => pageBlockId.value,
  async (newId) => {
    if (newId) {
      await handleMounted();
    }
  },
  { immediate: true }, // basically onMounted
);

const currentPageBlock = computed(() =>
  customPage.value?.pages.find((p) => p.id == pageBlockId.value),
);
const parentOptions: Ref<PageBlock[]> = computed(
  () => customPage.value?.pages.filter((p) => p.id != pageBlockId.value) ?? [],
);
const childBlocks = computed(
  () => customPage.value?.pages.filter((p) => p.parentPageId === pageBlockId.value) ?? [],
);

const sortedBlocks = computed(() =>
  customPage.value?.pages
    ? [
        ...customPage.value.pages.filter(
          (p) => p.parentPageId == currentPageBlock.value?.parentPageId,
        ),
      ].sort((a, b) => a.position - b.position)
    : [],
);

const currentIndex = computed(() =>
  sortedBlocks.value.findIndex((b) => b.id === pageBlockId.value),
);

const previousBlock = computed(() =>
  currentIndex.value > 0 ? sortedBlocks.value[currentIndex.value - 1] : null,
);
const nextBlock = computed(() =>
  currentIndex.value >= 0 && currentIndex.value < sortedBlocks.value.length - 1
    ? sortedBlocks.value[currentIndex.value + 1]
    : null,
);

const parentBlock = computed(() =>
  currentPageBlock.value?.parentPageId
    ? customPage.value?.pages.find((p) => p.id === currentPageBlock.value?.parentPageId)
    : null,
);

const formattedJson = computed(() => {
  return form.value.data;
});

function cancelEditData() {
  editDataMode.value = false;
}

function saveJsonData() {
  try {
    // form.value.data = JSON.stringify(jsonEditorValue.value, null, 2)
    form.value.data = jsonEditorValue.value;
    editDataMode.value = false;
  } catch (_) {
    void _;
    alert('Invalid JSON');
  }
}

function handleUpdate() {
  if (!currentPageBlock.value) return;
  emit('updatePageBlock', customPageId, {
    ...currentPageBlock.value,
    ...form.value,
    parentPageId: form.value.parentPageId || null,
  } as PageBlock);
}

function buildTree(blocks: PageBlock[]): TreeNode[] {
  const map = new Map<string, TreeNode>();
  const roots: TreeNode[] = [];

  // Initialize nodes
  blocks.forEach((b) => map.set(b.id, { ...b, children: [], visited: false, recursive: false }));

  function addChildren(node: TreeNode) {
    if (node.visited) return;
    node.visited = true;

    // path.add(node.id)

    blocks.forEach((b) => {
      if (b.parentPageId !== node.id) return;

      const childNode = map.get(b.id)!;
      if (childNode.visited) node.children.push({ ...childNode, recursive: true });
      else if (!childNode.recursive) node.children.push(childNode);

      addChildren(childNode);
    });
  }

  // Build tree
  blocks.forEach((b) => {
    const node = map.get(b.id)!;
    if (!node.visited) {
      addChildren(node);
      roots.push(node);
    }
  });

  return roots;
}

const tree = computed(() => buildTree(customPage.value?.pages ?? []));
</script>
