<template>
  <div class="space-y-8 bg-white shadow-md rounded-lg overflow-hidden p-6">
    <!-- Active Proxy -->
    <div v-if="activeProxy" class="bg-blue-50 border border-blue-200 rounded-lg p-6">
      <div class="flex items-center mb-4">
        <CloudinaryFile v-if="activeProxy.imageFile" :file="activeProxy.imageFile" :display-only="true"
          class="h-16 w-16 rounded-full object-cover" />
        <img v-else :src="getAvatarSrc(activeProxy)" :alt="activeProxy.username"
          class="h-16 w-16 rounded-full object-cover" />

        <!-- <img v-if="activeProxy.imageUrl" :src="activeProxy.imageUrl" alt="Profile"
          class="h-16 w-16 rounded-full object-cover" />
        <div v-else
          class="h-16 w-16 flex items-center justify-center bg-blue-200 rounded-full text-lg font-bold text-blue-800">
          {{ getInitials(activeProxy) }}
        </div> -->
        <div class="ml-4">
          <h2 class="text-xl font-semibold text-gray-900">
            {{ getFullName(activeProxy) }}
            <span class="ml-2 px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800"
              v-if="activeProxy.isDefault">
              Default
            </span>
          </h2>
          <p class="text-sm text-gray-500">@{{ activeProxy.username }}</p>
          <p class="text-sm text-gray-500">{{ activeProxy.email }}</p>
        </div>
      </div>
      <div class="flex space-x-3">
        <button @click="$emit('editProxy', activeProxy.id)"
          class="px-4 py-2 border border-blue-300 rounded-md text-blue-700 hover:bg-blue-50">
          Edit Proxy
        </button>
        <button v-if="!isActiveProxy(activeProxy)" @click="$emit('switchProxy', activeProxy.id)"
          class="px-4 py-2 border border-green-300 text-green-700 rounded-md hover:bg-green-50">
          Switch Proxy
        </button>
      </div>
    </div>

    <!-- Proxy List -->
    <div>
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-medium text-gray-700">All Proxies</h3>
        <button @click="$emit('createProxy')"
          class="px-4 py-2 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700">
          + Create New Proxy
        </button>
      </div>

      <div v-if="allProxies.length === 0" class="text-center py-8 bg-gray-50 rounded-lg">
        <p class="text-gray-500">No proxies found. Add your first proxy.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6 pb-16">
        <div v-for="proxy in allProxies" :key="proxy.id"
          class="border rounded-lg p-4 bg-white hover:shadow-md transition">
          <div class="flex items-start justify-between w-full">
            <div class="flex items-center mb-3">
              <CloudinaryFile v-if="proxy.imageFile" :file="proxy.imageFile" :display-only="true"
                class="h-16 w-16 rounded-full object-cover" />
              <img v-else :src="getAvatarSrc(proxy)" :alt="proxy.username"
                class="h-16 w-16 rounded-full object-cover" />
              <div class="ml-3">
                <h4 class="text-sm font-medium text-gray-900">
                  {{ getFullName(proxy) || 'Unnamed Proxy' }}
                  <span v-if="proxy.isDefault"
                    class="ml-2 px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 text-green-800">
                    Default
                  </span>
                </h4>
                <p class="text-xs text-gray-500">@{{ proxy.username }}</p>
                <p class="text-xs text-gray-500 truncate">{{ proxy.email }}</p>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex space-x-2">
              <button @click="$emit('editProxy', proxy.id)"
                class="px-3 py-1 border border-blue-300 rounded-md text-blue-700 hover:bg-blue-50 text-xs">
                Edit
              </button>
              <button v-if="!isActiveProxy(proxy)" @click="$emit('switchProxy', proxy.id)"
                class="px-3 py-1 border border-green-300 text-green-700 rounded-md hover:bg-green-50 text-xs">
                Switch
              </button>
            </div>
          </div>

          <!-- Networks -->
          <div>
            <h5 class="text-xs font-semibold text-gray-600 mb-2">Joined Networks</h5>
            <div class="flex flex-wrap gap-2">
              <div v-for="network in getNetworksForProxy(proxy)" :key="network.id" class="relative group">
                <a :href="`/networks/${network.id}`" class="block">
                  <CloudinaryFile v-if="network.imageFile" :file="network.imageFile" :display-only="true"
                    class="h-10 w-10 rounded-full object-cover" />
                  <img v-else
                    :src="`https://ui-avatars.com/api/?name=${encodeURIComponent(network.name)}&background=random`"
                    :alt="network.name" class="h-16 w-16 rounded-full object-cover" />
                </a>
                <!-- Hover Card -->
                <div
                  class="absolute z-10 hidden group-hover:block bg-white border border-gray-200 rounded-md shadow-lg p-3 w-56 top-12 left-0">
                  <h6 class="font-medium text-gray-900">{{ network.name }}</h6>
                  <p class="text-xs text-gray-500 truncate">{{ network.description }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Proxy List -->
    <div>
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-medium text-gray-700">Proxy Previews</h3>
      </div>

      <div v-if="allProxies.length === 0" class="text-center py-8 bg-gray-50 rounded-lg">
        <p class="text-gray-500">No proxies found. Add your first proxy.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6 pb-16">
        <div v-for="proxy in allProxies" :key="proxy.id"
          class="border rounded-lg p-4 bg-white hover:shadow-md transition">
          <div class="flex items-start justify-between w-full">
            <div class="flex items-center mb-3">
              <CloudinaryFile :file="proxy.imageFile ?? activeProxy!.imageFile!" :display-only="true"
                class="h-16 w-16 rounded-full object-cover" />
              <div class="ml-3">
                <h4 class="text-sm font-medium text-gray-900">
                  {{ getFullName(proxy) || getFullName(activeProxy!) }}
                  <span v-if="proxy.isDefault"
                    class="ml-2 px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 text-green-800">
                    Default
                  </span>
                </h4>
                <p class="text-xs text-gray-500">@{{ proxy.username ?? activeProxy?.username }}</p>
                <p class="text-xs text-gray-500 truncate">{{ proxy.email ?? activeProxy?.email }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import type { UserProxy, Network } from '@/types'
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';

const userProxy = ref<UserProxy | null>(null);
const authStore = useAuthStore()

defineEmits<{
  (e: 'editProxy', id: string): void
  (e: 'switchProxy', id: string): void
  (e: 'createProxy'): void
}>()

onMounted(async () => {
  userProxy.value = await authStore.getUserProxy();
})

const activeProxy = computed(() =>
  allProxies.value.find(p => p.id === authStore.getUserProxyId())
)

const allProxies = computed(() => {
  if (!userProxy.value) return [];
  return [userProxy.value, ...userProxy.value?.user.userProxies.filter(u => u != null)];
})

function isActiveProxy(proxy: UserProxy) {
  return proxy.id === authStore.getUserProxyId()
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

function getNetworksForProxy(proxy: UserProxy): Network[] {
  // assuming networks[].networkUsers contains userProxy relation
  return proxy.networkUsers.map(nu => nu.network)
}
</script>
