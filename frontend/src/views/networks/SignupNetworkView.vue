<template>
  <div class="w-full min-h-screen flex justify-center items-center bg-gray-100 relative p-5">
    <div
      class="absolute top-5 left-5 flex items-center gap-1 justify-center cursor-pointer text-sm text-slate-700 px-4 py-2 rounded bg-white shadow hover:bg-gray-100 hover:shadow-md transition"
      @click="goBack">
      <span class="relative w-4 h-4">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
          stroke-linecap="round" stroke-linejoin="round">
          <path d="M19 12H5" />
          <path d="M12 19l-7-7 7-7" />
        </svg>
      </span>
      <span>
        Back
      </span>
    </div>

    <div class="w-full max-w-xl">
      <div class="w-full bg-white rounded-lg shadow-lg overflow-hidden">
        <div class="flex justify-center py-6 relative">
          <div class="absolute top-4 left-4 z-10">
            <div class="w-14 h-14 bg-white rounded-md shadow flex items-center justify-center p-2">
              <img v-if="!networkDetails.loading.value" :src="imageSrc" alt="Network Logo"
                class="w-full h-full object-contain" />
              <div v-else class="w-7 h-7 border-4 border-gray-300 border-t-indigo-500 rounded-full animate-spin"></div>
            </div>
          </div>
        </div>

        <div class="w-full px-8 pb-10">
          <div class="mb-6 text-center">
            <h2 class="text-2xl font-semibold text-slate-800 mb-1">Create Account</h2>
            <p class="text-sm text-slate-600">
              {{ networkDetails.network.value ? `Join ${networkDetails.network.value.name}` : 'Join the platform' }}
            </p>
          </div>

          <p v-if="signupError" class="text-sm text-red-700 bg-red-100 p-3 rounded mb-4 text-center">
            {{ signupError }}
          </p>

          <form @submit.prevent="signUp" class="flex flex-col gap-5">
            <div class="flex flex-wrap gap-5">
              <div class="flex-1 min-w-[140px]">
                <label for="signup-firstname" class="block mb-2 text-sm text-slate-700 font-medium">First Name</label>
                <input id="signup-firstname" v-model="signupFirstname" type="text" required
                  placeholder="Your first name"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div class="flex-1 min-w-[140px]">
                <label for="signup-lastname" class="block mb-2 text-sm text-slate-700 font-medium">Last Name</label>
                <input id="signup-lastname" v-model="signupLastname" type="text" required placeholder="Your last name"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>

            <div class="flex flex-wrap gap-5">
              <div class="flex-1 min-w-[140px]">
                <label for="signup-username" class="block mb-2 text-sm text-slate-700 font-medium">Username</label>
                <input id="signup-username" v-model="signupUsername" type="text" required
                  placeholder="Choose a username"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div class="flex-1 min-w-[140px]">
                <label for="signup-email" class="block mb-2 text-sm text-slate-700 font-medium">Email</label>
                <input id="signup-email" v-model="signupEmail" type="email" required placeholder="Your email"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>

            <div class="flex flex-wrap gap-5">
              <div class="flex-1 min-w-[140px]">
                <label for="signup-password" class="block mb-2 text-sm text-slate-700 font-medium">Password</label>
                <input id="signup-password" v-model="signupPassword" type="password" required
                  placeholder="Create a password"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div class="flex-1 min-w-[140px]">
                <label for="confirm-password" class="block mb-2 text-sm text-slate-700 font-medium">Confirm
                  Password</label>
                <input id="confirm-password" v-model="confirmPassword" type="password" required
                  placeholder="Confirm your password"
                  class="w-full p-3 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>

            <div class="flex items-center gap-3 mt-1">
              <input id="confirm-tos" v-model="confirmToS" type="checkbox" required
                class="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500" />
              <label for="confirm-tos" class="text-sm text-slate-700">
                I accept the <span class="text-blue-600 font-medium cursor-pointer hover:underline"
                  @click="redirectToTos">Terms and Conditions</span>.
              </label>
            </div>

            <button type="submit"
              class="w-full py-3 mt-3 text-white bg-blue-600 hover:bg-blue-700 font-medium rounded transition disabled:bg-blue-300 disabled:cursor-not-allowed"
              :disabled="isSigningUp">
              {{ isSigningUp ? 'Creating account...' : 'Create Account' }}
            </button>
          </form>

          <div class="mt-6 text-center text-sm text-slate-600">
            <p>Already have an account?</p>
            <button @click="navigateToLogin" class="text-blue-600 font-medium hover:underline mt-1">Sign In</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRoute, useRouter } from 'vue-router';
import type { AxiosError } from 'axios';
import type { ErrorMessage } from '@/types';
import useNetworkDetails from '@/composables/useNetworkDetails';
import { isValidHttpUrl } from '@/lib/utils';

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();
const networkId = route.params.networkId as string;

const signupUsername = ref('');
const signupFirstname = ref('');
const signupLastname = ref('');
const signupEmail = ref('');
const signupPassword = ref('');
const confirmPassword = ref('');
const confirmToS = ref(false);
const signupError = ref('');
const isSigningUp = ref(false);
const imageSrc = ref(`https://ui-avatars.com/api/?name=default&background=random`);

const networkDetails = useNetworkDetails();

onMounted(async () => {
  await networkDetails.fetchNetworkDetails(networkId);

  if (route.query.uname) signupUsername.value = route.query.uname as string;
  if (route.query.fname) signupFirstname.value = route.query.ftname as string;
  if (route.query.lname) signupLastname.value = route.query.lname as string;
  if (route.query.email) signupEmail.value = route.query.email as string;

  if (networkDetails.network.value?.imageUrl && isValidHttpUrl(networkDetails.network.value?.imageUrl)) {
    imageSrc.value = networkDetails.network.value?.imageUrl
  } else {
    const name = `${networkDetails.network.value?.name}`
    imageSrc.value = `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=random`
  }
});

const goBack = () => {
  router.go(-1);
};

const navigateToLogin = () => {
  router.push(`/networks/${networkId}/login`);
};

const redirectToTos = () => {
  const currentPath = route.fullPath;
  const signupInfo = `?uname=${signupUsername.value}&fname=${signupFirstname.value}&lname=${signupLastname.value}&email=${signupEmail.value}`
  const encodedPath = btoa(currentPath + signupInfo);

  router.push(`/tos?redirect=${encodedPath}&fromExternal=&hideNavbar=`);
};

const signUp = async () => {
  if (!signupUsername.value || !signupPassword.value || !confirmPassword.value ||
    !signupEmail.value || !signupFirstname.value || !signupLastname.value) {
    signupError.value = 'Please fill in all fields';
    return;
  }

  if (signupPassword.value !== confirmPassword.value) {
    signupError.value = 'Passwords do not match';
    return;
  }

  if (!confirmToS.value) {
    signupError.value = 'Required to accept the Terms of Service';
    return;
  }

  isSigningUp.value = true;
  signupError.value = '';

  try {
    const response = await authStore.signUp(
      signupUsername.value.trim(),
      signupEmail.value.trim(),
      signupFirstname.value.trim(),
      signupLastname.value.trim(),
      signupPassword.value
    );

    if (response.status === 201) {
      const redirectPath = route.query.redirect
        ? atob(route.query.redirect as string)
        : `/networks/${networkId}`;

      router.push(redirectPath);
    } else {
      signupError.value = 'Unable to create account. Please try again.';
    }
  } catch (err) {
    signupError.value = (err as AxiosError<ErrorMessage>).response?.data.message || "Something went wrong, please try again.";
  } finally {
    isSigningUp.value = false;
  }
};
</script>
