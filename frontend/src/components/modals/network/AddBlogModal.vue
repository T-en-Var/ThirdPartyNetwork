<template>
  <modal-container title="Create New Blog" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-6">

      <div>
        <label for="title"
          class="block text-xs font-black uppercase tracking-widest text-slate-500 mb-2 ml-1">
          Blog Title
        </label>
        <input id="title" v-model="form.title" type="text"
          placeholder="My Awesome Journey..."
          class="block w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm shadow-sm focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none"
          required />
      </div>

      <div class="rounded-xl border border-slate-100 bg-slate-50 p-4 space-y-3">
        <label
          class="block text-[10px] font-black uppercase tracking-tighter text-slate-400">
          Permanent URL Preview
        </label>

        <div class="flex items-center gap-2 font-mono text-sm overflow-hidden">
          <span class="text-slate-400 shrink-0">.../blogs/</span>
          <span :class="[
            'font-bold truncate',
            slugExists ? 'text-amber-600' : 'text-blue-600',
            !slugPreview ? 'italic text-slate-300' : ''
          ]">
            {{ slugPreview || 'your-slug-here' }}
          </span>

          <div class="ml-auto shrink-0">
            <LoadingSpinner v-if="isChecking" size="sm" />
            <div v-else-if="slugExists" class="text-amber-500" title="Slug Taken">
              ⚠️
            </div>
            <div v-else-if="isValid" class="text-emerald-500" title="Available">
              ✓
            </div>
          </div>
        </div>

        <transition name="fade">
          <div v-if="slugExists"
            class="text-[11px] text-amber-700 bg-amber-100/50 p-2 rounded-lg border border-amber-200">
            This URL is already taken. The system will append a unique ID to your
            link.
          </div>
          <div v-else-if="isChanged && slugPreview"
            class="text-[11px] text-slate-400 italic">
            Verifying availability...
          </div>
        </transition>
      </div>

      <div class="flex justify-end space-x-3 pt-4 border-t border-gray-200">
        <button type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')" :disabled="isSubmitting">
          Cancel
        </button>
        <button type="submit"
          class="rounded-md border border-transparent disabled:bg-gray-500 disabled:cursor-not-allowed bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          :disabled="isSubmitting || !isValid">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Blog</span>
        </button>
      </div>
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { CreateBlog } from '@/types/userContent/blog';
import api from '@/api/api';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'createBlog', payload: CreateBlog): void;
}>();


const props = defineProps({
  isSubmitting: {
    type: Boolean,
    default: false,
  },
  networkId: {
    type: String,
    required: false,
  }
});

const form = ref({ title: '' });
const isChecking = ref(false);
const slugExists = ref(false);
const isChanged = ref(false); // Tracks if the user has typed since the last check

const slugPreview = computed(() => generateSlug(form.value.title));

const isValid = computed(() =>
  !!form.value.title.trim() &&
  !isChecking.value &&
  !isChanged.value &&
  !slugExists.value
);

let debounceTimer: ReturnType<typeof setTimeout>;

watch(slugPreview, (newSlug) => {
  if (!newSlug) {
    slugExists.value = false;
    isChecking.value = false;
    isChanged.value = false;
    return;
  }

  isChanged.value = true;
  isChecking.value = true;
  slugExists.value = false;

  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(async () => {
    if (!props.networkId) return;

    try {
      await api.get(`/networks/${props.networkId}/blogs/${newSlug}`);
      slugExists.value = true;
    } catch (err) {
      // 404 = available
      void err;
      slugExists.value = false;
    } finally {
      isChecking.value = false;
      isChanged.value = false;
    }
  }, 500);
});

function generateSlug(title: string) {
  if (!title) return '';
  let str = title.toLowerCase();
  // remove diacritics
  str = str.normalize('NFD').replace(/\p{Diacritic}/gu, '');
  // remove invalid chars
  str = str.replace(/[^a-z0-9\s-]/g, '');
  // replace whitespace and dashes with single dash
  str = str.replace(/[\s-]+/g, '-');
  return str.replace(/^-+|-+$/g, '');
}


function handleSubmit() {
  emit('createBlog', { title: form.value.title, summary: '', body: {}, accessLevel: 0 });
}
</script>
