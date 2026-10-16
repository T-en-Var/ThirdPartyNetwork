<template>
  <div class="edit-custom-page-tab">
    <div class="card">
      <LoadingErrorComponent
        :loading="loading"
        :error="error"
        :has-value="!!customPage"
      />

      <div v-if="customPage">
        <h2 class="edit-custom-page-tab__title">Edit Custom Page</h2>
        <UserContentForm
          button-text="Update"
          :is-submitting="isSubmitting"
          :input-is-valid="inputIsValid"
          :network-id="networkId"
          @submit="handleUpdate"
          @close="handleReturn"
        >
          <div class="edit-custom-page-tab__row">
            <div class="form-field">
              <label class="form-label">Name</label>
              <input
                v-model="form.name"
                type="text"
                class="form-input"
              />
            </div>
            <div class="form-field">
              <label class="form-label">Slug</label>
              <input
                v-model="form.slug"
                type="text"
                class="form-input"
              />
            </div>
          </div>
        </UserContentForm>

        <ContentVisibilityPanel
          :network-id="networkId"
          :published-at="customPage.publishedAt"
          :ownership-mode="customPage.ownershipMode"
          :grants="grants"
          :roles="roles"
          :permissions="permissions"
          :network-users="networkUsers"
          :publishing="publishCustomPage.loading.value || unpublishCustomPage.loading.value"
          :setting-ownership="setCustomPageOwnership.loading.value"
          :creating-grant="createCustomPageAccessGrant.loading.value"
          :revoking-grant="revokeCustomPageAccessGrant.loading.value"
          @publish="publishCustomPageExec(networkId, customPage.id)"
          @unpublish="unpublishCustomPageExec(networkId, customPage.id)"
          @set-ownership="(mode) => setOwnershipExec(networkId, customPage!.id, mode)"
          @add-grant="(payload) => createGrantExec(networkId, customPage!.id, payload)"
          @revoke-grant="(grantId) => revokeGrantExec(networkId, customPage!.id, grantId)"
        />
      </div>
    </div>

    <div class="card">
      <div v-if="customPage">
        <div class="edit-custom-page-tab__blocks-header">
          <div class="edit-custom-page-tab__blocks-heading">
            <h2 class="edit-custom-page-tab__blocks-title">Page Blocks</h2>
            <button
              @click="viewMode = 'flat'"
              :class="[
                'edit-custom-page-tab__view-btn',
                viewMode === 'flat' ? 'edit-custom-page-tab__view-btn--active' : '',
              ]"
            >
              Flat View
            </button>
            <button
              @click="viewMode = 'grouped'"
              :class="[
                'edit-custom-page-tab__view-btn',
                viewMode === 'grouped' ? 'edit-custom-page-tab__view-btn--active' : '',
              ]"
            >
              Grouped View
            </button>
          </div>

          <div>
            <button
              @click="showCreatePageBlockModal = true"
              class="btn btn--primary btn--sm"
            >
              Add page block
            </button>
          </div>
        </div>

        <div
          v-if="viewMode === 'flat'"
          class="edit-custom-page-tab__blocks-list"
        >
          <div
            v-for="block in customPage!.blocks || []"
            :key="block.id"
            class="edit-custom-page-tab__block-card"
          >
            <div class="edit-custom-page-tab__block-card-header">
              <h3 class="edit-custom-page-tab__block-card-title">{{ block.text }}</h3>
              <div class="edit-custom-page-tab__block-card-actions">
                <button
                  @click="handleEditPageBlock(block)"
                  class="edit-custom-page-tab__block-card-action"
                >
                  Edit
                </button>
                <button
                  @click="handleDeletePageBlock(block)"
                  class="edit-custom-page-tab__block-card-action edit-custom-page-tab__block-card-action--danger"
                >
                  Delete
                </button>
              </div>
            </div>
            <div class="edit-custom-page-tab__block-card-meta">
              Position: {{ block.position }} | Parent: {{ getParentText(block.parentPageId) }}
            </div>
            <pre class="json-preview">{{ formatJson(block.data) }}</pre>
          </div>
        </div>

        <div
          v-else
          class="edit-custom-page-tab__grouped"
        >
          <div
            v-for="(blocks, parentId) in groupedBlocks"
            :key="parentId"
            class="edit-custom-page-tab__grouped-group"
          >
            <h4 class="edit-custom-page-tab__group-heading">
              Parent:
              <span class="edit-custom-page-tab__group-heading-name">{{
                getParentText(parentId)
              }}</span>
            </h4>
            <div
              v-for="block in blocks"
              :key="block.id"
              class="edit-custom-page-tab__block-card"
            >
              <div class="edit-custom-page-tab__block-card-header">
                <h3 class="edit-custom-page-tab__block-card-title">{{ block.text }}</h3>
                <button
                  @click="handleEditPageBlock(block)"
                  class="edit-custom-page-tab__block-card-action"
                >
                  Open
                </button>
              </div>
              <div class="edit-custom-page-tab__block-card-meta">Position: {{ block.position }}</div>
              <pre class="json-preview">{{ formatJson(block.data) }}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>

    <AddPageBlockModal
      v-if="showCreatePageBlockModal && customPage"
      :custom-page="customPage"
      :is-submitting="isSubmitting"
      @submit="handleCreatePageBlock"
      @close="showCreatePageBlockModal = false"
    />
  </div>
</template>

<script setup lang="ts">
import type {
  CreatePageBlockDto,
  CreateUserContentForm,
  CustomPageDto,
  PageBlockLightDto,
  UpdateCustomPageDto,
} from '@/types';
import { onMounted, ref, computed, watch, type Ref } from 'vue';
import { useRoute } from 'vue-router';
import UserContentForm from '@/components/UserContentForm.vue';
import ContentVisibilityPanel from '@/components/ContentVisibilityPanel.vue';
import { useEventStore } from '@/stores/event';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import AddPageBlockModal from '@/components/modals/usercontent/AddPageBlockModal.vue';
import { useEditableEntity } from '@/composables/useEditableEntity';
import useCustomPages from '@/composables/network/useCustomPages';
import useRoles from '@/composables/useRoles';
import usePermissions from '@/composables/usePermissions';
import useNetworkUsers from '@/composables/useNetworkUsers';

const route = useRoute();
const events = useEventStore();

const emit = defineEmits<{
  (e: 'custom-pages-update', networkId: string, customPageId: string, page: UpdateCustomPageDto): void;
  (e: 'page-blocks-edit', pageBlock: PageBlockLightDto): void;
  (
    e: 'page-blocks-create',
    networkId: string,
    customPageId: string,
    payload: CreatePageBlockDto,
  ): void;
  (e: 'page-blocks-update', networkId: string, customPageId: string, block: PageBlockLightDto): void;
  (e: 'page-blocks-delete', pageBlock: PageBlockLightDto): void;
  (e: 'return', section: string): void;
}>();

const props = defineProps<{
  fetchCustomPage: (customPageId: string) => Promise<Ref<CustomPageDto | null>>;
}>();

const customPageId = computed(() => route.params.customPageId as string);
const networkId = computed(() => route.params.networkId as string);

const isSubmitting = ref(false);

const showCreatePageBlockModal = ref(false);

const inputIsValid = computed(() => true);

const form = ref({ name: '', slug: '' });
const viewMode = ref<'flat' | 'grouped'>('flat');

const {
  entity: customPage,
  loading,
  error,
} = useEditableEntity<CustomPageDto>({
  id: customPageId,
  fetch: props.fetchCustomPage,
  notFoundMessage: 'Custom page not found.',
  onNotFound: () => handleReturn(),
  onLoaded: (loadedCustomPage) => {
    form.value.name = loadedCustomPage.name;
    form.value.slug = loadedCustomPage.slug;
  },
});

const groupedBlocks = computed(() => {
  if (!customPage.value?.blocks) return {};
  return customPage.value.blocks.reduce((acc: Record<string, PageBlockLightDto[]>, block) => {
    const parentId = block.parentPageId || 'none';
    if (!acc[parentId]) acc[parentId] = [];
    acc[parentId].push(block);
    return acc;
  }, {});
});

function handleUpdate(userContentform: CreateUserContentForm) {
  if (!customPage.value) return;

  events.listen.customPages.update(() => {
    isSubmitting.value = false;
  }, true);

  isSubmitting.value = true;
  emit('custom-pages-update', userContentform.networkId, customPage.value.id, {
    ...form.value,
  });
}

function handleReturn() {
  emit('return', 'custom-pages');
}

function formatJson(data: unknown) {
  try {
    return JSON.stringify(data, null, 2);
  } catch {
    return String(data);
  }
}

function handleCreatePageBlock(
  networkId: string,
  customPageId: string,
  pageBlock: CreatePageBlockDto,
) {
  emit('page-blocks-create', networkId, customPageId, pageBlock);
}

function handleEditPageBlock(pageBlock: PageBlockLightDto) {
  emit('page-blocks-edit', pageBlock);
}

function handleDeletePageBlock(pageBlock: PageBlockLightDto) {
  emit('page-blocks-delete', pageBlock);
}

function getParentText(parentId?: string | null) {
  if (!parentId) return 'None';
  const parentBlock = customPage.value?.blocks?.find((block) => block.id === parentId);
  return parentBlock?.text || parentId;
}

// --- Visibility panel --------------------------------------------------------
const customPagesState = useCustomPages();
const { execute: publishCustomPageExec, ...publishCustomPage } = customPagesState.publishCustomPage;
const { execute: unpublishCustomPageExec, ...unpublishCustomPage } =
  customPagesState.unpublishCustomPage;
const { execute: setOwnershipExec, ...setCustomPageOwnership } =
  customPagesState.setCustomPageOwnership;
const { execute: fetchGrants, data: grants } = customPagesState.fetchCustomPageAccessGrants;
const { execute: createGrantExec, ...createCustomPageAccessGrant } =
  customPagesState.createCustomPageAccessGrant;
const { execute: revokeGrantExec, ...revokeCustomPageAccessGrant } =
  customPagesState.revokeCustomPageAccessGrant;

const { execute: rolesExec, data: roles } = useRoles().fetchRoles;
const { execute: permissionsExec, data: permissions } = usePermissions().fetchPermissions;
const { execute: networkUsersExec, data: networkUsers } = useNetworkUsers().fetchNetworkUsers;

watch(
  customPageId,
  async (id) => {
    if (!id) return;
    await fetchGrants(networkId.value, id);
  },
  { immediate: true },
);

onMounted(async () => {
  await rolesExec(networkId.value);
  await permissionsExec();
  await networkUsersExec(networkId.value);
});
</script>
