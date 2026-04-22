<template>
  <div class="relative z-30">
    <div
      :class="['transition-all duration-300 ease-in-out', isOpen ? 'w-64 p-4' : 'w-0 p-0']"
    ></div>
    <aside
      class="fixed bottom-0 left-0 top-0 z-30 w-64 border-r bg-gray-50 p-4 transition-transform duration-300 ease-in-out"
      :class="{ '-translate-x-full': !isOpen, 'translate-x-0': isOpen }"
    >
      <!-- Header -->
      <div
        v-if="isOpen"
        class="mb-6 mt-20 flex items-center justify-between"
      >
        <slot name="header"></slot>
        <div></div>

        <button
          @click="isOpen = false"
          class="rounded p-1 transition hover:bg-gray-200"
          aria-label="Collapse sidebar"
        >
          <svg
            class="h-4 w-4"
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
        class="flex flex-col gap-6 text-sm"
      >
        <div
          v-for="category in navItems"
          :key="category.title"
          class="space-y-2"
        >
          <h2 class="px-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
            {{ category.title }}
          </h2>

          <div class="flex flex-col">
            <div
              v-for="item in category.items"
              :key="item.page"
            >
              <RouterLink
                :to="`${basePath}/${item.page}`"
                class="block w-full rounded-md px-4 py-2 text-gray-700 transition hover:bg-blue-100 hover:text-blue-800"
                exact-active-class="bg-blue-200 text-blue-800 font-medium"
              >
                {{ item.label }}
              </RouterLink>

              <!-- Sub-items -->
              <div
                v-if="item.subItems && item.subItems.length"
                class="ml-6 flex flex-col"
              >
                <RouterLink
                  v-for="sub in processedSubItems(item)"
                  :key="sub.page"
                  :to="`${basePath}/${item.getURI(sub)}`"
                  class="block w-full rounded-md px-3 py-1.5 text-xs text-gray-600 transition hover:bg-blue-50 hover:text-blue-700"
                  exact-active-class="bg-blue-100 text-blue-800 font-medium"
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
      class="fixed top-4 z-50 mt-20 rounded-r-full border bg-white py-1 pr-2 text-gray-700 shadow transition hover:bg-blue-100"
      aria-label="Expand sidebar"
    >
      <svg
        class="h-5 w-5"
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
