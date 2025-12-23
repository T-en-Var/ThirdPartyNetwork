<template>
  <div>
    <label v-if="label" class="block text-sm font-semibold text-gray-800 mb-2">
      {{ label }}
    </label>
    <select
      :value="modelValue"
      @change="(e) => updateValue((e.target as HTMLSelectElement).value)"
      class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
    >
      <option value="">-- Select Access Level --</option>
      <option v-for="level in ACCESS_LEVELS" :key="level.value" :value="level.value">
        {{ level.label }}
      </option>
    </select>
    <p class="mt-1 text-xs text-gray-500">{{ displayLevel.label }} — {{ displayLevel.description }}</p>
    <p v-if="description" class="mt-1 text-xs text-gray-500">{{ description }}</p>
  </div>
</template>

<script setup lang="ts">
import { ACCESS_LEVELS, isValidAccessLevel, getAccessLevel } from '@/lib/accessLevels'
import { computed } from 'vue'

interface Props {
  modelValue?: number
  label?: string
  description?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: 0,
  label: 'Access Level',
  description: ''
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: number): void
}>()

const displayLevel = computed(() => {
  const validValue = isValidAccessLevel(props.modelValue) ? props.modelValue : 0
  return getAccessLevel(validValue)
})

function updateValue(stringValue: string) {
  if (stringValue === '') return
  const numValue = parseInt(stringValue, 10)
  if (isValidAccessLevel(numValue)) {
    emit('update:modelValue', numValue)
  }
}
</script>
