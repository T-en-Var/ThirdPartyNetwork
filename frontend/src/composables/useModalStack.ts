import { ref } from 'vue';

export function useModalStack(id: string) {
  const stack = ref<string[]>([]);

  function open() {
    stack.value.push(id);
  }

  function close() {
    const idx = stack.value.lastIndexOf(id);
    if (idx !== -1) stack.value.splice(idx, 1);
  }

  const isTop = () => stack.value[stack.value.length - 1] === id;

  return { open, close, isTop };
}
