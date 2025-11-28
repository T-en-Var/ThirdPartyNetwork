<template>
  <div class="text-center p-6 bg-red-50 border border-red-200 rounded-lg space-y-3" role="alert" aria-live="assertive">
    <p class="text-red-600 text-xl font-semibold">Network Not Found</p>
    <p class="text-gray-700">
      The network ID <br>'<b>{{ networkId }}</b>'<br> could not be located.
    </p>
    <p class="text-sm text-gray-500">
      Please check the URL or contact your system administrator.
    </p>
    <router-link to="/" class="text-blue-600 hover:text-blue-800 font-medium block mt-4">
      Go to Homepage
    </router-link>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  networkId: string;
}>();
</script>