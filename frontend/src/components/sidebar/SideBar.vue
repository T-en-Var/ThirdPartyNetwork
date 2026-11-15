<template>
  <div class="sidebar" :class="{ 'sidebar--closed': !isOpen }">
    <div class="sidebar__track">
      <aside
        class="sidebar__panel"
         :inert="!isOpen"
      >
        <!-- Header -->
        <div class="sidebar__header">
          <slot name="header"></slot>
          <div></div>
  
          <button
            @click="isOpen = false"
            class="sidebar__collapse-btn"
            aria-label="Collapse sidebar"
          >
            <svg
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>
        </div>
  
        <!-- Navigation -->
        <nav class="sidebar__nav">
          <div
            v-for="category in navItems"
            :key="category.title"
            class="sidebar__category"
          >
            <h2 class="sidebar__category-title">
              {{ category.title }}
            </h2>
  
            <div class="sidebar__links">
              <div
                v-for="item in category.items"
                :key="item.page"
              >
                <RouterLink
                  :to="`${basePath}/${item.page}`"
                  class="sidebar__link"
                  exact-active-class="sidebar__link--active"
                >
                  {{ item.label }}
                </RouterLink>
  
                <!-- Sub-items -->
                <div
                  v-if="item.subItems && item.subItems.length"
                  class="sidebar__sub-links"
                >
                  <RouterLink
                    v-for="sub in processedSubItems(item)"
                    :key="sub.page"
                    :to="`${basePath}/${item.getURI(sub)}`"
                    class="sidebar__sub-link"
                    exact-active-class="sidebar__sub-link--active"
                  >
                    {{ sub.label }}
                  </RouterLink>
                </div>
              </div>
            </div>
          </div>
        </nav>
      </aside>
    </div>

    <!-- Expand button -->
    <button
      v-if="!isOpen"
      @click="isOpen = true"
      class="sidebar__expand-btn"
      aria-label="Expand sidebar"
    >
      <svg
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M9 5l7 7-7 7"
        />
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
import type { NavCategory, NavItem } from '@/types/sidebar';
import { ref } from 'vue';

const props = defineProps<{
  navItems: NavCategory[];
  basePath: string;
  subLabelKey: string;
  subReverse?: boolean;
}>();

const isOpen = ref(true);

const processedSubItems = (item: NavItem) => {
  if (!item.subItems) return [];
  const list = [...item.subItems];
  return props.subReverse ? list.reverse() : list;
};
</script>

<style scoped lang="scss">
$sidebar-width: 16rem;
$sidebar-transition: 0.3s ease-in-out;

.sidebar {
  position: relative;
  z-index: 30;
  flex-shrink: 0;
}
.sidebar__track {
  width: $sidebar-width;
  height: 100%;
  overflow: hidden;
  transition: width $sidebar-transition;
}
.sidebar__panel {
  width: $sidebar-width;
  height: 100%;
  border-right: 1px solid var(--input-border);
  background-color: var(--bg-color);
  padding: 1rem;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  transition: transform $sidebar-transition;
}
.sidebar--closed {
  .sidebar__track {
    width: 0;
  }
  .sidebar__panel {
    transform: translateX(-100%);
  }
}
.sidebar__header {
  margin: 1.5rem 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.sidebar__collapse-btn {
  border-radius: 0.25rem;
  padding: 0.25rem;
  transition: background-color 0.2s ease;

  svg {
    width: 1rem;
    height: 1rem;
  }

  &:hover {
    background-color: color-mix(in srgb, var(--text-color) 10%, transparent);
  }
}
.sidebar__expand-btn {
  position: fixed;
  top: 5rem;
  left: 0;
  z-index: 50;
  border: 1px solid var(--input-border);
  border-left: none;
  border-radius: 0 9999px 9999px 0;
  background-color: var(--bg-color);
  padding: 0.25rem 0.5rem 0.25rem 0.25rem;
  color: var(--text-color);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: background-color 0.2s ease;

  svg {
    width: 1.25rem;
    height: 1.25rem;
  }

  &:hover {
    background-color: color-mix(in srgb, var(--primary-color) 25%, transparent);
  }
}
.sidebar__nav {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  font-size: 0.875rem;
}
.sidebar__category {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.sidebar__category-title {
  padding: 0 0.5rem;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-light);
}
.sidebar__links {
  display: flex;
  flex-direction: column;
}
.sidebar__link {
  display: block;
  width: 100%;
  border-radius: 0.375rem;
  padding: 0.5rem 1rem;
  color: var(--text-color);
  text-decoration: none;
  transition:
    background-color 0.2s ease,
    color 0.2s ease;

  &:hover {
    background-color: color-mix(in srgb, var(--primary-color) 20%, transparent);
    color: var(--primary-accent-color);
  }
}
.sidebar__link--active {
  background-color: color-mix(in srgb, var(--primary-color) 35%, transparent);
  color: var(--primary-accent-color);
  font-weight: 500;
}
.sidebar__sub-links {
  margin-left: 1.5rem;
  display: flex;
  flex-direction: column;
}
.sidebar__sub-link {
  display: block;
  width: 100%;
  border-radius: 0.375rem;
  padding: 0.375rem 0.75rem;
  font-size: 0.75rem;
  color: var(--text-light);
  text-decoration: none;
  transition:
    background-color 0.2s ease,
    color 0.2s ease;

  &:hover {
    background-color: color-mix(in srgb, var(--primary-color) 10%, transparent);
    color: var(--primary-accent-color);
  }
}
.sidebar__sub-link--active {
  background-color: color-mix(in srgb, var(--primary-color) 20%, transparent);
  color: var(--primary-accent-color);
  font-weight: 500;
}
</style>
