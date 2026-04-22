<template>
  <div class="space-y-4 rounded-xl border border-gray-300 bg-white p-6 shadow-md">
    <div class="flex items-start space-x-6">
      <!-- User Avatar -->
      <div>
        <div
          v-if="userProxy?.imageFile"
          class="h-20 w-20 overflow-hidden rounded-full border border-gray-300"
        >
          <ProfileAvatar
            :userProxy="userProxy"
            :size="20"
          />
        </div>
        <div
          v-else
          class="flex h-20 w-20 items-center justify-center rounded-full border border-gray-300 bg-gray-100"
        >
          <span class="text-2xl font-semibold text-gray-500">{{ getInitials() }}</span>
        </div>
      </div>

      <!-- User Information -->
      <div class="flex-1">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-xl font-semibold text-gray-900">
              {{ userProxy ? getNameDisplayUserProxy(userProxy) : 'Unknown User' }}
            </h3>
            <span
              v-if="userProxy?.isDefault"
              class="mt-1 inline-block rounded-full bg-blue-50 px-3 py-1 text-xs font-medium text-blue-700"
            >
              Default Proxy
            </span>
          </div>

          <!-- Switch Account Button -->
          <button
            v-if="showSwitch"
            @click="emit('switch-account')"
            class="rounded-md border border-gray-300 bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-200"
          >
            Switch Account
          </button>
        </div>

        <!-- UserProxy Fields -->
        <div class="mt-4 space-y-2">
          <div
            v-for="(field, key) in displayFields"
            :key="key"
            class="text-sm text-gray-700"
          >
            <div class="flex items-center">
              <span class="w-32 font-medium text-gray-600">{{ formatFieldName(key) }}:</span>
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
        class="ml-4"
      >
        <slot name="actions"></slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { UserProxy } from '@/types';
import SecurableField from '@/components/fields/SecurableField.vue';
import ProfileAvatar from '@/components/ProfileAvatar.vue';
import { getNameDisplayUserProxy } from '@/lib/user';

const props = defineProps({
  userProxy: {
    type: Object as () => UserProxy | null,
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
    const value = props.userProxy?.[key as keyof UserProxy] as string;
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

function formatDate(date: Date): string {
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
