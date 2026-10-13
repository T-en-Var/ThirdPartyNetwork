<template>
  <div class="sidebar">
    <div :class="['sidebar__spacer', isOpen ? 'sidebar__spacer--open' : 'sidebar__spacer--closed']"></div>
    <aside
      class="sidebar__panel"
      :class="{ 'sidebar__panel--closed': !isOpen, 'sidebar__panel--open': isOpen }"
    >
      <!-- Header -->
      <div
        v-if="isOpen"
        class="sidebar__header"
      >
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
      <nav
        v-if="isOpen"
        class="sidebar__nav"
      >
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
