<template>
  <div class="user-proxy-tab">
    <!-- Active Proxy -->
    <div v-if="activeProxy">
      <h3 class="user-proxy-tab__subtitle">Currently logged in as</h3>
      <div class="user-proxy-tab__active-card">
        <div class="user-proxy-tab__active-card-header">
          <CloudinaryFile
            v-if="activeProxy.imageFile"
            :file="activeProxy.imageFile"
            :display-only="true"
            class="avatar__image user-proxy-tab__avatar--lg"
          />
          <img
            v-else
            :src="getAvatarSrc(activeProxy)"
            :alt="activeProxy.username"
            class="avatar__image user-proxy-tab__avatar--lg"
          />

          <div class="user-proxy-tab__active-card-text">
            <h2 class="user-proxy-tab__active-card-name">
              {{ getFullName(activeProxy) }}
              <span
                class="badge badge--success"
                v-if="activeProxy.isDefault"
              >
                Default
              </span>
            </h2>
            <p class="user-proxy-tab__muted-text">@{{ activeProxy.username }}</p>
            <p class="user-proxy-tab__muted-text">{{ activeProxy.email }}</p>
          </div>
        </div>
        <div class="user-proxy-tab__active-card-actions">
          <button
            @click="$emit('editProxy', activeProxy.id)"
            class="btn btn--outline"
          >
            Edit Proxy
          </button>
          <button
            v-if="!isActiveProxy(activeProxy)"
            @click="$emit('switchProxy', activeProxy.id)"
            class="btn btn--outline"
          >
            Switch Proxy
          </button>
        </div>
      </div>
    </div>

    <!-- Proxy List -->
    <div>
      <div class="user-proxy-tab__section-header">
        <h3 class="user-proxy-tab__subtitle">All Proxies</h3>
        <button
          @click="$emit('createProxy')"
          class="btn btn--primary btn--sm"
        >
          + Create New Proxy
        </button>
      </div>

      <div
        v-if="allProxies.length === 0"
        class="empty-state"
      >
        <p class="empty-state__message">No proxies found. Add your first proxy.</p>
      </div>

      <div
        v-else
        class="user-proxy-tab__proxy-grid"
      >
        <div
          v-for="proxy in allProxies"
          :key="proxy.id"
          class="user-proxy-tab__proxy-card"
        >
          <div class="user-proxy-tab__proxy-card-top">
            <div class="user-proxy-tab__proxy-card-header">
              <CloudinaryFile
                v-if="proxy.imageFile"
                :file="proxy.imageFile"
                :display-only="true"
                class="avatar__image user-proxy-tab__avatar--lg"
              />
              <img
                v-else
                :src="getAvatarSrc(proxy)"
                :alt="proxy.username"
                class="avatar__image user-proxy-tab__avatar--lg"
              />
              <div class="user-proxy-tab__proxy-card-text">
                <h4 class="user-proxy-tab__proxy-card-name">
                  {{ getFullName(proxy) || 'Unnamed Proxy' }}
                  <span
                    v-if="proxy.isDefault"
                    class="badge badge--success"
                  >
                    Default
                  </span>
                </h4>
                <p class="user-proxy-tab__muted-text user-proxy-tab__muted-text--xs">
                  @{{ proxy.username }}
                </p>
                <p
                  class="user-proxy-tab__muted-text user-proxy-tab__muted-text--xs user-proxy-tab__muted-text--truncate"
                >
                  {{ proxy.email }}
                </p>
              </div>
            </div>

            <!-- Actions -->
            <div class="user-proxy-tab__proxy-card-actions">
              <button
                @click="$emit('editProxy', proxy.id)"
                class="btn btn--outline btn--sm"
              >
                Edit
              </button>
              <button
                v-if="!isActiveProxy(proxy)"
                @click="$emit('switchProxy', proxy.id)"
                class="btn btn--outline btn--sm"
              >
                Switch
              </button>
            </div>
          </div>

          <!-- Networks -->
          <div>
            <h5 class="user-proxy-tab__networks-title">Joined Networks</h5>
            <div class="user-proxy-tab__networks-list">
              <div
                v-for="network in getNetworksForProxy(proxy)"
                :key="network.id"
                class="user-proxy-tab__network-item"
              >
                <a
                  :href="`/networks/${network.id}`"
                  class="user-proxy-tab__network-link"
                >
                  <CloudinaryFile
                    v-if="network.imageFile"
                    :file="network.imageFile"
                    :display-only="true"
                    class="avatar__image user-proxy-tab__avatar--sm"
                  />
                  <img
                    v-else
                    :src="`https://ui-avatars.com/api/?name=${encodeURIComponent(network.name)}&background=random`"
                    :alt="network.name"
                    class="avatar__image user-proxy-tab__avatar--sm"
                  />
                </a>
                <!-- Hover Card -->
                <div class="user-proxy-tab__network-hover-card">
                  <h6 class="user-proxy-tab__network-hover-title">{{ network.name }}</h6>
                  <p class="user-proxy-tab__muted-text user-proxy-tab__muted-text--xs user-proxy-tab__muted-text--truncate">
                    {{ network.description }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Proxy List -->
    <div>
      <div class="user-proxy-tab__section-header">
        <h3 class="user-proxy-tab__subtitle">Proxy Previews</h3>
      </div>

      <div
        class="user-proxy-tab__access-filters"
        v-if="accesses && accesses.length"
      >
        <label
          v-for="access in accesses"
          :key="access.id"
          class="user-proxy-tab__access-filter"
        >
          <div class="user-proxy-tab__access-filter-checkbox">
            <input
              type="checkbox"
              v-model="activeAccessFilters[access.name]"
              class="checkbox"
            />
          </div>
          <div>
            <span class="user-proxy-tab__access-filter-label">
              {{ access.name }}
            </span>
          </div>
        </label>
      </div>

      <div
        v-if="allProxies.length === 0"
        class="empty-state"
      >
        <p class="empty-state__message">No proxies found. Add your first proxy.</p>
      </div>

      <div
        v-else
        class="user-proxy-tab__proxy-grid"
      >
        <div
          v-for="proxy in proxyPreviews"
          :key="proxy.id"
          class="user-proxy-tab__proxy-card"
        >
          <div class="user-proxy-tab__proxy-card-top">
            <div class="user-proxy-tab__proxy-card-header">
              <ProfileAvatar
                :user-proxy="proxy"
                class="user-proxy-tab__avatar--lg"
              />
              <div class="user-proxy-tab__proxy-card-text">
                <h4 class="user-proxy-tab__proxy-card-name">
                  {{ getFullName(proxy) }}
                  <span
                    v-if="proxy.isDefault"
                    class="badge badge--success"
                  >
                    Default
                  </span>
                </h4>
                <p class="user-proxy-tab__muted-text user-proxy-tab__muted-text--xs">
                  @{{ proxy.username }}
                </p>
                <p
                  class="user-proxy-tab__muted-text user-proxy-tab__muted-text--xs user-proxy-tab__muted-text--truncate"
                >
                  {{ proxy.email }}
                </p>
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
