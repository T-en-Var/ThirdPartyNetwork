<template>
  <div class="container mx-auto px-4 py-8">
    <div class="overflow-hidden rounded-lg bg-white py-16 text-center shadow-md">
      <div class="flex flex-col items-center justify-center mt-40">
        <h1 class="mb-2 text-6xl font-bold text-gray-800">Bye bye!</h1>
        <p class="mb-8 text-xl text-gray-600">We hope to see you again!</p>

        <router-link
          to="/"
          class="mt-8 rounded-md bg-blue-600 px-6 py-3 text-white transition-colors duration-200 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Return to Home Page
        </router-link>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
</script>
