import { capitalize, ref } from 'vue';
import type { Network, SettableEntitlementForm, SettableEntitlement } from '@/types';

export const entitlementKeys = {
  allowFiles: ['fileCountLimit', 'fileSizeLimit', 'fileStorageLimit'],
  allowBlogs: ['blogCountLimit'],
  allowConfigurations: ['configurationCountLimit'],
  allowCustomPages: [
    'customPageCountLimit',
    'customPageBlockCountLimit',
    'customPageBlockSizeLimit',
  ],
} as const;

type EntitlementFlagKey = keyof typeof entitlementKeys;
type EntitlementLimitKey = (typeof entitlementKeys)[EntitlementFlagKey][number];

function setField<K extends keyof SettableEntitlementForm>(
  form: SettableEntitlementForm,
  key: K,
  value: SettableEntitlementForm[K],
) {
  form[key] = value;
}

function setFlagFor(itemKey: EntitlementLimitKey): keyof SettableEntitlementForm {
  return `set${capitalize(itemKey)}` as keyof SettableEntitlementForm;
}

export function useEntitlements(network: Network) {
  const entitlementsData = ref<SettableEntitlementForm>({});

  const initEntitlements = (
    sourceEntitlements: SettableEntitlement,
    unsetIsOff: boolean = true,
  ) => {
    if (!network.entitlement) return;

    (Object.keys(entitlementKeys) as EntitlementFlagKey[]).forEach((flagKey) => {
      const limitKeys: readonly EntitlementLimitKey[] = entitlementKeys[flagKey];

      if (!unsetIsOff && sourceEntitlements[flagKey]) {
        setField(entitlementsData.value, flagKey, true);
      }

      limitKeys.forEach((itemKey) => {
        const value = sourceEntitlements[itemKey];
        const turnOn = unsetIsOff ? value !== undefined && value !== null : !!value;
        if (turnOn) {
          setField(entitlementsData.value, itemKey, value);
          setField(entitlementsData.value, setFlagFor(itemKey), true);
          setField(entitlementsData.value, flagKey, true);
        }
      });
    });
  };

  const getSubmitData = () => {
    const result: SettableEntitlement = {};

    (Object.keys(entitlementKeys) as EntitlementFlagKey[]).forEach((flagKey) => {
      const limitKeys: readonly EntitlementLimitKey[] = entitlementKeys[flagKey];
      if (!entitlementsData.value[flagKey]) return;

      result[flagKey] = true;

      limitKeys.forEach((itemKey) => {
        const value = entitlementsData.value[itemKey];
        if (value !== undefined || value !== null) {
          if (entitlementsData.value[setFlagFor(itemKey)]) {
            result[itemKey] = value;
          }
        }
      });
    });

    return result;
  };

  return { entitlementsData, initEntitlements, getSubmitData };
}
