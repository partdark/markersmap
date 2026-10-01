import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { API_BASE } from '../config';
import { useAuthStore } from '../store/authStore';
import type { RefreshTokenResponse } from '../types';

const client = axios.create({ baseURL: API_BASE });

client.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let refreshPromise: Promise<string> | null = null;

client.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
    const status = error.response?.status;

    if (status === 401 && original && !original._retry) {
      const { token, refreshToken } = useAuthStore.getState();
      if (refreshToken) {
        original._retry = true;
        try {
          refreshPromise ??= (async () => {
            const { data } = await axios.post<RefreshTokenResponse>(`${API_BASE}/auth/refresh`, {
              accessToken: useAuthStore.getState().token,
              refreshToken: useAuthStore.getState().refreshToken,
            });
            useAuthStore.getState().setTokens(data.token, data.refreshToken);
            return data.token;
          })();

          const newToken = await refreshPromise;
          refreshPromise = null;
          original.headers.Authorization = `Bearer ${newToken}`;
          return client(original);
        } catch {
          refreshPromise = null;
          useAuthStore.getState().logout();
        }
      } else {
        // Токен ещё есть, но сервер его не принял — вероятно, истёк без refresh
        if (token) useAuthStore.getState().logout();
      }
    }

    return Promise.reject(error);
  },
);

export default client;