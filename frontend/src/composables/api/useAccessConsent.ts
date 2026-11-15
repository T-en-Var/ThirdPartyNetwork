import type { NetworkAccessDto, NetworkAccessNtADto } from '@/types';
import api from '@/api/api';

export interface AccessConsentState {
  value: boolean;
  userChecked: boolean;
}

export default function useAccessConsent() {
  function buildInitialAccessState(
    networkAccesses: NetworkAccessDto[] | NetworkAccessNtADto[],
    getInitialValue: (access: NetworkAccessDto | NetworkAccessNtADto) => boolean = () => false,
  ): Record<string, AccessConsentState> {
    const state: Record<string, AccessConsentState> = {};

    for (const access of networkAccesses) {
      state[access.access.id] = { value: getInitialValue(access), userChecked: false };
    }

    return state;
  }

  async function applyAccessConsent(
    networkId: string,
    networkUserId: string,
    accessIds: string[],
    isAccepted: boolean,
    accessToken?: string,
  ): Promise<void> {
    const config = accessToken
      ? { headers: { Authorization: `Bearer ${accessToken}` } }
      : undefined;

    await Promise.all(
      accessIds.map((accessId) =>
        api.put(
          `/networks/${networkId}/users/${networkUserId}/accesses/${accessId}/`,
          { isAccepted },
          config,
        ),
      ),
    );
  }

  return { buildInitialAccessState, applyAccessConsent };
}
