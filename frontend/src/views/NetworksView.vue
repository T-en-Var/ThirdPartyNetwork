<!-- NetworksPage.vue -->
<template>
  <div class="container mx-auto mt-8 px-4 py-8">
    <div class="overflow-hidden rounded-lg bg-white shadow-md">
      <div class="flex items-center justify-between border-b border-gray-200 bg-gray-100 px-6 py-4">
        <h1 class="text-2xl font-semibold text-gray-800">Networks</h1>
        <RouterLink
          to="/networks/create"
          class="rounded bg-blue-600 px-4 py-2 text-white transition-colors hover:bg-blue-700"
        >
          + Create Network
        </RouterLink>
      </div>

      <div class="border-b border-gray-200">
        <nav class="flex">
          <button
            @click="activeTab = 'my-networks'"
            class="px-6 py-4 text-sm font-medium"
            :class="
              activeTab === 'my-networks'
                ? 'border-b-2 border-blue-600 text-blue-600'
                : 'text-gray-600 hover:text-gray-800'
            "
          >
            My Networks
          </button>
          <button
            @click="activeTab = 'browse-networks'"
            class="px-6 py-4 text-sm font-medium"
            :class="
              activeTab === 'browse-networks'
                ? 'border-b-2 border-blue-600 text-blue-600'
                : 'text-gray-600 hover:text-gray-800'
            "
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

      <div class="p-6">
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

<style scoped>
.spinner-border {
  width: 3rem;
  height: 3rem;
  border: 3px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}

@keyframes spinner-border {
  100% {
    transform: rotate(360deg);
  }
}
</style>
