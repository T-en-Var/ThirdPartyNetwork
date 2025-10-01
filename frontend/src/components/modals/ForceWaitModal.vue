<template>
  <modal-container :title="title" @close="$emit('close')" :enable-closing="false">
    <div class="space-y-4">
      <div class="text-sm text-gray-500">
        {{ message }}
      </div>
      <div v-if="progress && progress != 100" class="w-full bg-gray-200 rounded-full h-2 mt-2 overflow-hidden">
        <div class="bg-indigo-600 h-2 transition-all duration-200" :style="{ width: progress + '%' }"></div>
      </div>
      <div v-else class="spinner-border text-blue-500 mx-auto"></div>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import ModalContainer from '@/components/modals/ModalContainer.vue'
import { onMounted, watch } from 'vue'

const props = defineProps({
  title: {
    type: String,
    required: true
  },
  progress: {
    type: Number,
    required: false
  },
  message: {
    type: String,
    default: 'Please hold on while we process the request...'
  }
})

onMounted(() => console.log("Forcing!", props.progress))

watch(props, (props) => console.log(props.progress))

defineEmits(['close', 'confirm'])
</script>

<style scoped>
.spinner-border {
  width: 3rem;
  height: 3rem;
  border: 3px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}
@keyframes spinner-border {
  100% {
    transform: rotate(360deg);
  }
}
</style>
