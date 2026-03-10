<template>
  <div class="flex justify-between items-center mb-8">
    <div>
      <h1 class="text-2xl font-bold text-slate-900">Manage Users</h1>
      <p class="text-slate-500 mt-1">Global user registry and network
        memberships</p>
    </div>
    <button @click="forceFetchUsers"
      class="p-2 text-slate-400 hover:text-purple-600 transition-colors"
      title="Refresh">
      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none"
        viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
      </svg>
    </button>
  </div>

  <LoadingErrorComponent :loading="loading" :error="error" :has-value="!!users" />

  <div v-if="!!users"
    class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="bg-slate-50 border-b border-slate-200">
            <th
              class="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              User</th>
            <th
              class="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              ID</th>
            <th
              class="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Networks</th>
            <th
              class="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider text-right">
              Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100">
          <tr v-for="user in users" :key="user.id"
            class="hover:bg-slate-50/50 transition-colors">
            <td class="px-6 py-4">
              <div class="text-sm text-slate-500">@{{ defaultProxy(user).username }}</div>
            </td>
            <td class="px-6 py-4 text-sm text-slate-600"><SecurableField :value="user.id" :sensitive="true" /></td>
            <td class="px-6 py-4">
              <div class="flex flex-wrap gap-1">
                <div v-for="network in getNetworksForUser(user)" :key="network.id" class="relative group">
                  <a :href="`/networks/${network.id}`" class="block">
                    <CloudinaryFile v-if="network.imageFile" :file="network.imageFile" :display-only="true"
                      class="h-8 w-8 rounded-full object-cover" />
                    <img v-else
                      :src="`https://ui-avatars.com/api/?name=${encodeURIComponent(network.name)}&background=random`"
                      :alt="network.name" class="h-8 w-8 rounded-full object-cover" />
                  </a>
                  <!-- Hover Card -->
                  <div
                    class="absolute z-10 hidden group-hover:block bg-white border border-gray-200 rounded-md shadow-lg p-3 w-56 top-12 left-0">
                    <h6 class="font-medium text-gray-900">{{ network.name }}</h6>
                    <p class="text-xs text-gray-500 truncate">{{ network.description }}</p>
                  </div>
                </div>
                <span v-if="getNetworksForUser(user).length === 0"
                  class="text-slate-400 text-sm italic">No networks</span>
              </div>
            </td>
            <td class="px-6 py-4 text-right">
              <span class="text-slate-400 text-sm">--</span>
            </td>
          </tr>
          <tr v-if="users.length === 0 && !loading">
            <td colspan="4" class="px-6 py-12 text-center text-slate-500">No
              users found.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script lang="ts" setup>
import CloudinaryFile from '@/components/cdn/CloudinaryFile.vue';
import SecurableField from '@/components/fields/SecurableField.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import useUsers from '@/composables/useUsers';
import { defaultProxy } from '@/lib/user';
import type { Network, User } from '@/types';
import { onMounted } from 'vue';

const { execute: fetchUsers, error, loading, data: users } = useUsers().fetchUsers;
const { execute: forceFetchUsers } = useUsers().forceFetchUsers;

function getNetworksForUser(user: User): Network[] {
  // assuming networks[].networkUsers contains userProxy relation
  return defaultProxy(user).networkUsers.map(nu => nu.network);
}

onMounted(async () => {
  await fetchUsers();
  console.log(users.value)
})

</script>
