import { ref, computed, watch } from 'vue';
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

function isTokenExpired(token: string): boolean {
  const decodedPayload = decodeJWT<AccessTokenClaims>(token);
  return decodedPayload.exp < Math.floor(Date.now() / 1000);
}

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
  const permissionCollection = ref<NetworkPermissionCollection[]>([]);

  watch(permissionCollection, (newCollections) => {
    console.log('updated newCollections', newCollections);
  });

  setInterval(() => {
    if (!accessToken.value) return;
    const decodedPayload = decodeJWT<AccessTokenClaims>(accessToken.value);

    console.log(Math.round(decodedPayload.exp - Date.now()/1000), "until expiry")
  }, 3000);

  const isAuthenticated = computed(() => {
    loadTokens();
    if (!accessToken.value) return false;
    return !isTokenExpired(accessToken.value);
  });

  const canI = {
    bypassEverything: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions.Administrator,
      ),
    readNetwork: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read Network'],
      ),
    manageNetwork: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage Network'],
      ),
    readAccess: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read Access'],
      ),
    manageAccess: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage Access'],
      ),
    readPermission: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read Permission'],
      ),
    managePermission: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage Permission'],
      ),
    readRole: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read Role'],
      ),
    manageRole: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage Role'],
      ),
    readUser: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read User'],
      ),
    manageUser: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage User'],
      ),
    readCustomPage: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read CustomPage'],
      ),
    manageCustomPage: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage CustomPage'],
      ),
    readPageBlock: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read PageBlock'],
      ),
    managePageBlock: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage PageBlock'],
      ),
    readFile: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read File'],
      ),
    manageFile: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage File'],
      ),
    readConfiguration: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read Configuration'],
      ),
    manageConfiguration: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage Configuration'],
      ),
    readBlog: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Read Blog'],
      ),
    manageBlog: (network: Network) =>
      claimChecker.hasPermission(
        permissionCollection.value,
        network.id,
        claimChecker.permissions['Manage Blog'],
      ),
  };

  const isSuperAdmin = async () => {
    await initialize();
    if (!isAuthenticated.value) return false;
    if (!mainNetwork.value) return false;

    const coll = await getPermissions();
    return claimChecker.hasPermission(
      coll,
      mainNetwork.value.id,
      claimChecker.permissions.Administrator,
    );
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
    permissionCollection.value = collection.value;

    return collection.value;
  }

  // --- Core Methods ---
  function loadTokens() {
    const aToken = localStorage.getItem(ACCESS_TOKEN_KEY);

    if (accessToken.value !== aToken) {
      accessToken.value = aToken;
    }
  }

  function saveTokens(aToken: string) {
    accessToken.value = aToken;
    localStorage.setItem(ACCESS_TOKEN_KEY, aToken);
  }

  function clearTokens() {
    accessToken.value = null;
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    currentUserProxy.value = null;
  }

  // --- Authentication Flow ---
  async function login(form: UserLogin) {
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
    saveTokens(tokenPair.accessToken);
    clearAllHistoryStores();
  }

  function logout() {
    clearAllHistoryStores();
    clearTokens();
  }

  async function refreshTokens() {
    await auth.refreshTokens();

    if (error.value) {
      clearTokens();
      return;
    }
    if (!auth.tokenPair.value) return console.warn('TokenPair was null');

    const tokenPair = auth.tokenPair.value;
    saveTokens(tokenPair.accessToken);
  }

  async function applyHeaders(
    config: InternalAxiosRequestConfig,
  ): Promise<InternalAxiosRequestConfig> {
    loadTokens();

    const tokenIsExpired = accessToken.value && isTokenExpired(accessToken.value);

    if (tokenIsExpired) {
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
