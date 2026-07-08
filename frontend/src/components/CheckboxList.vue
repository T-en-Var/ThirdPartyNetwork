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
        class="group flex cursor-pointer items-center rounded-md px-4 py-3 transition-colors hover:bg-slate-50"
      >
        <input
          type="checkbox"
          :value="item.id"
          v-model="selected"
          class="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
        />
        <div class="ml-3">
          <span class="block text-sm font-semibold text-slate-700 group-hover:text-blue-600">{{
            item.name
          }}</span>
          <span
            v-if="showId"
            class="block font-mono text-[10px] uppercase tracking-tighter text-slate-400"
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
          class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
        />
        <label
          :for="`${idPrefix}-${item.id}`"
          class="ml-2 block text-sm text-gray-700"
        >
          {{ item.name }}
          <span
            v-if="showId"
            class="ml-1 text-xs text-gray-500"
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
    containerClass: 'mt-2 max-h-48 overflow-y-auto rounded-md border p-2',
    itemClass: 'flex items-center py-1',
    emptyClass: 'text-sm text-gray-500',
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
