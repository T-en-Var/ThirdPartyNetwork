<template>
  <modal-container
    title="Add User to Network"
    @close="$emit('close')"
  >
    <form
      @submit.prevent="handleSubmit"
      class="form"
    >
      <div class="form-field">
        <label
          for="user"
          class="form-label"
          >Select User</label
        >
        <select
          id="user"
          v-model="form.userId"
          class="form-select"
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
        <loading-error-component
          :loading="usersLoading"
          :error="usersError"
          :has-value="!!users"
        />
      </div>

      <div class="form-field">
        <label class="form-label">Assign Roles</label>
        <checkbox-list
          :items="availableRoles"
          v-model="form.roleIds"
          id-prefix="role"
          empty-message="No roles available in this network"
          container-class="add-user-modal__roles-list"
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
