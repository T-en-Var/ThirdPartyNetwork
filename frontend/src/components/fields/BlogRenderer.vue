<template>
  <div class="blog-renderer">
    <header class="blog-renderer__header">
      <h1 class="blog-renderer__title">
        {{ title || 'Untitled Blog Post' }}
      </h1>
      <div
        v-if="publishedAt"
        class="blog-renderer__date"
      >
        Published on {{ formatDate(publishedAt) }}
      </div>
    </header>

    <editor-content
      :editor="editor"
      class="blog-content"
    />
  </div>
</template>

<script setup lang="ts">
import { useEditor, EditorContent } from '@tiptap/vue-3';
import StarterKit from '@tiptap/starter-kit';
import Image from '@tiptap/extension-image';
import Highlight from '@tiptap/extension-highlight';
import { TextStyle } from '@tiptap/extension-text-style';
import TextAlign from '@tiptap/extension-text-align';
import { watch, onBeforeUnmount } from 'vue';

const props = defineProps<{
  title: string;
  content?: object | null;
  publishedAt?: string | Date | null;
}>();

const CustomImage = Image.extend({
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
          style: `width: ${attributes.width}`,
        }),
      },
    };
  },
}).configure({ inline: true });

const editor = useEditor({
  editable: false,
  content: props.content,
  extensions: [
    StarterKit.configure({
      heading: { levels: [1, 2, 3, 4] },
      link: {
        HTMLAttributes: {
          target: '_blank',
          rel: 'noopener noreferrer',
          class: 'blog-content__link',
        },
      },
    }),
    Highlight,
    TextStyle,

    TextAlign.configure({ types: ['heading', 'paragraph', 'image'] }),

    CustomImage,
  ],
  editorProps: {
    attributes: { class: 'blog-content__body' },
  },
});

watch(
  () => props.content,
  (newContent) => {
    if (editor.value && newContent) {
      const current = JSON.stringify(editor.value.getJSON());
      const next = JSON.stringify(newContent);
      if (current !== next) {
        editor.value.commands.setContent(newContent);
      }
    }
  },
);

function formatDate(date: string | Date) {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

onBeforeUnmount(() => {
  editor.value?.destroy();
});
</script>
