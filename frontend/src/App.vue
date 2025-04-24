<template>
  <header>
    <NavBar />
  </header>

  <main>
    <RouterView />
    <FetchingToast />
  </main>
</template>

<script setup lang="ts">
import FetchingToast from '@/components/toasts/FetchingToast.vue';
import { RouterView } from 'vue-router'
import NavBar from '@/components/NavBar.vue';
</script>

<style>
header {
  position: fixed;
  width: 100vw;
}

main {
  padding-top: 2rem;
  height: 100%;
  overflow: scroll;
}

#app {
  height: 100vh;
  background-color: #f8f8fb;
  overflow: hidden;
}
</style>
