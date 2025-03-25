// apiClient.ts
import { useAuthStore } from '@/stores/auth';
import axios from 'axios';
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
class ApiClient {
  private instance: AxiosInstance;
  private auth!: ReturnType<typeof useAuthStore>;

  constructor(baseURL: string, config?: AxiosRequestConfig) {
    this.instance = axios.create({
      baseURL,
      ...config, // Allow overriding default config
    });

    // Add interceptors for request/response handling if needed
    this.instance.interceptors.request.use(async (config) => {
      config = await this.auth.applyHeaders(config)
      return config;
    });

    this.instance.interceptors.response.use(
      (response) => {
        return response
      },
      (error) => {
        // Handle errors globally
        return Promise.reject(error);
      }
    );
  }

  public setAuthStore(store: ReturnType<typeof useAuthStore>) {
    this.auth = store;
  }

  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.get(url, config);
    return response;
  }

  public async post<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.post(url, data, config);
    return response;
  }

  // Add other methods (put, delete, patch) as needed
}

export default ApiClient;
