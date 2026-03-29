<template>
  <div class="space-y-6">
    <div class="flex justify-start">
      <RouterLink
        :to="`/networks/${networkId}/manage/configurations`"
        class="inline-flex items-center gap-2 rounded-md bg-gray-100 px-3 py-1.5 text-sm text-gray-700 transition hover:bg-gray-200"
      >
        ← Back to Configurations</RouterLink
      >
    </div>

    <div class="rounded-lg bg-white p-6 shadow-md">
      <LoadingErrorComponent
        :loading="loading"
        :error="error ?? undefined"
        button-value="Go back"
        @button-action="router.go(-1)"
        :has-value="!!cfg"
      />

      <div
        v-if="!loading && !error && cfg"
        class="space-y-6"
      >
        <h2 class="mb-4 text-xl font-semibold text-gray-800">Edit Configuration</h2>

        <form
          @submit.prevent="handleUpdate"
          class="space-y-4"
        >
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Key</label>
            <input
              v-model="form.key"
              disabled
              class="block w-full rounded-lg border border-gray-300 bg-gray-100 px-4 py-2 text-sm text-gray-700"
            />
          </div>

          <AccessLevelPicker
            v-model="form.accessLevel"
            label="Access Level"
          />

          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Value</label>
            <div v-if="!editMode">
              <pre class="overflow-x-auto rounded-md border bg-gray-50 p-2 text-xs">{{
                formattedValue
              }}</pre>
              <button
                type="button"
                class="mt-2 rounded-md bg-blue-600 px-3 py-1 text-sm text-white"
                @click="editMode = true"
              >
                Edit JSON
              </button>
            </div>
            <div v-else>
              <JsonEditorVue
                v-model="jsonValue"
                :stringified="false"
                class="h-64 rounded-md border"
              />
              <div class="mt-2 flex justify-end space-x-2">
                <button
                  type="button"
                  class="rounded-md bg-gray-200 px-3 py-1 text-sm text-gray-700"
                  @click="cancelEdit"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  class="rounded-md bg-green-600 px-3 py-1 text-sm text-white"
                  @click="saveJson"
                >
                  Save JSON
                </button>
              </div>
            </div>
          </div>

          <div class="flex justify-end space-x-2">
            <button
              type="button"
              class="rounded-md bg-red-600 px-4 py-2 text-white"
              @click="confirmDelete"
            >
              Delete
            </button>
            <button
              type="submit"
              class="rounded-md bg-blue-600 px-4 py-2 text-white"
            >
              Save
            </button>
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
import useConfigurations from '@/composables/network/useConfigurations';
import type { Configuration, CreateConfiguration } from '@/types';

const router = useRouter();
const route = useRoute();

const networkId = route.params.networkId as string;
const configurationId = computed(() => route.params.configurationId as string);

const configurationsState = useConfigurations();
const {
  data: configuration,
  execute: fetchConfiguration,
  loading,
  error,
} = configurationsState.fetchConfiguration;

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
    form.value.key = configuration.value.key;
    form.value.accessLevel = configuration.value.accessLevel;
    form.value.value = configuration.value.value;
    try {
      jsonValue.value = configuration.value.value ?? {};
    } catch {
      jsonValue.value = {};
    }
  }
}

watch(
  () => configurationId.value,
  async (newId) => {
    if (newId) {
      await load();
    }
  },
  { immediate: true }, // basically onMounted
);

function cancelEdit() {
  editMode.value = false;
}
function saveJson() {
  form.value.value = jsonValue.value;
  editMode.value = false;
}

async function handleUpdate() {
  if (!configuration.value) return;
  emit('updateConfiguration', configuration.value.id, form.value);
}

function confirmDelete() {
  if (!configuration.value) return;
  emit('removeConfiguration', configuration.value);
}

const emit = defineEmits<{
  (e: 'updateConfiguration', cfgId: string, payload: CreateConfiguration): void;
  (e: 'removeConfiguration', payload: Configuration): void;
}>();
</script>
