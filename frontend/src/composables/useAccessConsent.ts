import type { NetworkAccess } from '@/types';
import api from '@/api/api';

export interface AccessConsentState {
  value: boolean;
  userChecked: boolean;
}

/**
 * Shared logic for the network access-consent step, duplicated across the
 * signup/join/complete-access flows:
 *  - Seeding the `Record<accessId, { value, userChecked }>` state used to
 *    drive the access checkboxes.
 *  - Sending the `PUT /networks/:networkId/users/:networkUserId/accesses/:accessId`
 *    calls that persist the user's consent for a set of accesses.
 */
export default function useAccessConsent() {
  /**
   * Builds the initial consent state for a list of network accesses.
   *
   * @param networkAccesses The accesses defined on the network.
   * @param getInitialValue Optional callback to determine the starting
   *   `value` for each access (e.g. based on already-accepted accesses, or
   *   whether the access is required). Defaults to `false` for every access.
   */
  function buildInitialAccessState(
    networkAccesses: NetworkAccess[],
    getInitialValue: (access: NetworkAccess) => boolean = () => false,
  ): Record<string, AccessConsentState> {
    const state: Record<string, AccessConsentState> = {};

    for (const access of networkAccesses) {
      state[access.accessId] = { value: getInitialValue(access), userChecked: false };
    }

    return state;
  }

  /**
   * Applies a single consent decision (accept/reject) to a list of accesses.
   *
   * @param networkId The network the accesses belong to.
   * @param networkUserId The network-user the consent applies to.
   * @param accessIds The access ids to update.
   * @param isAccepted Whether the accesses should be marked as accepted.
   * @param accessToken Optional bearer token to use instead of the default
   *   authenticated `api` client. Needed for pre-auth flows (signup / OAuth
   *   completion) where the user isn't fully logged in yet.
   */
  async function applyAccessConsent(
    networkId: string,
    networkUserId: string,
    accessIds: string[],
    isAccepted: boolean,
    accessToken?: string,
  ): Promise<void> {
    const config = accessToken ? { headers: { Authorization: `Bearer ${accessToken}` } } : undefined;

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
