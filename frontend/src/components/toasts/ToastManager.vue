<template>
  <div class="pointer-events-none fixed bottom-6 left-6 z-[9999] flex w-80 flex-col">
    <TransitionGroup name="toast">
      <div
        v-for="toast in displayToasts"
        :key="toast.id"
        class="toast-wrapper"
      >
        <div class="toast-sizer">
          <div
            class="toast-content pointer-events-auto relative flex w-full items-center justify-between overflow-hidden rounded-lg px-4 py-2 text-sm font-medium shadow-xl"
            :class="getToastStyle(toast.type)"
            @mouseenter="pauseTimer(toast.id)"
            @mouseleave="resumeTimer(toast.id)"
          >
            <div class="flex items-center gap-3">
              <svg
                v-if="toast.id === 'system-fetching'"
                class="h-5 w-5 animate-spin"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                ></circle>
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                ></path>
              </svg>

              <svg
                v-else-if="toast.type === 'success'"
                class="h-5 w-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>

              <svg
                v-else-if="toast.type === 'error'"
                class="h-5 w-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>

              <svg
                v-else-if="toast.type === 'warning'"
                class="h-5 w-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>

              <svg
                v-else
                class="h-5 w-5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>

              <span>{{ toast.message }}</span>
            </div>

            <button
              v-if="toast.id !== 'system-fetching'"
              @click="$emit('remove', toast.id)"
              class="relative z-10 flex h-6 w-6 items-center justify-center rounded-md transition-colors hover:bg-black/20"
            >
              <svg
                class="h-4 w-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>

            <div
              v-if="toast.duration"
              class="progress-bar absolute bottom-0 left-0 h-1 bg-white/30"
              :style="{ '--duration': `${toast.duration}ms` }"
            ></div>
          </div>
        </div>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { computed, watch, onUnmounted } from 'vue';
import type { ToastList, ToastType } from '@/types';

const props = defineProps<{
  toasts: ToastList;
  isFetching: boolean;
}>();

const emit = defineEmits<{
  (e: 'remove', id: string): void;
}>();

interface PausableTimer {
  remaining: number;
  start: number;
  timeoutId: number;
  isPaused: boolean;
  pause: () => void;
  resume: () => void;
}

const activeTimers = new Map<string, PausableTimer>();

const getToastStyle = (type?: ToastType | string) => {
  switch (type) {
    case 'success':
      return 'bg-emerald-600 text-white';
    case 'error':
      return 'bg-red-600 text-white';
    case 'warning':
      return 'bg-amber-500 text-white';
    case 'info':
    default:
      return 'bg-gray-900 text-white';
  }
};

const displayToasts = computed<ToastList>(() => {
  const list = [...props.toasts];
  if (props.isFetching) {
    list.push({
      id: 'system-fetching',
      message: 'Fetching...',
    });
  }
  return list;
});

const pauseTimer = (id: string) => {
  activeTimers.get(id)?.pause();
};

const resumeTimer = (id: string) => {
  activeTimers.get(id)?.resume();
};

watch(
  () => props.toasts,
  (newToasts) => {
    const currentIds = new Set<string>();

    newToasts.forEach((toast) => {
      currentIds.add(toast.id);

      if (toast.duration && !activeTimers.has(toast.id)) {
        const timerState: PausableTimer = {
          remaining: toast.duration,
          start: Date.now(),
          timeoutId: 0,
          isPaused: false,

          pause() {
            if (this.isPaused) return;
            window.clearTimeout(this.timeoutId);
            this.remaining -= Date.now() - this.start;
            this.isPaused = true;
          },

          resume() {
            if (!this.isPaused && this.timeoutId) return;
            this.start = Date.now();
            this.timeoutId = window.setTimeout(() => {
              emit('remove', toast.id);
              activeTimers.delete(toast.id);
            }, this.remaining);
            this.isPaused = false;
          },
        };

        timerState.resume();
        activeTimers.set(toast.id, timerState);
      }
    });

    for (const [id, timer] of activeTimers.entries()) {
      if (!currentIds.has(id)) {
        window.clearTimeout(timer.timeoutId);
        activeTimers.delete(id);
      }
    }
  },
  { deep: true, immediate: true },
);

onUnmounted(() => {
  for (const timer of activeTimers.values()) {
    window.clearTimeout(timer.timeoutId);
  }
  activeTimers.clear();
});
</script>

<style scoped>
.toast-wrapper {
  display: grid;
  grid-template-rows: 1fr;
  margin-top: 12px;
  transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.toast-sizer {
  overflow: hidden;
  min-height: 0;
}

.toast-enter-from,
.toast-leave-to {
  grid-template-rows: 0fr;
  margin-top: 0;
  opacity: 0;
  transform: translateX(-20px);
}

.progress-bar {
  animation: shrink linear forwards;
  animation-duration: var(--duration);
  animation-play-state: running;
}

.toast-content:hover .progress-bar {
  animation-play-state: paused;
}

@keyframes shrink {
  from {
    width: 100%;
  }
  to {
    width: 0%;
  }
}
</style>
