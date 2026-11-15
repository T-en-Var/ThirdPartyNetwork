<template>
  <div class="network-list-panel">
    <div class="network-list-panel__header">
      <h2 class="network-list-panel__title">Network Accesses</h2>
      <button
        @click="$emit('addAccess')"
        class="btn btn--primary"
      >
        Add Access
      </button>
    </div>

    <div class="network-list-panel__items">
      <div
        v-for="networkAccess in network.networkAccesses"
        :key="networkAccess.access.id"
        class="network-list-panel__item"
      >
        <div class="network-list-panel__item-header">
          <div>
            <h3 class="network-list-panel__item-title">{{ networkAccess.access.name }}</h3>
            <p class="network-list-panel__item-description">{{ networkAccess.access.description }}</p>
          </div>
          <div class="network-list-panel__item-actions">
            <button
              @click="$emit('toggleAccessRequired', networkAccess)"
              class="network-list-panel__toggle-btn"
              :class="{ 'network-list-panel__toggle-btn--active': networkAccess.isRequired }"
            >
              {{ networkAccess.isRequired ? 'Required' : 'Optional' }}
            </button>
            <button
              @click="$emit('removeAccess', networkAccess)"
              class="network-list-panel__remove-btn"
            >
              Remove
            </button>
          </div>
        </div>
      </div>

      <div
        v-if="!network.networkAccesses?.length"
        class="network-list-panel__empty"
      >
        No accesses defined in this network
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NetworkDto, NetworkAccessNtADto } from '@/types';

defineProps<{
  network: NetworkDto;
}>();

defineEmits<{
  (e: 'addAccess'): void;
  (e: 'toggleAccessRequired', access: NetworkAccessNtADto): void;
  (e: 'removeAccess', access: NetworkAccessNtADto): void;
}>();
</script>
