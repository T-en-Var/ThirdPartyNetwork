<template>
  <div class="searchable-select">
    <!-- Input -->
    <input
      type="text"
      v-model="search"
      @focus="open = true"
      @blur="closeDropdown"
      placeholder="Search..."
      class="form-input"
    />

    <!-- Dropdown -->
    <ul
      v-if="open && filteredOptions.length"
      class="dropdown__menu searchable-select__menu"
    >
      <li
        v-if="nullable"
        @mousedown.prevent="selectOption(null)"
        class="dropdown__item"
      >
        None
      </li>
      <li
        v-for="option in filteredOptions"
        :key="option.id"
        @mousedown.prevent="selectOption(option.id)"
        class="dropdown__item"
      >
        {{ option.text }} ({{ option.id }})
      </li>
    </ul>

    <!-- No results -->
    <div
      v-if="open && !filteredOptions.length"
      class="searchable-select__empty"
    >
      No results
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

interface Option {
  id: string;
  text: string;
}

const props = defineProps<{
  modelValue?: string | null;
  options: Option[];
  label?: string;
  nullable?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | null): void;
}>();

const search = ref('');
const open = ref(false);

const filteredOptions = computed(() =>
  props.options.filter(
    (opt) =>
      opt.text.toLowerCase().includes(search.value.toLowerCase()) ||
      opt.id.toLowerCase().includes(search.value.toLowerCase()),
  ),
);

function selectOption(optionId: string | null) {
  emit('update:modelValue', optionId);
  const match = props.options.find((o) => o.id === optionId);
  search.value = match ? match.text : '';
  open.value = false;
}

function closeDropdown() {
  setTimeout(() => {
    open.value = false;
  }, 100);
}

watch(
  () => props.modelValue,
  (newVal) => {
    const match = props.options.find((o) => o.id === newVal);
    search.value = match ? match.text : '';
  },
  { immediate: true },
);
</script>
