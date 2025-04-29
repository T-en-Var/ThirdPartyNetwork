<template>
  <div class="bg-white border border-gray-300 rounded-xl shadow-md p-6 space-y-4">
    <div class="flex items-start space-x-6">
      <!-- User Avatar -->
      <div>
        <div v-if="userProxy?.imageUrl" class="h-20 w-20 rounded-full overflow-hidden border border-gray-300">
          <ProfileAvatar
            :userProxy="userProxy"
            :size="20"
          />
        </div>
        <div v-else class="h-20 w-20 rounded-full bg-gray-100 flex items-center justify-center border border-gray-300">
          <span class="text-gray-500 text-2xl font-semibold">{{ getInitials() }}</span>
        </div>
      </div>

      <!-- User Information -->
      <div class="flex-1">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-xl font-semibold text-gray-900">{{ getDisplayName() }}</h3>
            <span v-if="userProxy?.isDefault" 
              class="inline-block mt-1 px-3 py-1 text-xs font-medium bg-blue-50 text-blue-700 rounded-full">
              Default Proxy
            </span>
          </div>

          <!-- Switch Account Button -->
          <button 
            @click="emit('switch-account')" 
            class="px-4 py-2 text-sm font-medium bg-gray-100 hover:bg-gray-200 text-gray-700 border border-gray-300 rounded-md transition">
            Switch Account
          </button>
        </div>

        <!-- UserProxy Fields -->
        <div class="mt-4 space-y-2">
          <div v-for="(field, key) in displayFields" :key="key" class="text-sm text-gray-700">
            <div class="flex items-center">
              <span class="font-medium text-gray-600 w-32">{{ formatFieldName(key) }}:</span>
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
      <div v-if="$slots.actions" class="ml-4">
        <slot name="actions"></slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, defineProps } from 'vue';
import type { UserProxy } from '@/types';
import SecurableField from '@/components/fields/SecurableField.vue'
import ProfileAvatar from '@/components/ProfileAvatar.vue'

const props = defineProps({
  userProxy: {
    type: Object as () => UserProxy | null,
    required: true
  },
  // Object defining which fields are sensitive
  sensitiveFields: {
    type: Array as () => string[],
    default: () => ['email', 'lastName']
  },
  // Fields to display (if not specified, show all available)
  fieldsToDisplay: {
    type: Array as () => string[],
    default: () => []
  }
});

const emit = defineEmits(['switch-account']);

// State for field visibility
const visibleFields = ref<Record<string, boolean>>({});

// Compute fields to display based on userProxy and fieldsToDisplay props
const displayFields = computed(() => {
  if (!props.userProxy) return {};
  
  const fields: Record<string, { value: string, sensitive: boolean }> = {};
  
  // Get all available fields from userProxy
  const allFields = Object.keys(props.userProxy).filter(key => 
    key !== 'id' && // Skip ID
    key !== 'user' && // Skip user object
    key !== 'isDefault' && // Handled separately
    key !== 'imageUrl' && // Handled separately
    key !== 'createdOn' // Skip or format differently if needed
  );
  
  // Use specified fields or all available fields
  const fieldsToUse = props.fieldsToDisplay.length > 0 
    ? props.fieldsToDisplay.filter(f => allFields.includes(f))
    : allFields;
  
  // Build fields object
  fieldsToUse.forEach(key => {
    const value = props.userProxy?.[key as keyof UserProxy] as string;
    if (value !== undefined) {
      fields[key] = {
        value,
        sensitive: props.sensitiveFields.includes(key)
      };
    }
  });
  
  // Add formatted date if we want to show it
  if (props.fieldsToDisplay.includes('createdOn') || props.fieldsToDisplay.length === 0) {
    fields['createdOn'] = {
      value: formatDate(props.userProxy.createdOn),
      sensitive: false
    };
  }
  
  return fields;
});

// Format field names for display
function formatFieldName(key: string): string {
  // Convert camelCase to Title Case with spaces
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase());
}

// Format date for display
function formatDate(date: Date): string {
  if (!date) return 'Unknown';
  return new Date(date).toLocaleDateString();
}

// Get user's display name
function getDisplayName(): string {
  if (!props.userProxy) return 'User';
  
  // if (props.userProxy.firstName && props.userProxy.lastName) {
  //   return `${props.userProxy.firstName} ${props.userProxy.lastName}`;
  // }
  
  // if (props.userProxy.firstName) {
  //   return props.userProxy.firstName;
  // }
  
  if (props.userProxy.username) {
    return props.userProxy.username;
  }
  
  return 'User';
}

// Get user's initials for avatar fallback
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

// Toggle field visibility
function toggleFieldVisibility(key: string): void {
  visibleFields.value[key] = !visibleFields.value[key];
}
</script>