<template>
  <modal-container
    title="Add Access Requirement"
    @close="$emit('close')"
  >
    <form
      @submit.prevent="handleSubmit"
      class="space-y-4"
    >
      <div class="mb-6">
        <label
          for="access"
          class="mb-2 block text-sm font-semibold text-gray-800"
        >
          Select Access Requirement
        </label>
        <div class="relative">
          <select
            id="access"
            v-model="accessId"
            class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm transition-all focus:border-blue-600 focus:ring focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-500"
            required
          >
            <option
              value=""
              disabled
              selected
            >
              Select an access requirement
            </option>
            <option
              v-for="access in availableAccesses"
              :key="access.id"
              :value="access.id"
            >
              {{ access.name }}
            </option>
          </select>
        </div>
        <loading-error-component
          :loading="accessesState.loading.value"
          :error="accessesState.error.value"
          :has-value="!!accessesState.data.value"
        />
      </div>

      <!-- Access description if one is selected -->
      <div
        v-if="selectedAccessDescription"
        class="rounded-md bg-gray-50 p-3"
      >
        <h4 class="text-sm font-medium text-gray-700">Access Description:</h4>
        <p class="text-sm text-gray-600">{{ selectedAccessDescription }}</p>
      </div>

      <div class="flex items-center">
        <input
          id="isRequired"
          v-model="form.isRequired"
          type="checkbox"
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
        />
        <label
          for="isRequired"
          class="ml-2 block text-sm text-gray-700"
        >
          Access is required
        </label>
      </div>

      <modal-form-actions
        :is-submitting="isSubmitting"
        :disable-submit="!form.access"
        submit-label="Add Access"
        submitting-label="Adding..."
        @cancel="$emit('close')"
      />
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ModalFormActions from '@/components/modals/ModalFormActions.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import type { Network, NetworkAccessCreate } from '@/types';
import useAccesses from '@/composables/useAccesses';

const accessesState = useAccesses().fetchAccesses;

const props = withDefaults(
  defineProps<{
    network: Network;
    isSubmitting?: boolean;
  }>(),
  {
    isSubmitting: false,
  },
);

const accessId = ref('');
watch(accessId, (newId) => {
  if (newId) {
    form.value.access = accessesState.data.value!.find((x) => x.id == newId);
  }
});

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'add-access', networkAccess: NetworkAccessCreate): void;
}>();

const form = ref<NetworkAccessCreate>({
  access: undefined,
  networkId: '',
  isRequired: false,
});

const availableAccesses = computed(() => {
  if (!accessesState.data.value) return [];
  const networkAccessIds = props.network.networkAccesses.map((na) => na.access.id);
  return accessesState.data.value.filter((access) => !networkAccessIds.includes(access.id));
});

const selectedAccessDescription = computed(() => {
  if (!accessesState.data.value) return '';
  if (!form.value.access) return '';

  const access = accessesState.data.value.find((a) => a.id === form.value.access?.id);
  return access?.description || '';
});

onMounted(async () => {
  await accessesState.execute();
});

function handleSubmit() {
  emit('add-access', {
    networkId: props.network.id,
    isRequired: form.value.isRequired,
    access: form.value.access,
  });
}
</script>
