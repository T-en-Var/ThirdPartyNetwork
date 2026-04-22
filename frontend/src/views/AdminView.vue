<template>
  <div class="flex h-full pt-16">
    <!-- Main Content -->
    <AdminSidebar />

    <div class="flex-1 overflow-auto p-6">
      <RouterView @delete-network="confirmDeleteNetwork" />

      <!-- Modals -->
      <ConfirmationModal
        v-if="showConfirmationModal"
        :title="confirmationTitle"
        :message="confirmationMessage"
        :button-text="confirmButtonText"
        :color="confirmButtonColor"
        :is-submitting="isSubmitting"
        @close="showConfirmationModal = false"
        @confirm="confirmAction"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import ConfirmationModal from '@/components/modals/ConfirmationModal.vue';
import AdminSidebar from '@/components/sidebar/AdminSidebar.vue';
import useNetworks from '@/composables/useNetworks';
import type { Network } from '@/types';

const isSubmitting = ref(false);

const showConfirmationModal = ref(false);
const confirmationTitle = ref('');
const confirmationMessage = ref('');
const confirmButtonText = ref('');
const confirmButtonColor = ref('');
const confirmationAction = ref(() => {});

const networksState = useNetworks();

function confirmDeleteNetwork(network: Network) {
  confirmationTitle.value = 'Delete Network';
  confirmationMessage.value = `Are you sure you want to delete "${network.name}"? This action cannot be undone.`;
  confirmButtonText.value = 'Delete';
  confirmButtonColor.value = 'red';

  confirmationAction.value = async () => {
    confirmationTitle.value = 'Delete Confirmation Network';
    confirmationMessage.value = `Are you SURE you want to delete "${network.name}"? This action CANNOT be undone.`;
    confirmButtonText.value = 'YES, DELETE';
    confirmButtonColor.value = 'red';

    confirmationAction.value = async () => {
      isSubmitting.value = true;
      const { execute: deleteNetwork } = networksState.deleteNetwork;
      try {
        await deleteNetwork(network.id);
      } catch (err) {
        console.error('Failed to delete network', err);
      } finally {
        isSubmitting.value = false;
      }
    };

    showConfirmationModal.value = true;
  };

  showConfirmationModal.value = true;
}

function confirmAction() {
  showConfirmationModal.value = false;
  confirmationAction.value();
}
</script>
