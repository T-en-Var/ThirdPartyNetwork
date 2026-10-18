<!-- EditUserProxyPage.vue -->
<template>
  <div class="edit-user-proxy-tab">
    <LoadingErrorComponent
      :loading="loading"
      :error="error ?? undefined"
      button-value="Go back"
      @button-action="router.go(-1)"
      :has-value="!!userProxy"
    />

    <div
      v-if="!loading && !error && userProxy"
      class="edit-user-proxy-tab__body"
    >
      <div class="edit-user-proxy-tab__top-row">
        <section class="edit-user-proxy-tab__profile-section">
          <h2 class="edit-user-proxy-tab__section-title">Profile Info</h2>
          <div class="edit-user-proxy-tab__field-grid">
            <div>
              <label class="form-label">Username</label>
              <div class="edit-user-proxy-tab__username-field">
                <span class="edit-user-proxy-tab__username-prefix">@</span>
                <input
                  v-model="form.username"
                  type="text"
                  class="form-input edit-user-proxy-tab__username-input"
                />
              </div>
            </div>

            <div></div>
            <div>
              <label class="form-label">First Name</label>
              <input
                v-model="form.firstName"
                type="text"
                class="form-input"
              />
            </div>
            <div>
              <label class="form-label">Last Name</label>
              <input
                v-model="form.lastName"
                type="text"
                class="form-input"
              />
            </div>

            <div class="edit-user-proxy-tab__field-grid-full">
              <label
                class="edit-user-proxy-tab__checkbox-label"
                :class="{ 'edit-user-proxy-tab__checkbox-label--disabled': userProxy.isDefault }"
              >
                <input
                  type="checkbox"
                  v-model="form.isDefault"
                  :disabled="userProxy.isDefault"
                  class="checkbox"
                />
                <span class="edit-user-proxy-tab__checkbox-text">Set as default proxy</span>
              </label>
              <p class="form-help">
                <span v-if="userProxy.isDefault">
                  This proxy is currently your default. You cannot uncheck it because you must
                  always have one default proxy.
                </span>
                <span v-else>
                  Check this box to make this proxy your default. Only one proxy can be the default
                  at a time.
                </span>
              </p>
            </div>
          </div>
        </section>

        <!-- Profile Image -->
        <section class="edit-user-proxy-tab__image-section">
          <h2 class="edit-user-proxy-tab__section-title">Profile Image</h2>
          <div class="edit-user-proxy-tab__image-section-body">
            <!-- If an image is already set -->
            <div
              v-if="uploadedFile"
              class="edit-user-proxy-tab__image-section-body"
            >
              <CdnFile
                :file="uploadedFile"
                :display-only="true"
                class="avatar__image edit-user-proxy-tab__avatar"
              />
              <div class="edit-user-proxy-tab__image-actions">
                <button
                  type="button"
                  class="edit-user-proxy-tab__link-btn edit-user-proxy-tab__link-btn--danger"
                  @click="removeImage"
                >
                  Remove
                </button>
                <button
                  type="button"
                  class="edit-user-proxy-tab__link-btn"
                  @click="openUploadModal"
                >
                  Change
                </button>
              </div>
            </div>

            <!-- If no image yet -->
            <div
              v-else
              class="edit-user-proxy-tab__image-section-empty"
            >
              <div class="edit-user-proxy-tab__avatar-placeholder">No Image</div>
              <button
                type="button"
                class="btn btn--outline"
                @click="openUploadModal"
              >
                Upload Image
              </button>
            </div>
          </div>

          <!-- Upload Modal -->
          <AddFileModal
            v-if="showUploadModal"
            :networkIds="networks.map((n) => n.id)"
            @close="showUploadModal = false"
            @submit="handleImageUploaded"
            media-type="image"
          />
        </section>
      </div>

      <div class="divider"></div>

      <!-- Email -->
      <section>
        <h2 class="edit-user-proxy-tab__section-title">Email</h2>
        <input
          v-model="form.email"
          type="email"
          autocomplete="off"
          class="form-input"
        />
      </section>

      <div class="divider"></div>

      <!-- Password -->
      <section>
        <h2 class="edit-user-proxy-tab__section-title">Password</h2>

        <!-- Existing password -->
        <div
          v-if="userProxy.hasPassword || form.isDefault"
          class="edit-user-proxy-tab__password-box"
        >
          <label class="edit-user-proxy-tab__checkbox-label">
            <input
              type="checkbox"
              v-model="form.keepPassword"
              :disabled="form.isDefault"
              class="checkbox"
            />
            <span class="edit-user-proxy-tab__checkbox-text"> Keep existing password </span>
          </label>

          <p class="edit-user-proxy-tab__password-note">
            <span v-if="form.isDefault">
              This proxy is set as default and must have a password. You cannot remove it. Leaving
              the field empty will keep the old password.
            </span>
            <span v-else>
              Uncheck to remove the password. If you enter a new one below, it will
              <span class="edit-user-proxy-tab__password-note-emphasis">overwrite</span> the old
              password.
            </span>
          </p>
        </div>

        <!-- Password inputs -->
        <div class="edit-user-proxy-tab__field-grid">
          <div>
            <label class="form-label">New Password</label>
            <input
              v-model="form.password"
              type="password"
              autocomplete="new-password"
              class="form-input"
              :class="{
                'form-input--error': form.isDefault && !userProxy.hasPassword && !form.password,
              }"
            />
            <p
              v-if="form.isDefault && !userProxy.hasPassword && !form.password"
              class="form-error"
            >
              Default proxies must have a password.
            </p>
          </div>

          <div>
            <label class="form-label">Retype Password</label>
            <input
              v-model="form.password_retype"
              type="password"
              autocomplete="off"
              class="form-input"
            />
            <p
              v-if="form.password && form.password !== form.password_retype"
              class="form-error"
            >
              Passwords do not match
            </p>
          </div>
        </div>

        <!-- If no password exists and not default -->
        <div
          v-if="!userProxy.hasPassword && !form.isDefault"
          class="edit-user-proxy-tab__password-hint"
        >
          You currently do not have a password for this proxy. It's recommended to set one.
        </div>
      </section>

      <div class="divider"></div>

      <!-- Networks -->
      <section>
        <h2 class="edit-user-proxy-tab__section-title">Networks</h2>
        <div class="edit-user-proxy-tab__network-grid">
          <NetworkCard
            v-for="network in networks"
            :key="network.id"
            :network="network"
            :can-manage="checkCanManage(network)"
          />
          <div
            v-if="!networks.length"
            class="empty-state edit-user-proxy-tab__network-empty"
          >
            You haven't joined any networks yet. Browse available networks to join.
          </div>
        </div>
      </section>

      <div class="divider"></div>

      <!-- Danger Zone -->
      <section class="edit-user-proxy-tab__danger-zone">
        <h2 class="edit-user-proxy-tab__danger-zone-title">Danger Zone</h2>
        <p class="edit-user-proxy-tab__danger-zone-text">
          Deleting this proxy is permanent and cannot be undone. Please proceed with caution.
        </p>
        <button
          @click="deleteProxy"
          class="btn btn--danger"
        >
          Delete Proxy
        </button>
      </section>

      <!-- Floating Save Bar -->
      <transition name="fade">
        <div
          v-if="hasChanges"
          class="edit-user-proxy-tab__save-bar"
        >
          <div class="edit-user-proxy-tab__save-bar-inner">
            <span class="edit-user-proxy-tab__save-bar-text">You have unsaved changes</span>
            <div class="edit-user-proxy-tab__save-bar-actions">
              <button
                @click="resetForm"
                class="btn btn--secondary btn--sm"
              >
                Undo Changes
              </button>
              <button
                @click="saveChanges"
                class="btn btn--success btn--sm"
              >
                Save Changes
              </button>
            </div>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import CdnFile from '@/components/cdn/CdnFile.vue';
import { useHistoryStore } from '@/stores/history';
import { useRoute, useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import NetworkCard from '@/components/NetworkCard.vue';
import { useAuthStore } from '@/stores/auth';
import AddFileModal from '@/components/modals/usercontent/AddFileModal.vue';
import useUsers from '@/composables/useUsers';
import type { FileDto, NetworkDto, UpdateUserProxyDto, UpdateUserProxyForm, UserProxyDto, UserProxyLightWithNetworksDto } from '@/types';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const { data: user, loading, error, execute: fetchUser } = useUsers().fetchUser;

const uploadedFile = ref<FileDto | null>(null);
const showUploadModal = ref(false);
const userProxy = ref<UserProxyLightWithNetworksDto | null>(null);

const emit = defineEmits<{
  (e: 'updateUserProxy', id: string, payload: UpdateUserProxyDto): void;
  (e: 'deleteUserProxy', id: string): void;
}>();

const userProxyId = computed(() => route.params.userProxyId as string);

const historyStore = useHistoryStore(userProxyId.value);

watch(
  userProxyId,
  async (newId) => {
    if (newId) {
      await handleMounted();
    }
  },
  { immediate: true },
);

async function handleMounted() {
  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error('UserProxy not found');

  await fetchUser(currentUser.user.id);
  if (!user.value) throw new Error('User not found');

  userProxy.value = user.value.userProxies.find((up) => up.id == userProxyId.value) ?? null;
  if (!userProxy.value) throw new Error('UserProxy not found');

  // historyStore's visit map is typed for the full UserProxyDto but only ever reads `.id` at
  // runtime; this tab only has the light variant available.
  historyStore.visit.userProxies(userProxy.value as unknown as UserProxyDto);

  form.value = {
    username: userProxy.value.username ?? undefined,
    firstName: userProxy.value.firstName ?? undefined,
    lastName: userProxy.value.lastName ?? undefined,
    email: userProxy.value.email ?? undefined,
    isDefault: userProxy.value.isDefault,
    keepPassword: true,
    password: '',
    password_retype: '',
  };

  if (userProxy.value.imageFile) form.value.fileLink = { id: userProxy.value.imageFile.id };
  uploadedFile.value = userProxy.value.imageFile ?? null;

  original = JSON.stringify(form.value);
}

const form = ref<UpdateUserProxyForm>({ keepPassword: true, isDefault: false });

let original = '';
const hasChanges = computed(() => JSON.stringify(form.value) !== original);

// NetworkCard expects the full NetworkDto, but this nested relation only gives the light
// variant (missing externalContentKinds/networkAccesses/roles/networkUsers) - NetworkCard only
// reads `.networkUsers?.length` for a member count, which degrades to 0 gracefully when absent.
const networks = computed<NetworkDto[]>(
  () => (userProxy.value?.networkUsers.map((nu) => nu.network) ?? []) as unknown as NetworkDto[],
);

function resetForm() {
  Object.assign(form.value, JSON.parse(original));
}

function saveChanges() {
  if (!userProxy.value) return;
  if (form.value.password && form.value.password !== form.value.password_retype) return;

  const payload: UpdateUserProxyDto = {
    ...form.value,
    username: form.value.username || null,
    firstName: form.value.firstName || null,
    lastName: form.value.lastName || null,
    email: form.value.email || null,
    password: form.value.password || null,
    fileLink: form.value.fileLink || null,
  };

  emit('updateUserProxy', userProxy.value.id, payload);

  setTimeout(async () => {
    await handleMounted();
  }, 100);
}

function deleteProxy() {
  if (!userProxy.value) return;

  emit('deleteUserProxy', userProxy.value.id);
}

const checkCanManage = (network: NetworkDto) => authStore.canI.manageNetwork(network);

function openUploadModal() {
  showUploadModal.value = true;
}

function handleImageUploaded(file: FileDto) {
  form.value.fileLink = { id: file.id };
  uploadedFile.value = file;
  showUploadModal.value = false;
}

function removeImage() {
  form.value.fileLink = undefined;
  uploadedFile.value = null;
}
</script>
