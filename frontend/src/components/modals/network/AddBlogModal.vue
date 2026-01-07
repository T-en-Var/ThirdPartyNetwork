<template>
  <modal-container title="Add Blog" @close="$emit('close')">
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label for="title"
          class="block text-sm font-semibold text-gray-800 mb-2">Title</label>
        <input id="title" v-model="form.title" type="text"
          placeholder="Enter blog title"
          class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500 transition-all"
          required />
      </div>

      <div>
        <label class="block text-sm font-semibold text-gray-800 mb-2">Slug
          preview</label>
        <input type="text" :value="slugPreview" disabled
          class="block w-full rounded-lg border border-gray-200 bg-gray-50 px-4 py-2 text-sm text-gray-500 shadow-sm" />

        <div class="mt-4 flex items-center text-sm text-gray-600">
          <span class="mr-2 font-medium">Permanent link:</span>

          <div
            class="bg-gray-100 border border-gray-300 rounded px-1.5 py-0.5 font-mono text-gray-800">
            <span class="text-gray-500">.../blogs/</span>
            <span class="text-indigo-600 font-bold">{{ slugPreview }}</span>
          </div>
        </div>
        <div class="mt-3">
          <div v-if="isChecking"
            class="flex items-center gap-2 text-sm text-gray-600">
            <LoadingSpinner />
            <span>Checking your slug...</span>
          </div>

          <div v-else-if="slugExists"
            class="mt-2 p-2 bg-yellow-50 border border-yellow-200 text-yellow-800 text-sm rounded">
            A blog with this url already exists, an arbitrary number may be added to
            the url by the system
          </div>
        </div>

        <p class="mt-1 text-xs text-gray-500">You can change the title, but not the
          link.</p>
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
import type { AxiosError } from 'axios';
import type { ErrorMessage } from '@/types';

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'createBlog', payload: CreateBlog): void;
}>();

const props = withDefaults(defineProps<{
  isSubmitting?: boolean;
  networkId?: string;
}>(), {
  isSubmitting: false,
  networkId: undefined
});

const form = ref({ title: '' });

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

const slugPreview = computed(() => generateSlug(form.value.title));

const isChecking = ref(false);
const slugExists = ref(false);
const isChanged = ref(false);
let checkTimeout: ReturnType<typeof setTimeout> | null = null;
let checkLock: ReturnType<typeof setTimeout> | null = null;
let callback = () => { };
const isValid = computed(() => !isChanged.value && !slugExists.value && !isChecking.value && !!slugPreview.value);

function handleSlugCheck(s: string) {
  slugExists.value = false;
  if (checkTimeout) clearTimeout(checkTimeout);

  if (!s) {
    isChecking.value = false;
    return;
  }

  // debounce before checking
  isChecking.value = true;
  checkTimeout = setTimeout(async () => {
    const networkId = props.networkId as string | undefined;
    if (!networkId) {
      isChecking.value = false;
      return;
    }

    try {
      callback = () => {};
      checkLock = setTimeout(() => {
        console.log('Slug check timeout, assuming not changed');
        isChanged.value = false;
        checkLock = null;
        callback();
        callback = () => {};
      }, 2000);

      await api.get(`/networks/${networkId}/blogs/${s}`);
      clearTimeout(checkLock);
      slugExists.value = true;
      isChanged.value = false;
    } catch (err) {
      // if 404 -> not found
      const status = (err as AxiosError<ErrorMessage>)?.response?.status;
      if (status === 404) {
        slugExists.value = false;
      } else {
        console.error('Error checking slug', err);
        slugExists.value = false;
      }
    } finally {
      isChecking.value = false;
    }
  }, 350);
}

watch(slugPreview, (s) => {
  isChanged.value = true;
  if (checkLock) {
    callback = () => handleSlugCheck(s);
    return;
  }

  handleSlugCheck(s);
});

function handleSubmit() {
  emit('createBlog', { title: form.value.title, summary: '', body: {}, accessLevel: 0 });
}
</script>
