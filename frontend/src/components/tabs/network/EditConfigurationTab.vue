<template>
  <div class="space-y-6">
    <div class="flex justify-start">
      <RouterLink :to="`/networks/${networkId}/manage/configurations`"
        class="inline-flex items-center gap-2 px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md transition">
        ← Back to Configurations</RouterLink>
    </div>

    <div class="bg-white shadow-md rounded-lg p-6">
      <LoadingErrorComponent :loading="loading" :error="error" button-value="Go back"
        @button-action="router.go(-1)" />

      <div v-if="!loading && !error && cfg" class="space-y-6">
        <h2 class="text-xl font-semibold text-gray-800 mb-4">Edit Configuration</h2>

        <form @submit.prevent="handleUpdate" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Key</label>
            <input v-model="form.key" disabled
              class="block w-full rounded-lg border border-gray-300 bg-gray-100 px-4 py-2 text-sm text-gray-700" />
          </div>

          <AccessLevelPicker v-model="form.accessLevel" label="Access Level" />

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Value</label>
            <div v-if="!editMode">
              <pre
                class="bg-gray-50 border rounded-md p-2 text-xs overflow-x-auto">{{ formattedValue }}</pre>
              <button type="button"
                class="mt-2 px-3 py-1 bg-blue-600 text-white rounded-md text-sm"
                @click="editMode = true">Edit JSON</button>
            </div>
            <div v-else>
              <JsonEditorVue v-model="jsonValue" :stringified="false"
                class="h-64 border rounded-md" />
              <div class="flex justify-end mt-2 space-x-2">
                <button type="button"
                  class="px-3 py-1 bg-gray-200 text-gray-700 rounded-md text-sm"
                  @click="cancelEdit">Cancel</button>
                <button type="button"
                  class="px-3 py-1 bg-green-600 text-white rounded-md text-sm"
                  @click="saveJson">Save JSON</button>
              </div>
            </div>
          </div>

          <div class="flex justify-end space-x-2">
            <button type="button" class="px-4 py-2 bg-red-600 text-white rounded-md"
              @click="confirmDelete">Delete</button>
            <button type="submit"
              class="px-4 py-2 bg-blue-600 text-white rounded-md">Save</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import JsonEditorVue from 'json-editor-vue';
import AccessLevelPicker from '@/components/fields/AccessLevelPicker.vue';
import { ref, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import useConfigurations from '@/composables/useConfigurations';
import type { Configuration, CreateConfiguration } from '@/types';
import { useHistoryStore } from '@/stores/history';

const router = useRouter();
const route = useRoute();
const historyStore = useHistoryStore();

const networkId = route.params.networkId as string;
const configurationId = computed(() => route.params.configurationId as string);

const { configuration, fetchConfiguration, loading, error } = useConfigurations();

const form = ref<CreateConfiguration>({ key: '', accessLevel: 0, value: {} });
const editMode = ref(false);
const jsonValue = ref<object>({});

const cfg = computed(() => configuration.value);

const formattedValue = computed(() => {
  return JSON.stringify(form.value.value ?? {}, null, 2);
});

async function load() {
  await fetchConfiguration(networkId, configurationId.value);
  if (configuration.value) {
    historyStore.configurationVisit(configuration.value);
    form.value.key = configuration.value.key;
    form.value.accessLevel = configuration.value.accessLevel;
    form.value.value = configuration.value.value;
    try { jsonValue.value = configuration.value.value ?? {}; } catch { jsonValue.value = {}; }
  }
}

watch(
  () => configurationId.value,
  async (newId) => {
    if (newId) {
      await load();
    }
  },
  { immediate: true }  // basically onMounted
);

function cancelEdit() { editMode.value = false; }
function saveJson() { form.value.value = jsonValue.value; editMode.value = false; }

async function handleUpdate() {
  if (!configuration.value) return;
  emit('updateConfiguration', configuration.value.id, form.value);
}

function confirmDelete() {
  if (!configuration.value) return;
  emit('removeConfiguration', configuration.value);
}

const emit = defineEmits<{
  (e: 'updateConfiguration', cfgId: string, cfg: CreateConfiguration): void;
  (e: 'removeConfiguration', cfg: Configuration): void;
}>();
</script>
