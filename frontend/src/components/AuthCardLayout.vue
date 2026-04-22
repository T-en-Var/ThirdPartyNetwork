<template>
  <div class="flex min-h-screen flex-col items-center justify-center bg-gray-100 p-4">
    <div class="w-full max-w-md space-y-6 rounded-xl border border-gray-200 bg-white p-8 shadow-xl">
      <header class="space-y-2 text-center">
        <slot name="logo" />
        <h1 class="text-3xl font-bold text-gray-900">{{ title }}</h1>
      </header>

      <main>
        <slot />
      </main>

      <footer
        v-if="$slots.footer"
        class="border-t pt-4 text-center text-sm text-gray-600"
      >
        <slot name="footer" />
      </footer>
    </div>

    <div
      v-if="error"
      role="alert"
      aria-live="assertive"
      class="mt-4 max-w-md rounded-md border border-red-300 bg-red-100 px-4 py-3 text-red-700"
    >
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
