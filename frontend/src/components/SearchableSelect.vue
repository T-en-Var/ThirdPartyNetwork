<template>
  <div class="relative w-full">
    <!-- Input -->
    <input
      type="text"
      v-model="search"
      @focus="open = true"
      @blur="closeDropdown"
      placeholder="Search..."
      class="block w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-600 focus:ring focus:ring-blue-100"
    />

    <!-- Dropdown -->
    <ul
      v-if="open && filteredOptions.length"
      class="absolute z-20 mt-1 max-h-48 w-full overflow-auto rounded-lg border border-gray-200 bg-white shadow-lg"
    >
      <li
        v-if="nullable"
        @mousedown.prevent="selectOption(null)"
        class="px-4 py-2 text-sm text-gray-700 cursor-pointer hover:bg-blue-100"
      >
        None
      </li>
      <li
        v-for="option in filteredOptions"
        :key="option.id"
        @mousedown.prevent="selectOption(option.id)"
        class="px-4 py-2 text-sm text-gray-700 cursor-pointer hover:bg-blue-100"
      >
        {{ option.text }} ({{ option.id }})
      </li>
    </ul>

    <!-- No results -->
    <div
      v-if="open && !filteredOptions.length"
      class="absolute z-20 mt-1 w-full rounded-lg border border-gray-200 bg-white px-4 py-2 text-sm text-gray-400"
    >
      No results
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue"

interface Option {
  id: string
  text: string
}

const props = defineProps<{
  modelValue?: string | null
  options: Option[]
  label?: string
  nullable?: boolean
}>()

const emit = defineEmits<{
  (e: "update:modelValue", value: string | null): void
}>()

const search = ref("")
const open = ref(false)

// Compute filtered options
const filteredOptions = computed(() =>
  props.options.filter(opt =>
    opt.text.toLowerCase().includes(search.value.toLowerCase()) ||
    opt.id.toLowerCase().includes(search.value.toLowerCase())
  )
)

function selectOption(optionId: string | null) {
  emit("update:modelValue", optionId)
  const match = props.options.find(o => o.id === optionId)
  search.value = match ? match.text : ""
  open.value = false
}

function closeDropdown() {
  setTimeout(() => {
    open.value = false
  }, 100)
}

// Keep input in sync if external modelValue changes
watch(
  () => props.modelValue,
  (newVal) => {
    const match = props.options.find(o => o.id === newVal)
    search.value = match ? match.text : ""
  },
  { immediate: true }
)
</script>
