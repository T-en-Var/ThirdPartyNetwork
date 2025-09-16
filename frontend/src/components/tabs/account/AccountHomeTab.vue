<template>
  <div>
    <div>

      <!-- Header -->
      <div class="flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-semibold text-gray-800">
            Account
          </h1>
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

        <!-- Proxy List -->
        <div>
          <h2 class="text-xl font-medium text-gray-700 mb-4">Profile</h2>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer"
              :class="{ 'border-blue-300 bg-blue-50': isCurrentProxy(userProxy) }">
              <div class="flex items-center">
                <div class="flex-shrink-0 h-12 w-12 bg-gray-200 rounded-full overflow-hidden flex items-center justify-center text-gray-500">
                  <CloudinaryFile v-if="userProxy.imageFile" :file="userProxy.imageFile" :display-only="true" class="h-full w-full object-cover" />
                  <img v-else :src="getAvatarSrc(userProxy)" :alt="userProxy.username"
                    class="h-full w-full object-cover" />
                </div>
                <div class="ml-3">
                  <h3 class="text-sm font-medium text-gray-900">
                    {{ getFullName(userProxy) || 'Unnamed Proxy' }}
                    <span v-if="userProxy.isDefault"
                      class="ml-2 px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 text-green-800">
                      Default
                    </span>
                  </h3>
                  <p class="text-xs text-gray-500" v-if="userProxy.username">@{{ userProxy.username }}</p>
                  <p class="text-xs text-gray-500 truncate" v-if="userProxy.email">{{ userProxy.email }}</p>
                  <p class="text-xs text-gray-400">Created: {{ formatDate(userProxy.createdOn) }}</p>
                </div>
              </div>
            </div>

            <div v-if="proxyList.length > 0">
              + {{ proxyList.length == 1 ? '1 proxy' : `${proxyList.length} proxies` }}
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
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

const authStore = useAuthStore();
const loading = ref(true);
const userProxy = ref<UserProxy | null>(null);

onMounted(async () => {
  userProxy.value = await authStore.getUserProxy();
  loading.value = false;
})

const selectedProxyId = ref<string | null>(
  userProxy.value?.user.userProxies.find(p => p.isDefault)?.id || null
);

const proxyList = computed(() => {
  if (userProxy.value == null) return [];
  return userProxy.value.user.userProxies.filter(p => p != null)
})

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

function getAvatarSrc(proxy: UserProxy): string {
  const name = getFullName(proxy);
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`
}

function formatDate(date: Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
}
</script>
