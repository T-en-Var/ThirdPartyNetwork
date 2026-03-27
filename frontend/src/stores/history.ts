import type { Blog, Configuration, CustomPage, Network, PageBlock, User, UserProxy } from '@/types';
import { defineStore } from 'pinia';
import { reactive } from 'vue';

export const DEFAULT_STORES = {
  adminPage: 'admin-page',
  account: 'account',
};

export interface HistoryModelMap {
  userProxies: UserProxy;
  networks: Network;
  users: User;
  configurations: Configuration;
  blogs: Blog;
  customPages: CustomPage;
  pageBlocks: PageBlock;
}

export type HistoryType = keyof HistoryModelMap;

export const HISTORY_LIMITS: Record<HistoryType, number> = {
  userProxies: 3,
  networks: 3,
  users: 3,
  configurations: 3,
  blogs: 3,
  customPages: 3,
  pageBlocks: 10,
};

function genericHistory<T extends { id: string }>(theList: T[], theItem: T, max: number): T[] {
  const filteredList = theList.filter((x) => x.id !== theItem.id);
  filteredList.push(theItem);
  if (filteredList.length > max) filteredList.splice(0, filteredList.length - max);
  return filteredList;
}

const activeStores = new Set<{ reset: () => void }>();

export const useHistoryStore = (namespace: string) => {
  const store = defineStore(`history-${namespace}`, () => {
    const data = reactive(
      (Object.keys(HISTORY_LIMITS) as HistoryType[]).reduce(
        (acc, key) => {
          acc[key] = [];
          return acc;
        },
        {} as { [K in HistoryType]: HistoryModelMap[K][] },
      ),
    );

    const getStorageKey = (type: HistoryType) => `history_${namespace}_${type}`;

    type VisitMap = { [K in HistoryType]: (item: HistoryModelMap[K]) => void };

    const visit = (Object.keys(HISTORY_LIMITS) as HistoryType[]).reduce((acc, key) => {
      acc[key] = (item: { id: string }) => {
        const currentList = data[key] as { id: string }[];
        const updatedList = genericHistory(currentList, item, HISTORY_LIMITS[key]);

        (data as Record<HistoryType, unknown[]>)[key] = updatedList;

        localStorage.setItem(getStorageKey(key), JSON.stringify(updatedList));
      };

      return acc;
    }, {} as VisitMap);

    type RemoveMap = { [K in HistoryType]: (itemOrId: HistoryModelMap[K] | string) => void };

    const remove = (Object.keys(HISTORY_LIMITS) as HistoryType[]).reduce((acc, key) => {
      acc[key] = (itemOrId: { id: string } | string) => {
        const idToRemove = typeof itemOrId === 'string' ? itemOrId : itemOrId.id;
        const currentList = data[key] as { id: string }[];

        const updatedList = currentList.filter((x) => x.id !== idToRemove);

        (data as Record<HistoryType, unknown[]>)[key] = updatedList;

        localStorage.setItem(getStorageKey(key), JSON.stringify(updatedList));
      };

      return acc;
    }, {} as RemoveMap);

    function initialize() {
      (Object.keys(HISTORY_LIMITS) as HistoryType[]).forEach((type) => {
        const stored = localStorage.getItem(getStorageKey(type));
        if (stored) {
          try {
            (data as Record<HistoryType, unknown[]>)[type] = JSON.parse(stored) as unknown[];
          } catch {
            (data as Record<HistoryType, never[]>)[type] = [];
          }
        }
      });
    }

    function reset() {
      (Object.keys(HISTORY_LIMITS) as HistoryType[]).forEach((type) => {
        localStorage.removeItem(getStorageKey(type));
        (data as Record<HistoryType, never[]>)[type] = [];
      });
    }

    initialize();

    return {
      data,
      visit,
      remove,
      initialize,
      reset,
    };
  })();

  activeStores.add(store);

  return store;
};

export function clearAllHistoryStores() {
  activeStores.forEach((store) => store.reset());

  const keysToRemove: string[] = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key?.startsWith('history_')) {
      keysToRemove.push(key);
    }
  }

  keysToRemove.forEach((key) => localStorage.removeItem(key));
}
