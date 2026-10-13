<template>
  <div :class="containerClass">
    <div
      v-if="items.length === 0"
      :class="emptyClass"
    >
      {{ emptyMessage }}
    </div>

    <template v-if="variant === 'card'">
      <label
        v-for="item in items"
        :key="item.id"
        class="checkbox-list__card-item"
      >
        <input
          type="checkbox"
          :value="item.id"
          v-model="selected"
          class="checkbox"
        />
        <div class="checkbox-list__card-item-content">
          <span class="checkbox-list__card-item-name">{{ item.name }}</span>
          <span
            v-if="showId"
            class="checkbox-list__card-item-id"
            >{{ item.id }}</span
          >
        </div>
      </label>
    </template>

    <template v-else>
      <div
        v-for="item in items"
        :key="item.id"
        :class="itemClass"
      >
        <input
          :id="`${idPrefix}-${item.id}`"
          type="checkbox"
          :value="item.id"
          v-model="selected"
          class="checkbox"
        />
        <label
          :for="`${idPrefix}-${item.id}`"
          class="checkbox-list__item-label"
        >
          {{ item.name }}
          <span
            v-if="showId"
            class="checkbox-list__item-id"
            >({{ item.id }})</span
          >
        </label>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts" generic="T extends { id: string; name: string }">
import { computed } from 'vue';

/**
 * Generic checkbox list for id/name-shaped items (permissions, roles, ...),
 * matching the two checkbox-list presentations already used across the
 * network modals:
 * - `variant="list"` (default): bordered container, one row per item with a
 *   separate `<label for>` (used by AddRoleModal/AddUserModal/EditUserModal).
 * - `variant="card"`: each item is a hoverable card wrapping its own
 *   checkbox (used by EditRoleModal).
 *
 * Visual details that differ per usage (container/item classes, empty-state
 * copy, whether to show the raw id) are left as props so call sites keep
 * their exact existing appearance.
 */
const props = withDefaults(
  defineProps<{
    items: T[];
    modelValue: string[];
    idPrefix?: string;
    emptyMessage?: string;
    showId?: boolean;
    variant?: 'list' | 'card';
    containerClass?: string;
    itemClass?: string;
    emptyClass?: string;
  }>(),
  {
    idPrefix: 'item',
    emptyMessage: 'No items available',
    showId: false,
    variant: 'list',
    containerClass: 'checkbox-list',
    itemClass: 'checkbox-list__item',
    emptyClass: 'checkbox-list__empty',
  },
);

const emit = defineEmits<{
  (e: 'update:modelValue', value: string[]): void;
}>();

const selected = computed<string[]>({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
});
</script>
