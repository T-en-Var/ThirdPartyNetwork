// apiClient.ts
import { useAuthStore } from '@/stores/auth';
import type { ErrorMessage, TokenPair } from '@/types';
import axios from 'axios';
import type { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { type Router, type RouteLocationNormalizedLoaded } from 'vue-router';

class ApiClient {
  private instance: AxiosInstance;
  private auth!: ReturnType<typeof useAuthStore>;
  private route!: RouteLocationNormalizedLoaded;
  private router!: Router;

  constructor(baseURL: string, config?: AxiosRequestConfig) {
    this.instance = axios.create({
      baseURL,
      ...config,
    });

    this.instance.interceptors.request.use(async (config) => {
      if (config.headers['x-skip-auth-headers']) {
        delete config.headers['x-skip-auth-headers'];
        return config;
      }
      if (config.headers.Authorization) return config;

      config = await this.auth.applyHeaders(config);
      return config;
    });

    this.instance.interceptors.response.use(
      (response) => {
        // Automatically convert fields that are string dates
        response.data = this.convertDatesInResponse(response.data);
        console.log(response.data)
        return response;
      },
      (error) => {
        const statusCode = (error as AxiosError<ErrorMessage>).response?.status;

        if (statusCode == 401) {
          let uri = this.route.query.redirect;
          if (this.route.name !== "Terms of Service") uri = btoa(this.route.fullPath);

          this.router.push(`/401?redirect=${uri}`);
          this.auth.setModalOpen(true);
          this.auth.setModalMode("login");
        }

        if (statusCode == 403) {
          this.auth.setUnauthModalOpen(true);
        }

        return Promise.reject(error)
      }
    );
  }

  private convertDatesInResponse<T>(data: T): T {
    if (Array.isArray(data)) {
      return data.map(d => this.convertDatesInResponse(d)) as T; // Recursively handle arrays
    }

    if (data !== null && typeof data === 'object') {
      const convertedData: { [key: string]: unknown } = {};
      for (const key in data) {
        if (data.hasOwnProperty(key)) {
          // Check if the value is a valid date string
          if (typeof data[key] === 'string' && this.isValidDateString(data[key])) {
            convertedData[key] = new Date(data[key]); // Convert to Date
          } else {
            convertedData[key] = this.convertDatesInResponse(data[key]); // Recurse through nested objects
          }
        }
      }
      return convertedData as T;
    }

    return data; // Return the data as-is if it's neither an object nor an array
  }

  private isValidDateString(dateString: string): boolean {
    // ISO 8601 full date/time (e.g., "2023-08-08T12:34:56Z")
    const iso8601Regex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z?$/;

    if (!iso8601Regex.test(dateString)) return false;

    const date = new Date(dateString);
    return !isNaN(date.getTime());
  }

  public initialize(store: ReturnType<typeof useAuthStore>, route: RouteLocationNormalizedLoaded, router: Router) {
    this.auth = store;
    this.route = route;
    this.router = router;
  }

  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.get(url, config);
    return response;
  }

  public async post<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.post(url, data, config);
    return response;
  }

  public async refresh(refreshToken: string): Promise<AxiosResponse<TokenPair>> {
    const config = { headers: { 'x-skip-auth-headers': true } };
    const response: AxiosResponse<TokenPair> = await this.instance.post(`/auth/refresh`, { refreshToken: refreshToken }, config);
    return response;
  }

  public async put<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.put(url, data, config);
    return response;
  }

  public async patch<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.patch(url, data, config);
    return response;
  }

  public async delete<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.delete(url, config);
    return response;
  }
}

export default ApiClient;
