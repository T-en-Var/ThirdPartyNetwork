// api.ts
import { useAuthStore } from '@/stores/auth';
import ApiClient from './apiClient';
import { API_BASE_URL } from '@/config';
import type { RouteLocationNormalizedLoaded, Router } from 'vue-router';

const api = new ApiClient(API_BASE_URL);

export function initializeApiClient(router: Router, route: RouteLocationNormalizedLoaded) {
    api.initialize(
        useAuthStore(), 
        route,
        router
    );
}

export default api;
