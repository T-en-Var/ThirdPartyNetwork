import { ref } from 'vue';
import { defineStore } from 'pinia';
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';
import type { AccessTokenClaims, ErrorMessage, NetworkClaims, TokenPair, UserProxy } from '@/types';
import rawApi from '@/api/rawApi';
import api from '@/api/api';
import { useGlobalStore } from './global';
import { decodeJWT } from '@/lib/utils';
import { ClaimChecker } from '@/lib/claimChecker';

export const useAuthStore = defineStore('auth', () => {
    const accessToken = ref<string | null>(null);
    const refreshToken = ref<string | null>(null);

    const accessTokenKey = "access_token";
    const refreshTokenKey = "refresh_token";

    const isModelOpen = ref<boolean>(false);
    const isUnauthModalOpen = ref<boolean>(false);

    const networkClaims = ref<NetworkClaims[]>([]);
    const claimChecker = ref(new ClaimChecker([]));

    const modelMode = ref<"signup" | "login">("signup");
    let modalCallback = () => { };

    const currentUser = ref<UserProxy | null>(null);
    const loading = ref(false);
    const error = ref<string | null>(null);

    const global = useGlobalStore();

    async function login(email: string, password: string) {
        global.startFetching();
        let response;
        loading.value = true;
        try {
            response = await rawApi.post<TokenPair>('/auth/login', { email, password });
        } catch (err) {
            error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to login.';
            throw err;
        } finally {
            loading.value = false;
            global.stopFetching();
        }

        if (response.status != 200) return response;

        saveTokens(response.data.accessToken, response.data.refreshToken);
        return response;
    }

    async function signUp(username: string, email: string, firstname: string, lastname: string, password: string) {
        global.startFetching();
        let response;
        loading.value = true;
        try {
            response = await rawApi.post<TokenPair>('/auth/register', {
                username: username,
                firstName: firstname,
                lastName: lastname,
                email: email,
                password: password
            });
        } catch (err) {
            error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to signup.';
            throw err;
        } finally {
            global.stopFetching();
            loading.value = false;
        }

        saveTokens(response.data.accessToken, response.data.refreshToken);
        return response;
    }

    async function networkLogin(networkId: string, email: string, password: string) {
        global.startFetching();
        let response;
        loading.value = true;
        try {
            response = await rawApi.post<TokenPair>(`/auth/${networkId}/login`, { email, password });
        } catch (err) {
            error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to login to network.';
            throw err;
        } finally {
            global.stopFetching();
            loading.value = false;
        }

        return response;
    }

    async function networkSignUp(networkId: string, username: string, email: string, firstname: string, lastname: string, password: string) {
        global.startFetching();
        let response;
        loading.value = true;
        try {
            response = await rawApi.post<TokenPair>(`/auth/${networkId}/register`, {
                username: username,
                firstName: firstname,
                lastName: lastname,
                email: email,
                password: password
            });
        } catch (err) {
            console.log(err)
            error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to signup to network.';
            throw err;
        } finally {
            global.stopFetching();
            loading.value = false;
        }

        return response;
    }

    function logout() {
        clearTokens();
        currentUser.value = null;
    }

    async function getUserProxy() {
        if (currentUser.value) return currentUser.value;

        global.startFetching();
        let response: AxiosResponse<UserProxy>;
        loading.value = true;
        try {
            response = await api.get<UserProxy>("/me");
        } catch (err) {
            error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to get user.';
            return null;
        } finally {
            global.stopFetching();
            loading.value = false;
        }

        if (response.status != 200) {
            logout()
            return null
        }

        currentUser.value = response.data;
        return currentUser.value;
    }

    function getUserProxyId() {
        if (!isAuthenticated()) throw new Error("User not authenticated");
        const decodedPayload = decodeJWT<AccessTokenClaims>(accessToken.value!);

        return decodedPayload.uid;
    }

    function isAuthenticated() {
        loadTokens();

        if (!accessToken.value) return false;
        if (isTokenExpired(accessToken.value)) return false;

        return true;
    }

    function isAdmin() {
        if (!isAuthenticated()) return false;
        // const decodedPayload = decodeToken(accessToken.value!);

        return false;
    }

    function setNetworkClaims() {
        if (!accessToken.value) throw new Error("accesstoken undefined");
        const decodedPayload = decodeJWT<AccessTokenClaims>(accessToken.value);

        networkClaims.value = JSON.parse(decodedPayload.networks);
        claimChecker.value.setNetworkClaims(networkClaims.value);
    }

    function saveTokens(aToken: string, rToken: string) {
        accessToken.value = aToken;
        refreshToken.value = rToken;
        localStorage.setItem(accessTokenKey, aToken);
        localStorage.setItem(refreshTokenKey, rToken);
        setNetworkClaims();
    }

    function clearTokens() {
        accessToken.value = null;
        refreshToken.value = null;
        localStorage.removeItem(accessTokenKey);
        localStorage.removeItem(refreshTokenKey);
        claimChecker.value.setNetworkClaims([])
    }

    async function refreshTokens() {
        try {
            if (!refreshToken.value) {
                throw new Error("No refresh token available");
            }

            global.startFetching();
            loading.value = true;
            const response = await api.refresh(refreshToken.value);

            saveTokens(response.data.accessToken, response.data.refreshToken);
            return response.data.accessToken;
        } catch (err) {
            error.value = (err as AxiosError<ErrorMessage>).response?.data.message || 'Failed to login.';
            clearTokens();
            return null;
        } finally {
            global.stopFetching();
            loading.value = false;
        }
    }

    function loadTokens() {
        const aToken = localStorage.getItem(accessTokenKey);
        const rToken = localStorage.getItem(refreshTokenKey);

        if (accessToken.value == aToken) return;

        accessToken.value = aToken;
        refreshToken.value = rToken;
        setNetworkClaims();
    }

    async function applyHeaders(config: InternalAxiosRequestConfig) {
        loadTokens();

        if (accessToken.value && isTokenExpired(accessToken.value)) {
            accessToken.value = await refreshTokens();
        }

        else if (!accessToken.value && refreshToken.value) {
            accessToken.value = await refreshTokens();
        }

        if (accessToken.value) {
            config.headers.Authorization = `Bearer ${accessToken.value}`;
        }

        return config;
    }

    function isTokenExpired(token: string): boolean {
        const decodedPayload = decodeJWT<AccessTokenClaims>(token);
        return decodedPayload.exp < Math.floor(Date.now() / 1000);
    }

    function setModelMode(newNodalMode: "signup" | "login") {
        modelMode.value = newNodalMode;
    }

    function setModelOpen(newModelState: boolean) {
        isModelOpen.value = newModelState;

        if (newModelState) modalCallback();
    }

    function setModelOpenCallback(callback: () => void) {
        modalCallback = callback;
    }

    function setUnauthModalOpen(newModelState: boolean) {
        isUnauthModalOpen.value = newModelState;
    }

    return {
        applyHeaders,
        saveTokens,
        clearTokens,
        refreshTokens,
        isAuthenticated,
        isAdmin,
        login,
        signUp,
        networkLogin,
        networkSignUp,
        logout,
        getUserProxyId,
        getUserProxy,
        setModelOpen,
        isModelOpen,
        setModelMode,
        modelMode,
        setModelOpenCallback,
        setUnauthModalOpen,
        isUnauthModalOpen,
        loading,
        error,
        currentUser,
        networkClaims,
        claimChecker,
    };
});
