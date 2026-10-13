<template>
  <div class="entitlements-form">
    <div
      v-for="group in entitlementGroups"
      :key="group.title"
    >
      <section
        v-if="network.entitlement && (autoIncreaseMax || network.entitlement?.[group.allowKey])"
        class="entitlements-form__group"
      >
        <div class="entitlements-form__group-header">
          <h3 class="entitlements-form__group-title">
            {{ group.title }}
          </h3>

          <label class="toggle-switch">
            <input
              type="checkbox"
              v-model="localValue[group.allowKey as keyof SettableEntitlementForm]"
              class="toggle-switch__input"
            />
            <div class="toggle-switch__track"></div>
          </label>
        </div>

        <div
          v-if="localValue[group.allowKey as keyof SettableEntitlementForm]"
          class="entitlements-form__group-body"
        >
          <div
            v-for="limit in group.limits"
            :key="limit.key"
            class="entitlements-form__limit"
          >
            <div class="entitlements-form__limit-header">
              <span class="entitlements-form__limit-label">{{ limit.label }}</span>
              <label class="toggle-switch">
                <input
                  type="checkbox"
                  v-model="localValue[limit.setKey]"
                  class="toggle-switch__input"
                />
                <div class="toggle-switch__track"></div>
              </label>
            </div>

            <div
              :class="[
                'entitlements-form__limit-content',
                { 'entitlements-form__limit-content--active': localValue[limit.setKey] },
              ]"
            >
              <div class="entitlements-form__limit-meta">
                <span
                  v-if="!autoIncreaseMax"
                  class="entitlements-form__limit-range"
                >
                  Range: 0 - {{ Number(network.entitlement[limit.key]) }}
                </span>
                <span v-else></span>

                <div class="entitlements-form__limit-input-wrap">
                  <input
                    type="number"
                    v-model.number="localValue[limit.key as keyof SettableEntitlementForm]"
                    @blur="
                      clamp(
                        limit.key as keyof SettableEntitlementForm,
                        network.entitlement[limit.key] as number,
                      )
                    "
                    @change="
                      clamp(
                        limit.key as keyof SettableEntitlementForm,
                        network.entitlement[limit.key] as number,
                      )
                    "
                    class="entitlements-form__limit-input"
                  />
                  <span
                    v-if="limit.unit"
                    class="entitlements-form__limit-unit"
                  >
                    {{ limit.unit }}
                  </span>
                </div>
              </div>

              <input
                v-if="!autoIncreaseMax"
                type="range"
                v-model.number="localValue[limit.key as keyof SettableEntitlementForm]"
                :min="0"
                :max="Number(network.entitlement[limit.key])"
                class="entitlements-form__limit-slider"
              />
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Network, SettableEntitlementForm, NetworkEntitlement } from '@/types';
import { computed } from 'vue';

const props = defineProps<{
  network: Network;
  modelValue: SettableEntitlementForm;
  autoIncreaseMax?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const localValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
});

export interface EntitlementLimit {
  label: string;
  key: keyof NetworkEntitlement;
  setKey: keyof SettableEntitlementForm;
  unit?: string;
}

export interface EntitlementGroup {
  title: string;
  allowKey: keyof NetworkEntitlement;
  limits: EntitlementLimit[];
}

const entitlementGroups: EntitlementGroup[] = [
  {
    title: 'File Management',
    allowKey: 'allowFiles',
    limits: [
      { label: 'File Count Limit', key: 'fileCountLimit', setKey: 'setFileCountLimit' },
      { label: 'File Size Limit', key: 'fileSizeLimit', setKey: 'setFileSizeLimit', unit: 'KB' },
      {
        label: 'Storage Total Limit',
        key: 'fileStorageLimit',
        setKey: 'setFileStorageLimit',
        unit: 'KB',
      },
    ],
  },
  {
    title: 'Blog Settings',
    allowKey: 'allowBlogs',
    limits: [{ label: 'Blog Count Limit', key: 'blogCountLimit', setKey: 'setBlogCountLimit' }],
  },
  {
    title: 'Configurations',
    allowKey: 'allowConfigurations',
    limits: [
      {
        label: 'Config Count Limit',
        key: 'configurationCountLimit',
        setKey: 'setConfigurationCountLimit',
      },
    ],
  },
  {
    title: 'Custom Pages',
    allowKey: 'allowCustomPages',
    limits: [
      {
        label: 'Custom Page Count Limit',
        key: 'customPageCountLimit',
        setKey: 'setCustomPageCountLimit',
      },
      {
        label: 'Custom Page Block Count Limit',
        key: 'customPageBlockCountLimit',
        setKey: 'setCustomPageBlockCountLimit',
      },
      {
        label: 'Custom Page Block Size Limit',
        key: 'customPageBlockSizeLimit',
        setKey: 'setCustomPageBlockSizeLimit',
        unit: 'Chars',
      },
    ],
  },
];

function clamp(key: keyof SettableEntitlementForm, max: number) {
  const val = props.modelValue[key];
  if (typeof val !== 'number') return;

  let clamped = val;
  const maxNum = Number(max);

  if (val < 0) clamped = 0;
  else if (!props.autoIncreaseMax && val > maxNum) clamped = maxNum;

  if (clamped !== val) {
    const updatedForm = { ...props.modelValue, [key]: clamped };
    emit('update:modelValue', updatedForm);
  }
}
</script>
