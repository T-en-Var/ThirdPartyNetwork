import { computed } from 'vue';
import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router';
import type { AxiosError } from 'axios';
import type {
  AccessTokenClaims,
  AuthorizationCode,
  ErrorMessage,
  UserLogin,
  UserSignup,
} from '@/types';
import { decodeJWT, extractApiErrorMessage, safeBtoa } from '@/lib/utils';
import rawApi from '@/api/rawApi';
import useNetworks from '@/composables/useNetworks';

export const TEMPORARY_ACCESS_TOKEN_KEY = 'temporaryAccessToken';

/**
 * Shared state and actions for the OAuth-style network login/signup pages.
 * These pages receive `clientId`, `codeChallenge` and `state` from the external
 * client and must pass them along (properly URL-encoded) at every step.
 */
export default function useNetworkAuthFlow() {
  const route = useRoute();
  const router = useRouter();
  const networkDetails = useNetworks().fetchNetworkDetails;

  const networkId = computed(() => route.params.networkId as string);
  const clientId = computed(() => route.query.clientId as string | undefined);
  const codeChallenge = computed(() => route.query.codeChallenge as string | undefined);
  const state = computed(() => route.query.state as string | undefined);
  const backUrl = computed(() => route.query.back as string | undefined);

  const isValidUrl = computed(
    () => !!networkId.value && !!clientId.value && !!codeChallenge.value && !!state.value,
  );

  const networkNotFound = computed(() => !!networkId.value && !networkDetails.data.value);

  const loadNetwork = () => networkDetails.execute(networkId.value);

  const authQuery = computed<LocationQueryRaw>(() => ({
    clientId: clientId.value,
    codeChallenge: codeChallenge.value,
    state: state.value,
    back: backUrl.value,
  }));

  const goToLogin = () =>
    router.push({ path: `/networks/${networkId.value}/login`, query: authQuery.value });

  const goToSignup = () =>
    router.push({ path: `/networks/${networkId.value}/signup`, query: authQuery.value });

  async function authorize(
    endpoint: 'login' | 'register',
    payload: UserLogin | UserSignup,
  ): Promise<AuthorizationCode> {
    const network = networkDetails.data.value;
    if (!network) throw new Error('Network details are not available yet.');

    const response = await rawApi.post<AuthorizationCode>(
      `/auth/${networkId.value}/${endpoint}`,
      payload,
      {
        params: {
          clientId: clientId.value,
          redirectUri: network.redirectURI,
          codeChallenge: codeChallenge.value,
        },
      },
    );
    return response.data;
  }

  function buildRedirectUrl(code: string): string {
    const network = networkDetails.data.value;
    if (!network) throw new Error('Network details are not available yet.');

    const params = new URLSearchParams({ code, state: state.value || '' });
    return `${network.redirectURI}?${params}`;
  }

  const isAccessIncomplete = (accessToken: string) =>
    decodeJWT<AccessTokenClaims>(accessToken).AccessIncomplete === 'true';

  function goToCompleteAccess(accessToken: string, redirectUrl: string) {
    localStorage.setItem(TEMPORARY_ACCESS_TOKEN_KEY, accessToken);
    router.push({
      path: `/networks/${networkId.value}/complete-access`,
      query: { redirectUri: safeBtoa(redirectUrl) },
    });
  }

  function extractError(err: unknown, fallback: string): { status?: number; message: string } {
    const axiosError = err as AxiosError<ErrorMessage>;
    return {
      status: axiosError.response?.status,
      message: extractApiErrorMessage(err, fallback),
    };
  }

  return {
    networkDetails,
    networkId,
    clientId,
    codeChallenge,
    state,
    backUrl,
    isValidUrl,
    networkNotFound,
    loadNetwork,
    goToLogin,
    goToSignup,
    authorize,
    buildRedirectUrl,
    isAccessIncomplete,
    goToCompleteAccess,
    extractError,
  };
}
