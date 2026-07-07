import type { Blog, Configuration, CustomPage, NetworkFile, PageBlock } from '@/types';
import { defineStore } from 'pinia';

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

// `satisfies` guarantees this lists exactly the domains/actions declared in
// AppEventPayloads: TypeScript errors on a missing or an extra key.
const EVENT_DOMAINS = {
  test: { myevent: null },
  file: { openEdit: null, update: null },
  blogs: { create: null, update: null },
  configurations: { create: null, update: null },
  customPages: { create: null, update: null },
  pageBlocks: { create: null, delete: null },
} satisfies { [K in keyof AppEventPayloads]: Record<keyof AppEventPayloads[K], null> };

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

    for (const action of Object.keys(actions)) {
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
