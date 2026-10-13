<template>
  <div class="dual-slider">
    <div class="dual-slider__track"></div>

    <div
      class="dual-slider__track-fill"
      :style="trackStyle"
    ></div>

    <input
      type="range"
      :value="values[0]"
      :min="min"
      :max="max"
      @input="(e) => handleMinInput(e.target as HTMLInputElement)"
      class="dual-range-input dual-range-input--min"
    />
    <input
      type="range"
      :value="values[1]"
      :min="min"
      :max="max"
      @input="(e) => handleMaxInput(e.target as HTMLInputElement)"
      class="dual-range-input dual-range-input--max"
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
    modelValue: () => [0, 100],
    min: 0,
    max: 100,
  },
);

const emit = defineEmits(['update:modelValue']);

const values = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
});

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
