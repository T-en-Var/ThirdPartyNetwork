<template>
  <modal-container
    title="Add Page Block"
    @close="$emit('close')"
  >
    <UserContentForm
      :is-submitting="isSubmitting"
      :input-is-valid="inputIsValid"
      :network-id="customPage.networkId"
      :hide-access-picker="true"
      button-text="Add Page Block"
      @submit="handleSubmit"
      @close="emit('close')"
    >
      <div class="form-field">
        <label
          for="pageText"
          class="form-label"
        >
          Page Block Text
        </label>
        <input
          id="pageText"
          v-model="form.text"
          type="text"
          placeholder="Enter text"
          class="form-input"
          required
        />
      </div>

      <div class="form-field">
        <label
          for="pagePosition"
          class="form-label"
        >
          Page Block Posistion
        </label>
        <input
          id="pagePosition"
          v-model="form.position"
          type="number"
          class="form-input"
          required
        />
        <p class="form-help">You can change all this later as well.</p>
      </div>
    </UserContentForm>
  </modal-container>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { CreatePageBlockDto, CreateUserContentForm, CustomPageDto } from '@/types';
import UserContentForm from '../../UserContentForm.vue';
import { useEventStore } from '@/stores/event';

const events = useEventStore();

const props = defineProps<{
  isSubmitting: boolean;
  customPage: CustomPageDto;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', networkId: string, customPageId: string, pageBlock: CreatePageBlockDto): void;
}>();

const inputIsValid = computed(() => true);

const form = ref<CreatePageBlockDto>({
  text: '',
  position: 1,
  data: {},
  parentPageId: null,
});

function handleSubmit(userContentFrom: CreateUserContentForm) {
  events.listen.pageBlocks.create(() => emit('close'), true);
  emit('submit', userContentFrom.networkId, props.customPage.id, { ...form.value });
}
</script>
