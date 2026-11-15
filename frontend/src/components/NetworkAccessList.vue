<template>
  <div
    v-if="loading || networkAccesses.length === 0"
    class="network-access-list__status"
  >
    <LoadingSpinner v-if="loading" />
    <p
      v-else-if="networkAccesses.length === 0"
      class="network-access-list__empty-message"
    >
      This network does not require any special permissions.
    </p>
  </div>

  <div v-else>
    <h3 class="network-access-list__title">Required Data Access</h3>
    <p class="network-access-list__subtitle">Update your consent for the following data accesses:</p>

    <div class="network-access-list__items">
      <div
        v-for="networkAccess in networkAccesses"
        :key="networkAccess.access.id"
        class="network-access-list__item"
      >
        <div
          class="network-access-list__item-row"
          v-if="internalAccesses[networkAccess.access.id]"
        >
          <div class="network-access-list__checkbox-wrap">
            <input
              :id="networkAccess.access.id"
              v-model="internalAccesses[networkAccess.access.id].value"
              type="checkbox"
              class="checkbox"
              :disabled="networkAccess.isRequired && isAlreadyAccepted(networkAccess.access.id)"
              @change="handleAccessChange"
              :aria-required="networkAccess.isRequired ? 'true' : 'false'"
            />
          </div>
          <div class="network-access-list__item-copy">
            <label
              :for="networkAccess.access.id"
              class="form-label"
            >
              {{ networkAccess.access.name }}
              <span
                v-if="networkAccess.isRequired"
                class="network-access-list__required-tag"
                >(Required)</span
              >
              <span
                v-if="isAlreadyAccepted(networkAccess.access.id)"
                class="network-access-list__accepted-tag"
              >
                (Accepted)</span
              >
            </label>
            <p class="network-access-list__item-description">{{ networkAccess.access.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { NetworkAccessDto, NetworkUserAccessDto } from '@/types';

interface UserAccessState {
  value: boolean;
  userChecked: boolean;
}

const props = defineProps<{
  networkAccesses: NetworkAccessDto[];
  initialUserAccesses: Record<string, UserAccessState>;
  networkUserAccesses: NetworkUserAccessDto[] | undefined;
  loading: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'access-change']);

const internalAccesses = ref<Record<string, UserAccessState>>({});

watch(
  () => props.initialUserAccesses,
  (newAccesses) => {
    internalAccesses.value = { ...newAccesses };
  },
  { deep: true, immediate: true },
);

const isAlreadyAccepted = (accessId: string) => {
  return props.networkUserAccesses?.some((nua) => nua.accessId === accessId && nua.isAccepted);
};

const handleAccessChange = (e: Event) => {
  const currentElementId = (e.target as HTMLInputElement).id;
  const isChecked = (e.target as HTMLInputElement).checked;

  const accessDefinition = props.networkAccesses.find((a) => a.access.id === currentElementId);

  if (accessDefinition) {
    emit(
      'access-change',
      currentElementId,
      accessDefinition.isRequired ? true : isChecked,
      accessDefinition.isRequired,
    );
  }
};

defineExpose({
  userAccesses: internalAccesses,
});
</script>
