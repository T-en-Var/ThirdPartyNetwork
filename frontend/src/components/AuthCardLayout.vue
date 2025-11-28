<template>
  <div class="min-h-screen flex flex-col justify-center items-center bg-gray-100 p-4">
    <div class="w-full max-w-md bg-white shadow-xl rounded-xl border border-gray-200 p-8 space-y-6">
      <header class="text-center space-y-2">
        <slot name="logo" />
        <h1 class="text-3xl font-bold text-gray-900">{{ title }}</h1>
      </header>
      
      <main>
        <slot />
      </main>

      <footer v-if="$slots.footer" class="text-center text-sm text-gray-600 border-t pt-4">
        <slot name="footer" />
      </footer>
    </div>

    <div v-if="error" role="alert" aria-live="assertive"
         class="mt-4 px-4 py-3 border border-red-300 bg-red-100 text-red-700 rounded-md max-w-md">
      {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string;
  error: string;
}>();
</script>