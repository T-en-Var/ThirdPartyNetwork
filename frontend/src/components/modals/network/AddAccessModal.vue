<template>
  <modal-container
    title="Add Access Requirement"
    @close="$emit('close')"
  >
    <form
      @submit.prevent="handleSubmit"
      class="form"
    >
      <div class="form-field">
        <label
          for="access"
          class="form-label"
        >
          Select Access Requirement
        </label>
        <select
          id="access"
          v-model="accessId"
          class="form-select"
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
        <loading-error-component
          :loading="accessesState.loading.value"
          :error="accessesState.error.value"
          :has-value="!!accessesState.data.value"
        />
      </div>

      <!-- Access description if one is selected -->
      <div
        v-if="selectedAccessDescription"
        class="add-access-modal__description"
      >
        <h4 class="add-access-modal__description-title">Access Description:</h4>
        <p class="add-access-modal__description-text">{{ selectedAccessDescription }}</p>
      </div>

      <div class="add-access-modal__required-row">
        <input
          id="isRequired"
          v-model="form.isRequired"
          type="checkbox"
          class="checkbox"
        />
        <label
          for="isRequired"
          class="form-label"
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
import type { CreateNetworkAccessForm, NetworkDto } from '@/types';
import useAccesses from '@/composables/useAccesses';

const accessesState = useAccesses().fetchAccesses;

const props = withDefaults(
  defineProps<{
    network: NetworkDto;
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
  (e: 'add-access', networkAccess: CreateNetworkAccessForm): void;
}>();

const form = ref<CreateNetworkAccessForm>({
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
