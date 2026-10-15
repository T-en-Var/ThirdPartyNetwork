<template>
  <div class="page">
    <section class="page-section page-hero">
      <div
        class="page-hero__title"
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
      >
        <span>Terms of Service</span>
      </div>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        If you don't follow these, I'll kick you out. <i>Violently</i>
      </p>
    </section>

    <section class="page-section">
      <h3
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        General
      </h3>
      <ul
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        <li>Do not limit test the system.</li>
        <li>Do not abuse bugs.</li>
        <li>If I don't like it, it's bad.</li>
      </ul>
      <br />
      <h3
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        User Generated Content
      </h3>
      <ul
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        <li>No child unsafe content.</li>
        <li>The network owner is responsible for the content of their network.</li>
        <li>If I don't like it, it's bad.</li>
      </ul>
      <br />
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        Takeaways: if I don't like it, I can remove it. Whatever, whenever. For the rest, respect
        the system, don't be stupid.
      </p>
    </section>

    <section
      class="page-section"
      v-if="hasRedirect"
    >
      <div class="tos-page__actions">
        <button
          @click="handleContinue"
          class="btn btn--success"
        >
          Continue Signing Up
        </button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import { safeAtob } from '@/lib/utils';
import { useRoute, useRouter } from 'vue-router';

const authStore = useAuthStore();

const route = useRoute();
const router = useRouter();

const hasRedirect = route.fullPath.includes('redirect');

const handleContinue = () => {
  if (route.query.fromExternal !== undefined) {
    router.push(safeAtob(route.query.redirect as string | undefined) || '/');
    return;
  }

  authStore.setModalOpen(true);
  authStore.setModalMode('signup');
};
</script>
