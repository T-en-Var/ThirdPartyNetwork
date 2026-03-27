<template>
  <div class="space-y-6">
    <div v-for="group in entitlementGroups" :key="group.title">
      <section v-if="network.entitlement && (autoIncreaseMax || network.entitlement?.[group.allowKey])"
        class="border border-slate-200 rounded-xl overflow-hidden mb-4">
        <div
          class="flex flex-row justify-between bg-slate-50 px-4 py-2 border-b border-slate-200">
          <h3
            class="text-[10px] font-black uppercase tracking-widest text-slate-500">
            {{ group.title }}
          </h3>

          <label class="relative inline-flex items-center cursor-pointer group">
            <input type="checkbox"
              v-model="localValue[group.allowKey as keyof SettableEntitlementForm]"
              class="sr-only peer">
            <div
              class="w-9 h-5 bg-slate-200 rounded-full peer peer-checked:bg-blue-600 after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:after:translate-x-full group-hover:after:scale-110">
            </div>
          </label>
        </div>

        <div v-if="localValue[group.allowKey as keyof SettableEntitlementForm]"
          class="p-4 space-y-6">
          <div v-for="limit in group.limits" :key="limit.key" class="space-y-3">

            <div class="flex items-center justify-between">
              <span class="text-sm font-bold text-slate-700">{{ limit.label }}</span>
              <label class="relative inline-flex items-center cursor-pointer group">
                <input type="checkbox" v-model="localValue[limit.setKey]"
                  class="sr-only peer">
                <div
                  class="w-9 h-5 bg-slate-200 rounded-full peer peer-checked:bg-blue-600 after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:after:translate-x-full group-hover:after:scale-110">
                </div>
              </label>
            </div>

            <div :class="[
              'transition-all duration-300',
              localValue[limit.setKey] ? 'opacity-100' : 'opacity-30 grayscale pointer-events-none'
            ]">
              <div class="flex justify-between items-center mb-2">
                <span v-if="!autoIncreaseMax" class="text-[10px] text-slate-400 font-bold uppercase">
                  Range: 0 - {{ Number(network.entitlement[limit.key]) }}
                </span>
                <span v-else></span>
                {{ limit.key }}

                <div
                  class="flex items-center gap-1 bg-white border border-slate-200 rounded px-2 py-0.5 shadow-sm focus-within:border-blue-400 transition-colors">
                  <input type="number"
                    v-model.number="localValue[limit.key as keyof SettableEntitlementForm]"
                    @blur="clamp(limit.key as keyof SettableEntitlementForm, network.entitlement[limit.key] as number)"
                    @change="clamp(limit.key as keyof SettableEntitlementForm, network.entitlement[limit.key] as number)"
                    class="w-20 text-xs font-bold text-blue-600 outline-none text-right bg-transparent appearance-none" />
                  <span v-if="limit.unit"
                    class="text-[9px] font-black text-slate-400 uppercase">
                    {{ limit.unit }}
                  </span>
                </div>
              </div>

              <input v-if="!autoIncreaseMax" type="range"
                v-model.number="localValue[limit.key as keyof SettableEntitlementForm]"
                :min="0" :max="Number(network.entitlement[limit.key])"
                class="w-full h-1 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-blue-600 hover:accent-blue-700 transition-all" />
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
  set: (val) => emit('update:modelValue', val)
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
      { label: 'Storage Total Limit', key: 'fileStorageLimit', setKey: 'setFileStorageLimit', unit: 'KB' },
    ]
  },
  {
    title: 'Blog Settings',
    allowKey: 'allowBlogs',
    limits: [
      { label: 'Blog Count Limit', key: 'blogCountLimit', setKey: 'setBlogCountLimit' },
    ]
  },
  {
    title: 'Configurations',
    allowKey: 'allowConfigurations',
    limits: [
      { label: 'Config Count Limit', key: 'configurationCountLimit', setKey: 'setConfigurationCountLimit' },
    ]
  },
  {
    title: 'Custom Pages',
    allowKey: 'allowCustomPages',
    limits: [
      { label: 'Custom Page Count Limit', key: 'customPageCountLimit', setKey: 'setCustomPageCountLimit' },
      { label: 'Custom Page Block Count Limit', key: 'customPageBlockCountLimit', setKey: 'setCustomPageBlockCountLimit' },
      { label: 'Custom Page Block Size Limit', key: 'customPageBlockSizeLimit', setKey: 'setCustomPageBlockSizeLimit', unit: 'Chars' },
    ]
  }
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