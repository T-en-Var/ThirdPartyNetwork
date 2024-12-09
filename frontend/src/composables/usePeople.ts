import { ref } from 'vue'
import api from '@/api/api.ts'
import type { AxiosError } from 'axios'
import type { PaginatedResult, Person } from '@/types'

export default function usePeople() {
  const people = ref<Person[]>([] as Person[]);
  const loading = ref<boolean>(false);
  const error = ref<AxiosError | null>(null);

  const fetchPeople = async () => {
    loading.value = true;
    try {
      const { results } = await api.get<PaginatedResult<Person>>('/people')
      people.value = results
    } catch (err) {
      error.value = err as AxiosError;
    } finally {
      loading.value = false;
    }
  };

  return { people, loading, error, fetchPeople };
}
