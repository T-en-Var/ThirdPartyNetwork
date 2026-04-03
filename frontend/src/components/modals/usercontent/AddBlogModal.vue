<template>
  <modal-container
    title="Create New Blog"
    @close="$emit('close')"
  >
    <UserContentForm
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="networkId"
      :network-ids="networkIds"
      button-text="Add Blog"
      @submit="handleSubmit"
      @selected-network-id="handleNetworkSelect"
    >
      <div>
        <label
          for="title"
          class="mb-2 ml-1 block text-xs font-black uppercase tracking-widest text-slate-500"
        >
          Blog Title
        </label>
        <input
          id="title"
          v-model="title"
          type="text"
          placeholder="My Awesome Journey..."
          class="block w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm shadow-sm outline-none transition-all focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10"
          required
        />
      </div>

      <div class="space-y-3 rounded-xl border border-slate-100 bg-slate-50 p-4">
        <label class="block text-[10px] font-black uppercase tracking-tighter text-slate-400">
          Permanent URL Preview
        </label>

        <div class="flex items-center gap-2 overflow-hidden font-mono text-sm">
          <span class="shrink-0 text-slate-400">.../blogs/</span>
          <span
            :class="[
              'truncate font-bold',
              slugExists ? 'text-amber-600' : 'text-blue-600',
              !slugPreview ? 'italic text-slate-300' : '',
            ]"
          >
            {{ slugPreview || 'your-slug-here' }}
          </span>

          <div class="ml-auto shrink-0">
            <div
              v-if="slugExists"
              class="text-amber-500"
              title="Slug Taken"
            >
              ⚠️
            </div>
            <div
              v-else
              class="text-emerald-500"
              title="Available"
            >
              ✓
            </div>
          </div>
        </div>

        <transition name="fade">
          <div
            v-if="slugExists"
            class="rounded-lg border border-amber-200 bg-amber-100/50 p-2 text-[11px] text-amber-700"
          >
            This URL is already taken. The system will append a unique ID to your link.
          </div>
        </transition>
      </div>
    </UserContentForm>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, type Ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { Blog, CreateBlog } from '@/types/userContent/blog';
import UserContentForm from '../../UserContentForm.vue';
import type { CreateUserContentForm } from '@/types';

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'create-blog', networkId: string, payload: CreateBlog): void;
}>();

const props = defineProps<{
  isSubmitting: boolean;
  networkId?: string;
  networkIds?: string[];
  fetchBlogs: () => Promise<Ref<Blog[] | null>>;
}>();

const title = ref('');
const selectedNetworkId = ref<string | null>(null);

const blogs = ref<Blog[]>([]);

const slugPreview = computed(() => generateSlug(title.value));
const inputIsValid = computed(() => true);

onMounted(async () => {
  if (!!props.networkId && !props.networkIds) handleNetworkSelect(props.networkId);

  const remoteRef = await props.fetchBlogs();

  watch(remoteRef, (newVal) => (blogs.value = newVal ?? []), { immediate: true });
});

const slugExists = computed(() => blogs.value.find(b => b.slug == slugPreview.value))


function generateSlug(title: string) {
  if (!title) return '';
  let str = title.toLowerCase();
  str = str.normalize('NFD').replace(/\p{Diacritic}/gu, '');
  str = str.replace(/[^a-z0-9\s-]/g, '');
  str = str.replace(/[\s-]+/g, '-');
  return str.replace(/^-+|-+$/g, '');
}

function handleNetworkSelect(networkId: string) {
  selectedNetworkId.value = networkId;
}

function handleSubmit(form: CreateUserContentForm) {
  emit('create-blog', form.networkId, {
    title: title.value.trim(),
    summary: '',
    body: {},
    accessLevel: form.accessLevel,
  });
}
</script>
