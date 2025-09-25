<template>
  <div
    class="relative min-h-screen w-full flex flex-col items-center justify-center p-8 bg-gradient-to-b from-purple-100 via-purple-50 to-white overflow-hidden">

    <!-- Stars -->
    <div v-motion :initial="{ opacity: 0, x: 50 }" :enter="{ opacity: 1, x: 0 }" class="absolute top-32 right-8 w-10 h-10 bg-black clip-star"></div>
    <div v-motion :initial="{ opacity: 0, x: -50 }" :enter="{ opacity: 1, x: 0 }" class="absolute bottom-16 left-8 w-10 h-10 bg-black clip-star"></div>

    <!-- Logo -->
    <div class="fixed w-full flex justify-center mb-72 z-10">
      <div
        class="relative w-64 h-64 rounded-full bg-gradient-radial from-yellow-200 to-purple-300 flex items-center justify-center border border-black/10 shadow-[0_10px_30px_rgba(149,104,210,0.2)]">
      </div>
    </div>

    <!-- Glass content -->
    <div v-motion :initial="{ opacity: 0, y: 50 }" :enter="{ opacity: 1, y: 0 }"
      class="relative z-20 w-full max-w-5xl text-center bg-white/80 backdrop-blur-lg rounded-2xl border-t border-purple-600 shadow-lg p-8 flex flex-col gap-6 mt-72 mb-36">
      <!-- Header -->
      <header>
        <div class="w-full flex justify-center">
          <h1
            class="text-4xl font-bold w-fit bg-gradient-to-r from-purple-400 to-purple-600 bg-clip-text text-transparent pb-1">
            Connect your Web
          </h1>
        </div>
      </header>

      <!-- Main description -->
      <main class="flex flex-col gap-4 text-gray-700">
        <p class="text-lg">TrojoNetworks is a way to connect the web.</p>
        <p class="text-lg">Create and manage public or private networks with customizable permissions.</p>
        <p class="text-lg">Allow users to reuse accounts used in other networks.</p>
        <p class="text-lg">Create contant fully customizable inside the network.</p>
      </main>

      <!-- Buttons -->
      <div class="flex justify-center gap-4 mt-6">
        <button
          class="px-6 py-3 rounded-full font-medium bg-purple-600 text-white hover:bg-purple-700 hover:shadow-lg transition">
          Get Started
        </button>
        <button
          class="px-6 py-3 rounded-full font-medium border border-purple-600 text-purple-600 hover:bg-purple-100 transition">
          Learn More
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
// import { useMotion } from '@vueuse/motion'
// import { onMounted, ref } from 'vue'

// const contentRef = ref<HTMLElement | null>(null)

// onMounted(() => {
//   if (contentRef.value) {
//     useMotion(contentRef.value, {
//       initial: { opacity: 0, y: 50 },
//       enter: { opacity: 1, y: 0 },
//       transition: { duration: 0.01, easing: 'ease-out', delay: 2 },
//     })
//   }
// })
</script>

<style scoped>
.clip-star {
  clip-path: polygon(50% 0%, 61% 35%, 98% 35%, 68% 57%, 79% 91%,
      50% 70%, 21% 91%, 32% 57%, 2% 35%, 39% 35%);
}

/* Radial gradient background for logo circle */
.bg-gradient-radial {
  background: radial-gradient(circle, rgba(255, 240, 170, 0.9) 20%, rgba(190, 149, 230, 0.85) 80%);
}
</style>
