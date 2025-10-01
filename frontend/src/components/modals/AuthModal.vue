<template>
  <Teleport to="body">
    <div v-if="authStore.isModelOpen" class="modal-overlay" @click="closeModal">
      <div class="auth-modal" @click.stop>
        <div class="form-wrapper">
          <button class="close-button" @click="closeModal">×</button>

          <div class="logo-container">
            <div class="logo">
              <img src="@/images/favicon-nobg.png" alt="Company Logo">
            </div>
          </div>

          <!-- Login Form -->
          <div class="form-panel" :class="{ 'active': authStore.modelMode == 'login' }">
            <div class="form-content">
              <div class="form-header">
                <h2>Sign In</h2>
                <p class="form-subtitle">Access your account</p>
              </div>

              <p v-if="error" class="error-message">{{ error }}</p>

              <form @submit.prevent="login">
                <div class="form-group">
                  <label for="email">Email</label>
                  <input 
                    id="email" 
                    name="email" 
                    v-model="email" 
                    type="text" 
                    placeholder="Enter your email" 
                    autocomplete="username" 
                    required
                  >
                </div>

                <div class="form-group">
                  <label for="password">Password</label>
                  <input 
                    id="password" 
                    name="password" 
                    v-model="password" 
                    type="password"
                    placeholder="Enter your password" 
                    autocomplete="username" 
                    required
                  >
                </div>

                <button type="submit" class="btn-primary" :disabled="isLoading">
                  {{ isLoading ? 'Signing in...' : 'Sign In' }}
                </button>
              </form>

              <div class="form-footer">
                <p>Don't have an account?</p>
                <button @click="switchMode('signup')" class="btn-switch">Create Account</button>
              </div>
            </div>
          </div>

          <!-- Sign Up Form -->
          <div class="form-panel" :class="{ 'active': authStore.modelMode == 'signup' }">
            <div class="form-content">
              <div class="form-header">
                <h2>Create Account</h2>
                <p class="form-subtitle">Join the platform</p>
              </div>

              <p v-if="signupError" class="error-message">{{ signupError }}</p>

              <form @submit.prevent="signUp">
                <div class="form-row">
                  <div class="form-group">
                    <label for="signup-firstname">Firstname</label>
                    <input id="signup-firstname" v-model="signupFirstname" type="text" placeholder="Your firstname"
                      required>
                  </div>

                  <div class="form-group">
                    <label for="confirm-lastname">Lastname</label>
                    <input id="confirm-lastname" v-model="signupLastname" type="text" placeholder="Your lastname"
                      required>
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group">
                    <label for="signup-username">Username</label>
                    <input id="signup-username" v-model="signupUsername" type="text" placeholder="Choose a username"
                      required>
                  </div>

                  <div class="form-group">
                    <label for="signup-email">Email</label>
                    <input id="signup-email" v-model="signupEmail" type="email" placeholder="Your email" required>
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group">
                    <label for="signup-password">Password</label>
                    <input id="signup-password" v-model="signupPassword" type="password" placeholder="Create a password"
                      required>
                  </div>

                  <div class="form-group">
                    <label for="confirm-password">Confirm Password</label>
                    <input id="confirm-password" v-model="confirmPassword" type="password"
                      placeholder="Confirm your password" required>
                  </div>
                </div>

                <div class="form-group checkbox-group">
                  <div class="checkbox-container">
                    <input id="confirm-tos" v-model="confirmToS" type="checkbox" required>
                    <label for="confirm-tos">
                      I accept the <span class="tos-link" @click="redirectToTos">Terms and Conditions</span>.
                    </label>
                  </div>
                </div>

                <button type="submit" class="btn-primary" :disabled="isSigningUp">
                  {{ isSigningUp ? 'Creating account...' : 'Create Account' }}
                </button>
              </form>

              <div class="form-footer">
                <p>Already have an account?</p>
                <button @click="switchMode('login')" class="btn-switch">Sign In</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRoute, useRouter } from 'vue-router';
import type { AxiosError } from 'axios';
import type { ErrorMessage } from '@/types';

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();

const email = ref('');
const password = ref('');
const error = ref('');
const isLoading = ref(false);

const signupUsername = ref('');
const signupFirstname = ref('');
const signupLastname = ref('');
const signupEmail = ref('');
const signupPassword = ref('');
const confirmPassword = ref('');
const confirmToS = ref(false);
const signupError = ref('');
const isSigningUp = ref(false);

const modalOpenCallback = () => {
  signupUsername.value = route.query.s_username as string || ''
  signupFirstname.value = route.query.s_firstname as string || ''
  signupLastname.value = route.query.s_lastname as string || ''
  signupEmail.value = route.query.s_email as string || ''
}

authStore.setModelOpenCallback(modalOpenCallback);

const redirectToTos = () => {
  authStore.setModelOpen(false);

  let uri = route.query.redirect;
  if (route.name !== "Terms of Service") uri = btoa(route.fullPath);

  router.push(`/tos?redirect=${uri}&s_username=${signupUsername.value}&s_firstname=${signupFirstname.value}&s_lastname=${signupLastname.value}&s_email=${signupEmail.value}`)
}

// Switch between login and signup modes
const switchMode = (modalValue: "signup" | "login") => {
  authStore.setModelMode(modalValue);
  error.value = '';
  signupError.value = '';
};

const closeModal = () => {
  authStore.setModelOpen(false);

  email.value = '';
  password.value = '';
  signupUsername.value = '';
  signupPassword.value = '';
  confirmPassword.value = '';
  error.value = '';
  signupError.value = '';
  confirmToS.value = false;
};

const login = async () => {
  if (!email.value || !password.value) {
    error.value = 'Please enter both username and password';
    return;
  }

  isLoading.value = true;
  error.value = '';

  let response;

  try {
    response = await authStore.login(email.value.trim(), password.value);
  } catch (err) {
    isLoading.value = false;
    error.value = (err as AxiosError<ErrorMessage>).response?.data.message || "Something went wrong, please try again";
    return;
  }

  closeModal();

  if (response.status != 200) {
    error.value = 'Invalid credentials. Please try again.';
    return;
  }

  const redirectPath = route.query.redirect ? atob(route.query.redirect as string) : '/networks';
  router.push(redirectPath);

  isLoading.value = false;
};

const signUp = async () => {
  if (!signupUsername.value || !signupPassword.value || !confirmPassword.value || !signupEmail.value || !signupFirstname.value || !signupLastname.value) {
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

  let response;

  try {
    response = await authStore.signUp(
      signupUsername.value.trim(),
      signupEmail.value.trim(),
      signupFirstname.value.trim(),
      signupLastname.value.trim(),
      signupPassword.value
    );
  } catch (err) {
    console.error(err)
    isLoading.value = false;
    isSigningUp.value = false;
    signupError.value = (err as AxiosError<ErrorMessage>).response?.data.message || "Something went wrong, please try again.";
    return;
  }

  if (response.status < 199 || response.status > 299) {
    isLoading.value = false;
    isSigningUp.value = false;
    signupError.value = 'Unable to create account. Please try again.';
    return;
  }

  closeModal();

  const redirectPath = route.query.redirect ? atob(route.query.redirect as string) : '/networks';
  router.push(redirectPath);

  isLoading.value = false;
  isSigningUp.value = false;
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(2px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
  overflow: auto;
}

.auth-modal {
  width: 100%;
  max-width: 480px;
  position: relative;
  animation: modal-appear 0.25s ease-out;
  max-height: 90vh;
  overflow-y: auto;
}

@keyframes modal-appear {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Logo styling */
.logo-container {
  position: absolute;
  top: 16px;
  left: 16px;
  z-index: 10;
}

.logo {
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 6px;
  font-weight: 600;
  font-size: 12px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.logo img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

/* Fix close button */
.close-button {
  position: absolute;
  top: 12px;
  right: 16px;
  width: 32px;
  height: 32px;
  border-radius: 4px;
  background: transparent;
  border: none;
  font-size: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  color: #666;
  transition: all 0.2s ease;
  padding: 0 0 6px 0;
}

.close-button:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #333;
}

/* Form Styling */
.form-wrapper {
  position: relative;
  width: 100%;
  overflow: hidden;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  background: white;
}

.form-panel {
  position: absolute;
  width: 100%;
  top: 0;
  left: 0;
  opacity: 0;
  visibility: hidden;
}

.form-panel.active {
  position: relative;
  opacity: 1;
  visibility: visible;
  z-index: 2;
}

.form-content {
  width: 100%;
  padding: 2rem;
  background: white;
  color: #333;
}

/* Responsive layout for form rows */
.form-row {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 0;
}

.form-row .form-group {
  flex: 1 1 calc(50% - 8px);
  min-width: 120px;
}

/* Checkbox styling */
.checkbox-group {
  margin-top: 0.5rem;
}

.checkbox-container {
  display: flex;
  align-items: flex-start;
  gap: 8px;
}

.checkbox-container input[type="checkbox"] {
  width: 18px;
  height: 18px;
  accent-color: #3f51b5;
  cursor: pointer;
  margin-top: 2px;
}

.checkbox-container label {
  margin-bottom: 0;
  font-size: 0.89rem;
  color: #555;
  cursor: pointer;
  line-height: 1.4;
}

.tos-link {
  color: #3f51b5;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.tos-link:hover {
  color: #5c6bc0;
  text-decoration: underline;
}

/* Form header */
.form-header {
  margin-bottom: 1.5rem;
  padding-top: 0.5rem;
  text-align: center;
}

h2 {
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #333;
}

.form-subtitle {
  color: #666;
  font-size: 0.875rem;
}

/* Form elements */
form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  position: relative;
  margin-bottom: 0;
}

label {
  display: block;
  margin-bottom: 6px;
  color: #555;
  font-size: 0.875rem;
  font-weight: 500;
}

input {
  width: 100%;
  padding: 10px 12px;
  font-size: 0.9rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: white;
  color: #333;
  transition: border-color 0.2s;
}

input:focus {
  outline: none;
  border-color: #3f51b5;
  box-shadow: 0 0 0 2px rgba(63, 81, 181, 0.1);
}

/* Buttons */
.btn-primary {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 4px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
  background-color: #3f51b5;
  color: white;
  margin-top: 1rem;
}

.btn-primary:hover {
  background-color: #303f9f;
}

.btn-primary:active {
  transform: translateY(1px);
}

.btn-primary:disabled {
  background-color: #c5cae9;
  cursor: not-allowed;
}

/* Form footer */
.form-footer {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  gap: 0.5rem;
  margin-top: 1.5rem;
  color: #666;
  font-size: 0.85rem;
  flex-wrap: wrap;
}

.btn-switch {
  background: none;
  border: none;
  color: #3f51b5;
  font-weight: 500;
  font-size: 0.85rem;
  cursor: pointer;
  transition: color 0.2s;
}

.btn-switch:hover {
  color: #5c6bc0;
  text-decoration: underline;
}

/* Error messages */
.error-message {
  color: #d32f2f;
  margin-bottom: 1rem;
  text-align: center;
  font-size: 0.85rem;
  padding: 8px 12px;
  border-radius: 4px;
  background: #ffebee;
}

/* Responsive adjustments */
@media (max-width: 576px) {
  .form-content {
    padding: 1.5rem;
  }

  .form-row {
    flex-direction: column;
    gap: 1rem;
  }

  .form-row .form-group {
    width: 100%;
  }

  .auth-modal {
    max-width: 100%;
    max-height: 100vh;
    border-radius: 0;
  }

  .form-wrapper {
    border-radius: 0;
    height: 100%;
  }

  .modal-overlay {
    padding: 0;
  }

  h2 {
    font-size: 1.3rem;
  }

  .form-footer {
    flex-direction: column;
    text-align: center;
  }
}

/* Handle very small screens */
@media (max-width: 360px) {
  .form-content {
    padding: 1rem;
  }

  .btn-primary {
    padding: 10px;
  }

  input {
    padding: 8px 10px;
  }
}

/* Handle tall forms on small screens */
@media (max-height: 700px) and (max-width: 576px) {
  .form-content {
    padding-top: 2.5rem;
  }

  .form-header {
    margin-bottom: 1rem;
  }

  form {
    gap: 1rem;
  }
}

/* Handle landscape orientation on mobile */
@media (max-height: 500px) {
  .auth-modal {
    max-height: 100vh;
    overflow-y: auto;
  }
}
</style>