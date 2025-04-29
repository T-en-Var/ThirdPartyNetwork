<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden">

      <!-- Header -->
      <div class="bg-gray-100 px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-semibold text-gray-800">
            Account Management
            <span class="ml-2 px-3 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800">
              {{ currentProxy ? 'Active Proxy' : 'No Proxy Selected' }}
            </span>
          </h1>
        </div>
        <div class="flex space-x-3">
          <button @click="$emit('back')"
            class="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50">
            Back to Dashboard
          </button>
          <button @click="$emit('createProxy')" class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700">
            Create New Proxy
          </button>
        </div>
      </div>

      <!-- Main Content -->
      <div class="p-6" v-if="!loading && userProxy">
        <!-- User Info -->
        <div class="mb-8">
          <h2 class="text-xl font-medium text-gray-700 mb-4">Base User</h2>
          <div class="bg-gray-50 p-4 rounded-lg flex items-center border border-gray-200">
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-900">User ID: {{ userProxy.user.id }}</p>
              <p class="text-sm text-gray-500">Created: {{ formatDate(userProxy.user.createdOn) }}</p>
              <p class="text-sm text-gray-500">Proxy Count: {{ userProxy.user.userProxies.length }}</p>
            </div>
          </div>
        </div>

        <!-- Current Proxy Info -->
        <div class="mb-8" v-if="currentProxy">
          <h2 class="text-xl font-medium text-gray-700 mb-4">Current Proxy</h2>
          <div class="bg-blue-50 p-6 rounded-lg border border-blue-200">
            <div class="flex items-center mb-4">
              <div
                class="flex-shrink-0 h-16 w-16 bg-blue-200 rounded-full overflow-hidden flex items-center justify-center text-blue-700">
                <span v-if="!currentProxy.imageUrl">
                  {{ getInitials(currentProxy) }}
                </span>
                <img v-else :src="currentProxy.imageUrl" alt="User avatar" class="h-16 w-16 object-cover" />
              </div>
              <div class="ml-4">
                <h3 class="text-lg font-medium text-gray-900">
                  {{ getFullName(currentProxy) || 'Unnamed Proxy' }}
                  <span v-if="currentProxy.isDefault"
                    class="ml-2 px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                    Default
                  </span>
                </h3>
                <p class="text-sm text-gray-500" v-if="currentProxy.username">@{{ currentProxy.username }}</p>
                <p class="text-sm text-gray-500" v-if="currentProxy.email">{{ currentProxy.email }}</p>
              </div>
            </div>
            <div class="flex space-x-3">
              <button @click="$emit('editProxy', currentProxy.id)"
                class="px-4 py-2 border border-blue-300 rounded-md text-blue-700 hover:bg-blue-50">
                Edit Proxy
              </button>
              <button v-if="!currentProxy.isDefault" @click="$emit('makeDefault', currentProxy.id)"
                class="px-4 py-2 border border-green-300 text-green-700 rounded-md hover:bg-green-50">
                Set as Default
              </button>
            </div>
          </div>
        </div>

        <!-- Proxy List -->
        <div>
          <h2 class="text-xl font-medium text-gray-700 mb-4">Your Proxies</h2>

          <div v-if="userProxy?.user.userProxies.length === 0" class="text-center py-8 bg-gray-50 rounded-lg">
            <p class="text-gray-500">No proxies available. Create your first proxy.</p>
          </div>

          <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div v-for="proxy in userProxy?.user.userProxies" :key="proxy.id"
              class="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer"
              :class="{ 'border-blue-300 bg-blue-50': isCurrentProxy(proxy) }" @click="selectProxy(proxy)">
              <div class="flex items-center">
                <div
                  class="flex-shrink-0 h-12 w-12 bg-gray-200 rounded-full overflow-hidden flex items-center justify-center text-gray-500">
                  <span v-if="!proxy.imageUrl">
                    {{ getInitials(proxy) }}
                  </span>
                  <img v-else :src="proxy.imageUrl" alt="User avatar" class="h-12 w-12 object-cover" />
                </div>
                <div class="ml-3">
                  <h3 class="text-sm font-medium text-gray-900">
                    {{ getFullName(proxy) || 'Unnamed Proxy' }}
                    <span v-if="proxy.isDefault"
                      class="ml-2 px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 text-green-800">
                      Default
                    </span>
                  </h3>
                  <p class="text-xs text-gray-500" v-if="proxy.username">@{{ proxy.username }}</p>
                  <p class="text-xs text-gray-500 truncate" v-if="proxy.email">{{ proxy.email }}</p>
                  <p class="text-xs text-gray-400">Created: {{ formatDate(proxy.createdOn) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="p-6" v-else>
        Loading...
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { UserProxy } from '@/types';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();
const loading = ref(true);
const userProxy = ref<UserProxy | null>(null);

onMounted(async () => {
  userProxy.value = await authStore.getUserProxy();
  loading.value = false;
})

const emits = defineEmits<{
  (e: 'back'): void;
  (e: 'createProxy'): void;
  (e: 'editProxy', proxyId: string): void;
  (e: 'makeDefault', proxyId: string): void;
  (e: 'switchProxy', proxyId: string): void;
}>();

const selectedProxyId = ref<string | null>(
  userProxy.value?.user.userProxies.find(p => p.isDefault)?.id || null
);

const currentProxy = computed(() => {
  if (userProxy.value == null) return null;
  return userProxy.value.user.userProxies.find(p => p.id === selectedProxyId.value) || null;
});

function selectProxy(proxy: UserProxy) {
  selectedProxyId.value = proxy.id;
  emits('switchProxy', proxy.id);
}

function isCurrentProxy(proxy: UserProxy): boolean {
  return selectedProxyId.value === proxy.id;
}

function getFullName(proxy: UserProxy): string {
  if (proxy.firstName && proxy.lastName) {
    return `${proxy.firstName} ${proxy.lastName}`;
  } else if (proxy.firstName) {
    return proxy.firstName;
  } else if (proxy.lastName) {
    return proxy.lastName;
  }
  return '';
}

function getInitials(proxy: UserProxy): string {
  if (proxy.firstName && proxy.lastName) {
    return `${proxy.firstName.charAt(0)}${proxy.lastName.charAt(0)}`;
  } else if (proxy.firstName) {
    return proxy.firstName.charAt(0);
  } else if (proxy.lastName) {
    return proxy.lastName.charAt(0);
  } else if (proxy.username) {
    return proxy.username.charAt(0);
  }
  return 'U';
}

function formatDate(date: Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
}
</script>
