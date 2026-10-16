<template>
  <modal-container
    :title="`Manage User: ${getNameDisplayUserProxy(selectedUser.userProxy)}`"
    @close="$emit('close')"
    :close-on-outside-click="false"
  >
    <div class="edit-user-modal__ids">
      <div class="edit-user-modal__id-row">
        <span class="edit-user-modal__id-label">Network User ID</span>
        <span class="edit-user-modal__id-value">
          {{ selectedUser.id }}
        </span>
      </div>
      <div class="edit-user-modal__id-row">
        <span class="edit-user-modal__id-label">User Proxy ID</span>
        <span class="edit-user-modal__id-value">
          {{ selectedUser.userProxyId }}
        </span>
      </div>
    </div>

    <div class="edit-user-modal__body">
      <!-- Roles Section -->
      <div>
        <h3 class="edit-user-modal__section-title">User Roles</h3>
        <p class="edit-user-modal__section-subtitle">Manage roles assigned to this user</p>

        <checkbox-list
          :items="availableRoles"
          v-model="localForm.roleIds"
          id-prefix="role"
          empty-message="No roles available in this network"
          container-class="edit-user-modal__roles-list"
        />
      </div>

      <div class="network-modal__info-banner">
        <span class="network-modal__info-icon">i</span>
        <p class="network-modal__info-text">
          Unspecified entitlements will default to role-specific/network-wide limits.
        </p>
      </div>

      <entitlements-form
        :network="network"
        v-model="entitlementsData"
      />

      <modal-form-actions
        :is-submitting="isSubmitting"
        :bordered="false"
        submit-type="button"
        submit-label="Save Changes"
        submitting-label="Saving..."
        @cancel="$emit('close')"
        @submit="handleSubmit"
      />
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import ModalFormActions from '@/components/modals/ModalFormActions.vue';
import CheckboxList from '@/components/CheckboxList.vue';
import type { NetworkDto, NetworkUserDto, ManageUserForm, SettableEntitlement } from '@/types';
import { getNameDisplayUserProxy } from '@/lib/user';
import EntitlementsForm from '@/components/EntitlementsForm.vue';
import { useEntitlements } from '@/composables/useEntitlements';

const props = defineProps<{
  network: NetworkDto;
  selectedUser: NetworkUserDto;
  isSubmitting: boolean;
  manageUserForm: ManageUserForm;
}>();

const emit = defineEmits(['close', 'update']);

const localForm = ref({ ...props.manageUserForm });

const { entitlementsData, initEntitlements, getSubmitData } = useEntitlements(props.network);

onMounted(() => {
  initEntitlements(props.selectedUser.entitlements as SettableEntitlement);
});

watch(
  () => props.manageUserForm,
  (newVal) => {
    localForm.value = { ...newVal };
  },
);

const availableRoles = computed(() => {
  return props.network.roles || [];
});

function handleSubmit() {
  localForm.value.entitlements = getSubmitData();

  emit('update', localForm.value);
}
</script>
