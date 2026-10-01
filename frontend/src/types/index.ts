export interface TokenResponse {
  token: string;
  refreshToken: string;
  userName: string;
  email: string;
  roles: string[];
}

export interface RefreshTokenResponse {
  token: string;
  refreshToken: string;
}

export interface MarkerDto {
  id: string;
  name: string;
  description: string;
  latitude: number | null;
  longitude: number | null;
  categoryId: string;
  categoryName: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
  isPublic: boolean;
}

export interface MarkerInput {
  name: string;
  description: string;
  latitude: number | null;
  longitude: number | null;
  categoryId: string;
  isPublic: boolean;
}

export interface CategoryDto {
  id: string;
  name: string;
}

export interface ContentDto {
  id: string;
  path?: string | null;
  url?: string | null;
  markerId: string;
  createdAt: string;
}

export interface CurrentUser {
  id: string;
  userName: string;
  email: string;
  roles: string[];
}

interface JwtPayload {
  nameid?: string;
  unique_name?: string;
  email?: string;
  role?: string | string[];
  exp?: number;
  // Полные URI claims, которые реально кладёт бэкенд в JWT
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
}

function pick(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

export function decodeJwt(token: string): CurrentUser | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    );
    const payload = JSON.parse(json) as JwtPayload;

    // Сначала полные URI, потом короткие имена — на случай другого провайдера
    const rawRole =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? payload.role;
    const roles = Array.isArray(rawRole) ? rawRole.map(pick).filter(Boolean) : rawRole ? [pick(rawRole)] : [];

    return {
      id:
        pick(payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']) ??
        pick(payload.nameid),
      userName:
        pick(payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']) ??
        pick(payload.unique_name),
      email:
        pick(payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']) ??
        pick(payload.email),
      roles,
    };
  } catch {
    return null;
  }
}