<template>
  <div class="container mx-auto px-4 py-8">
    <div class="overflow-hidden rounded-lg bg-white py-16 text-center shadow-md">
      <div class="flex flex-col items-center justify-center">
        <div class="mb-6 rounded-full bg-red-50 p-6">
          <svg
            class="h-16 w-16 text-red-500"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
        </div>
        <h1 class="mb-2 text-6xl font-bold text-gray-800">404</h1>
        <p class="mb-8 text-xl text-gray-600">Page not found</p>
        <p class="text-base leading-relaxed text-slate-600">
          You were trying to access
          <span
            class="inline-flex items-center rounded-md border border-slate-200 bg-slate-100 px-2 py-1 font-mono text-sm text-indigo-600 shadow-sm"
          >
            {{ path }}
          </span>
          , but we can't find it!
        </p>

        <p
          v-if="path == '/404'"
          class="text-sm italic leading-relaxed text-slate-600"
        >
          (Or maybe we did!??)
        </p>

        <router-link
          to="/"
          class="mt-8 rounded-md bg-blue-600 px-6 py-3 text-white transition-colors duration-200 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Return Home
        </router-link>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const router = useRouter();
const route = useRoute();

const path = ref<string>(route.fullPath);

onMounted(() => {
  router.replace('/404');
});
</script>
