<template>
  <header>
    <NavBar />
  </header>

  <main>
    <RouterView />
  </main>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import NavBar from '@/components/NavBar.vue';
</script>

<style scoped>
header {
  position: fixed;
  width: 100vw;
}

main {
  padding-top: 2rem;
  background-color: #f8f8fb;
}
</style>
