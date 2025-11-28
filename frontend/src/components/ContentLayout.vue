<template>
  <div class="container mx-auto px-4 py-8 mt-8 max-w-4xl">
    <div class="bg-white shadow-xl rounded-xl overflow-hidden border border-gray-200">
      <header class="bg-gray-50 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h1 class="text-2xl font-bold text-gray-800">{{ title }}</h1>
        <slot name="header-actions" />
      </header>
      <main class="p-6">
        <slot />
      </main>
      <footer v-if="$slots.footer" class="px-6 py-4 border-t border-gray-200">
        <slot name="footer" />
      </footer>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string;
}>();
</script>
