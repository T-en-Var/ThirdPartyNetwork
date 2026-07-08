import { extractApiErrorMessage, safeBtoa } from '@/lib/utils';
import { useAuthStore } from '@/stores/auth';
import type { useGlobalStore } from '@/stores/global';
import type { ErrorMessage, TokenPair } from '@/types';
import axios from 'axios';
import type { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, Method } from 'axios';
import { type Router, type RouteLocationNormalizedLoaded } from 'vue-router';

class ApiClient {
  private instance: AxiosInstance;
  private auth!: ReturnType<typeof useAuthStore>;
  private route!: RouteLocationNormalizedLoaded;
  private router!: Router;
  private global!: ReturnType<typeof useGlobalStore>;
  private initResolver?: () => void;
  private initPromise: Promise<void>;
  private readonly INIT_TIMEOUT = 5000;

  constructor(baseURL: string, config?: AxiosRequestConfig) {
    this.instance = axios.create({
      baseURL,
      withCredentials: true,
      ...config,
    });

    let resolver: () => void;
    const signalPromise = new Promise<void>((resolve) => {
      resolver = resolve;
    });

    this.initResolver = resolver!;

    const timeoutPromise = new Promise<void>((_, reject) => {
      setTimeout(
        () => reject(new Error('Axios init timeout: App failed to initialize')),
        this.INIT_TIMEOUT,
      );
    });

    this.initPromise = Promise.race([signalPromise, timeoutPromise]);

    this.instance.interceptors.request.use(async (config) => {
      await this.initPromise;

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
        response.data = this.convertDatesInResponse(response.data);
        return response;
      },
      (error: AxiosError<ErrorMessage>) => {
        const statusCode = error.response?.status;

        if (statusCode == 401) {
          let uri = this.route.query.redirect as string | undefined;
          if (this.route.name !== 'terms-of-service') uri = safeBtoa(this.route.fullPath);

          this.router.push({ path: '/401', query: uri ? { redirect: uri } : {} });
          this.auth.setModalOpen(true);
          this.auth.setModalMode('login');
        }

        if (statusCode == 403) {
          this.auth.setUnauthModalOpen(true);
        }

        this.global.addToast({
          message: extractApiErrorMessage(error, 'Something went wrong.'),
          type: 'error',
          duration: 5000,
        });

        return Promise.reject(error);
      },
    );
  }

  private convertDatesInResponse<T>(data: T): T {
    if (Array.isArray(data)) {
      return data.map((d) => this.convertDatesInResponse(d)) as T;
    }

    if (data !== null && typeof data === 'object') {
      const convertedData: { [key: string]: unknown } = {};
      for (const key in data) {
        if (data.hasOwnProperty(key)) {
          if (typeof data[key] === 'string' && this.isValidDateString(data[key])) {
            convertedData[key] = new Date(data[key]);
          } else {
            convertedData[key] = this.convertDatesInResponse(data[key]);
          }
        }
      }
      return convertedData as T;
    }

    return data;
  }

  private isValidDateString(dateString: string): boolean {
    // ISO 8601 full date/time (e.g., "2023-08-08T12:34:56Z")
    const iso8601Regex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z?$/;

    if (!iso8601Regex.test(dateString)) return false;

    const date = new Date(dateString);
    return !isNaN(date.getTime());
  }

  public initialize(
    store: ReturnType<typeof useAuthStore>,
    route: RouteLocationNormalizedLoaded,
    router: Router,
    global: ReturnType<typeof useGlobalStore>,
  ) {
    this.auth = store;
    this.route = route;
    this.router = router;
    this.global = global;

    if (this.initResolver) this.initResolver();
  }

  private async request<T>(
    method: Method,
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return await this.instance.request<T>({ ...config, method, url, data });
  }

  public get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.request<T>('get', url, undefined, config);
  }

  public post<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.request<T>('post', url, data, config);
  }

  public put<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.request<T>('put', url, data, config);
  }

  public patch<T, B>(url: string, data?: B, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.request<T>('patch', url, data, config);
  }

  public delete<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.request<T>('delete', url, undefined, config);
  }

  public refresh(): Promise<AxiosResponse<TokenPair>> {
    return this.request<TokenPair>('post', '/auth/refresh', {}, {
      headers: { 'x-skip-auth-headers': true },
    });
  }
}

export default ApiClient;
