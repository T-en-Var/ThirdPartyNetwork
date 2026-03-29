<template>
  <div
    class="space-y-3 rounded-lg border border-red-200 bg-red-50 p-6 text-center"
    role="alert"
    aria-live="assertive"
  >
    <p class="text-xl font-semibold text-red-600">Network Not Found</p>
    <p class="text-gray-700">
      The network ID <br />'<b>{{ networkId }}</b
      >'<br />
      could not be located.
    </p>
    <p class="text-sm text-gray-500">Please check the URL or contact your system administrator.</p>
    <router-link
      to="/"
      class="mt-4 block font-medium text-blue-600 hover:text-blue-800"
    >
      Go to Homepage
    </router-link>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  networkId: string;
}>();
</script>
