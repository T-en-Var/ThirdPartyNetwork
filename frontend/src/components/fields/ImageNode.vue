<template>
  <node-view-wrapper
    as="span"
    class="image-node"
  >
    <img
      :src="node.attrs.src"
      :alt="node.attrs.alt"
      :title="node.attrs.title"
      :style="{ width: currentWidth }"
      class="image-node__image"
      :class="{ 'image-node__image--selected': selected }"
      @click="handleClick"
    />

    <div
      v-if="editor.isEditable && selected"
      class="image-node__resize-handle"
      @mousedown.stop.prevent="onMouseDown"
    ></div>
  </node-view-wrapper>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { nodeViewProps, NodeViewWrapper } from '@tiptap/vue-3';

const props = defineProps(nodeViewProps);

const resizing = ref(false);
const startX = ref(0);
const startWidth = ref(0);

const currentWidth = computed(() => {
  const w = props.node.attrs.width;
  if (!w) return '100%';
  if (typeof w === 'number') return w + 'px';
  return w;
});

function handleClick(event: MouseEvent) {
  void event;
  const pos = props.getPos();

  if (typeof pos === 'number') {
    props.editor.commands.setNodeSelection(pos);
  }
}

function onMouseDown(event: MouseEvent) {
  resizing.value = true;
  startX.value = event.clientX;

  const imgElement = (event.target as HTMLElement).previousElementSibling as HTMLImageElement;

  if (imgElement) {
    startWidth.value = imgElement.offsetWidth;
  }

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
}

function onMouseMove(event: MouseEvent) {
  if (!resizing.value) return;

  const dx = event.clientX - startX.value;
  const newWidth = Math.max(50, startWidth.value + dx);

  props.updateAttributes({
    width: `${newWidth}px`,
  });
}

function onMouseUp() {
  resizing.value = false;
  document.removeEventListener('mousemove', onMouseMove);
  document.removeEventListener('mouseup', onMouseUp);
}
</script>
