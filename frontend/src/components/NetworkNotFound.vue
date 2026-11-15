<template>
  <div
    class="network-not-found"
    role="alert"
    aria-live="assertive"
  >
    <p class="network-not-found__title">Network Not Found</p>
    <p class="network-not-found__message">
      The network ID <br />'<b>{{ networkId }}</b
      >'<br />
      could not be located.
    </p>
    <p class="network-not-found__hint">Please check the URL or contact your system administrator.</p>
    <router-link
      to="/"
      class="network-not-found__link"
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
