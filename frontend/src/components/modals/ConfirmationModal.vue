<template>
  <modal-container :title="title" @close="$emit('close')">
    <div class="space-y-4">
      <div class="text-sm text-gray-500">
        {{ message }}
      </div>

      <div class="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
          @click="$emit('close')"
          :disabled="isSubmitting"
        >
          Cancel
        </button>

        <button
          type="button"
          :class="confirmButtonClass"
          :disabled="isSubmitting"
          @click="$emit('confirm')"
        >
          <span v-if="isSubmitting">Processing...</span>
          <span v-else>{{ buttonText }}</span>
        </button>
      </div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import ModalContainer from '@/components/modals/ModalContainer.vue'

const props = defineProps({
  title: {
    type: String,
    required: true
  },
  message: {
    type: String,
    required: true
  },
  buttonText: {
    type: String,
    default: 'Confirm'
  },
  isSubmitting: {
    type: Boolean,
    default: false
  },
  color: {
    type: String,
    default: 'red' // fallback to red if not provided
  }
})

defineEmits(['close', 'confirm'])

const confirmButtonClass = computed(() => {
  const base =
    'rounded-md border px-4 py-2 text-sm font-medium shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2'

  const colorMap: Record<string, string> = {
    red: 'text-white border-transparent bg-red-600 hover:bg-red-700 focus:ring-red-500',
    blue: 'text-white border-transparent bg-blue-600 hover:bg-blue-700 focus:ring-blue-500',
    green: 'text-white border-transparent bg-green-600 hover:bg-green-700 focus:ring-green-500',
    yellow: 'text-white border-transparent bg-yellow-500 hover:bg-yellow-600 focus:ring-yellow-400',
    gray: 'text-white border-transparent bg-gray-600 hover:bg-gray-700 focus:ring-gray-500',
    white: 'text-gray-700 border-gray-300 bg-white hover:bg-gray-50',
  }

  return `${base} ${colorMap[props.color] || colorMap.red}`
})

console.log(confirmButtonClass.value, props.color)
</script>
