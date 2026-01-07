<template>
  <div class="flex flex-wrap items-center">
    <!-- Undo/Redo -->
    <div class="flex gap-0.5 border-r border-gray-300 pr-2 mr-2 my-0.5">
      <button type="button" @click="editor.chain().focus().undo().run()" :disabled="!editor.can().undo()" class="editor-btn">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 7v6h6"/><path d="M21 17a9 9 0 0 0-9-9 9 9 0 0 0-6 2.3L3 13"/></svg>
      </button>
      <button type="button" @click="editor.chain().focus().redo().run()" :disabled="!editor.can().redo()" class="editor-btn">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 7v6h-6"/><path d="M3 17a9 9 0 0 1 9-9 9 9 0 0 1 6 2.3l3 3.7"/></svg>
      </button>
    </div>

    <!-- Headings -->
    <div class="flex gap-0.5 border-r border-gray-300 pr-2 mr-2 my-0.5">
      <button type="button" @click="editor.chain().focus().toggleHeading({ level: 1 }).run()" :class="['editor-btn font-bold text-xs', isActive('heading', { level: 1 })]">H1</button>
      <button type="button" @click="editor.chain().focus().toggleHeading({ level: 2 }).run()" :class="['editor-btn font-bold text-xs', isActive('heading', { level: 2 })]">H2</button>
      <button type="button" @click="editor.chain().focus().toggleHeading({ level: 3 }).run()" :class="['editor-btn font-bold text-xs', isActive('heading', { level: 3 })]">H3</button>
      <button type="button" @click="editor.chain().focus().toggleHeading({ level: 4 }).run()" :class="['editor-btn font-bold text-xs', isActive('heading', { level: 4 })]">H4</button>
    </div>

    <!-- Formatting -->
    <div class="flex gap-0.5 border-r border-gray-300 pr-2 mr-2 my-0.5">
      <button type="button" @click="editor.chain().focus().toggleBold().run()" :class="['editor-btn font-bold', isActive('bold')]">B</button>
      <button type="button" @click="editor.chain().focus().toggleItalic().run()" :class="['editor-btn italic', isActive('italic')]">I</button>
      <button type="button" @click="editor.chain().focus().toggleUnderline().run()" :class="['editor-btn', isActive('underline')]"><span class="underline">U</span></button>
      <button type="button" @click="editor.chain().focus().toggleStrike().run()" :class="['editor-btn line-through', isActive('strike')]">S</button>
      <button type="button" @click="editor.chain().focus().toggleHighlight().run()" :class="['editor-btn', isActive('highlight')]"><span class="bg-yellow-300 px-1 rounded-sm text-xs">H</span></button>
    </div>

    <!-- Alignment -->
    <div class="flex gap-0.5 border-r border-gray-300 pr-2 mr-2 my-0.5">
      <button type="button" @click="editor.chain().focus().setTextAlign('left').run()" :class="['editor-btn', isActive('textAlign', { textAlign: 'left' })]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="17" y1="10" x2="3" y2="10"/><line x1="21" y1="6" x2="3" y2="6"/><line x1="21" y1="14" x2="3" y2="14"/><line x1="17" y1="18" x2="3" y2="18"/></svg>
      </button>
      <button type="button" @click="editor.chain().focus().setTextAlign('center').run()" :class="['editor-btn', isActive('textAlign', { textAlign: 'center' })]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="21" y1="10" x2="3" y2="10"/><line x1="21" y1="6" x2="3" y2="6"/><line x1="21" y1="14" x2="3" y2="14"/><line x1="21" y1="18" x2="3" y2="18"/></svg>
      </button>
      <button type="button" @click="editor.chain().focus().setTextAlign('right').run()" :class="['editor-btn', isActive('textAlign', { textAlign: 'right' })]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="21" y1="10" x2="7" y2="10"/><line x1="21" y1="6" x2="3" y2="6"/><line x1="21" y1="14" x2="3" y2="14"/><line x1="21" y1="18" x2="7" y2="18"/></svg>
      </button>
      <button type="button" @click="editor.chain().focus().setTextAlign('justify').run()" :class="['editor-btn', isActive('textAlign', { textAlign: 'justify' })]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="21" y1="10" x2="3" y2="10"/><line x1="21" y1="6" x2="3" y2="6"/><line x1="21" y1="14" x2="3" y2="14"/><line x1="21" y1="18" x2="3" y2="18"/></svg>
      </button>
    </div>

    <!-- Lists -->
    <div class="flex gap-0.5 border-r border-gray-300 pr-2 mr-2 my-0.5">
      <button type="button" @click="editor.chain().focus().toggleBulletList().run()" :class="['editor-btn', isActive('bulletList')]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
      </button>
      <button type="button" @click="editor.chain().focus().toggleOrderedList().run()" :class="['editor-btn', isActive('orderedList')]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="10" y1="6" x2="21" y2="6"/><line x1="10" y1="12" x2="21" y2="12"/><line x1="10" y1="18" x2="21" y2="18"/><path d="M4 6h1v4"/><path d="M4 10h2"/><path d="M6 18H4c0-1 2-2 2-3s-1-1.5-2-1"/></svg>
      </button>
    </div>

    <!-- Insert -->
    <div class="flex gap-0.5 my-0.5">
      <button type="button" @click="$emit('set-link')" :class="['editor-btn', isActive('link')]">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>
      </button>
      <button type="button" @click="$emit('request-image')" class="editor-btn text-gray-600 hover:bg-gray-100">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2" ry="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Editor } from '@tiptap/core';

const props = defineProps<{
  editor: Editor;
}>();

defineEmits(['set-link', 'request-image']);

const isActive = (name: string, opts?: object | undefined) =>
  props.editor.isActive(name, opts) ? 'bg-blue-100 text-blue-700' : 'text-gray-600 hover:bg-gray-100';
</script>

<style scoped>
.editor-btn {
  @apply p-1.5 rounded transition flex items-center justify-center min-w-[28px] h-[28px];
}
</style>