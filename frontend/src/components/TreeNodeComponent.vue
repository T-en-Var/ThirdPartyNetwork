<template>
  <div class="relative pl-4">
    <div class="flex items-center space-x-1">
      <button v-if="!node.recursive && node.children?.length" @click="toggle"
        class="w-4 h-4 flex items-center justify-center text-gray-500 hover:text-gray-700 focus:outline-none">
        <svg xmlns="http://www.w3.org/2000/svg" class="w-3 h-3 transform transition-transform duration-200"
          :class="{ 'rotate-90': open }" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
        </svg>
      </button>
      <span v-else class="w-4"></span>

      <template v-if="!node.recursive">
        <RouterLink :to="`/networks/${networkId}/manage/custom-pages/${customPageId}/blocks/${node.id}/edit`"
          class="inline-block px-2 py-1 text-xs rounded transition" :class="{
            'bg-red-200 text-red-800 font-semibold': node.id === currentId,
            'bg-gray-50 text-gray-600 hover:bg-gray-100': node.id !== currentId
          }">
          {{ node.text || '(untitled)' }}
        </RouterLink>
      </template>
      <template v-else>
        <span class="inline-block px-2 py-1 text-xs rounded bg-yellow-100 text-yellow-800 font-semibold">
          {{ node.text || '(untitled)' }} (recursion)
        </span>
      </template>
    </div>

    <div v-if="!node.recursive" v-show="open && node.children?.length" class="ml-5 mt-1 space-y-1 border-l border-gray-300 pl-2">
      <TreeNodeComponent
        v-for="child in node.children"
        :key="child.id"
        :node="child"
        :current-id="currentId"
        :network-id="networkId"
        :custom-page-id="customPageId"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { TreeNode } from '@/types'

defineProps<{
  node: TreeNode
  currentId: string
  networkId: string
  customPageId: string
  visitedIds?: string[]
}>()

const open = ref(true)
const toggle = () => (open.value = !open.value)

// Detect recursion
// const visitedIds = ref(props.visitedIds || []);
// const isRecursive = computed(() => visitedIds.value.includes(props.node.id))

// // Decide which text to display in recursion
// const recursiveText = computed(() => props.node.text || '(untitled)')
</script>
