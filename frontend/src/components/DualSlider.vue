<template>
  <div class="relative flex h-6 w-full items-center">
    <div class="absolute h-1 w-full rounded-lg bg-slate-200"></div>

    <div
      class="absolute h-1 rounded-lg bg-purple-600"
      :style="trackStyle"
    ></div>

    <input
      type="range"
      :value="values[0]"
      :min="min"
      :max="max"
      @input="(e) => handleMinInput(e.target as HTMLInputElement)"
      class="dual-range-input z-30"
    />
    <input
      type="range"
      :value="values[1]"
      :min="min"
      :max="max"
      @input="(e) => handleMaxInput(e.target as HTMLInputElement)"
      class="dual-range-input z-40"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

type DualNumber = [number, number];

const props = withDefaults(
  defineProps<{
    modelValue: DualNumber;
    min?: number;
    max?: number;
  }>(),
  {
    modelValue: () => [0, 100], // Defaults for types must be wrapped in withDefaults
    min: 0,
    max: 100,
  },
);

const emit = defineEmits(['update:modelValue']);

// Local computed for easier access and validation
const values = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
});

// Calculate percentages for the colored track
const getPercent = (value: number) => {
  return ((value - props.min) / (props.max - props.min)) * 100;
};

const trackStyle = computed(() => {
  const left = getPercent(values.value[0]);
  const right = 100 - getPercent(values.value[1]);
  return {
    left: `${left}%`,
    right: `${right}%`,
  };
});

const handleMinInput = (target: HTMLInputElement) => {
  const val = Math.min(Number(target.value), values.value[1]);
  values.value = [val, values.value[1]];
};

const handleMaxInput = (target: HTMLInputElement) => {
  const val = Math.max(Number(target.value), values.value[0]);
  values.value = [values.value[0], val];
};
</script>

<style scoped>
.dual-range-input {
  @apply pointer-events-none absolute w-full appearance-none bg-transparent outline-none;
}

/* Chrome, Safari, Edge, Opera */
.dual-range-input::-webkit-slider-thumb {
  @apply pointer-events-auto h-4 w-4 cursor-pointer appearance-none rounded-full border-2 border-white bg-purple-600 shadow-md transition-transform active:scale-110;
}

/* Firefox */
.dual-range-input::-moz-range-thumb {
  @apply pointer-events-auto h-4 w-4 cursor-pointer appearance-none rounded-full border-2 border-white bg-purple-600 shadow-md transition-transform active:scale-110;
}
</style>
