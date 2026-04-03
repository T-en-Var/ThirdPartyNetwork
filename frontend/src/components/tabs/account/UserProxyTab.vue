<template>
  <div class="space-y-8 overflow-hidden rounded-lg bg-white p-6 shadow-md">
    <!-- Active Proxy -->
    <div v-if="activeProxy">
      <h3 class="text-lg font-medium text-gray-700">Currently logged in as</h3>
      <div class="mt-4 rounded-lg border border-blue-200 bg-blue-50 p-6">
        <div class="mb-4 flex items-center">
          <CloudinaryFile
            v-if="activeProxy.imageFile"
            :file="activeProxy.imageFile"
            :display-only="true"
            class="h-16 w-16 rounded-full object-cover"
          />
          <img
            v-else
            :src="getAvatarSrc(activeProxy)"
            :alt="activeProxy.username"
            class="h-16 w-16 rounded-full object-cover"
          />

          <div class="ml-4">
            <h2 class="text-xl font-semibold text-gray-900">
              {{ getFullName(activeProxy) }}
              <span
                class="ml-2 rounded-full bg-green-100 px-2 py-1 text-xs font-medium text-green-800"
                v-if="activeProxy.isDefault"
              >
                Default
              </span>
            </h2>
            <p class="text-sm text-gray-500">@{{ activeProxy.username }}</p>
            <p class="text-sm text-gray-500">{{ activeProxy.email }}</p>
          </div>
        </div>
        <div class="flex space-x-3">
          <button
            @click="$emit('editProxy', activeProxy.id)"
            class="rounded-md border border-blue-300 px-4 py-2 text-blue-700 hover:bg-blue-50"
          >
            Edit Proxy
          </button>
          <button
            v-if="!isActiveProxy(activeProxy)"
            @click="$emit('switchProxy', activeProxy.id)"
            class="rounded-md border border-green-300 px-4 py-2 text-green-700 hover:bg-green-50"
          >
            Switch Proxy
          </button>
        </div>
      </div>
    </div>

    <!-- Proxy List -->
    <div>
      <div class="mb-4 flex items-center justify-between">
        <h3 class="text-lg font-medium text-gray-700">All Proxies</h3>
        <button
          @click="$emit('createProxy')"
          class="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          + Create New Proxy
        </button>
      </div>

      <div
        v-if="allProxies.length === 0"
        class="rounded-lg bg-gray-50 py-8 text-center"
      >
        <p class="text-gray-500">No proxies found. Add your first proxy.</p>
      </div>

      <div
        v-else
        class="grid grid-cols-1 gap-6 pb-16 md:grid-cols-2"
      >
        <div
          v-for="proxy in allProxies"
          :key="proxy.id"
          class="rounded-lg border bg-white p-4 transition hover:shadow-md"
        >
          <div class="flex w-full items-start justify-between">
            <div class="mb-3 flex items-center">
              <CloudinaryFile
                v-if="proxy.imageFile"
                :file="proxy.imageFile"
                :display-only="true"
                class="h-16 w-16 rounded-full object-cover"
              />
              <img
                v-else
                :src="getAvatarSrc(proxy)"
                :alt="proxy.username"
                class="h-16 w-16 rounded-full object-cover"
              />
              <div class="ml-3">
                <h4 class="text-sm font-medium text-gray-900">
                  {{ getFullName(proxy) || 'Unnamed Proxy' }}
                  <span
                    v-if="proxy.isDefault"
                    class="ml-2 rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"
                  >
                    Default
                  </span>
                </h4>
                <p class="text-xs text-gray-500">@{{ proxy.username }}</p>
                <p class="truncate text-xs text-gray-500">{{ proxy.email }}</p>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex space-x-2">
              <button
                @click="$emit('editProxy', proxy.id)"
                class="rounded-md border border-blue-300 px-3 py-1 text-xs text-blue-700 hover:bg-blue-50"
              >
                Edit
              </button>
              <button
                v-if="!isActiveProxy(proxy)"
                @click="$emit('switchProxy', proxy.id)"
                class="rounded-md border border-green-300 px-3 py-1 text-xs text-green-700 hover:bg-green-50"
              >
                Switch
              </button>
            </div>
          </div>

          <!-- Networks -->
          <div>
            <h5 class="mb-2 text-xs font-semibold text-gray-600">Joined Networks</h5>
            <div class="flex flex-wrap gap-2">
              <div
                v-for="network in getNetworksForProxy(proxy)"
                :key="network.id"
                class="group relative"
              >
                <a
                  :href="`/networks/${network.id}`"
                  class="block"
                >
                  <CloudinaryFile
                    v-if="network.imageFile"
                    :file="network.imageFile"
                    :display-only="true"
                    class="h-10 w-10 rounded-full object-cover"
                  />
                  <img
                    v-else
                    :src="`https://ui-avatars.com/api/?name=${encodeURIComponent(network.name)}&background=random`"
                    :alt="network.name"
                    class="h-10 w-10 rounded-full object-cover"
                  />
                </a>
                <!-- Hover Card -->
                <div
                  class="absolute left-0 top-12 z-10 hidden w-56 rounded-md border border-gray-200 bg-white p-3 shadow-lg group-hover:block"
                >
                  <h6 class="font-medium text-gray-900">{{ network.name }}</h6>
                  <p class="truncate text-xs text-gray-500">{{ network.description }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Proxy List -->
    <div>
      <div class="mb-4 flex items-center justify-between">
        <h3 class="text-lg font-medium text-gray-700">Proxy Previews</h3>
      </div>

      <div
        class="mb-4 flex flex-row gap-2"
        v-if="accesses && accesses.length"
      >
        <label
          v-for="access in accesses"
          :key="access.id"
          class="flex cursor-pointer items-center gap-2 rounded-lg border border-slate-200 p-3 px-4 transition-colors hover:bg-slate-50"
        >
          <div class="mt-0.5 flex h-5 items-center">
            <input
              type="checkbox"
              v-model="activeAccessFilters[access.name]"
              class="h-4 w-4 cursor-pointer rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
            />
          </div>
          <div class="flex flex-col">
            <span class="text-sm font-medium text-slate-700">
              {{ access.name }}
            </span>
          </div>
        </label>
      </div>

      <div
        v-if="allProxies.length === 0"
        class="rounded-lg bg-gray-50 py-8 text-center"
      >
        <p class="text-gray-500">No proxies found. Add your first proxy.</p>
      </div>

      <div
        v-else
        class="grid grid-cols-1 gap-6 pb-16 md:grid-cols-2"
      >
        <div
          v-for="proxy in proxyPreviews"
          :key="proxy.id"
          class="rounded-lg border bg-white p-4 transition hover:shadow-md"
        >
          <div class="flex w-full items-start justify-between">
            <div class="mb-3 flex items-center">
              <ProfileAvatar
                :user-proxy="proxy"
                class="h-16 w-16"
              />
              <div class="ml-3">
                <h4 class="text-sm font-medium text-gray-900">
                  {{ getFullName(proxy) }}
                  <span
                    v-if="proxy.isDefault"
                    class="ml-2 rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"
                  >
                    Default
                  </span>
                </h4>
                <p class="text-xs text-gray-500">@{{ proxy.username }}</p>
                <p class="truncate text-xs text-gray-500">{{ proxy.email }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import type { UserProxy, Network } from '@/types';
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import useUsers from '@/composables/useUsers';
import useAccesses from '@/composables/useAccesses';
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import { defaultProxy } from '@/lib/user';

const userProxy = ref<UserProxy | null>(null);
const authStore = useAuthStore();
const { execute: fetchUser, data: user } = useUsers().fetchUser;
const { execute: fetchAccesses, data: accesses } = useAccesses().fetchAccesses;

defineEmits<{
  (e: 'editProxy', id: string): void;
  (e: 'switchProxy', id: string): void;
  (e: 'createProxy'): void;
}>();

onMounted(async () => {
  await fetchAccesses();
  if (!accesses.value) throw new Error('No Accesses found');

  accesses.value.forEach((access) => {
    activeAccessFilters.value[access.name] = true;
  });

  const currentUser = await authStore.getUserProxy();
  if (!currentUser) throw new Error('UserProxy not found');

  await fetchUser(currentUser.user.id);
  if (!user.value) throw new Error('User not found');

  userProxy.value = user.value.userProxies.find((up) => up.id === currentUser.id) ?? null;
  if (!userProxy.value) throw new Error('UserProxy not found');
});

const activeProxy = computed(() =>
  allProxies.value.find((p) => p.id === authStore.getUserProxyId()),
);

const defaultUserProxy = computed(() => (user.value ? defaultProxy(user.value) : null));
const allProxies = computed(() => {
  if (!user.value) return [];
  return user.value.userProxies;
});

const activeAccessFilters = ref<Record<string, boolean>>({});
const proxyPreviews = computed(() => {
  if (!accesses.value || !allProxies.value || !defaultUserProxy.value) return [];

  const prePreview = allProxies.value.map((up) => ({
    ...up,
    username: up.username ? up.username : defaultUserProxy.value!.username,
    firstName: up.firstName ? up.firstName : defaultUserProxy.value!.firstName,
    lastName: up.lastName ? up.lastName : defaultUserProxy.value!.lastName,
    email: up.email ? up.email : defaultUserProxy.value!.email,
    imageFile: up.imageFile ? up.imageFile : defaultUserProxy.value!.imageFile,
  }));

  return prePreview.map((up) => ({
    ...up,
    username: activeAccessFilters.value['Access Username'] ? up.username : undefined,
    firstName: activeAccessFilters.value['Access Firstname'] ? up.firstName : undefined,
    lastName: activeAccessFilters.value['Access Lastname'] ? up.lastName : undefined,
    email: activeAccessFilters.value['Access Email'] ? up.email : undefined,
    imageFile: activeAccessFilters.value['Access ProfilePicture'] ? up.imageFile : undefined,
  }));
});

function isActiveProxy(proxy: UserProxy) {
  return proxy.id === authStore.getUserProxyId();
}

function getFullName(proxy: UserProxy): string {
  return `${proxy.firstName ?? '-'} ${proxy.lastName ?? '-'}`;
}

function getAvatarSrc(proxy: UserProxy): string {
  const name = getFullName(proxy);
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`;
}

function getNetworksForProxy(proxy: UserProxy): Network[] {
  return proxy.networkUsers.map((nu) => nu.network);
}
</script>
