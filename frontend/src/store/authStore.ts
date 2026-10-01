import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { login as loginApi, register as registerApi } from '../api/auth';
import { decodeJwt, type CurrentUser } from '../types';

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: CurrentUser | null;
  setTokens: (token: string, refreshToken: string) => void;
  login: (email: string, password: string) => Promise<void>;
  register: (userName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

function applySession(set: (p: Partial<AuthState>) => void, token: string, refreshToken: string) {
  const user = decodeJwt(token);
  set({ token, refreshToken, user });
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      user: null,

      setTokens: (token, refreshToken) => {
        applySession(set, token, refreshToken);
      },

      login: async (email, password) => {
        const resp = await loginApi(email, password);
        applySession(set, resp.token, resp.refreshToken);
      },

      register: async (userName, email, password) => {
        const resp = await registerApi(userName, email, password);
        applySession(set, resp.token, resp.refreshToken);
      },

      logout: () => set({ token: null, refreshToken: null, user: null }),
    }),
    {
      name: 'markers-auth',
      partialize: (state) => ({ token: state.token, refreshToken: state.refreshToken }),
      // При восстановлении сессии заново декодируем user из сохранённого токена
      merge: (persisted, current) => {
        const p = (persisted ?? {}) as { token?: string; refreshToken?: string };
        const merged = { ...current, token: p.token ?? null, refreshToken: p.refreshToken ?? null };
        if (merged.token) {
          merged.user = decodeJwt(merged.token);
        }
        return merged;
      },
    },
  ),
);

export const isAdmin = (user: CurrentUser | null) => user?.roles.includes('Admin') ?? false;