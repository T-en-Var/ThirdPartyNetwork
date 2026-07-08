<template>
  <div :class="wrapperClass">
    <button
      type="button"
      class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
      :disabled="isSubmitting"
      @click="$emit('cancel')"
    >
      {{ cancelLabel }}
    </button>
    <button
      :type="submitType"
      class="rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
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
    /** Whether to render the `border-t border-gray-200` divider above the actions. */
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
  props.bordered
    ? 'flex justify-end space-x-3 border-t border-gray-200 pt-4'
    : 'flex justify-end space-x-3 pt-4',
);
</script>
