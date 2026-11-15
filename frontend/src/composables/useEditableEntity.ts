import { ref, watch, type Ref, type ComputedRef } from 'vue';
import { useGlobalStore } from '@/stores/global';

export interface UseEditableEntityOptions<T> {
  /**
   * The reactive id (usually a route param, e.g. `computed(() => route.params.blogId as string)`)
   * that identifies which entity to load. Changing it triggers a refetch.
   */
  id: Ref<string> | ComputedRef<string>;
  /**
   * Fetches the entity for the given id. Mirrors the `fetch<Entity>` props the usercontent
   * manage views pass down to their Edit*Tab components, e.g.
   * `(blogId: string) => Promise<Ref<Blog | null>>`.
   */
  fetch: (id: string) => Promise<Ref<T | null>>;
  /**
   * Called every time a non-null entity value arrives (including subsequent updates to the
   * same remote ref), typically to populate a local edit form.
   */
  onLoaded?: (entity: T) => void;
  /**
   * Extra check to decide whether the fetched entity should be treated as "not found", for
   * cases where the fetched value itself is never null but the thing actually being edited is
   * derived from it (e.g. a page block looked up by id inside a fetched custom page).
   */
  isNotFound?: (entity: T) => boolean;
  /** Toast message shown when the entity can't be found. Omit to skip the toast. */
  notFoundMessage?: string;
  /** Called after a not-found toast, typically to navigate back to the parent list/page. */
  onNotFound?: () => void;
}

export interface UseEditableEntityReturn<T> {
  entity: Ref<T | null>;
  loading: Ref<boolean>;
  error: Ref<string | null>;
}

/**
 * Shared "load an entity identified by a route param, track loading/error state, and refetch
 * whenever the id changes" pattern used by the usercontent Edit*Tab components
 * (EditBlogTab, EditConfigurationTab, EditCustomPageTab, EditPageBlockTab).
 */
export function useEditableEntity<T>(
  options: UseEditableEntityOptions<T>,
): UseEditableEntityReturn<T> {
  const entity = ref<T | null>(null) as Ref<T | null>;
  const loading = ref(false);
  const error = ref<string | null>(null);
  const global = useGlobalStore();

  watch(
    options.id,
    async (newId) => {
      entity.value = null;
      loading.value = true;
      error.value = null;

      const remote = await options.fetch(newId);
      loading.value = false;

      const notFound = !remote.value || (options.isNotFound?.(remote.value) ?? false);
      if (notFound) {
        if (options.notFoundMessage) {
          global.addToast({ message: options.notFoundMessage, type: 'error', duration: 5000 });
        }
        options.onNotFound?.();
        return;
      }

      watch(
        remote,
        (newEntry) => {
          if (!newEntry) return;
          entity.value = newEntry;
          options.onLoaded?.(newEntry);
        },
        { immediate: true },
      );
    },
    { immediate: true },
  );

  return { entity, loading, error };
}
