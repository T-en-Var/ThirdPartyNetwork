<template>
  <div class="flex flex-col">
    <div class="flex items-center gap-1.5">
      <button
        v-if="!node.recursive && node.children?.length"
        @click.stop="toggle"
        class="flex h-5 w-5 shrink-0 items-center justify-center rounded text-gray-400 transition-colors hover:bg-gray-200 hover:text-gray-700 focus:outline-none"
        aria-label="Toggle children"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-3.5 w-3.5 transform transition-transform duration-200"
          :class="{ 'rotate-90': open }"
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
        class="h-5 w-5 shrink-0"
      ></div>

      <div
        @click="!node.recursive ? handleNavigate(node.id) : null"
        class="group flex flex-1 items-center rounded-md border-b border-l-2 px-2.5 py-1 text-sm transition-all"
        :class="[
          node.recursive
            ? 'border-orange-200 bg-orange-50 text-orange-800'
            : node.id === currentId
              ? 'border-blue-200 bg-blue-50 font-medium text-blue-700'
              : 'cursor-pointer border-slate-300 text-gray-700 hover:border-slate-600 hover:bg-gray-100',
        ]"
      >
        <span class="truncate"
          ><span class="pr-2 text-xs italic text-gray-500">#{{ node.position }}</span
          >{{ node.text || '(untitled)' }}</span
        >

        <span
          v-if="node.recursive"
          class="ml-2 rounded bg-orange-200/60 px-1.5 text-[10px] font-bold uppercase tracking-wider text-orange-700"
        >
          Recursive
        </span>
      </div>
    </div>

    <div
      v-if="!node.recursive"
      v-show="open && node.children?.length"
      class="ml-[9px] border-l border-gray-200 pl-3"
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
import type { TreeNode } from '@/types';

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
