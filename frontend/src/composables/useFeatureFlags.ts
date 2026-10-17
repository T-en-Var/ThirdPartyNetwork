import { capitalize, ref } from 'vue';
import type { NetworkDto, SettableFeatureFlagForm, SettableFeatureFlag } from '@/types';

export const featureFlagKeys = {
  allowFiles: ['fileCountLimit', 'fileSizeLimit', 'fileStorageLimit'],
  allowBlogs: ['blogCountLimit'],
  allowConfigurations: ['configurationCountLimit'],
  allowCustomPages: [
    'customPageCountLimit',
    'customPageBlockCountLimit',
    'customPageBlockSizeLimit',
  ],
} as const;

type FeatureFlagKey = keyof typeof featureFlagKeys;
type FeatureFlagLimitKey = (typeof featureFlagKeys)[FeatureFlagKey][number];

function setField<K extends keyof SettableFeatureFlagForm>(
  form: SettableFeatureFlagForm,
  key: K,
  value: SettableFeatureFlagForm[K],
) {
  form[key] = value;
}

function setFlagFor(itemKey: FeatureFlagLimitKey): keyof SettableFeatureFlagForm {
  return `set${capitalize(itemKey)}` as keyof SettableFeatureFlagForm;
}

export function useFeatureFlags(network: NetworkDto) {
  const featureFlagsData = ref<SettableFeatureFlagForm>({});

  const initFeatureFlags = (
    sourceFeatureFlags: SettableFeatureFlag,
    unsetIsOff: boolean = true,
  ) => {
    if (!network.featureFlag) return;

    (Object.keys(featureFlagKeys) as FeatureFlagKey[]).forEach((flagKey) => {
      const limitKeys: readonly FeatureFlagLimitKey[] = featureFlagKeys[flagKey];

      if (!unsetIsOff && sourceFeatureFlags[flagKey]) {
        setField(featureFlagsData.value, flagKey, true);
      }

      limitKeys.forEach((itemKey) => {
        const value = sourceFeatureFlags[itemKey];
        const turnOn = unsetIsOff ? value !== undefined && value !== null : !!value;
        if (turnOn) {
          setField(featureFlagsData.value, itemKey, value);
          setField(featureFlagsData.value, setFlagFor(itemKey), true);
          setField(featureFlagsData.value, flagKey, true);
        }
      });
    });
  };

  const getSubmitData = () => {
    const result: SettableFeatureFlag = {};

    (Object.keys(featureFlagKeys) as FeatureFlagKey[]).forEach((flagKey) => {
      const limitKeys: readonly FeatureFlagLimitKey[] = featureFlagKeys[flagKey];
      if (!featureFlagsData.value[flagKey]) return;

      result[flagKey] = true;

      limitKeys.forEach((itemKey) => {
        const value = featureFlagsData.value[itemKey];
        if (value !== undefined || value !== null) {
          if (featureFlagsData.value[setFlagFor(itemKey)]) {
            result[itemKey] = value;
          }
        }
      });
    });

    return result;
  };

  return { featureFlagsData, initFeatureFlags, getSubmitData };
}
