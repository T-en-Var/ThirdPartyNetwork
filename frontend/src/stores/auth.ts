import { ref, computed } from 'vue';
import { defineStore } from 'pinia';
import { type InternalAxiosRequestConfig } from 'axios';
import type {
  AccessTokenClaims,
  Network,
  NetworkPermissionCollection,
  UserLogin,
  UserProxy,
  UserSignup,
} from '@/types';
import { decodeJWT } from '@/lib/utils';
import { ClaimChecker } from '@/lib/claimChecker';
import useNetworks from '@/composables/useNetworks';
import { clearAllHistoryStores } from './history';
import useAuthentication from '@/composables/useAuthentication';
import useUserProxy from '@/composables/useUserProxy';

const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';

function isTokenExpired(token: string): boolean {
  const decodedPayload = decodeJWT<AccessTokenClaims>(token);
  return decodedPayload.exp < Math.floor(Date.now() / 1000);
}

// --- Pinia Auth Store Definition ---
export const useAuthStore = defineStore('auth', () => {
  const auth = useAuthentication();
  const proxyState = useUserProxy();

  const isInitialized = ref(false);
  let initPromise: Promise<void> | null = null;

  const isModalOpen = ref<boolean>(false);
  const isUnauthModalOpen = ref<boolean>(false);
  const { execute: fetchMainNetwork, data: mainNetwork } = useNetworks().fetchMainNetwork;

  const claimChecker = new ClaimChecker();

  const modalMode = ref<'signup' | 'login'>('signup');
  let modalCallback = () => {};

  const currentUserProxy = ref<UserProxy | null>(null);
  const loading = computed(() => auth.loading.value);
  const error = computed(() => auth.error.value);
  const accessToken = ref<string | null>(null);
  const refreshToken = ref<string | null>(null);
  const permissionCollection = ref<NetworkPermissionCollection[]>([]);

  // --- Auth properties ---
  const isAuthenticated = computed(() => {
    loadTokens();
    if (!accessToken.value) return false;
    return !isTokenExpired(accessToken.value);
  });

  const canI = {
    bypassEverything: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.Administrator),
    readNetwork: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadNetwork),
    manageNetwork: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageNetwork),
    readAccess: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadAccess),
    manageAccess: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageAccess),
    readPermission: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadPermission),
    managePermission: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManagePermission),
    readRole: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadRole),
    manageRole: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageRole),
    readUser: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadUser),
    manageUser: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageUser),
    readCustomPage: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadCustomPage),
    manageCustomPage: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageCustomPage),
    readPageBlock: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadPageBlock),
    managePageBlock: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManagePageBlock),
    readFile: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadFile),
    manageFile: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageFile),
    readConfiguration: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadConfiguration),
    manageConfiguration: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageConfiguration),
    readBlog: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ReadBlog),
    manageBlog: (network: Network) => claimChecker.hasPermission(permissionCollection.value, network, claimChecker.permissions.ManageBlog),
  };

  const isSuperAdmin = async () => {
    await initialize();
    if (!isAuthenticated.value) return false;
    if (!mainNetwork.value) return false;

    const coll = await getPermissions();
    return claimChecker.hasPermission(coll, mainNetwork.value, claimChecker.permissions.Administrator);
  };

  async function getPermissions() {
    await initialize();

    const {
      execute: fetchPermissionCollection,
      error: err,
      data: collection,
    } = proxyState.fetchPermissionCollection;
    await fetchPermissionCollection();

    if (!collection.value) throw new Error(`Abnormal: Collection not found ${err.value}`);

    return collection.value;
  }

  // --- Core Methods ---
  function loadTokens() {
    const aToken = localStorage.getItem(ACCESS_TOKEN_KEY);
    const rToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (accessToken.value !== aToken || refreshToken.value !== rToken) {
      accessToken.value = aToken;
      refreshToken.value = rToken;
    }
  }

  function saveTokens(aToken: string, rToken: string) {
    accessToken.value = aToken;
    refreshToken.value = rToken;
    localStorage.setItem(ACCESS_TOKEN_KEY, aToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, rToken);
  }

  function clearTokens() {
    accessToken.value = null;
    refreshToken.value = null;
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    currentUserProxy.value = null;
  }

  // --- Authentication Flow ---
  async function login(form: UserLogin) {
    console.log("Logging in!")
    await auth.login(form.email, form.password);
    handleAuthenticated();
  }

  async function signUp(form: UserSignup) {
    await auth.signUp(form.username, form.email, form.firstName, form.lastName, form.password);
    handleAuthenticated();
  }

  async function networkLogin(networkId: string, form: UserLogin) {
    await auth.networkLogin(networkId, form.email, form.password);
    handleAuthenticated();
  }

  async function networkSignUp(networkId: string, form: UserSignup) {
    await auth.networkSignUp(
      networkId,
      form.username,
      form.email,
      form.firstName,
      form.lastName,
      form.password,
    );
    handleAuthenticated();
  }

  function handleAuthenticated() {
    if (error.value) return;
    if (!auth.tokenPair.value) return console.warn('TokenPair was null');

    const tokenPair = auth.tokenPair.value;
    saveTokens(tokenPair.accessToken, tokenPair.refreshToken);
    clearAllHistoryStores();
  }

  function logout() {
    clearAllHistoryStores();
    clearTokens();
  }

  async function refreshTokens() {
    if (!refreshToken.value) {
      clearTokens();
      return null;
    }

    await auth.refreshTokens(refreshToken.value);

    if (error.value) {
      clearTokens();
      return;
    }
    if (!auth.tokenPair.value) return console.warn('TokenPair was null');

    const tokenPair = auth.tokenPair.value;
    saveTokens(tokenPair.accessToken, tokenPair.refreshToken);
  }

  async function applyHeaders(
    config: InternalAxiosRequestConfig,
  ): Promise<InternalAxiosRequestConfig> {
    loadTokens();

    const tokenIsExpired = accessToken.value && isTokenExpired(accessToken.value);
    const tokenIsMissingButRefreshExists = !accessToken.value && refreshToken.value;

    if (tokenIsExpired || tokenIsMissingButRefreshExists) {
      await refreshTokens();
    }

    if (accessToken.value) {
      config.headers.Authorization = `Bearer ${accessToken.value}`;
    }

    return config;
  }

  // --- User Data ---
  async function getUserProxy() {
    await initialize();

    if (currentUserProxy.value) return currentUserProxy.value;
    if (!isAuthenticated.value) throw new Error('Cannot call this while unauthenticated.');

    const { execute: fetchMe, data: userProxy } = proxyState.fetchMe;
    await fetchMe();
    if (!userProxy.value) throw new Error('Abnormality: Logged in UserProxy not found.');

    currentUserProxy.value = userProxy.value;
    return currentUserProxy.value;
  }

  function getUserProxyId() {
    if (!accessToken.value || !isAuthenticated.value)
      throw new Error('User not authenticated or access token is missing/expired');

    const decodedPayload = decodeJWT<AccessTokenClaims>(accessToken.value);
    return decodedPayload.uid;
  }

  // --- Modal Functions ---
  function setModalMode(newModalMode: 'signup' | 'login') {
    modalMode.value = newModalMode;
  }

  function setModalOpen(newModelState: boolean) {
    isModalOpen.value = newModelState;
    if (newModelState) modalCallback();
  }

  function setModalOpenCallback(callback: () => void) {
    modalCallback = callback;
  }

  function setUnauthModalOpen(newModelState: boolean) {
    isUnauthModalOpen.value = newModelState;
  }

  // --- State Management ---
  async function initialize() {
    if (initPromise) return initPromise;

    loadTokens();

    initPromise = (async () => {
      try {
        await fetchMainNetwork();
        if (!mainNetwork.value) throw new Error('MAIN NETWORK NOT FOUND');
        isInitialized.value = true;
      } catch (e) {
        console.error(e);
        throw e;
      }
    })();

    return initPromise;
  }

  return {
    // State
    accessToken,
    refreshToken,
    isModalOpen,
    isUnauthModalOpen,
    claimChecker,
    modalMode,
    loading,
    error,

    // Data
    currentUserProxy,
    getUserProxyId,
    getUserProxy,

    // Computed
    isAuthenticated,
    isSuperAdmin,
    canI,

    // Actions
    login,
    signUp,
    networkLogin,
    networkSignUp,
    logout,
    loadTokens,
    clearTokens,
    refreshTokens,
    applyHeaders,
    initialize,

    // Modal Functions
    setModalOpen,
    setModalMode,
    setModalOpenCallback,
    setUnauthModalOpen,
  };
});
