<template>
  <div class="form-field">
    <label
      v-if="label"
      class="form-label"
    >
      {{ label }}
    </label>
    <select
      :value="modelValue"
      @change="(e) => updateValue((e.target as HTMLSelectElement).value)"
      class="form-select"
    >
      <option value="">-- Select Access Level --</option>
      <option
        v-for="level in ACCESS_LEVELS"
        :key="level.value"
        :value="level.value"
      >
        {{ level.label }}
      </option>
    </select>
    <p class="form-help">{{ displayLevel.label }} — {{ displayLevel.description }}</p>
    <p
      v-if="description"
      class="form-help"
    >
      {{ description }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { ACCESS_LEVELS, isValidAccessLevel, getAccessLevel } from '@/lib/accessLevels';
import { computed } from 'vue';

interface Props {
  modelValue?: number;
  label?: string;
  description?: string;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: 0,
  label: 'Access Level',
  description: '',
});

const emit = defineEmits<{
  (e: 'update:modelValue', value: number): void;
}>();

const displayLevel = computed(() => {
  const validValue = isValidAccessLevel(props.modelValue) ? props.modelValue : 0;
  return getAccessLevel(validValue);
});

function updateValue(stringValue: string) {
  if (stringValue === '') return;
  const numValue = parseInt(stringValue, 10);
  if (isValidAccessLevel(numValue)) {
    emit('update:modelValue', numValue);
  }
}
</script>
