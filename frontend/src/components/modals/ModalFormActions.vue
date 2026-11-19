<template>
  <div :class="wrapperClass">
    <button
      type="button"
      class="btn btn--outline"
      :disabled="isSubmitting"
      @click="$emit('cancel')"
    >
      {{ cancelLabel }}
    </button>
    <button
      :type="submitType"
      class="btn btn--primary"
      :disabled="isSubmitting || disableSubmit"
      @click="submitType === 'button' && $emit('submit')"
    >
      <span v-if="isSubmitting">{{ submittingLabel }}</span>
      <span v-else>{{ submitLabel }}</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

/**
 * Shared Cancel/Submit footer for the network modal family.
 *
 * For modals that wrap their fields in a native `<form @submit.prevent="...">`,
 * keep `submit-type="submit"` (the default) so the browser's native form
 * submission still drives the handler. For modals without a `<form>` element
 * (e.g. "manage" modals with a tabbed layout), pass `submit-type="button"` and
 * listen for the `submit` event instead.
 */
const props = withDefaults(
  defineProps<{
    isSubmitting: boolean;
    submitLabel: string;
    submittingLabel: string;
    cancelLabel?: string;
    disableSubmit?: boolean;
    submitType?: 'button' | 'submit';
    /** Whether to render the divider above the actions. */
    bordered?: boolean;
  }>(),
  {
    cancelLabel: 'Cancel',
    disableSubmit: false,
    submitType: 'submit',
    bordered: true,
  },
);

defineEmits<{
  (e: 'cancel'): void;
  (e: 'submit'): void;
}>();

const wrapperClass = computed(() =>
  props.bordered ? 'form__actions form__actions--bordered' : 'form__actions',
);
</script>
