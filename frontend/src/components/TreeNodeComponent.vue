<template>
  <div class="tree-node">
    <div class="tree-node__row">
      <button
        v-if="!node.recursive && node.children?.length"
        @click.stop="toggle"
        class="tree-node__toggle"
        aria-label="Toggle children"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="tree-node__toggle-icon"
          :class="{ 'tree-node__toggle-icon--open': open }"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          stroke-width="2.5"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            d="M9 5l7 7-7 7"
          />
        </svg>
      </button>

      <div
        v-else
        class="tree-node__spacer"
      ></div>

      <div
        @click="!node.recursive ? handleNavigate(node.id) : null"
        class="tree-node__label"
        :class="[
          node.recursive
            ? 'tree-node__label--recursive'
            : node.id === currentId
              ? 'tree-node__label--current'
              : 'tree-node__label--clickable',
        ]"
      >
        <span class="tree-node__text"
          ><span class="tree-node__index">#{{ node.position }}</span
          >{{ node.text || '(untitled)' }}</span
        >

        <span
          v-if="node.recursive"
          class="badge badge--warning"
        >
          Recursive
        </span>
      </div>
    </div>

    <div
      v-if="!node.recursive"
      v-show="open && node.children?.length"
      class="tree-node__children"
    >
      <TreeNodeComponent
        v-for="child in node.children"
        :key="child.id"
        :node="child"
        :current-id="currentId"
        :network-id="networkId"
        :custom-page-id="customPageId"
        @navigate-to-block="handleNavigate"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { PageBlockLightDto } from '@/types';

// Pure frontend-only tree shape (not a backend DTO) built client-side in EditPageBlockTab.vue
// from a flat CustomPageDto.blocks list, for rendering a navigable parent/child block tree.
export interface TreeNode extends PageBlockLightDto {
  children: TreeNode[];
  visited: boolean;
  recursive: boolean;
}

defineProps<{
  node: TreeNode;
  currentId: string;
  networkId: string;
  customPageId: string;
  visitedIds?: string[];
}>();

const emit = defineEmits<{
  (e: 'navigate-to-block', pageBlockId: string): void;
}>();

const open = ref(true);

function toggle() {
  open.value = !open.value;
}

function handleNavigate(pageBlockId: string) {
  emit('navigate-to-block', pageBlockId);
}
</script>
