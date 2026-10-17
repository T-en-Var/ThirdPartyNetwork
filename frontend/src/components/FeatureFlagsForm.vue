<template>
  <div class="feature-flags-form">
    <div
      v-for="group in featureFlagGroups"
      :key="group.title"
    >
      <section
        v-if="network.featureFlag && (autoIncreaseMax || network.featureFlag?.[group.allowKey])"
        class="feature-flags-form__group"
      >
        <div class="feature-flags-form__group-header">
          <h3 class="feature-flags-form__group-title">
            {{ group.title }}
          </h3>

          <label class="toggle-switch">
            <input
              type="checkbox"
              v-model="localValue[group.allowKey as keyof SettableFeatureFlagForm]"
              class="toggle-switch__input"
            />
            <div class="toggle-switch__track"></div>
          </label>
        </div>

        <div
          v-if="localValue[group.allowKey as keyof SettableFeatureFlagForm]"
          class="feature-flags-form__group-body"
        >
          <div
            v-for="limit in group.limits"
            :key="limit.key"
            class="feature-flags-form__limit"
          >
            <div class="feature-flags-form__limit-header">
              <span class="feature-flags-form__limit-label">{{ limit.label }}</span>
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
                'feature-flags-form__limit-content',
                { 'feature-flags-form__limit-content--active': localValue[limit.setKey] },
              ]"
            >
              <div class="feature-flags-form__limit-meta">
                <span
                  v-if="!autoIncreaseMax"
                  class="feature-flags-form__limit-range"
                >
                  Range: 0 - {{ Number(network.featureFlag[limit.key]) }}
                </span>
                <span v-else></span>

                <div class="feature-flags-form__limit-input-wrap">
                  <input
                    type="number"
                    v-model.number="localValue[limit.key as keyof SettableFeatureFlagForm]"
                    @blur="
                      clamp(
                        limit.key as keyof SettableFeatureFlagForm,
                        network.featureFlag[limit.key] as number,
                      )
                    "
                    @change="
                      clamp(
                        limit.key as keyof SettableFeatureFlagForm,
                        network.featureFlag[limit.key] as number,
                      )
                    "
                    class="feature-flags-form__limit-input"
                  />
                  <span
                    v-if="limit.unit"
                    class="feature-flags-form__limit-unit"
                  >
                    {{ limit.unit }}
                  </span>
                </div>
              </div>

              <input
                v-if="!autoIncreaseMax"
                type="range"
                v-model.number="localValue[limit.key as keyof SettableFeatureFlagForm]"
                :min="0"
                :max="Number(network.featureFlag[limit.key])"
                class="feature-flags-form__limit-slider"
              />
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NetworkDto, SettableFeatureFlagForm, NetworkFeatureFlagDto } from '@/types';
import { computed } from 'vue';

const props = defineProps<{
  network: NetworkDto;
  modelValue: SettableFeatureFlagForm;
  autoIncreaseMax?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const localValue = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
});

export interface FeatureFlagLimit {
  label: string;
  key: keyof NetworkFeatureFlagDto;
  setKey: keyof SettableFeatureFlagForm;
  unit?: string;
}

export interface FeatureFlagGroup {
  title: string;
  allowKey: keyof NetworkFeatureFlagDto;
  limits: FeatureFlagLimit[];
}

const featureFlagGroups: FeatureFlagGroup[] = [
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

function clamp(key: keyof SettableFeatureFlagForm, max: number) {
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
