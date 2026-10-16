<template>
  <div class="edit-configuration-tab">
    <div class="edit-configuration-tab__back-wrap">
      <div
        @click="handleReturn"
        class="back-link"
      >
        ← Back to Configurations
      </div>
    </div>

    <div class="card">
      <LoadingErrorComponent
        :loading="loading"
        :error="error ?? undefined"
        button-value="Go back"
        @button-action="router.go(-1)"
        :has-value="!!cfg"
      />

      <div
        v-if="!loading && !error && cfg"
        class="edit-configuration-tab__body"
      >
        <h2 class="edit-configuration-tab__title">Edit Configuration</h2>

        <form
          @submit.prevent="handleUpdate"
          class="form"
        >
          <div class="form-field">
            <label class="form-label">Key</label>
            <input
              v-model="form.key"
              disabled
              class="form-input"
            />
          </div>

          <div class="form-field">
            <label class="form-label">Value</label>
            <div v-if="!editMode">
              <pre class="json-preview">{{ formattedValue }}</pre>
              <button
                type="button"
                class="btn btn--primary btn--sm"
                @click="editMode = true"
              >
                Edit JSON
              </button>
            </div>
            <div v-else>
              <JsonEditorVue
                v-model="jsonValue"
                :stringified="false"
                class="edit-configuration-tab__json-editor"
              />
              <div class="edit-configuration-tab__json-actions">
                <button
                  type="button"
                  class="btn btn--outline btn--sm"
                  @click="cancelEdit"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  class="btn btn--success btn--sm"
                  @click="saveJson"
                >
                  Save JSON
                </button>
              </div>
            </div>
          </div>

          <div class="form__actions">
            <button
              type="button"
              class="btn btn--danger"
              @click="confirmDelete"
            >
              Delete
            </button>
            <button
              type="submit"
              class="btn btn--primary"
            >
              Save
            </button>
          </div>
        </form>

        <ContentVisibilityPanel
          :network-id="networkId"
          :published-at="cfg.publishedAt"
          :ownership-mode="cfg.ownershipMode"
          :grants="grants"
          :roles="roles"
          :permissions="permissions"
          :network-users="networkUsers"
          :publishing="publishConfiguration.loading.value || unpublishConfiguration.loading.value"
          :setting-ownership="setConfigurationOwnership.loading.value"
          :creating-grant="createConfigurationAccessGrant.loading.value"
          :revoking-grant="revokeConfigurationAccessGrant.loading.value"
          @publish="publishExec(networkId, cfg!.id)"
          @unpublish="unpublishExec(networkId, cfg!.id)"
          @set-ownership="(mode) => setOwnershipExec(networkId, cfg!.id, mode)"
          @add-grant="(payload) => createGrantExec(networkId, cfg!.id, payload)"
          @revoke-grant="(grantId) => revokeGrantExec(networkId, cfg!.id, grantId)"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import JsonEditorVue from 'json-editor-vue';
import ContentVisibilityPanel from '@/components/ContentVisibilityPanel.vue';
import { onMounted, ref, computed, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { ConfigurationDto, UpdateConfigurationDto } from '@/types';
import { useEventStore } from '@/stores/event';
import { useEditableEntity } from '@/composables/useEditableEntity';
import useConfigurations from '@/composables/network/useConfigurations';
import useRoles from '@/composables/useRoles';
import usePermissions from '@/composables/usePermissions';
import useNetworkUsers from '@/composables/useNetworkUsers';

const router = useRouter();
const route = useRoute();
const events = useEventStore();

const emit = defineEmits<{
  (
    e: 'configurations-update',
    networkId: string,
    cfgId: string,
    payload: UpdateConfigurationDto,
  ): void;
  (e: 'configurations-delete', payload: ConfigurationDto): void;
  (e: 'return', section: string): void;
}>();

const props = defineProps<{
  fetchConfiguration: (configurationId: string) => Promise<Ref<ConfigurationDto | null>>;
}>();

const networkId = route.params.networkId as string;
const configurationId = computed(() => route.params.configurationId as string);

const form = ref<UpdateConfigurationDto>({ key: '', value: {} });
const editMode = ref(false);
const jsonValue = ref<object>({});

const {
  entity: configuration,
  loading,
  error,
} = useEditableEntity<ConfigurationDto>({
  id: configurationId,
  fetch: props.fetchConfiguration,
  notFoundMessage: 'Configuration not found.',
  onNotFound: () => handleReturn(),
  onLoaded: (loadedConfiguration) => {
    form.value.key = loadedConfiguration.key;
    form.value.value = loadedConfiguration.value;
    try {
      jsonValue.value = (loadedConfiguration.value as object) ?? {};
    } catch {
      jsonValue.value = {};
    }
  },
});

const {
  execute: publishExec,
  ...publishConfiguration
} = useConfigurations().publishConfiguration;
const {
  execute: unpublishExec,
  ...unpublishConfiguration
} = useConfigurations().unpublishConfiguration;
const {
  execute: setOwnershipExec,
  ...setConfigurationOwnership
} = useConfigurations().setConfigurationOwnership;
const { execute: fetchGrants, data: grants } = useConfigurations().fetchConfigurationAccessGrants;
const {
  execute: createGrantExec,
  ...createConfigurationAccessGrant
} = useConfigurations().createConfigurationAccessGrant;
const {
  execute: revokeGrantExec,
  ...revokeConfigurationAccessGrant
} = useConfigurations().revokeConfigurationAccessGrant;

const { execute: rolesExec, data: roles } = useRoles().fetchRoles;
const { execute: permissionsExec, data: permissions } = usePermissions().fetchPermissions;
const { execute: networkUsersExec, data: networkUsers } = useNetworkUsers().fetchNetworkUsers;

onMounted(async () => {
  await fetchGrants(networkId, configurationId.value);
  await rolesExec(networkId);
  await permissionsExec();
  await networkUsersExec(networkId);
});

const cfg = computed(() => configuration.value);

const formattedValue = computed(() => {
  return JSON.stringify(form.value.value ?? {}, null, 2);
});

function cancelEdit() {
  editMode.value = false;
}
function saveJson() {
  form.value.value = jsonValue.value;
  editMode.value = false;
}

function handleReturn() {
  emit('return', 'configurations');
}

async function handleUpdate() {
  if (!configuration.value) return;

  events.listen.configurations.update(() => {
    handleReturn();
  }, true);

  emit('configurations-update', networkId, configuration.value.id, form.value);
}

function confirmDelete() {
  if (!configuration.value) return;
  emit('configurations-delete', configuration.value);
}
</script>
