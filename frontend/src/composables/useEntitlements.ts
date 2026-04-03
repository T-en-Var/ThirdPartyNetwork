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

export function useEntitlements(network: Network) {
  const entitlementsData = ref<SettableEntitlementForm>({});

  const initEntitlements = (
    sourceEntitlements: SettableEntitlement,
    unsetIsOff: boolean = true,
  ) => {
    if (!network.entitlement) return;

    Object.entries(entitlementKeys).forEach(([key, list]) => {
      if (!unsetIsOff && sourceEntitlements[key as keyof SettableEntitlement]) {
        (entitlementsData.value[key as keyof SettableEntitlementForm] as boolean | undefined) =
          true;
      }
      list.forEach((itemKey) => {
        const value = sourceEntitlements[itemKey];
        const turnOn = unsetIsOff ? value !== undefined && value !== null : !!value;
        if (turnOn) {
          (entitlementsData.value[itemKey] as number | boolean | undefined) = value;
          const setKey = `set${capitalize(itemKey)}` as keyof SettableEntitlementForm;
          (entitlementsData.value[setKey] as boolean | undefined) = true;
          (entitlementsData.value[key as keyof SettableEntitlementForm] as boolean | undefined) =
            true;
        }
      });
    });
  };

  const getSubmitData = () => {
    const result: SettableEntitlement = {};

    Object.entries(entitlementKeys).forEach(([key, list]) => {
      if (!entitlementsData.value[key as keyof SettableEntitlement]) return;

      (result[key as keyof SettableEntitlement] as boolean | undefined) = true;

      list.forEach((itemKey) => {
        const value = entitlementsData.value[itemKey];
        if (value !== undefined || value !== null) {
          const setKey = `set${capitalize(itemKey)}` as keyof SettableEntitlementForm;
          if (entitlementsData.value[setKey]) {
            (result[itemKey as keyof SettableEntitlement] as number | undefined) = value;
          }
        }
      });
    });

    return result;
  };

  return { entitlementsData, initEntitlements, getSubmitData };
}
