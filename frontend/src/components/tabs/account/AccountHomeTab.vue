<template>
  <div class="account-home-tab">
    <LoadingErrorComponent
      :loading="loading"
      :error="error"
      :has-value="!!user"
    />

    <div v-if="!loading && !!user && !!userProxy">
      <!-- Header -->
      <div class="account-home-tab__header">
        <div>
          <h1 class="account-home-tab__title">Account</h1>
        </div>
      </div>

      <!-- Main Content -->
      <div class="account-home-tab__content">
        <!-- User Info -->
        <div class="account-home-tab__section">
          <h2 class="account-home-tab__section-title">Base User</h2>
          <div class="account-home-tab__user-info">
            <div class="account-home-tab__user-info-text">
              <p class="account-home-tab__user-info-line">User ID: {{ user.id }}</p>
              <p class="account-home-tab__user-info-line account-home-tab__user-info-line--muted">
                Created: {{ formatDate(user.createdOn) }}
              </p>
              <p class="account-home-tab__user-info-line account-home-tab__user-info-line--muted">
                Proxy Count: {{ user.userProxies.length }}
              </p>
            </div>
          </div>
        </div>

        <!-- Proxy List -->
        <div>
          <h2 class="account-home-tab__section-title">Profile</h2>

          <div class="account-home-tab__proxy-grid">
            <div
              class="account-home-tab__proxy-card"
              @click="handleUserProxyClick"
            >
              <div class="account-home-tab__proxy-card-inner">
                <div class="avatar">
                  <CdnFile
                    v-if="userProxy.imageFile"
                    :file="userProxy.imageFile"
                    :display-only="true"
                    class="avatar__image"
                  />
                  <img
                    v-else
                    :src="getAvatarSrc(userProxy)"
                    :alt="userProxy.username ?? undefined"
                    class="avatar__image"
                  />
                </div>
                <div class="account-home-tab__proxy-card-text">
                  <h3 class="account-home-tab__proxy-card-name">
                    {{ getFullName(userProxy) || 'Unnamed Proxy' }}
                    <span
                      v-if="userProxy.isDefault"
                      class="badge badge--success"
                    >
                      Default
                    </span>
                  </h3>
                  <p
                    class="account-home-tab__proxy-card-meta"
                    v-if="userProxy.username"
                  >
                    @{{ userProxy.username }}
                  </p>
                  <p
                    class="account-home-tab__proxy-card-meta account-home-tab__proxy-card-meta--truncate"
                    v-if="userProxy.email"
                  >
                    {{ userProxy.email }}
                  </p>
                  <p class="account-home-tab__proxy-card-meta account-home-tab__proxy-card-meta--faint">
                    Created: {{ formatDate(userProxy.createdOn) }}
                  </p>
                </div>
              </div>
            </div>

            <div v-if="proxyList.length > 2">
              + {{ proxyList.length == 2 ? '1 proxy' : `${proxyList.length - 1} proxies` }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { UserProxyDto } from '@/types';
import { useAuthStore } from '@/stores/auth';
import CdnFile from '@/components/cdn/CdnFile.vue';
import { useRouter } from 'vue-router';
import useUsers from '@/composables/useUsers';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';

const authStore = useAuthStore();
const userProxy = ref<UserProxyDto | null>(null);
const router = useRouter();

const { execute: fetchUser, loading, error, data: user } = useUsers().fetchUser;

onMounted(async () => {
  userProxy.value = await authStore.getUserProxy();
  if (!userProxy.value) throw new Error('UserProxy not found');
  await fetchUser(userProxy.value.user.id);
});

const proxyList = computed(() => {
  if (user.value == null) return [];
  return user.value.userProxies.filter((p) => p != null);
});

function getFullName(proxy: UserProxyDto): string {
  if (proxy.firstName && proxy.lastName) {
    return `${proxy.firstName} ${proxy.lastName}`;
  } else if (proxy.firstName) {
    return proxy.firstName;
  } else if (proxy.lastName) {
    return proxy.lastName;
  }
  return '';
}

function getAvatarSrc(proxy: UserProxyDto): string {
  const name = getFullName(proxy);
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`;
}

function formatDate(date: string | Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function handleUserProxyClick() {
  router.push(`/account/proxies/${userProxy.value?.id}/edit`);
}
</script>
