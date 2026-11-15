<template>
  <div class="status-page">
    <div class="status-page__card">
      <div class="status-page__content">
        <div class="status-page__icon status-page__icon--danger">
          <svg
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
        <h1 class="status-page__code">404</h1>
        <p class="status-page__subtitle">Page not found</p>
        <p class="status-page__description">
          You were trying to access
          <span class="status-page__path">
            {{ path }}
          </span>
          , but we can't find it!
        </p>

        <p
          v-if="path == '/404'"
          class="status-page__hint"
        >
          (Or maybe we did!??)
        </p>

        <router-link
          to="/"
          class="btn btn--primary"
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
