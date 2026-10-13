<template>
  <modal-container
    v-if="authStore.isUnauthModalOpen"
    :title="'Unauthorized Access'"
    @close="closeModal"
  >
    <div class="unauthorized-modal__notice">
      <div class="unauthorized-modal__icon-wrap">
        <div class="unauthorized-modal__icon">
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
              d="M20.618 5.984A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01"
            />
          </svg>
        </div>
      </div>
      <div>
        <p class="unauthorized-modal__message">
          You don't have permission to access this resource. Please contact your administrator if
          you believe this is a mistake.
        </p>
      </div>
    </div>

    <div class="form__actions">
      <button
        type="button"
        title="Go back in browser history."
        class="btn btn--outline"
        @click="goBack"
      >
        Go back
      </button>
      <button
        type="button"
        class="btn btn--primary"
        @click="closeModal"
      >
        Got it
      </button>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import ModalContainer from './ModalContainer.vue';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

function closeModal() {
  authStore.setUnauthModalOpen(false);
}

function goBack() {
  router.back();
  closeModal();
}
</script>
