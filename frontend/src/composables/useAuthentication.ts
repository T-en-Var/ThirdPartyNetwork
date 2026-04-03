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
    console.log(err);
    console.log('err.cause', err.cause);
    console.log('err.code', err.code);
    console.log('err.config', err.config);
    console.log('err.event', err.event);
    console.log('err.isAxiosError', err.isAxiosError);
    console.log('err.message', err.message);
    console.log('err.name', err.name);
    console.log('err.request', err.request);
    console.log('err.response', err.response);
    console.log('err.toJSON', err.toJSON());

    if (err.code == AxiosError.ERR_NETWORK) {
      error.value = 'Network Error. Either you are not connected, or the server is offline.';
      return;
    }

    if (err.code == AxiosError.ERR_BAD_REQUEST) {
      error.value = err.response?.data.message || err.message || defaultMessage;
      return;
    }

    const message = err.response?.data.message || err.message || defaultMessage;
    error.value = message;
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

  const refreshTokens = async (refreshToken: string) => {
    global.startFetching();
    try {
      const response = await api.refresh(refreshToken);
      tokenPair.value = response.data;
    } catch (err) {
      const fullError = err as AxiosError<ErrorMessage>;
      const message = fullError.response?.data.message || fullError.message || 'Failed to sign up.';
      error.value = message;
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
