import type { NetworkFile } from '@/types';
import { defineStore } from 'pinia';

const defineDomains = <T extends Record<keyof AppEventPayloads, readonly string[]>>(
  domains: T & {
    [K in keyof AppEventPayloads]: // Check for missing keys
    Exclude<keyof AppEventPayloads[K], T[K][number]> extends never
      ? // Check for extra/invalid keys
        Exclude<T[K][number], keyof AppEventPayloads[K]> extends never
        ? T[K] // Perfect match!
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
  };
}

const EVENT_DOMAINS = defineDomains({
  test: ['myevent'] as const,
  file: ['openEdit'] as const,
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
      ? (callback: (...args: AppEventPayloads[Domain][Action]) => void) => () => void
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

      // --- EMIT ---
      // We gather all arguments into the `args` array using the rest operator
      domainEmit[action] = (...args: unknown[]) => {
        const eventListeners = listeners.get(eventKey);
        if (eventListeners) {
          // Spread the arguments back out to the individual callbacks
          eventListeners.forEach((cb) => cb(...args));
        }
      };

      // --- LISTEN ---
      domainListen[action] = (cb: GenericEventListener) => {
        if (!listeners.has(eventKey)) {
          listeners.set(eventKey, new Set());
        }

        listeners.get(eventKey)!.add(cb);

        return () => {
          listeners.get(eventKey)?.delete(cb);
        };
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
