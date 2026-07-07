<template>
  <modal-container
    title="Add User to Network"
    @close="$emit('close')"
  >
    <form
      @submit.prevent="handleSubmit"
      class="space-y-4"
    >
      <div>
        <label
          for="user"
          class="block text-sm font-medium text-gray-700"
          >Select User</label
        >
        <div class="relative mt-1">
          <select
            id="user"
            v-model="form.userId"
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            required
          >
            <option
              value=""
              disabled
              selected
            >
              Select a user
            </option>
            <option
              v-for="user in availableUsers"
              :key="user.id"
              :value="user.id"
            >
              {{ user.firstName }} {{ user.lastName }} ({{ user.email }})
            </option>
          </select>
        </div>
        <loading-error-component
          :loading="usersLoading"
          :error="usersError"
          :has-value="!!users"
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700">Assign Roles</label>
        <checkbox-list
          :items="availableRoles"
          v-model="form.roleIds"
          id-prefix="role"
          empty-message="No roles available in this network"
          container-class="mt-2 max-h-48 overflow-y-auto rounded-md border p-2"
        />
      </div>

      <modal-form-actions
        :is-submitting="isSubmitting ?? false"
        :disable-submit="!form.userId"
        submit-label="Add User"
        submitting-label="Adding..."
        @cancel="$emit('close')"
      />
    </form>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ModalFormActions from '@/components/modals/ModalFormActions.vue';
import CheckboxList from '@/components/CheckboxList.vue';
import LoadingErrorComponent from '@/components/LoadingErrorComponent.vue';
import type { UserProxy, Network } from '@/types';
import useUsers from '@/composables/useUsers';

const props = defineProps<{
  network: Network;
  isSubmitting?: boolean;
}>();

const emit = defineEmits(['close', 'add-user']);

const form = ref({
  userId: '',
  roleIds: [] as string[],
});

const {
  data: users,
  loading: usersLoading,
  error: usersError,
  execute: fetchUsers,
} = useUsers().fetchUsers;

const availableUsers = computed<UserProxy[]>(() => {
  if (!users.value) return [];

  const networkUserIds = props.network.networkUsers.map((nu) => nu.userProxy.id);
  return users.value
    .flatMap((user) => user.userProxies)
    .filter((proxy) => !networkUserIds.includes(proxy.id));
});

const availableRoles = computed(() => props.network.roles);

onMounted(async () => {
  await fetchUsers();
});

function handleSubmit() {
  emit('add-user', {
    userId: form.value.userId,
    roleIds: form.value.roleIds,
  });
}
</script>
