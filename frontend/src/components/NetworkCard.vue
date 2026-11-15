<template>
  <div
    class="network-card"
    v-if="network"
  >
    <div class="network-card__body">
      <div class="network-card__header">
        <div class="network-card__logo">
          <CdnFile
            v-if="network.imageFile"
            :display-only="true"
            :file="network.imageFile"
            class="network-card__logo-image"
          />
          <div
            v-else
            class="network-card__logo-fallback"
          >
            <img
              :src="`https://ui-avatars.com/api/?name=${network?.name}&size=24&background=random`"
              :alt="network.name"
            />
          </div>
        </div>
        <div class="network-card__title-row">
          <div class="network-card__titles">
            <h2 class="network-card__title">{{ network.name }}</h2>
            <p
              v-if="network.networkUsers?.length > 0"
              class="network-card__subtitle"
            >
              {{ network.networkUsers?.length || 0 }} members
            </p>
          </div>

          <div class="network-card__badges">
            <span
              v-if="network.isSystemProtected"
              class="badge badge--success"
            >
              Protected
            </span>
            <span
              v-if="!network.isPublic"
              class="badge badge--danger"
            >
              Private
            </span>
          </div>
        </div>
      </div>

      <p class="network-card__description">
        {{ network.description || 'No description available' }}
      </p>

      <div class="network-card__footer">
        <RouterLink
          :to="`/networks/${network.id}`"
          class="network-card__link"
        >
          View Details
        </RouterLink>

        <RouterLink
          v-if="canManage"
          :to="`/networks/${network.id}/manage`"
          class="network-card__link"
        >
          Manage
        </RouterLink>

        <button
          v-if="showJoin"
          @click="$emit('joinNetwork', network)"
          class="network-card__join-btn"
        >
          Join Network
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NetworkDto } from '@/types';
import CdnFile from '@/components/cdn/CdnFile.vue';

defineProps<{
  network: NetworkDto;
  showJoin?: boolean;
  canManage?: boolean;
}>();

defineEmits<{
  (e: 'joinNetwork', network: NetworkDto): void;
}>();
</script>
