import api from "@/api/api";
import type { UserProxyExtraCascadeDto } from "@/types";
import { ref } from "vue";


export const TEMPORARY_ACCESS_TOKEN_KEY = 'temporaryAccessToken';

export default function useTempAuth() {
  const temporaryAccessToken = ref<string | null>(null);

  const ensureToken = () => {
    if (temporaryAccessToken.value) return;
    
    const token = getTemporaryAccessToken();

    if (!token) {
      throw new Error('No temporary access token.');
    }

    temporaryAccessToken.value = token;
  }

  const getTemporaryAccessToken = () => {
    temporaryAccessToken.value = localStorage.getItem(TEMPORARY_ACCESS_TOKEN_KEY);
    return temporaryAccessToken.value;
  }

  const setTemporaryAccessToken = (accessToken: string) => {
    temporaryAccessToken.value = accessToken;
    return localStorage.setItem(TEMPORARY_ACCESS_TOKEN_KEY, accessToken);
  }

  const removeTemporaryAccessToken = () => {
    temporaryAccessToken.value = null;
    localStorage.removeItem(TEMPORARY_ACCESS_TOKEN_KEY);
  }

  const fetchUser = async () => {
    ensureToken();

    const userResponse = await api.get<UserProxyExtraCascadeDto>(`/me`, {
      headers: { Authorization: `Bearer ${temporaryAccessToken.value}` },
    });

    if (userResponse.status !== 200) {
      throw new Error('Failed to fetch user data.');
    }

    return userResponse.data;
  }

  return {
    getTemporaryAccessToken,
    setTemporaryAccessToken,
    removeTemporaryAccessToken,
    fetchUser
  }
}