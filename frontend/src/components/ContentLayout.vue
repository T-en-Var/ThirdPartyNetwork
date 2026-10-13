<template>
  <div class="content-layout">
    <div class="card">
      <header class="card__header">
        <h1 class="card__title">{{ title }}</h1>
        <slot name="header-actions" />
      </header>
      <main class="card__body">
        <slot />
      </main>
      <footer
        v-if="$slots.footer"
        class="card__footer"
      >
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
