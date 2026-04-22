<template>
  <div class="space-y-6">
    <!-- Back button -->
    <div class="flex justify-start">
      <button
        @click="handleReturn"
        class="inline-flex items-center gap-2 rounded-md bg-gray-100 px-3 py-1.5 text-sm text-gray-700 transition hover:bg-gray-200"
      >
        ← Back to Custom Page
      </button>
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
        <div class="space-y-4">
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Parent Block</label>
            <SearchableSelect
              v-model="form.parentPageId"
              :options="parentOptions"
              label="Parent Block"
              :nullable="true"
            />
          </div>

          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Text</label>
            <input
              v-model="form.text"
              type="text"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
            />
          </div>

          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Position</label>
            <input
              v-model.number="form.position"
              type="number"
              class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
            />
          </div>

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

          <div class="flex justify-end gap-4">
            <button
              @click="handleDelete"
              class="rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700"
            >
              Delete Block
            </button>
            <button
              @click="handleUpdate"
              class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
            >
              Update Block
            </button>
          </div>
        </div>
      </div>
    </div>

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
              @navigate-to-block="handleEdit"
            />
          </div>
        </div>
      </div>

      <div
        v-if="history.data.pageBlocks.length"
        class="mt-4"
      >
        <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
          Recently Visited
        </h4>
        <div class="flex flex-wrap gap-2">
          <RouterLink
            v-for="pb in [...history.data.pageBlocks].reverse()"
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
import type { CustomPage, PageBlock, TreeNode } from '@/types';
import { ref, computed, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import JsonEditorVue from 'json-editor-vue';
import TreeNodeComponent from '@/components/TreeNodeComponent.vue';
import SearchableSelect from '@/components/SearchableSelect.vue';
import type { useHistoryStore } from '@/stores/history';
import { useGlobalStore } from '@/stores/global';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();

const emit = defineEmits<{
  (e: 'page-blocks-edit', block: PageBlock): void;
  (e: 'page-blocks-update', networkId: string, customPageId: string, block: PageBlock): void;
  (e: 'page-blocks-delete', block: PageBlock): void;
  (e: 'nav-to-custom-pages', networkId: string, customPageId: string): void;
}>();

const props = defineProps<{
  fetchCustomPage: (customPageId: string) => Promise<Ref<CustomPage | null>>;
  history: ReturnType<typeof useHistoryStore>;
}>();

const networkId = computed(() => route.params.networkId as string);
const customPageId = computed(() => route.params.customPageId as string);
const pageBlockId = computed(() => route.params.pageBlockId as string);

const customPage = ref<CustomPage | null>(null);
const loading = ref(false);
const error = ref<string | null>();

const form = ref<Partial<PageBlock>>({
  parentPageId: '',
  text: '',
  position: 0,
  data: {},
});

const editDataMode = ref(false);
const jsonEditorValue = ref<object>({});

watch(
  pageBlockId,
  async () => {
    customPage.value = null;
    loading.value = true;

    const data = await props.fetchCustomPage(customPageId.value);
    loading.value = false;

    if (!data.value) throw new Error('CustomPage not found');
    watch(data, (newEntry) => (customPage.value = newEntry), { immediate: true });

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
  },
  { immediate: true },
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

const parentBlock = computed(() =>
  currentPageBlock.value?.parentPageId
    ? customPage.value?.pages.find((p) => p.id === currentPageBlock.value?.parentPageId)
    : null,
);
const siblings = computed(() => {
  if (!parentBlock.value) {
    return customPage.value?.pages.filter((p) => !p.parentPageId);
  }
  return customPage.value?.pages.filter((p) => p.parentPageId == parentBlock.value!.id);
});

const previousBlock = computed(() => {
  if (!currentPageBlock.value || !siblings.value) return null;
  const currentPos = currentPageBlock.value.position;

  return siblings.value.find((p) => p.position == currentPos - 1);
});
const nextBlock = computed(() => {
  if (!currentPageBlock.value || !siblings.value) return null;
  const currentPos = currentPageBlock.value.position;

  return siblings.value.find((p) => p.position == currentPos + 1);
});

const formattedJson = computed(() => {
  return form.value.data;
});

function cancelEditData() {
  editDataMode.value = false;
}

function saveJsonData() {
  try {
    form.value.data = jsonEditorValue.value;
    editDataMode.value = false;
  } catch (_) {
    void _;
    alert('Invalid JSON');
  }
}

function handleReturn() {
  emit('nav-to-custom-pages', networkId.value, customPageId.value);
}

function handleEdit(pageBlockId: string) {
  if (currentPageBlock.value?.id == pageBlockId) return;

  const pageBlock = customPage.value?.pages.find((page) => page.id == pageBlockId);
  if (!pageBlock) {
    global.addToast({ message: 'PageBlock not found.', type: 'error', duration: 5000 });
    return;
  }
  emit('page-blocks-edit', pageBlock);
}

function handleUpdate() {
  if (!currentPageBlock.value) return;
  emit('page-blocks-update', networkId.value, customPageId.value, {
    ...currentPageBlock.value,
    ...form.value,
    parentPageId: form.value.parentPageId || null,
  } as PageBlock);
}

function handleDelete() {
  if (!currentPageBlock.value) return;
  emit('page-blocks-delete', currentPageBlock.value);
}

function buildTree(blocks: PageBlock[]): TreeNode[] {
  const map = new Map<string, TreeNode>();
  const roots: TreeNode[] = [];

  blocks.forEach((b) => map.set(b.id, { ...b, children: [], visited: false, recursive: false }));

  function addChildren(node: TreeNode) {
    if (node.visited) return;
    node.visited = true;

    blocks.forEach((block) => {
      if (block.parentPageId !== node.id) return;

      const childNode = map.get(block.id)!;
      if (childNode.visited) {
        node.children.push({ ...childNode, recursive: true });
      } else {
        if (!childNode.recursive) node.children.push(childNode);
        addChildren(childNode);
      }
    });
  }

  blocks.forEach((block) => {
    const node = map.get(block.id)!;
    const isRoot = !block.parentPageId || !map.has(block.parentPageId);

    if (isRoot && !node.visited) {
      addChildren(node);
      roots.push(node);
    }
  });

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
