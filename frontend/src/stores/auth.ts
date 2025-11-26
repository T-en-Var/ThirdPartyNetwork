import { ref, computed, type Ref } from 'vue';
import { defineStore } from 'pinia';
import { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type {
  AccessTokenClaims,
  ErrorMessage,
  NetworkClaims,
  NetworkFile,
  TokenPair,
  UserProxy,
} from '@/types';
import rawApi from '@/api/rawApi';
import api from '@/api/api';
import { useGlobalStore } from './global';
import { decodeJWT } from '@/lib/utils';
import { ClaimChecker } from '@/lib/claimChecker';

// --- Constants for Local Storage Keys ---
const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';

// --- Type Definition for the Utility Function's Context ---
// This defines exactly what the utility needs from the store, removing 'any'.
interface AuthStoreContext {
  accessToken: Ref<string | null>;
  refreshToken: Ref<string | null>;
  setNetworkClaims: () => void;
}

// --- Utility Functions for Token Management ---

/**
 * Saves tokens to local storage and updates the store state.
 * @param aToken - Access Token
 * @param rToken - Refresh Token
 * @param context - Necessary reactive references and methods from the store.
 */
function saveTokensToStorage(aToken: string, rToken: string, context: AuthStoreContext) {
  context.accessToken.value = aToken;
  context.refreshToken.value = rToken;
  localStorage.setItem(ACCESS_TOKEN_KEY, aToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, rToken);
  context.setNetworkClaims(); // Update claims immediately after saving
}

/**
 * Checks if a JWT token is expired based on the 'exp' claim.
 * @param token - The JWT string
 * @returns true if expired, false otherwise
 */
function isTokenExpired(token: string): boolean {
  const decodedPayload = decodeJWT<AccessTokenClaims>(token);
  return decodedPayload.exp < Math.floor(Date.now() / 1000);
}

// --- Pinia Auth Store Definition ---
export const useAuthStore = defineStore('auth', () => {
  // --- State ---
  const accessToken = ref<string | null>(null);
  const refreshToken = ref<string | null>(null);

  const isModalOpen = ref<boolean>(false);
  const isUnauthModalOpen = ref<boolean>(false);

  const networkClaims = ref<NetworkClaims[]>([]);
  const claimChecker = ref(new ClaimChecker([]));

  const modalMode = ref<'signup' | 'login'>('signup');
  let modalCallback = () => {};

  const currentUserProxy = ref<UserProxy | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const global = useGlobalStore();

  // --- Computed Properties ---

  const isAuthenticated = computed(() => {
    loadTokens();
    if (!accessToken.value) return false;
    return !isTokenExpired(accessToken.value);
  });

  const isAdmin = computed(() => {
    if (!isAuthenticated.value) return false;
    return false;
  });

  // --- Core Methods ---

  /**
   * Loads tokens from local storage into the store state.
   */
  function loadTokens() {
    const aToken = localStorage.getItem(ACCESS_TOKEN_KEY);
    const rToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (accessToken.value !== aToken || refreshToken.value !== rToken) {
      accessToken.value = aToken;
      refreshToken.value = rToken;
      if (aToken) {
        setNetworkClaims();
      }
    }
  }

  /**
   * Updates the network claims in the store and the ClaimChecker.
   */
  function setNetworkClaims() {
    if (!accessToken.value) {
      return clearTokens();
    }

    try {
      const decodedPayload = decodeJWT<AccessTokenClaims>(accessToken.value);

      if (!decodedPayload.networks) {
        console.error("Token missing 'networks' claim. Clearing tokens.");
        return clearTokens();
      }

      networkClaims.value = JSON.parse(decodedPayload.networks);
      claimChecker.value.setNetworkClaims(networkClaims.value);
    } catch (e) {
      console.error('Failed to decode token or parse network claims.', e);
      clearTokens();
    }
  }

  /**
   * Centralized function to save tokens and update state.
   */
  function saveTokens(aToken: string, rToken: string) {
    // Pass a context object matching the AuthStoreContext interface
    saveTokensToStorage(aToken, rToken, {
      accessToken,
      refreshToken,
      setNetworkClaims,
    });
  }

  /**
   * Centralized function to clear tokens and reset state.
   */
  function clearTokens() {
    accessToken.value = null;
    refreshToken.value = null;
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    claimChecker.value.setNetworkClaims([]);
    currentUserProxy.value = null;
  }

  // --- Authentication Flow ---

  async function login(email: string, password: string) {
    global.startFetching();
    try {
      const response = await rawApi.post<TokenPair>('/auth/login', { email, password });

      if (response.status === 200) {
        saveTokens(response.data.accessToken, response.data.refreshToken);
      }
      return response;
    } catch (err) {
      const message =
        (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to login.';
      error.value = message;
      throw new Error(message);
    } finally {
      global.stopFetching();
    }
  }

  async function refreshTokens(): Promise<string | null> {
    if (!refreshToken.value) {
      clearTokens();
      return null;
    }

    global.startFetching();
    try {
      const response = await api.refresh(refreshToken.value);
      saveTokens(response.data.accessToken, response.data.refreshToken);
      return response.data.accessToken;
    } catch (err) {
      error.value =
        (err as AxiosError<ErrorMessage>).response?.data.message ||
        'Session expired. Please log in again.';
      clearTokens();
      return null;
    } finally {
      global.stopFetching();
    }
  }

  async function applyHeaders(
    config: InternalAxiosRequestConfig,
  ): Promise<InternalAxiosRequestConfig> {
    loadTokens();

    const tokenIsExpired = accessToken.value && isTokenExpired(accessToken.value);
    const tokenIsMissingButRefreshExists = !accessToken.value && refreshToken.value;

    if (tokenIsExpired || tokenIsMissingButRefreshExists) {
      accessToken.value = await refreshTokens();
    }

    if (accessToken.value) {
      config.headers.Authorization = `Bearer ${accessToken.value}`;
    }

    return config;
  }

  async function signUp(
    username: string,
    email: string,
    firstname: string,
    lastname: string,
    password: string,
  ) {
    global.startFetching();
    try {
      const response = await rawApi.post<TokenPair>('/auth/register', {
        username,
        firstName: firstname,
        lastName: lastname,
        email,
        password,
      });

      if (response.status === 200) {
        saveTokens(response.data.accessToken, response.data.refreshToken);
      }
      return response;
    } catch (err) {
      const message =
        (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to signup.';
      error.value = message;
      throw new Error(message);
    } finally {
      global.stopFetching();
    }
  }

  // --- User Data and Proxy ---

  async function getUserProxy() {
    if (currentUserProxy.value) return currentUserProxy.value;

    if (!isAuthenticated.value) return null;

    global.startFetching();
    try {
      const response = await api.get<UserProxy>('/me');

      if (response.status === 200) {
        currentUserProxy.value = response.data;
        return currentUserProxy.value;
      } else {
        clearTokens();
        return null;
      }
    } catch (err) {
      error.value =
        (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to get user details.';
      return null;
    } finally {
      global.stopFetching();
    }
  }

  function getUserProxyId() {
    if (!accessToken.value || !isAuthenticated.value) {
      throw new Error('User not authenticated or access token is missing/expired');
    }
    const decodedPayload = decodeJWT<AccessTokenClaims>(accessToken.value);
    return decodedPayload.uid;
  }

  async function getAllUserFiles() {
    await getUserProxy();

    if (!currentUserProxy.value) return [];

    const allProxies = [
      currentUserProxy.value,
      ...(currentUserProxy.value.user.userProxies.filter((u) => u != null) ?? []),
    ];
    const files: NetworkFile[] = [];

    for (const up of allProxies) {
      for (const nu of up.networkUsers) {
        nu.userProxy = up;
        if (nu.files) {
          for (const f of nu.files) {
            f.author = nu;
            files.push(f);
          }
        }
      }
    }

    return files;
  }

  // --- Network and Modal functions (omitted for brevity, unchanged) ---
  async function networkLogin(networkId: string, email: string, password: string) {
    global.startFetching();
    try {
      return await rawApi.post<TokenPair>(`/auth/${networkId}/login`, { email, password });
    } catch (err) {
      error.value =
        (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to login to network.';
      throw err;
    } finally {
      global.stopFetching();
    }
  }

  async function networkSignUp(
    networkId: string,
    username: string,
    email: string,
    firstname: string,
    lastname: string,
    password: string,
  ) {
    global.startFetching();
    try {
      return await rawApi.post<TokenPair>(`/auth/${networkId}/register`, {
        username: username,
        firstName: firstname,
        lastName: lastname,
        email: email,
        password: password,
      });
    } catch (err) {
      error.value =
        (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to signup to network.';
      throw err;
    } finally {
      global.stopFetching();
    }
  }

  function logout() {
    clearTokens();
  }

  function setModalMode(newModalMode: 'signup' | 'login') {
    modalMode.value = newModalMode;
  }

  function setModalOpen(newModelState: boolean) {
    console.log(newModelState)
    console.log(modalCallback)
    isModalOpen.value = newModelState;
    if (newModelState) modalCallback();
  }

  function setModalOpenCallback(callback: () => void) {
    modalCallback = callback;
  }

  function setUnauthModalOpen(newModelState: boolean) {
    isUnauthModalOpen.value = newModelState;
  }

  // --- Return exposed state and actions ---
  return {
    // State
    accessToken,
    refreshToken,
    isModalOpen,
    isUnauthModalOpen,
    networkClaims,
    claimChecker: computed(() => claimChecker.value),
    modalMode,
    currentUserProxy,
    loading,
    error,

    // Computed
    isAuthenticated,
    isAdmin,

    // Actions
    applyHeaders,
    saveTokens,
    clearTokens,
    refreshTokens,
    login,
    signUp,
    networkLogin,
    networkSignUp,
    logout,
    getUserProxyId,
    getUserProxy,
    getAllUserFiles,

    // Modal Functions
    setModalOpen,
    setModalMode,
    setModalOpenCallback,
    setUnauthModalOpen,
    loadTokens,
    setNetworkClaims,
  };
});
