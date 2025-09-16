<template>
  <div class="flex h-full pt-16">
    <!-- Sidebar -->
    <AccountSidebar />

    <!-- Main Content -->
    <div class="flex-1 p-6 overflow-auto">
      <!-- Loading and Error States -->
      <LoadingErrorComponent :loading="authStore.loading" :error="authStore.error"
        button-value="Reload page" @button-action="router.go(0)" />

      <!-- Route View (Page Content) -->
      <div class="bg-white shadow-md rounded-lg overflow-hidden p-6" v-if="authStore.currentUser">
        <RouterView v-if="!authStore.loading && !authStore.error && authStore.currentUser"/>
      </div>

      <div v-if="authStore.currentUser">
        <!-- Modals -->
        <ConfirmationModal v-if="showConfirmationModal" :title="confirmationTitle" :message="confirmationMessage"
          :button-text="confirmButtonText" :color="confirmButtonColor" :is-submitting="isSubmitting"
          @close="showConfirmationModal = false" @confirm="confirmAction" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useGlobalStore } from '@/stores/global';

// Components
import LoadingErrorComponent from '@/components/LoadingError.vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';

import type {  } from '@/types';
import type {  } from '@/types/forms';

import AccountSidebar from '@/components/sidebar/AccountSidebar.vue';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();

const globalStore = useGlobalStore();
const authStore = useAuthStore();
const showConfirmationModal = ref(false);
const isSubmitting = ref(false);

const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => { });

onMounted(async () => {
  globalStore.startFetching();
  await authStore.getUserProxy();
  console.log(authStore.currentUser)
  globalStore.stopFetching();
});

// Methods
function confirmAction() {
  confirmationAction.value();
  showConfirmationModal.value = false;
}

</script>
