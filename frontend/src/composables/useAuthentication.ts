import api from '@/api/api';
import rawApi from '@/api/rawApi';
import { useGlobalStore } from '@/stores/global';
import type { ErrorMessage, TokenPair } from '@/types';
import { AxiosError } from 'axios';
import { ref } from 'vue';

export default function useAuthentication() {
  const global = useGlobalStore();

  const error = ref<string | null>(null);
  const loading = ref(false);

  const tokenPair = ref<TokenPair | null>(null);

  const handleError = (err: AxiosError<ErrorMessage>, defaultMessage: string = 'Action failed') => {
    if (err.code == AxiosError.ERR_NETWORK) {
      error.value = 'Network Error. Either you are not connected, or the server is offline.';
      return;
    }

    error.value = err.response?.data.message || err.message || defaultMessage;
  };

  const login = async (email: string, password: string) => {
    error.value = null;
    global.startFetching();
    loading.value = true;
    try {
      const response = await rawApi.post<TokenPair>('/auth/login', {
        email: email.trim(),
        password,
      });
      tokenPair.value = response.data;
      return response;
    } catch (err) {
      handleError(err as AxiosError<ErrorMessage>, 'Failed to login');
    } finally {
      loading.value = false;
      global.stopFetching();
    }
  };

  const signUp = async (
    username: string,
    email: string,
    firstname: string,
    lastname: string,
    password: string,
  ) => {
    error.value = null;
    global.startFetching();
    loading.value = true;
    try {
      const response = await rawApi.post<TokenPair>('/auth/register', {
        username: username.trim(),
        firstName: firstname.trim(),
        lastName: lastname.trim(),
        email: email.trim(),
        password,
      });

      tokenPair.value = response.data;
    } catch (err) {
      handleError(err as AxiosError<ErrorMessage>, 'Failed to sign up');
    } finally {
      loading.value = false;
      global.stopFetching();
    }
  };

  const networkLogin = async (networkId: string, email: string, password: string) => {
    error.value = null;
    global.startFetching();
    loading.value = true;
    try {
      const response = await rawApi.post<TokenPair>(`/auth/${networkId}/login`, {
        email: email.trim(),
        password: password,
      });
      tokenPair.value = response.data;
    } catch (err) {
      handleError(err as AxiosError<ErrorMessage>, 'Failed to login');
    } finally {
      loading.value = false;
      global.stopFetching();
    }
  };

  const networkSignUp = async (
    networkId: string,
    username: string,
    email: string,
    firstname: string,
    lastname: string,
    password: string,
  ) => {
    error.value = null;
    global.startFetching();
    loading.value = true;
    try {
      const response = await rawApi.post<TokenPair>(`/auth/${networkId}/register`, {
        username: username.trim(),
        firstName: firstname.trim(),
        lastName: lastname.trim(),
        email: email.trim(),
        password: password,
      });
      tokenPair.value = response.data;
    } catch (err) {
      handleError(err as AxiosError<ErrorMessage>, 'Failed to sign up');
    } finally {
      loading.value = false;
      global.stopFetching();
    }
  };

  const refreshTokens = async () => {
    global.startFetching();
    try {
      const response = await api.refresh();
      tokenPair.value = response.data;
    } catch (err) {
      const fullError = err as AxiosError<ErrorMessage>;
      error.value =
        fullError.response?.data.message || fullError.message || 'Failed to refresh session.';
    } finally {
      global.stopFetching();
    }
  };

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
