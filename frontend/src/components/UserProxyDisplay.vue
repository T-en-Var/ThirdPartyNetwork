<template>
  <div class="user-proxy-display">
    <div class="user-proxy-display__row">
      <!-- User Avatar -->
      <div>
        <div
          v-if="userProxy?.imageFile"
          class="user-proxy-display__avatar"
        >
          <ProfileAvatar
            :userProxy="userProxy"
            :size="80"
          />
        </div>
        <div
          v-else
          class="user-proxy-display__avatar user-proxy-display__avatar--fallback"
        >
          <span class="user-proxy-display__initials">{{ getInitials() }}</span>
        </div>
      </div>

      <!-- User Information -->
      <div class="user-proxy-display__info">
        <div class="user-proxy-display__header">
          <div>
            <h3 class="user-proxy-display__name">
              {{ userProxy ? getNameDisplayUserProxy(userProxy) : 'Unknown User' }}
            </h3>
            <span
              v-if="userProxy?.isDefault"
              class="badge badge--info"
            >
              Default Proxy
            </span>
          </div>

          <!-- Switch Account Button -->
          <button
            v-if="showSwitch"
            @click="emit('switch-account')"
            class="btn btn--secondary"
          >
            Switch Account
          </button>
        </div>

        <!-- UserProxyDto Fields -->
        <div class="user-proxy-display__fields">
          <div
            v-for="(field, key) in displayFields"
            :key="key"
            class="user-proxy-display__field"
          >
            <div class="user-proxy-display__field-row">
              <span class="user-proxy-display__field-label">{{ formatFieldName(key) }}:</span>
              <SecurableField
                :value="field.value"
                :sensitive="field.sensitive"
                @toggle="toggleFieldVisibility(key)"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Optional Slot for Custom Actions -->
      <div
        v-if="$slots.actions"
        class="user-proxy-display__actions"
      >
        <slot name="actions"></slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { UserProxyDto } from '@/types';
import SecurableField from '@/components/fields/SecurableField.vue';
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import { getNameDisplayUserProxy } from '@/lib/user';

const props = defineProps({
  userProxy: {
    type: Object as () => UserProxyDto | null,
    required: true,
  },
  showSwitch: {
    type: Boolean,
    default: () => true,
  },
  sensitiveFields: {
    type: Array as () => string[],
    default: () => ['email', 'lastName'],
  },
  fieldsToDisplay: {
    type: Array as () => string[],
    default: () => [],
  },
});

const emit = defineEmits(['switch-account']);

const visibleFields = ref<Record<string, boolean>>({});

const displayFields = computed(() => {
  if (!props.userProxy) return {};

  const fields: Record<string, { value: string; sensitive: boolean }> = {};

  const allFields = Object.keys(props.userProxy).filter(
    (key) =>
      key !== 'id' &&
      key !== 'user' &&
      key !== 'isDefault' &&
      key !== 'imageUrl' &&
      key !== 'createdOn',
  );

  const fieldsToUse =
    props.fieldsToDisplay.length > 0
      ? props.fieldsToDisplay.filter((f) => allFields.includes(f))
      : allFields;

  fieldsToUse.forEach((key) => {
    const value = props.userProxy?.[key as keyof UserProxyDto] as string;
    if (value !== undefined) {
      fields[key] = {
        value,
        sensitive: props.sensitiveFields.includes(key),
      };
    }
  });

  if (props.fieldsToDisplay.includes('createdOn') || props.fieldsToDisplay.length === 0) {
    fields['createdOn'] = {
      value: formatDate(props.userProxy.createdOn),
      sensitive: false,
    };
  }

  return fields;
});

function formatFieldName(key: string): string {
  return key.replace(/([A-Z])/g, ' $1').replace(/^./, (str) => str.toUpperCase());
}

function formatDate(date: string | Date): string {
  if (!date) return 'Unknown';
  return new Date(date).toLocaleDateString();
}

function getInitials(): string {
  if (!props.userProxy) return 'U';

  if (props.userProxy.firstName && props.userProxy.lastName) {
    return `${props.userProxy.firstName.charAt(0)}${props.userProxy.lastName.charAt(0)}`;
  }

  if (props.userProxy.firstName) {
    return props.userProxy.firstName.charAt(0);
  }

  if (props.userProxy.username) {
    return props.userProxy.username.charAt(0).toUpperCase();
  }

  return 'U';
}

function toggleFieldVisibility(key: string): void {
  visibleFields.value[key] = !visibleFields.value[key];
}
</script>
