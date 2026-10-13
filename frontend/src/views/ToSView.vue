<template>
  <div class="tos-page">
    <div class="card">
      <div class="card__header">
        <h1 class="card__title">Terms of Service</h1>
      </div>

      <div class="card__body">
        <div class="tos-page__content prose">
          <h2>Data Access and Privacy</h2>

          <div class="alert alert--info">
            <span
              ><strong>Important:</strong> Networks only access data you explicitly allow them to
              read from you. You maintain control over your information at all times.</span
            ><br />
            <span
              >Also, this is very incomplete. If something is unclear, I, the author and developer,
              will have the final say in any matter.</span
            >
          </div>

          <h3>Data Access Controls</h3>
          <p>
            Our platform is designed with privacy and security as core principles. When you join a
            network, you explicitly control what information is shared:
          </p>

          <ul>
            <li>All data access is opt-in, requiring your explicit permission</li>
            <li>You can revoke access to your data at any time</li>
            <li>Network administrators cannot override your privacy settings</li>
            <li>You will be notified of any changes to network access requests</li>
          </ul>

          <h3>Your Rights and Responsibilities</h3>
          <p>As a user of our platform, you have the right to:</p>

          <ul>
            <li>Review all data being shared with each network</li>
            <li>Export your personal data in a machine-readable format</li>
            <li>Request deletion of your account and associated data</li>
            <li>Receive notifications about data access changes</li>
          </ul>

          <h3>Security Measures</h3>
          <p>
            We implement industry-standard security practices to ensure your data remains protected:
          </p>

          <ul>
            <li>End-to-end encryption for sensitive information</li>
            <li>Regular security audits and vulnerability assessments</li>
            <li>Strict access controls to prevent unauthorized access</li>
            <li>Continuous monitoring for suspicious activities</li>
          </ul>

          <h3>Updates to Terms</h3>
          <p>
            We may update these terms from time to time. Significant changes will be communicated to
            you directly. Continued use of the platform constitutes acceptance of the updated terms.
          </p>
        </div>

        <div class="tos-page__actions">
          <template v-if="hasRedirect">
            <button
              @click="handleContinue"
              class="btn btn--success"
            >
              Continue Signing Up
            </button>
          </template>
          <template v-else>
            <router-link
              to="/"
              class="btn btn--primary"
            >
              Back to Home
            </router-link>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import { safeAtob } from '@/lib/utils';
import { useRoute, useRouter } from 'vue-router';

const authStore = useAuthStore();

const route = useRoute();
const router = useRouter();

const hasRedirect = route.fullPath.includes('redirect');

const handleContinue = () => {
  if (route.query.fromExternal !== undefined) {
    router.push(safeAtob(route.query.redirect as string | undefined) || '/');
    return;
  }

  authStore.setModalOpen(true);
  authStore.setModalMode('signup');
};
</script>
