<template>
  <modal-container
    title="User Entitlements"
    @close="$emit('close')"
    size="md"
  >
    <div
      v-if="proxy"
      class="view-user-entitlements-modal__scroll"
    >
      <div class="view-user-entitlements-modal__header">
        <h2 class="view-user-entitlements-modal__name">{{ proxy.firstName }} {{ proxy.lastName }}</h2>
        <p class="view-user-entitlements-modal__handle">@{{ proxy.username }}</p>
      </div>

      <div>
        <div
          v-if="breakdown.length === 0"
          class="view-user-entitlements-modal__empty"
        >
          This user is not a member of any networks.
        </div>

        <div
          v-for="nw in breakdown"
          :key="nw.networkId"
          class="card view-user-entitlements-modal__network-card"
        >
          <div class="view-user-entitlements-modal__network-header">
            <div>
              <span class="view-user-entitlements-modal__network-name">{{ nw.networkName }}</span>
              <div class="view-user-entitlements-modal__network-badges">
                <span
                  v-for="(val, key) in nw.baseEnt"
                  v-show="key.startsWith('allow')"
                  :key="key"
                  class="badge badge--neutral"
                >
                  <span class="view-user-entitlements-modal__badge-key">{{
                    key.replace('allow', '')
                  }}</span>
                  <span
                    class="view-user-entitlements-modal__badge-value"
                    :class="{
                      'view-user-entitlements-modal__badge-value--true': val === true,
                      'view-user-entitlements-modal__badge-value--false': val === false,
                    }"
                  >
                    {{ val }}
                  </span>
                </span>
              </div>
            </div>
            <div class="view-user-entitlements-modal__network-actions">
              <RouterLink
                :to="`/networks/${nw.networkId}/manage`"
                class="view-user-entitlements-modal__action-link"
                >Go to network</RouterLink
              >
              <button
                @click="$emit('edit-user-entitlement', nw.networkId)"
                class="view-user-entitlements-modal__action-link view-user-entitlements-modal__action-link--edit"
              >
                Edit entitlements
              </button>
            </div>
          </div>

          <div class="view-user-entitlements-modal__table-scroll">
            <table class="table">
              <thead>
                <tr class="table__row">
                  <th class="table__cell table__cell--head">Entitlement</th>
                  <th class="table__cell table__cell--head">Network Base</th>
                  <th class="table__cell table__cell--head view-user-entitlements-modal__col-role">
                    Role Overrides
                  </th>
                  <th class="table__cell table__cell--head view-user-entitlements-modal__col-user">
                    User Overrides
                  </th>
                  <th
                    class="table__cell table__cell--head view-user-entitlements-modal__col-effective"
                  >
                    Effective
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="key in nw.activeKeys"
                  :key="key"
                  class="table__row"
                >
                  <td class="table__cell">{{ formatKey(key) }}</td>

                  <td class="table__cell view-user-entitlements-modal__base-cell">
                    {{ formatValue(nw.baseEnt[key]) }}
                  </td>

                  <td class="table__cell view-user-entitlements-modal__role-cell">
                    <span
                      v-if="nw.roleEnt[key] !== undefined"
                      class="view-user-entitlements-modal__override-pill view-user-entitlements-modal__override-pill--role"
                    >
                      {{ formatValue(nw.roleEnt[key]) }}
                    </span>
                    <span
                      v-else
                      class="view-user-entitlements-modal__empty-value"
                      >-</span
                    >
                  </td>

                  <td class="table__cell view-user-entitlements-modal__user-cell">
                    <span
                      v-if="nw.userEnt[key] !== undefined"
                      class="view-user-entitlements-modal__override-pill view-user-entitlements-modal__override-pill--user"
                    >
                      {{ formatValue(nw.userEnt[key]) }}
                    </span>
                    <span
                      v-else
                      class="view-user-entitlements-modal__empty-value"
                      >-</span
                    >
                  </td>

                  <td class="table__cell view-user-entitlements-modal__effective-cell">
                    {{ formatValue(nw.effective[key]) }}
                  </td>
                </tr>
                <tr v-if="nw.activeKeys.length === 0">
                  <td
                    colspan="5"
                    class="table__cell view-user-entitlements-modal__no-limits"
                  >
                    No limits configured for this network.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <div class="view-user-entitlements-modal__footer">
      <button
        type="button"
        class="btn btn--outline"
        @click="$emit('close')"
      >
        Close
      </button>
    </div>
  </modal-container>
</template>

<script setup lang="ts">
import { capitalize, computed } from 'vue';
import ModalContainer from '@/components/modals/ModalContainer.vue';
import type { UserWithNetworksDto, NetworkEntitlementDto } from '@/types';
import { defaultProxy } from '@/lib/user';
import { entitlementKeys } from '@/composables/useEntitlements';

const props = defineProps<{
  user: UserWithNetworksDto;
}>();

defineEmits<{
  (e: 'close'): void;
  (e: 'edit-user-entitlement', networkId: string): void;
}>();

const proxy = computed(() => defaultProxy(props.user));
const allEntitlementKeys = computed(() => Object.values(entitlementKeys).flat());

const cleanObj = (obj: unknown): Record<string, number | boolean> =>
  Object.fromEntries(
    Object.entries((obj as Record<string, unknown>) || {}).filter(([_, v]) => {
      void _;
      return v !== undefined && v !== null;
    }),
  ) as Record<string, number | boolean>;

const formatKey = (key: string) => {
  return capitalize(key.replace(/([A-Z])/g, ' $1').trim());
};

const formatValue = (val?: boolean | number) => {
  if (val === undefined || val === null) return '-';
  if (val === true) return 'Yes';
  if (val === false) return 'No';
  return val.toLocaleString('nl-NL');
};

const breakdown = computed(() => {
  if (!proxy.value || !proxy.value.networkUsers) return [];

  return proxy.value.networkUsers.map((nu) => {
    const networkName = nu.network?.name || 'Unknown Network';
    const baseEnt = nu.network?.entitlement || ({} as Partial<NetworkEntitlementDto>);

    // This light network-user shape doesn't carry per-role entitlement data (only the full
    // NetworkUserDto, fetched per-network, does) - role overrides can't be shown from here.
    const roleEnt: Record<string, number | boolean> = {};

    const userEnt = cleanObj(nu.entitlements) || {};

    const effective = { ...baseEnt, ...roleEnt, ...userEnt };

    const activeKeys = allEntitlementKeys.value.filter(
      (key) =>
        baseEnt[key] !== undefined || roleEnt[key] !== undefined || userEnt[key] !== undefined,
    );

    return {
      networkId: nu.network?.id || Math.random().toString(),
      networkName,
      baseEnt,
      roleEnt,
      userEnt,
      effective,
      activeKeys,
    };
  });
});
</script>
