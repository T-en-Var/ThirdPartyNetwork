<template>
  <node-view-wrapper as="span" class="relative inline-block leading-none select-none group">
    <img
      :src="node.attrs.src"
      :alt="node.attrs.alt"
      :title="node.attrs.title"
      :style="{ width: currentWidth }"
      class="block"
      :class="[
        selected ? 'ring-2 ring-blue-500 rounded-sm' : '',
        'hover:ring-2 hover:ring-blue-300 cursor-pointer'
      ]"
      @click="handleClick"
    />

    <div
      v-if="editor.isEditable && selected"
      class="absolute bottom-2 right-2 w-4 h-4 bg-blue-500 border-2 border-white rounded-full cursor-nwse-resize z-50 shadow-sm hover:scale-110"
      @mousedown.stop.prevent="onMouseDown"
    ></div>
  </node-view-wrapper>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { nodeViewProps, NodeViewWrapper } from '@tiptap/vue-3'

const props = defineProps(nodeViewProps)

const resizing = ref(false)
const startX = ref(0)
const startWidth = ref(0)

const currentWidth = computed(() => {
    // Return the width, ensuring it has 'px' or '%' if missing, or default to 100%
    const w = props.node.attrs.width
    if (!w) return '100%'
    // If it's just a number, assume pixels (legacy support)
    if (typeof w === 'number') return w + 'px' 
    return w
})

// --- THE FIX IS HERE ---
function handleClick(event: MouseEvent) {
  void event;
  // 1. Get the current position of this node in the document
  // getPos() is a function provided by nodeViewProps
  const pos = props.getPos() 

  // 2. Set the selection to this specific node
  if (typeof pos === 'number') {
     props.editor.commands.setNodeSelection(pos)
  }
}

function onMouseDown(event: MouseEvent) {
    resizing.value = true
    startX.value = event.clientX
    
    const imgElement = (event.target as HTMLElement).previousElementSibling as HTMLImageElement
    
    if (imgElement) {
        startWidth.value = imgElement.offsetWidth
    }

    document.addEventListener('mousemove', onMouseMove)
    document.addEventListener('mouseup', onMouseUp)
}

function onMouseMove(event: MouseEvent) {
    if (!resizing.value) return

    const dx = event.clientX - startX.value
    const newWidth = Math.max(50, startWidth.value + dx) // Minimum 50px

    // Update the actual attributes
    props.updateAttributes({
        width: `${newWidth}px` 
    })
}

function onMouseUp() {
    resizing.value = false
    document.removeEventListener('mousemove', onMouseMove)
    document.removeEventListener('mouseup', onMouseUp)
}
</script>