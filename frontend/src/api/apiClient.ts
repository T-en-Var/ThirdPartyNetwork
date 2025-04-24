// apiClient.ts
import { useAuthStore } from '@/stores/auth';
import type { ErrorMessage } from '@/types';
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
      config = await this.auth.applyHeaders(config);
      return config;
    });

    this.instance.interceptors.response.use(
      (response) => response,
      (error) => {
        const statusCode = (error as AxiosError<ErrorMessage>).response?.status;

        if (statusCode == 401) {
          let uri = this.route.query.redirect;
          if (this.route.name !== "Terms of Service") uri = btoa(this.route.fullPath);
        
          this.router.push(`/401?redirect=${uri}`);
          this.auth.setModelOpen(true);
          this.auth.setModelMode("login");
        }

        if (statusCode == 403) {
          this.auth.setUnauthModalOpen(true);
        }

        return Promise.reject(error)}
    );
  }

  public initialize(store: ReturnType<typeof useAuthStore>, route: RouteLocationNormalizedLoaded, router: Router) {
    this.auth = store;
    this.route = route;
    this.router = router;
  }

  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    const response: AxiosResponse<T> = await this.instance.get(url, config);
    console.log(url, response.data);
    return response;
  }

  public async post<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    console.log(url, data)
    const response: AxiosResponse<T> = await this.instance.post(url, data, config);
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
