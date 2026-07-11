import api from '@/api/api';
import rawApi from '@/api/rawApi';
import type { TokenPair } from '@/types';
import { computed, ref } from 'vue';
import { useMutation } from './useApi';

export default function useAuthentication() {
  const tokenPair = ref<TokenPair | null>(null);
  const onSuccess = (data: TokenPair) => {
    tokenPair.value = data;
  };

  const loginMutation = useMutation<TokenPair, [email: string, password: string]>(
    async (email, password) =>
      await rawApi.post<TokenPair>('/auth/login', { email: email.trim(), password }),
    { onSuccess },
  );

  const signUpMutation = useMutation<
    TokenPair,
    [username: string, email: string, firstname: string, lastname: string, password: string]
  >(
    async (username, email, firstname, lastname, password) =>
      await rawApi.post<TokenPair>('/auth/register', {
        username: username.trim(),
        firstName: firstname.trim(),
        lastName: lastname.trim(),
        email: email.trim(),
        password,
      }),
    { onSuccess },
  );

  const networkLoginMutation = useMutation<
    TokenPair,
    [networkId: string, email: string, password: string]
  >(
    async (networkId, email, password) =>
      await rawApi.post<TokenPair>(`/auth/${networkId}/login`, {
        email: email.trim(),
        password,
      }),
    { onSuccess },
  );

  const networkSignUpMutation = useMutation<
    TokenPair,
    [
      networkId: string,
      username: string,
      email: string,
      firstname: string,
      lastname: string,
      password: string,
    ]
  >(
    async (networkId, username, email, firstname, lastname, password) =>
      await rawApi.post<TokenPair>(`/auth/${networkId}/register`, {
        username: username.trim(),
        firstName: firstname.trim(),
        lastName: lastname.trim(),
        email: email.trim(),
        password,
      }),
    { onSuccess },
  );

  const refreshMutation = useMutation<TokenPair, []>(async () => await api.refresh(), {
    onSuccess,
  });

  const loading = computed(
    () =>
      loginMutation.loading.value ||
      signUpMutation.loading.value ||
      networkLoginMutation.loading.value ||
      networkSignUpMutation.loading.value ||
      refreshMutation.loading.value,
  );

  const error = computed(
    () =>
      loginMutation.error.value ||
      signUpMutation.error.value ||
      networkLoginMutation.error.value ||
      networkSignUpMutation.error.value ||
      refreshMutation.error.value,
  );

  const login = (email: string, password: string) =>
    loginMutation.execute(email, password).catch(() => undefined);

  const signUp = (
    username: string,
    email: string,
    firstname: string,
    lastname: string,
    password: string,
  ) =>
    signUpMutation.execute(username, email, firstname, lastname, password).catch(() => undefined);

  const networkLogin = (networkId: string, email: string, password: string) =>
    networkLoginMutation.execute(networkId, email, password).catch(() => undefined);

  const networkSignUp = (
    networkId: string,
    username: string,
    email: string,
    firstname: string,
    lastname: string,
    password: string,
  ) =>
    networkSignUpMutation
      .execute(networkId, username, email, firstname, lastname, password)
      .catch(() => undefined);

  const refreshTokens = () => refreshMutation.execute().catch(() => undefined);

  return {
    loading,
    error,
    tokenPair,

    login,
    signUp,
    networkLogin,
    networkSignUp,

    refreshTokens,
  };
}
