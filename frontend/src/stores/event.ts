import type { Blog, Configuration, CustomPage, NetworkFile, PageBlock } from '@/types';
import { defineStore } from 'pinia';

const defineDomains = <T extends Record<keyof AppEventPayloads, readonly string[]>>(
  domains: T & {
    [K in keyof AppEventPayloads]: Exclude<keyof AppEventPayloads[K], T[K][number]> extends never // Check for missing keys
      ? // Check for extra/invalid keys
        Exclude<T[K][number], keyof AppEventPayloads[K]> extends never
        ? T[K] // match
        : readonly ['❌ EXTRA EVENT DETECTED:', Exclude<T[K][number], keyof AppEventPayloads[K]>]
      : readonly ['❌ MISSING EVENT:', Exclude<keyof AppEventPayloads[K], T[K][number]>];
  },
) => domains;

export interface AppEventPayloads {
  test: {
    myevent: [payload: string];
  };
  file: {
    openEdit: [file: NetworkFile];
    update: [file: NetworkFile];
  };
  blogs: {
    create: [blog: Blog];
    update: [blog: Blog];
  };
  configurations: {
    create: [configuration: Configuration];
    update: [configuration: Configuration];
  };
  customPages: {
    create: [customPage: CustomPage];
    update: [customPage: CustomPage];
  };
  pageBlocks: {
    create: [pageBlock: PageBlock];
    delete: [pageBlock: PageBlock];
  };
}

const EVENT_DOMAINS = defineDomains({
  test: ['myevent'] as const,
  file: ['openEdit', 'update'] as const,
  blogs: ['create', 'update'] as const,
  configurations: ['create', 'update'] as const,
  customPages: ['create', 'update'] as const,
  pageBlocks: ['create', 'delete'] as const,
});

type EmitMap = {
  [Domain in keyof AppEventPayloads]: {
    [Action in keyof AppEventPayloads[Domain]]: AppEventPayloads[Domain][Action] extends unknown[]
      ? (...args: AppEventPayloads[Domain][Action]) => void
      : never;
  };
};

type ListenMap = {
  [Domain in keyof AppEventPayloads]: {
    [Action in keyof AppEventPayloads[Domain]]: AppEventPayloads[Domain][Action] extends unknown[]
      ? (
          callback: (...args: AppEventPayloads[Domain][Action]) => void,
          once?: boolean,
        ) => () => void
      : never;
  };
};

type GenericEventListener = (...args: unknown[]) => void;

export const useEventStore = defineStore('event', () => {
  const listeners = new Map<string, Set<GenericEventListener>>();

  const emit = {} as EmitMap;
  const listen = {} as ListenMap;

  for (const [domain, actions] of Object.entries(EVENT_DOMAINS)) {
    type DomainKey = keyof AppEventPayloads;
    const currentDomain = domain as DomainKey;

    const domainEmit: Record<string, GenericEventListener> = {};
    const domainListen: Record<string, (cb: GenericEventListener) => () => void> = {};

    for (const action of actions) {
      const eventKey = `${currentDomain}:${action}`;

      domainEmit[action] = (...args: unknown[]) => {
        const eventListeners = listeners.get(eventKey);

        if (eventListeners) {
          eventListeners.forEach((cb) => cb(...args));
        }
      };

      domainListen[action] = (cb: GenericEventListener, once = false) => {
        if (!listeners.has(eventKey)) {
          listeners.set(eventKey, new Set());
        }

        const unregister = () => {
          listeners.get(eventKey)?.delete(finalCallback);
        };

        const finalCallback: GenericEventListener = (...args: unknown[]) => {
          if (once) unregister();
          cb(...args);
        };

        listeners.get(eventKey)!.add(finalCallback);
        
        let totalEverywhere = 0;
        for (const eventSet of listeners.values()) {
          totalEverywhere += eventSet.size;
        }
        console.log(`Registering listener on ${eventKey}. Total of ${totalEverywhere}`);

        return unregister;
      };
    }

    (emit as Record<DomainKey, unknown>)[currentDomain] = domainEmit;
    (listen as Record<DomainKey, unknown>)[currentDomain] = domainListen;
  }

  return {
    emit,
    listen,
  };
});
