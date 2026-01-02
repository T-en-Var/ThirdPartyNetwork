<template>
  <div
    class="border border-gray-300 rounded-lg overflow-hidden bg-white shadow-sm flex flex-col h-full group">

    <div v-if="editor"
      class="bg-gray-50 border-b border-gray-200 p-2 sticky top-0 z-10">
      <editor-menu 
        :editor="editor" 
        @set-link="setLink" 
        @request-image="$emit('request-image')" 
      />

      <!-- Main Bubble Menu (Formatting) -->
      <bubble-menu :editor="editor" :tippy-options="{ duration: 100, maxWidth: 600 }" :should-show="shouldShowMainBubble">
        <div class="bg-white border border-gray-200 shadow-lg rounded p-1 max-w-md">
          <editor-menu 
            :editor="editor" 
            @set-link="setLink" 
            @request-image="$emit('request-image')" 
          />
        </div>
      </bubble-menu>

      <!-- Link Bubble Menu -->
      <bubble-menu :editor="editor" :tippy-options="{ duration: 100, placement: 'bottom' }" :should-show="shouldShowLinkBubble">
        <div class="bg-white border border-gray-200 shadow-lg rounded p-2 flex items-center gap-2">
          <template v-if="isEditingLink">
            <input 
              v-model="linkMenuUrl" 
              type="text" 
              class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:border-blue-500 min-w-[200px]"
              placeholder="https://"
              @keydown.enter.prevent="saveLink"
            />
            <button @click="saveLink" class="text-xs bg-blue-600 text-white px-2 py-1 rounded hover:bg-blue-700">Save</button>
            <button @click="cancelLinkEdit" class="text-xs text-gray-600 hover:bg-gray-100 px-2 py-1 rounded">Cancel</button>
          </template>
          <template v-else>
            <a :href="editor.getAttributes('link').href" target="_blank" class="text-blue-600 underline text-sm max-w-[200px] truncate block" rel="noopener noreferrer">
              {{ editor.getAttributes('link').href }}
            </a>
            <div class="w-px h-4 bg-gray-300 mx-1"></div>
            <button @click="editLink" class="editor-btn text-gray-600 hover:text-blue-600" title="Edit Link">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
            </button>
            <button @click="editor.chain().focus().unsetLink().run()" class="editor-btn text-gray-600 hover:text-red-600" title="Remove Link">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
            </button>
          </template>
        </div>
      </bubble-menu>
    </div>

    <editor-content :editor="editor"
      class="flex-grow w-full p-6 min-h-[500px] outline-none mx-auto prose prose-lg max-w-3xl prose-p:mb-6 prose-p:leading-relaxed focus:outline-none" />
  </div>
</template>

<script setup lang="ts">
import { watch, onBeforeUnmount, ref } from 'vue';
import { useEditor, EditorContent, VueNodeViewRenderer } from '@tiptap/vue-3';
import { Editor as CoreEditor } from '@tiptap/core';
import { BubbleMenu } from '@tiptap/vue-3/menus';

// 4. Import Extensions
import StarterKit from '@tiptap/starter-kit';
import Image from '@tiptap/extension-image';
import Highlight from '@tiptap/extension-highlight';
import Placeholder from '@tiptap/extension-placeholder';
import { TextStyle } from '@tiptap/extension-text-style';
import TextAlign from '@tiptap/extension-text-align';
import BubbleMenuExtension from '@tiptap/extension-bubble-menu';
import Underline from '@tiptap/extension-underline';

// 5. Local Imports
import ImageNode from './ImageNode.vue';
import EditorMenu from './EditorMenu.vue';

const props = defineProps<{ modelValue?: object | string; }>();
const emit = defineEmits(['update:modelValue', 'request-image']);

// No reactive UI state required for font-size/line-height

// --- CUSTOM EXTENSIONS ---

// LineHeight extension removed

const editor = useEditor({
  extensions: [
    StarterKit.configure({
      heading: { levels: [1, 2, 3, 4] },
      link: {
        openOnClick: false,
        HTMLAttributes: {
          class: 'text-blue-600 underline hover:text-blue-800 cursor-pointer',
        },
      }
    }),
    BubbleMenuExtension, // Required for the menu logic to work
    Underline,
    Highlight,
    TextStyle,
    // Align: Allow alignment on images too!
    TextAlign.configure({ types: ['heading', 'paragraph', 'image'] }),
    // Link configured via starter kit / core; avoid duplicate registration
    Image.extend({
      // 1. Add the "width" attribute so it saves to JSON/HTML
      addAttributes() {
        return {
          src: {},
          alt: { default: null },
          title: { default: null },
          width: {
            default: '100%',
            // Parse width from existing HTML styles or attributes
            parseHTML: element => element.getAttribute('width') || element.style.width,
            // Render width to HTML
            renderHTML: attributes => ({
              width: attributes.width
            }),
          },
        };
      },
      // 2. Register the Vue component to render this node
      addNodeView() {
        return VueNodeViewRenderer(ImageNode);
      },
    }).configure({ inline: true }),
    Placeholder.configure({ placeholder: 'Start writing...' }),
  ],
  content: props.modelValue,
  editorProps: {
    attributes: { class: 'outline-none focus:outline-none h-full' },
  },
  onUpdate: ({ editor }) => emit('update:modelValue', editor.getJSON()),
});

// --- HELPERS ---

const linkMenuUrl = ref('');
const isEditingLink = ref(false);

const shouldShowMainBubble = ({ editor, from, to }: { editor: CoreEditor; from: number; to: number }) => {
  // Show main bubble menu only when text is selected and it's NOT a link
  return !editor.isActive('link') && from !== to;
};

const shouldShowLinkBubble = ({ editor }: { editor: CoreEditor }) => {
  // Show link bubble menu whenever a link is active
  return editor.isActive('link');
};

const editLink = () => {
  linkMenuUrl.value = editor.value?.getAttributes('link').href || '';
  isEditingLink.value = true;
};

const saveLink = () => {
  if (linkMenuUrl.value) {
    editor.value?.chain().focus().extendMarkRange('link').setLink({ href: linkMenuUrl.value }).run();
  } else {
    editor.value?.chain().focus().unsetLink().run();
  }
  isEditingLink.value = false;
};

const cancelLinkEdit = () => {
  const currentHref = editor.value?.getAttributes('link').href;
  if (!currentHref) {
    editor.value?.chain().focus().unsetLink().run();
  }
  isEditingLink.value = false;
};

// const shouldShowLinkMenu = ({ editor }: { editor: Editor; }) => {
//   return editor.isActive('link');
// };

// line-height sync and handlers removed

function setLink() {
  const previousUrl = editor.value?.getAttributes('link').href;
  linkMenuUrl.value = previousUrl || '';

  // If no link exists, create an empty one to trigger the bubble menu
  if (!previousUrl) {
    editor.value?.chain().focus().extendMarkRange('link').setLink({ href: '' }).run();
  }

  isEditingLink.value = true;
}

// function unsetLink() {
//   editor.value?.chain().focus().extendMarkRange('link').unsetLink().run();
// }

// --- EXPOSED METHODS FOR PARENT ---
// The parent component calls this after the modal uploads the file
function insertImage(url: string) {
  if (editor.value) {
    // We prompt for width here to keep the editor "Simple" 
    // Real drag-to-resize requires complex custom NodeViews
    const width = window.prompt("Image Width? (e.g. 100%, 500px, 50%)", "100%");

    editor.value.chain().focus().setImage({
      src: url,
      // @ts-expect-error: Tiptap doesn't type custom attributes by default easily
      style: width ? `width: ${width}` : undefined
    }).run();
  }
}

// Expose this function so the parent can access it via Template Ref
defineExpose({ insertImage });

watch(() => props.modelValue, (newValue) => {
  if (editor.value && newValue) {
    const currentJSON = JSON.stringify(editor.value.getJSON());
    const newJSON = JSON.stringify(newValue);
    if (currentJSON !== newJSON) editor.value.commands.setContent(newValue);
  }
});

onBeforeUnmount(() => editor.value?.destroy());
</script>

<style scoped>
/* Force images to respect Text Align in the editor view */
:deep(.ProseMirror img) {
  display: inline-block;
  /* Default Max Width for better UX */
  max-width: 100%;
  height: auto;
}

/* Alignment Classes applied by Tiptap */
:deep(.ProseMirror p[style*="text-align: center"]),
:deep(.ProseMirror h1[style*="text-align: center"]),
:deep(.ProseMirror h2[style*="text-align: center"]),
:deep(.ProseMirror h3[style*="text-align: center"]) {
  text-align: center;
}

:deep(.ProseMirror p[style*="text-align: right"]) {
  text-align: right;
}

:deep(.ProseMirror p[style*="text-align: justify"]) {
  text-align: justify;
}
</style>