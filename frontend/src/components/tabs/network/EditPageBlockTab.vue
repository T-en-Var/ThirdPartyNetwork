<template>
  <div class="space-y-6">
    <!-- Back button -->
    <div class="flex justify-start">
      <RouterLink :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/edit`"
        class="inline-flex items-center gap-2 px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md transition">
        ← Back to Custom Page
      </RouterLink>
    </div>

    <div class="bg-white shadow-md rounded-lg p-6">
      <LoadingErrorComponent :loading="loading" :error="error" button-value="Go back" @button-action="router.go(-1)" />

      <div v-if="!loading && !error && currentPageBlock" class="space-y-6">
        <h2 class="text-xl font-semibold text-gray-800 mb-4">Edit Page Block</h2>
        <form @submit.prevent="handleUpdate" class="space-y-4">
          <!-- Parent block selector -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Parent Block</label>
            <SearchableSelect v-model="form.parentPageId" :options="parentOptions" label="Parent Block"
              :nullable="true" />
          </div>

          <!-- Text -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Text</label>
            <input v-model="form.text" type="text"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
          </div>

          <!-- Position -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Position</label>
            <input v-model.number="form.position" type="number"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100" />
          </div>

          <!-- JSON Block Data Editor -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Block Data</label>

            <div v-if="!editDataMode">
              <pre class="bg-gray-50 border rounded-md p-2 text-xs overflow-x-auto">{{ formattedJson }}</pre>
              <button type="button" class="mt-2 px-3 py-1 bg-blue-600 text-white rounded-md text-sm hover:bg-blue-700"
                @click="editDataMode = true">
                Edit JSON
              </button>
            </div>

            <div v-else class="relative">
              <JsonEditorVue v-model="jsonEditorValue" class="h-64 border rounded-md" />
              <div class="flex justify-end mt-2 space-x-2">
                <button type="button" class="px-3 py-1 bg-gray-200 text-gray-700 rounded-md text-sm hover:bg-gray-300"
                  @click="cancelEditData">
                  Cancel
                </button>
                <button type="button" class="px-3 py-1 bg-green-600 text-white rounded-md text-sm hover:bg-green-700"
                  @click="saveJsonData">
                  Save JSON
                </button>
              </div>
            </div>
          </div>

          <div class="flex justify-end">
            <button type="submit" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
              Update Block
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Footer navigation -->
    <div class="bg-white shadow rounded-lg p-4">
      <h3 class="text-sm font-semibold text-gray-700 mb-3">Navigate Page Blocks</h3>

      <!-- Previous / Next -->
      <div class="flex justify-between items-center">
        <!-- Previous -->
        <RouterLink v-if="previousBlock"
          :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${previousBlock.id}/edit`"
          class="px-3 py-1 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md text-sm transition">
          ← Previous (#{{ previousBlock.position }})
        </RouterLink>
        <span v-else class="text-xs text-gray-400">No previous block</span>

        <!-- Next -->
        <RouterLink v-if="nextBlock"
          :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${nextBlock.id}/edit`"
          class="px-3 py-1 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md text-sm transition">
          Next (#{{ nextBlock.position }}) →
        </RouterLink>
        <span v-else class="text-xs text-gray-400">No next block</span>
      </div>

      <div v-if="parentBlock || childBlocks.length" class="flex gap-6 mt-6">
        <!-- Parent -->
        <div v-if="parentBlock">
          <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
            Parent Block
          </h4>
          <RouterLink :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${parentBlock.id}/edit`"
            class="py-3 px-5 bg-gray-50 hover:bg-gray-100 rounded-md border transition flex flex-col space-y-1">
            <span class="text-sm font-medium text-gray-800 truncate">↑ {{ parentBlock.text }}</span>
            <span class="text-xs text-gray-500">
              Position: {{ parentBlock.position }}
            </span>
          </RouterLink>
        </div>

        <!-- Children -->
        <div v-if="childBlocks.length">
          <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
            Child Blocks
          </h4>
          <div class="flex justify-center gap-3">
            <RouterLink v-for="child in childBlocks" :key="child.id"
              :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${child.id}/edit`"
              class="py-3 px-5 bg-gray-50 hover:bg-gray-100 rounded-md border transition flex flex-col space-y-1">
              <span class="text-sm font-medium text-gray-800 truncate">
                {{ child.text }}
              </span>
              <span class="text-xs text-gray-500">
                Position: {{ child.position }}
              </span>
            </RouterLink>
          </div>
        </div>
      </div>

      <div class="flex gap-6 mt-6">
        <!-- Tree display -->
        <div v-if="tree.length">
          <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
            Tree Display
          </h4>
          <div class="text-xs">
            <TreeNodeComponent v-for="root in tree" :key="root.id" :node="root" :current-id="pageBlockId"
              :network-id="networkId" :custom-page-id="customPageId" />
          </div>
        </div>

      </div>

      <!-- History -->
      <div v-if="historyStore.pageBlocks.length" class="mt-4">
        <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
          Recently Visited
        </h4>
        <div class="flex flex-wrap gap-2">
          <RouterLink v-for="pb in [...historyStore.pageBlocks].reverse()" :key="pb.id"
            :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${pb.id}/edit`"
            class="px-2 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 rounded text-xs transition">
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
import type { PageBlock, Network, TreeNode } from '@/types';
import { ref, computed, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import JsonEditorVue from 'json-editor-vue';
import useCustomPage from '@/composables/useCustomPage';
import { useHistoryStore } from '@/stores/history';
import TreeNodeComponent from '@/components/TreeNodeComponent.vue';
import SearchableSelect from '@/components/SearchableSelect.vue';

defineProps<{ network: Network; }>();

const emit = defineEmits<{
  (e: 'updatePageBlock', customPageId: string, block: PageBlock): void,
}>();

const router = useRouter();
const route = useRoute();
const historyStore = useHistoryStore();

const networkId = route.params.networkId as string;
const customPageId = route.params.customPageId as string;
const pageBlockId = computed(() => route.params.pageBlockId as string);

const form = ref<Partial<PageBlock>>({
  parentPageId: '',
  text: '',
  position: 0,
  data: {},
});

const editDataMode = ref(false);
const jsonEditorValue = ref<object>({});

const { customPage, loading, error, fetchCustomPage } = useCustomPage();

const handleMounted = async () => {
  await fetchCustomPage(networkId, customPageId);

  if (!currentPageBlock.value) throw new Error("PageBlock not found");

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

  historyStore.pageBlockVisit(currentPageBlock.value);
};

watch(
  () => pageBlockId.value,
  async (newId) => {
    if (newId) {
      await handleMounted();
    }
  },
  { immediate: true }  // basically onMounted
);

const currentPageBlock = computed(() => customPage.value?.pages.find(p => p.id == pageBlockId.value));
const parentOptions: Ref<PageBlock[]> = computed(() => customPage.value?.pages.filter(p => p.id != pageBlockId.value) ?? []);
const childBlocks = computed(() => customPage.value?.pages.filter(p => p.parentPageId === pageBlockId.value) ?? []);

const sortedBlocks = computed(() =>
  customPage.value?.pages
    ? [...customPage.value.pages.filter(p => p.parentPageId == currentPageBlock.value?.parentPageId)].sort((a, b) => a.position - b.position)
    : []
);

const currentIndex = computed(() => sortedBlocks.value.findIndex(b => b.id === pageBlockId.value));

const previousBlock = computed(() =>
  currentIndex.value > 0
    ? sortedBlocks.value[currentIndex.value - 1]
    : null
);
const nextBlock = computed(() =>
  currentIndex.value >= 0 && currentIndex.value < sortedBlocks.value.length - 1
    ? sortedBlocks.value[currentIndex.value + 1]
    : null
);

const parentBlock = computed(() =>
  currentPageBlock.value?.parentPageId
    ? customPage.value?.pages.find(p => p.id === currentPageBlock.value?.parentPageId)
    : null
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
  blocks.forEach(b => map.set(b.id, { ...b, children: [], visited: false, recursive: false }));

  function addChildren(node: TreeNode) {
    if (node.visited) return;
    node.visited = true;

    // path.add(node.id)

    blocks.forEach(b => {
      if (b.parentPageId !== node.id) return;

      const childNode = map.get(b.id)!;
      if (childNode.visited) node.children.push({ ...childNode, recursive: true });
      else if (!childNode.recursive) node.children.push(childNode);

      addChildren(childNode);
    });
  }

  // Build tree
  blocks.forEach(b => {
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
