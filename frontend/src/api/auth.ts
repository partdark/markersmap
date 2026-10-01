import client from './client';
import type { TokenResponse } from '../types';

export const login = (email: string, password: string) =>
  client.post<TokenResponse>('/auth/login', { email, password }).then((r) => r.data);

export const register = (userName: string, email: string, password: string) =>
  client.post<TokenResponse>('/auth/register', { userName, email, password }).then((r) => r.data);