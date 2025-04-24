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
    'rounded-md border border-transparent px-4 py-2 text-sm font-medium text-white shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2'

  const colorMap: Record<string, string> = {
    red: 'bg-red-600 hover:bg-red-700 focus:ring-red-500',
    blue: 'bg-blue-600 hover:bg-blue-700 focus:ring-blue-500',
    green: 'bg-green-600 hover:bg-green-700 focus:ring-green-500',
    yellow: 'bg-yellow-500 hover:bg-yellow-600 focus:ring-yellow-400',
    gray: 'bg-gray-600 hover:bg-gray-700 focus:ring-gray-500'
  }

  return `${base} ${colorMap[props.color] || colorMap.red}`
})
</script>
