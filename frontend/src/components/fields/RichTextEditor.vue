<template>
  <div class="rich-text-editor">
    <div
      v-if="editor"
      class="rich-text-editor__toolbar"
    >
      <editor-menu
        :editor="editor"
        @set-link="setLink"
        @request-image="$emit('request-image')"
      />

      <!-- Main Bubble Menu (Formatting) -->
      <bubble-menu
        :editor="editor"
        :tippy-options="{ duration: 100, maxWidth: 600 }"
        :should-show="shouldShowMainBubble"
      >
        <div class="rich-text-editor__bubble-menu">
          <editor-menu
            :editor="editor"
            @set-link="setLink"
            @request-image="$emit('request-image')"
          />
        </div>
      </bubble-menu>

      <!-- Link Bubble Menu -->
      <bubble-menu
        :editor="editor"
        :tippy-options="{ duration: 100, placement: 'bottom' }"
        :should-show="shouldShowLinkBubble"
      >
        <div class="rich-text-editor__link-bubble">
          <template v-if="isEditingLink">
            <input
              v-model="linkMenuUrl"
              type="text"
              class="rich-text-editor__link-input"
              placeholder="https://"
              @keydown.enter.prevent="saveLink"
            />
            <button
              @click="saveLink"
              class="btn btn--primary btn--sm"
            >
              Save
            </button>
            <button
              @click="cancelLinkEdit"
              class="btn btn--ghost btn--sm"
            >
              Cancel
            </button>
          </template>
          <template v-else>
            <a
              :href="editor.getAttributes('link').href"
              target="_blank"
              class="rich-text-editor__link-preview"
              rel="noopener noreferrer"
            >
              {{ editor.getAttributes('link').href }}
            </a>
            <div class="rich-text-editor__divider"></div>
            <button
              @click="editLink"
              class="editor-btn"
              title="Edit Link"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
              </svg>
            </button>
            <button
              @click="editor.chain().focus().unsetLink().run()"
              class="editor-btn"
              title="Remove Link"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <polyline points="3 6 5 6 21 6"></polyline>
                <path
                  d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"
                ></path>
              </svg>
            </button>
          </template>
        </div>
      </bubble-menu>
    </div>

    <editor-content
      :editor="editor"
      class="rich-text-editor__content"
    />
  </div>
</template>

<script setup lang="ts">
import { watch, onBeforeUnmount, ref } from 'vue';
import { useEditor, EditorContent, VueNodeViewRenderer } from '@tiptap/vue-3';
import { Editor as CoreEditor } from '@tiptap/core';
import { BubbleMenu } from '@tiptap/vue-3/menus';

import StarterKit from '@tiptap/starter-kit';
import Image from '@tiptap/extension-image';
import Highlight from '@tiptap/extension-highlight';
import Placeholder from '@tiptap/extension-placeholder';
import { TextStyle } from '@tiptap/extension-text-style';
import TextAlign from '@tiptap/extension-text-align';
import BubbleMenuExtension from '@tiptap/extension-bubble-menu';
import Underline from '@tiptap/extension-underline';

import ImageNode from './ImageNode.vue';
import EditorMenu from './EditorMenu.vue';

const props = defineProps<{ modelValue?: object | string }>();
const emit = defineEmits(['update:modelValue', 'request-image']);

// --- CUSTOM EXTENSIONS ---
const editor = useEditor({
  extensions: [
    StarterKit.configure({
      heading: { levels: [1, 2, 3, 4] },
      link: {
        openOnClick: false,
        HTMLAttributes: {
          class: 'rich-text-editor__content-link',
        },
      },
    }),
    BubbleMenuExtension,
    Underline,
    Highlight,
    TextStyle,
    TextAlign.configure({ types: ['heading', 'paragraph', 'image'] }),
    Image.extend({
      addAttributes() {
        return {
          src: {},
          alt: { default: null },
          title: { default: null },
          width: {
            default: '100%',
            parseHTML: (element) => element.getAttribute('width') || element.style.width,
            renderHTML: (attributes) => ({
              width: attributes.width,
            }),
          },
        };
      },
      addNodeView() {
        return VueNodeViewRenderer(ImageNode);
      },
    }).configure({ inline: true }),
    Placeholder.configure({ placeholder: 'Start writing...' }),
  ],
  content: props.modelValue,
  editorProps: {
    attributes: { class: 'rich-text-editor__prosemirror' },
  },
  onUpdate: ({ editor }) => emit('update:modelValue', editor.getJSON()),
});

// --- HELPERS ---
const linkMenuUrl = ref('');
const isEditingLink = ref(false);

const shouldShowMainBubble = ({
  editor,
  from,
  to,
}: {
  editor: CoreEditor;
  from: number;
  to: number;
}) => {
  return !editor.isActive('link') && from !== to;
};

const shouldShowLinkBubble = ({ editor }: { editor: CoreEditor }) => {
  return editor.isActive('link');
};

const editLink = () => {
  linkMenuUrl.value = editor.value?.getAttributes('link').href || '';
  isEditingLink.value = true;
};

const saveLink = () => {
  if (linkMenuUrl.value) {
    editor.value
      ?.chain()
      .focus()
      .extendMarkRange('link')
      .setLink({ href: linkMenuUrl.value })
      .run();
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

function setLink() {
  const previousUrl = editor.value?.getAttributes('link').href;
  linkMenuUrl.value = previousUrl || '';

  if (!previousUrl) {
    editor.value?.chain().focus().extendMarkRange('link').setLink({ href: '' }).run();
  }

  isEditingLink.value = true;
}

// --- EXPOSED METHODS FOR PARENT ---
function insertImage(url: string) {
  if (editor.value) {
    const width = window.prompt('Image Width? (e.g. 100%, 500px, 50%)', '100%');

    editor.value
      .chain()
      .focus()
      .setImage({
        src: url,
        // @ts-expect-error: Tiptap doesn't type custom attributes by default easily
        style: width ? `width: ${width}` : undefined,
      })
      .run();
  }
}

defineExpose({ insertImage });

watch(
  () => props.modelValue,
  (newValue) => {
    if (editor.value && newValue) {
      const currentJSON = JSON.stringify(editor.value.getJSON());
      const newJSON = JSON.stringify(newValue);
      if (currentJSON !== newJSON) editor.value.commands.setContent(newValue);
    }
  },
);

onBeforeUnmount(() => editor.value?.destroy());
</script>
