<template>
  <div class="edit-blog-tab">
    <LoadingErrorComponent
      :loading="loading"
      :error="error ?? undefined"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!blog"
    />

    <div
      v-if="!loading && !error"
      class="edit-blog-tab__card"
    >
      <div class="edit-blog-tab__header">
        <div class="edit-blog-tab__heading">
          <h2 class="edit-blog-tab__title">
            {{ isPreviewing ? 'Preview Mode' : 'Edit Blog Post' }}
          </h2>
          <span :class="['badge', blog?.publishedAt ? 'badge--success' : 'badge--neutral']">
            {{ blog?.publishedAt ? 'Published' : 'Draft' }}
          </span>
        </div>

        <button
          type="button"
          @click="isPreviewing = !isPreviewing"
          class="edit-blog-tab__preview-toggle"
          :class="isPreviewing ? 'edit-blog-tab__preview-toggle--active' : ''"
        >
          <svg
            v-if="!isPreviewing"
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
            <circle
              cx="12"
              cy="12"
              r="3"
            />
          </svg>
          <svg
            v-else
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z" />
            <path d="m15 5 4 4" />
          </svg>

          <span>{{ isPreviewing ? 'Back to Edit' : 'Preview' }}</span>
        </button>
      </div>

      <form
        v-if="!isPreviewing"
        @submit.prevent="handleUpdate"
        class="form"
      >
        <div class="edit-blog-tab__row">
          <div class="form-field edit-blog-tab__field--title">
            <label class="form-label">Title</label>
            <input
              v-model="form.title"
              type="text"
              @keydown.enter.prevent
              placeholder="Enter an engaging title..."
              class="form-input"
              required
            />
          </div>

        </div>

        <div class="form-field">
          <label class="form-label">Summary (SEO)</label>
          <textarea
            v-model="form.summary"
            rows="2"
            placeholder="A short description for search engines and card previews..."
            class="form-textarea"
          ></textarea>
        </div>

        <div class="form-field">
          <label class="form-label">Content</label>
          <RichTextEditor
            ref="editorRef"
            v-model="form.body"
            class="edit-blog-tab__editor"
            @request-image="showImageModal = true"
          />
        </div>

        <ContentVisibilityPanel
          v-if="blog"
          :network-id="networkId"
          :published-at="blog.publishedAt"
          :ownership-mode="blog.ownershipMode"
          :grants="grants"
          :roles="roles"
          :permissions="permissions"
          :network-users="networkUsers"
          :publishing="publishBlog.loading.value || unpublishBlog.loading.value"
          :setting-ownership="setBlogOwnership.loading.value"
          :creating-grant="createBlogAccessGrant.loading.value"
          :revoking-grant="revokeBlogAccessGrant.loading.value"
          @publish="publishBlogExec(networkId, blog.id)"
          @unpublish="unpublishBlogExec(networkId, blog.id)"
          @set-ownership="(mode) => setOwnershipExec(networkId, blog!.id, mode)"
          @add-grant="(payload) => createGrantExec(networkId, blog!.id, payload)"
          @revoke-grant="(grantId) => revokeGrantExec(networkId, blog!.id, grantId)"
        />

        <div class="form__actions">
          <button
            type="button"
            @click="router.back()"
            class="btn btn--outline"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="isSaving"
            class="btn btn--primary"
          >
            <span v-if="isSaving">Saving...</span>
            <span v-else>Save Changes</span>
          </button>
        </div>
      </form>

      <div
        v-if="isPreviewing"
        class="edit-blog-tab__preview"
      >
        <BlogRenderer
          :title="form.title"
          :content="form.body"
          :published-at="blog?.publishedAt ? new Date(blog.publishedAt) : null"
        />
      </div>
    </div>

    <AddFileModal
      v-if="showImageModal"
      :network-id="networkId"
      media-type="image"
      @close="showImageModal = false"
      @submit="handleImageInserted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import RichTextEditor from '@/components/fields/RichTextEditor.vue';
import BlogRenderer from '@/components/fields/BlogRenderer.vue';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import ContentVisibilityPanel from '@/components/ContentVisibilityPanel.vue';
import useBlogs from '@/composables/network/useBlogs';
import useRoles from '@/composables/useRoles';
import usePermissions from '@/composables/usePermissions';
import useNetworkUsers from '@/composables/useNetworkUsers';
import type { NetworkDto, BlogDto, UpdateBlogDto, FileDto } from '@/types';
import { useEditableEntity } from '@/composables/useEditableEntity';
import { useGlobalStore } from '@/stores/global';
import { extractApiErrorMessage } from '@/lib/utils';

const router = useRouter();
const route = useRoute();
const global = useGlobalStore();

const props = defineProps<{
  network: NetworkDto;
  fetchBlog: (blogId: string) => Promise<Ref<BlogDto | null>>;
}>();

const blogId = computed(() => route.params.blogId as string);
const networkId = route.params.networkId as string;

const isPreviewing = ref(false);
const isSaving = ref(false);
const form = ref<Omit<UpdateBlogDto, 'body'> & { body?: object }>({
  title: '',
  summary: '',
  coverImageId: null,
  body: undefined,
});
const showImageModal = ref(false);
const editorRef = ref<InstanceType<typeof RichTextEditor> | null>(null);

const {
  entity: blog,
  loading,
  error,
} = useEditableEntity<BlogDto>({
  id: blogId,
  fetch: props.fetchBlog,
  notFoundMessage: 'Blog not found.',
  onNotFound: () => router.back(),
  onLoaded: (loadedBlog) => {
    form.value.title = loadedBlog.title;
    form.value.summary = loadedBlog.summary ?? '';
    form.value.coverImageId = loadedBlog.coverImage?.id ?? null;
    form.value.body = loadedBlog.body ? JSON.parse(JSON.stringify(loadedBlog.body)) : undefined;
  },
});

const {
  updateBlog,
  publishBlog,
  unpublishBlog,
  setBlogOwnership,
  fetchBlogAccessGrants,
  createBlogAccessGrant,
  revokeBlogAccessGrant,
} = useBlogs();
const { execute: publishBlogExec } = publishBlog;
const { execute: unpublishBlogExec } = unpublishBlog;
const { execute: setOwnershipExec } = setBlogOwnership;
const { execute: fetchGrants, data: grants } = fetchBlogAccessGrants;
const { execute: createGrantExec } = createBlogAccessGrant;
const { execute: revokeGrantExec } = revokeBlogAccessGrant;

const { execute: rolesExec, data: roles } = useRoles().fetchRoles;
const { execute: permissionsExec, data: permissions } = usePermissions().fetchPermissions;
const { execute: networkUsersExec, data: networkUsers } = useNetworkUsers().fetchNetworkUsers;

if (blogId.value) {
  fetchGrants(networkId, blogId.value);
}
rolesExec(networkId);
permissionsExec();
networkUsersExec(networkId);

function handleImageInserted(file: FileDto) {
  // NOTE: viewingUrl is only stable for fully-public files (GetPublicUrl); for a non-public file
  // it's an ephemeral/one-time link, so it will go stale and break the embedded image once
  // consumed or expired. There's currently no "permanent embed" URL for private files - this is
  // a backend limitation, not something fixable from here.
  if (editorRef.value && file.viewingUrl) {
    editorRef.value.insertImage(file.viewingUrl);
  }
  showImageModal.value = false;
}

async function handleUpdate() {
  if (!blog.value) return;
  isSaving.value = true;

  try {
    const payload: UpdateBlogDto = {
      title: form.value.title,
      summary: form.value.summary,
      coverImageId: form.value.coverImageId,
      body: form.value.body ? form.value.body : {},
    };

    await updateBlog.execute(networkId, blog.value.id, payload);

    alert('Blog saved successfully!');
  } catch (err) {
    global.addToast({
      message: extractApiErrorMessage(err, 'Failed to save blog.'),
      type: 'error',
      duration: 5000,
    });
  } finally {
    isSaving.value = false;
  }
}
</script>
