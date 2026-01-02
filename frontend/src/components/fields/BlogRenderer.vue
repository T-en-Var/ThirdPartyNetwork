<template>
  <div class="max-w-3xl mx-auto px-6 py-8">
    <header class="mb-8 text-center">
      <h1 class="text-4xl font-extrabold text-gray-900 mb-4 leading-tight">
        {{ title || 'Untitled Blog Post' }}
      </h1>
      <div v-if="publishedAt" class="text-gray-500 text-sm">
        Published on {{ formatDate(publishedAt) }}
      </div>
    </header>

    <editor-content :editor="editor"
      class="blog-content prose prose-lg max-w-none prose-p:leading-relaxed prose-img:rounded-xl" />
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
        parseHTML: element => element.getAttribute('width') || element.style.width,
        renderHTML: attributes => ({
          width: attributes.width,
          // We explicitly keep the style here to ensure render consistency
          style: `width: ${attributes.width}`
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
          // target="_blank" is often good for blogs so users don't leave your site
          target: '_blank',
          rel: 'noopener noreferrer',
          class: 'text-blue-600 underline hover:text-blue-800 cursor-pointer',
        },
      }
    }),
    Highlight,
    TextStyle,

    TextAlign.configure({ types: ['heading', 'paragraph', 'image'] }),

    CustomImage,
  ],
  editorProps: {
    attributes: { class: 'outline-none focus:outline-none' },
  },
});

watch(() => props.content, (newContent) => {
  if (editor.value && newContent) {
    const current = JSON.stringify(editor.value.getJSON());
    const next = JSON.stringify(newContent);
    if (current !== next) {
      editor.value.commands.setContent(newContent);
    }
  }
});

function formatDate(date: string | Date) {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric', month: 'long', day: 'numeric'
  });
}

onBeforeUnmount(() => {
  editor.value?.destroy();
});
</script>

<style scoped>
/* 4. THE CSS FIX 
   Standard CSS ignores "text-align" on an IMG tag. 
   We have to translate that inline style into block display + margins.
*/
:deep(.blog-content) {
  img[style*="text-align: center"] {
    display: block;
    margin-left: auto;
    margin-right: auto;
  }

  img[style*="text-align: right"] {
    display: block;
    margin-left: auto;
    margin-right: 0;
  }

  img[style*="text-align: left"] {
    display: block;
    margin-right: auto;
    margin-left: 0;
  }
}
</style>