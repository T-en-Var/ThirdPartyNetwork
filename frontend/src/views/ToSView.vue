<template>
  <div class="container mx-auto mt-8 px-4 py-8">
    <div class="overflow-hidden rounded-lg bg-white shadow-md">
      <div class="border-b border-gray-200 bg-gray-100 px-6 py-4">
        <h1 class="text-2xl font-semibold text-gray-800">Terms of Service</h1>
      </div>

      <div class="p-6">
        <div class="prose max-w-none">
          <h2 class="mb-4 text-xl font-medium text-gray-800">Data Access and Privacy</h2>

          <div class="mb-6 border-l-4 border-blue-500 bg-blue-50 p-4">
            <span class="text-blue-700"
              ><strong>Important:</strong> Networks only access data you explicitly allow them to
              read from you. You maintain control over your information at all times.</span
            ><br />
            <span
              >Also, this is very incomplete. If something is unclear, I, the author and developer,
              will have the final say in any matter.</span
            >
          </div>

          <h3 class="mb-3 text-lg font-medium text-gray-700">Data Access Controls</h3>
          <p class="mb-4 text-gray-600">
            Our platform is designed with privacy and security as core principles. When you join a
            network, you explicitly control what information is shared:
          </p>

          <ul class="mb-6 list-disc pl-6 text-gray-600">
            <li class="mb-2">All data access is opt-in, requiring your explicit permission</li>
            <li class="mb-2">You can revoke access to your data at any time</li>
            <li class="mb-2">Network administrators cannot override your privacy settings</li>
            <li class="mb-2">You will be notified of any changes to network access requests</li>
          </ul>

          <h3 class="mb-3 text-lg font-medium text-gray-700">Your Rights and Responsibilities</h3>
          <p class="mb-4 text-gray-600">As a user of our platform, you have the right to:</p>

          <ul class="mb-6 list-disc pl-6 text-gray-600">
            <li class="mb-2">Review all data being shared with each network</li>
            <li class="mb-2">Export your personal data in a machine-readable format</li>
            <li class="mb-2">Request deletion of your account and associated data</li>
            <li class="mb-2">Receive notifications about data access changes</li>
          </ul>

          <h3 class="mb-3 text-lg font-medium text-gray-700">Security Measures</h3>
          <p class="mb-4 text-gray-600">
            We implement industry-standard security practices to ensure your data remains protected:
          </p>

          <ul class="mb-6 list-disc pl-6 text-gray-600">
            <li class="mb-2">End-to-end encryption for sensitive information</li>
            <li class="mb-2">Regular security audits and vulnerability assessments</li>
            <li class="mb-2">Strict access controls to prevent unauthorized access</li>
            <li class="mb-2">Continuous monitoring for suspicious activities</li>
          </ul>

          <h3 class="mb-3 text-lg font-medium text-gray-700">Updates to Terms</h3>
          <p class="text-gray-600">
            We may update these terms from time to time. Significant changes will be communicated to
            you directly. Continued use of the platform constitutes acceptance of the updated terms.
          </p>
        </div>

        <div class="mt-4 flex justify-end">
          <template v-if="hasRedirect">
            <button
              @click="handleContinue"
              class="rounded-md bg-green-600 px-4 py-2 text-white transition-colors hover:bg-green-700"
            >
              Continue Signing Up
            </button>
          </template>
          <template v-else>
            <router-link
              to="/"
              class="rounded-md bg-blue-600 px-4 py-2 text-white transition-colors hover:bg-blue-700"
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
import { useRoute, useRouter } from 'vue-router';

const authStore = useAuthStore();

const route = useRoute();
const router = useRouter();

const hasRedirect = route.fullPath.includes('redirect');

const handleContinue = () => {
  if (route.query.fromExternal !== undefined) {
    router.push(atob(route.query.redirect as string));
    return;
  }

  authStore.setModalOpen(true);
  authStore.setModalMode('signup');
};
</script>
