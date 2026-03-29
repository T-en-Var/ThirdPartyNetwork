<template>
  <div class="space-y-6">
    <div
      v-for="group in entitlementGroups"
      :key="group.title"
    >
      <section
        v-if="network.entitlement && (autoIncreaseMax || network.entitlement?.[group.allowKey])"
        class="mb-4 overflow-hidden rounded-xl border border-slate-200"
      >
        <div class="flex flex-row justify-between border-b border-slate-200 bg-slate-50 px-4 py-2">
          <h3 class="text-[10px] font-black uppercase tracking-widest text-slate-500">
            {{ group.title }}
          </h3>

          <label class="group relative inline-flex cursor-pointer items-center">
            <input
              type="checkbox"
              v-model="localValue[group.allowKey as keyof SettableEntitlementForm]"
              class="peer sr-only"
            />
            <div
              class="peer h-5 w-9 rounded-full bg-slate-200 after:absolute after:left-[2px] after:top-[2px] after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-all after:content-[''] group-hover:after:scale-110 peer-checked:bg-blue-600 peer-checked:after:translate-x-full"
            ></div>
          </label>
        </div>

        <div
          v-if="localValue[group.allowKey as keyof SettableEntitlementForm]"
          class="space-y-6 p-4"
        >
          <div
            v-for="limit in group.limits"
            :key="limit.key"
            class="space-y-3"
          >
            <div class="flex items-center justify-between">
              <span class="text-sm font-bold text-slate-700">{{ limit.label }}</span>
              <label class="group relative inline-flex cursor-pointer items-center">
                <input
                  type="checkbox"
                  v-model="localValue[limit.setKey]"
                  class="peer sr-only"
                />
                <div
                  class="peer h-5 w-9 rounded-full bg-slate-200 after:absolute after:left-[2px] after:top-[2px] after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-all after:content-[''] group-hover:after:scale-110 peer-checked:bg-blue-600 peer-checked:after:translate-x-full"
                ></div>
              </label>
            </div>

            <div
              :class="[
                'transition-all duration-300',
                localValue[limit.setKey]
                  ? 'opacity-100'
                  : 'pointer-events-none opacity-30 grayscale',
              ]"
            >
              <div class="mb-2 flex items-center justify-between">
                <span
                  v-if="!autoIncreaseMax"
                  class="text-[10px] font-bold uppercase text-slate-400"
                >
                  Range: 0 - {{ Number(network.entitlement[limit.key]) }}
                </span>
                <span v-else></span>
                {{ limit.key }}

                <div
                  class="flex items-center gap-1 rounded border border-slate-200 bg-white px-2 py-0.5 shadow-sm transition-colors focus-within:border-blue-400"
                >
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
                    class="w-20 appearance-none bg-transparent text-right text-xs font-bold text-blue-600 outline-none"
                  />
                  <span
                    v-if="limit.unit"
                    class="text-[9px] font-black uppercase text-slate-400"
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
                class="h-1 w-full cursor-pointer appearance-none rounded-lg bg-slate-200 accent-blue-600 transition-all hover:accent-blue-700"
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

/**
 * Ensures the value stays within 0 and the network max.
 */
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
