<template>
  <div class="edit-page-block-tab">
    <!-- Back button -->
    <div class="edit-page-block-tab__back-wrap">
      <button
        @click="handleReturn"
        class="back-link"
      >
        ← Back to Custom Page
      </button>
    </div>

    <div class="card">
      <LoadingErrorComponent
        :loading="loading"
        :error="error ?? undefined"
        button-value="Go back"
        @button-action="router.go(-1)"
        :has-value="!!currentPageBlock"
      />

      <div
        v-if="!loading && !error && currentPageBlock"
        class="edit-page-block-tab__body"
      >
        <h2 class="edit-page-block-tab__title">Edit Page Block</h2>
        <div class="edit-page-block-tab__fields">
          <div class="form-field">
            <label class="form-label">Parent Block</label>
            <SearchableSelect
              v-model="form.parentPageId"
              :options="parentOptions"
              label="Parent Block"
              :nullable="true"
            />
          </div>

          <div class="form-field">
            <label class="form-label">Text</label>
            <input
              v-model="form.text"
              type="text"
              class="form-input"
            />
          </div>

          <div class="form-field">
            <label class="form-label">Position</label>
            <input
              v-model.number="form.position"
              type="number"
              class="form-input"
            />
          </div>

          <div class="form-field">
            <label class="form-label">Block Data</label>

            <div v-if="!editDataMode">
              <pre class="json-preview">{{ formattedJson }}</pre>
              <button
                type="button"
                class="btn btn--primary btn--sm"
                @click="editDataMode = true"
              >
                Edit JSON
              </button>
            </div>

            <div
              v-else
              class="edit-page-block-tab__json-editor-wrap"
            >
              <JsonEditorVue
                v-model="jsonEditorValue"
                :stringified="false"
                class="edit-page-block-tab__json-editor"
              />
              <div class="edit-page-block-tab__json-actions">
                <button
                  type="button"
                  class="btn btn--outline btn--sm"
                  @click="cancelEditData"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  class="btn btn--success btn--sm"
                  @click="saveJsonData"
                >
                  Save JSON
                </button>
              </div>
            </div>
          </div>

          <div class="form__actions">
            <button
              @click="handleDelete"
              class="btn btn--danger"
            >
              Delete Block
            </button>
            <button
              @click="handleUpdate"
              class="btn btn--primary"
            >
              Update Block
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card">
      <h3 class="edit-page-block-tab__nav-title">Navigate Page Blocks</h3>

      <!-- Previous / Next -->
      <div class="edit-page-block-tab__prev-next">
        <!-- Previous -->
        <RouterLink
          v-if="previousBlock"
          :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${previousBlock.id}/edit`"
          class="edit-page-block-tab__nav-link"
        >
          ← Previous (#{{ previousBlock.position }})
        </RouterLink>
        <span
          v-else
          class="edit-page-block-tab__nav-empty"
          >No previous block</span
        >

        <!-- Next -->
        <RouterLink
          v-if="nextBlock"
          :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${nextBlock.id}/edit`"
          class="edit-page-block-tab__nav-link"
        >
          Next (#{{ nextBlock.position }}) →
        </RouterLink>
        <span
          v-else
          class="edit-page-block-tab__nav-empty"
          >No next block</span
        >
      </div>

      <div
        v-if="parentBlock || childBlocks.length"
        class="edit-page-block-tab__relations"
      >
        <!-- Parent -->
        <div v-if="parentBlock">
          <h4 class="edit-page-block-tab__relation-heading">Parent Block</h4>
          <RouterLink
            :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${parentBlock.id}/edit`"
            class="edit-page-block-tab__relation-card"
          >
            <span class="edit-page-block-tab__relation-card-title">↑ {{ parentBlock.text }}</span>
            <span class="edit-page-block-tab__relation-card-meta">
              Position: {{ parentBlock.position }}
            </span>
          </RouterLink>
        </div>

        <!-- Children -->
        <div v-if="childBlocks.length">
          <h4 class="edit-page-block-tab__relation-heading">Child Blocks</h4>
          <div class="edit-page-block-tab__relation-children">
            <RouterLink
              v-for="child in childBlocks"
              :key="child.id"
              :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${child.id}/edit`"
              class="edit-page-block-tab__relation-card"
            >
              <span class="edit-page-block-tab__relation-card-title">
                {{ child.text }}
              </span>
              <span class="edit-page-block-tab__relation-card-meta">
                Position: {{ child.position }}
              </span>
            </RouterLink>
          </div>
        </div>
      </div>

      <div class="edit-page-block-tab__tree-section">
        <!-- Tree display -->
        <div v-if="tree.length">
          <h4 class="edit-page-block-tab__relation-heading">Tree Display</h4>
          <div class="edit-page-block-tab__tree">
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
        class="edit-page-block-tab__history-section"
      >
        <h4 class="edit-page-block-tab__relation-heading">Recently Visited</h4>
        <div class="edit-page-block-tab__history-list">
          <RouterLink
            v-for="pb in [...history.data.pageBlocks].reverse()"
            :key="pb.id"
            :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${pb.id}/edit`"
            class="edit-page-block-tab__history-pill"
          >
            {{ pb.text }}
          </RouterLink>
        </div>
      </div>
    </div>

    <div class="edit-page-block-tab__spacer"></div>
  </div>
</template>

<script setup lang="ts">
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import type { CustomPageDto, PageBlockLightDto } from '@/types';
import type { TreeNode } from '@/components/TreeNodeComponent.vue';
import { ref, computed, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import JsonEditorVue from 'json-editor-vue';
import TreeNodeComponent from '@/components/TreeNodeComponent.vue';
import SearchableSelect from '@/components/SearchableSelect.vue';
import type { useHistoryStore } from '@/stores/history';
import { useGlobalStore } from '@/stores/global';
import { useEditableEntity } from '@/composables/useEditableEntity';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();

const emit = defineEmits<{
  (e: 'page-blocks-edit', block: PageBlockLightDto): void;
  (e: 'page-blocks-update', networkId: string, customPageId: string, block: PageBlockLightDto): void;
  (e: 'page-blocks-delete', block: PageBlockLightDto): void;
  (e: 'nav-to-custom-pages', networkId: string, customPageId: string): void;
}>();

const props = defineProps<{
  fetchCustomPage: (customPageId: string) => Promise<Ref<CustomPageDto | null>>;
  history: ReturnType<typeof useHistoryStore>;
}>();

const networkId = computed(() => route.params.networkId as string);
const customPageId = computed(() => route.params.customPageId as string);
const pageBlockId = computed(() => route.params.pageBlockId as string);

const form = ref<Partial<PageBlockLightDto>>({
  parentPageId: '',
  text: '',
  position: 0,
  data: {},
});

const editDataMode = ref(false);
const jsonEditorValue = ref<object>({});

// This tab fetches the *custom page* (keyed by customPageId), but the entity being edited is
// the page block derived from it via pageBlockId - so we watch pageBlockId (to refetch/re-derive
// when navigating between sibling blocks) while always fetching by customPageId, and use
// `isNotFound` to check the derived block rather than the fetched custom page itself.
const {
  entity: customPage,
  loading,
  error,
} = useEditableEntity<CustomPageDto>({
  id: pageBlockId,
  fetch: () => props.fetchCustomPage(customPageId.value),
  isNotFound: (loadedCustomPage) => !loadedCustomPage.blocks.find((p) => p.id === pageBlockId.value),
  notFoundMessage: 'Page block not found.',
  onNotFound: () => handleReturn(),
  onLoaded: () => {
    if (!currentPageBlock.value) return;

    form.value = {
      parentPageId: currentPageBlock.value.parentPageId || '',
      text: currentPageBlock.value.text,
      position: currentPageBlock.value.position,
      data: currentPageBlock.value.data,
    };

    try {
      jsonEditorValue.value = (currentPageBlock.value.data as object) ?? {};
    } catch {
      jsonEditorValue.value = {};
    }
  },
});

const currentPageBlock = computed(() =>
  customPage.value?.blocks.find((p) => p.id == pageBlockId.value),
);
const parentOptions: Ref<PageBlockLightDto[]> = computed(
  () => customPage.value?.blocks.filter((p) => p.id != pageBlockId.value) ?? [],
);
const childBlocks = computed(
  () => customPage.value?.blocks.filter((p) => p.parentPageId === pageBlockId.value) ?? [],
);

const parentBlock = computed(() =>
  currentPageBlock.value?.parentPageId
    ? customPage.value?.blocks.find((p) => p.id === currentPageBlock.value?.parentPageId)
    : null,
);
const siblings = computed(() => {
  if (!parentBlock.value) {
    return customPage.value?.blocks.filter((p) => !p.parentPageId);
  }
  return customPage.value?.blocks.filter((p) => p.parentPageId == parentBlock.value!.id);
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

  const pageBlock = customPage.value?.blocks.find((page) => page.id == pageBlockId);
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
  } as PageBlockLightDto);
}

function handleDelete() {
  if (!currentPageBlock.value) return;
  emit('page-blocks-delete', currentPageBlock.value);
}

function buildTree(blocks: PageBlockLightDto[]): TreeNode[] {
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

const tree = computed(() => buildTree(customPage.value?.blocks ?? []));
</script>
