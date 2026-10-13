<!-- NetworksPage.vue -->
<template>
  <div class="networks-view">
    <div class="networks-view__panel">
      <div class="networks-view__header">
        <h1 class="networks-view__heading">Networks</h1>
        <RouterLink
          to="/networks/create"
          class="btn btn--primary"
        >
          + Create Network
        </RouterLink>
      </div>

      <div class="networks-view__tabs-wrap">
        <nav class="networks-view__tabs">
          <button
            @click="activeTab = 'my-networks'"
            class="networks-view__tab"
            :class="{ 'networks-view__tab--active': activeTab === 'my-networks' }"
          >
            My Networks
          </button>
          <button
            @click="activeTab = 'browse-networks'"
            class="networks-view__tab"
            :class="{ 'networks-view__tab--active': activeTab === 'browse-networks' }"
          >
            Browse Networks
          </button>
        </nav>
      </div>

      <LoadingErrorComponent
        :loading="loading"
        :error="error"
        :has-value="!!networksState.data && !!userNetworksState.data"
        button-value="Reload page"
        @button-action="router.go(0)"
      />

      <div class="networks-view__content">
        <my-networks-tab
          v-if="activeTab === 'my-networks'"
          :networks="userNetworksState.data.value ?? []"
        />

        <browse-networks-tab
          v-if="activeTab === 'browse-networks'"
          :networks="filteredNetworks"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import useNetworks from '@/composables/useNetworks';
import MyNetworksTab from '@/components/tabs/network/MyNetworksTab.vue';
import BrowseNetworksTab from '@/components/tabs/network/BrowseNetworksTab.vue';
import useUsersNetworks from '@/composables/useUserNetworks';
import { useRouter } from 'vue-router';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';

const activeTab = ref('my-networks');
const networksState = useNetworks().fetchNetworks;
const userNetworksState = useUsersNetworks().fetchUserNetworks;

const router = useRouter();

const loading = computed(() => networksState.loading.value || userNetworksState.loading.value);
const error = computed(() =>
  [networksState.error.value, userNetworksState.error.value].filter(Boolean).join(' - '),
);

onMounted(async () => {
  await networksState.execute();
  await userNetworksState.execute();
});

const filteredNetworks = computed(() => {
  const userNetworkIds = new Set(userNetworksState.data.value!.map((n) => n.id));
  return networksState.data.value!.filter((n) => !userNetworkIds.has(n.id));
});
</script>
