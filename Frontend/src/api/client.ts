const TOKEN_KEY = 'auth_token';
const AUTH_PROFILE_KEY = 'auth_profile';

export type AuthProfile = {
  email: string;
  userName: string;
  role: string;
};

export function setStoredAuthProfile(profile: AuthProfile | null): void {
  if (typeof sessionStorage === 'undefined') return;
  try {
    if (profile) sessionStorage.setItem(AUTH_PROFILE_KEY, JSON.stringify(profile));
    else sessionStorage.removeItem(AUTH_PROFILE_KEY);
  } catch {
    /* ignore */
  }
}

export function getStoredAuthProfile(): AuthProfile | null {
  if (typeof sessionStorage === 'undefined') return null;
  try {
    const raw = sessionStorage.getItem(AUTH_PROFILE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as AuthProfile;
  } catch {
    return null;
  }
}

/** Oturum yalnızca sekme/pencere açıkken kalır; kapatınca token silinir (localStorage kalıcı değil). */
let legacyLocalStorageCleared = false;

function clearLegacyLocalStorageToken(): void {
  if (legacyLocalStorageCleared || typeof localStorage === 'undefined') return;
  legacyLocalStorageCleared = true;
  try {
    localStorage.removeItem(TOKEN_KEY);
  } catch {
    /* private mode vb. */
  }
}

export function getStoredToken(): string | null {
  clearLegacyLocalStorageToken();
  if (typeof sessionStorage === 'undefined') return null;
  try {
    return sessionStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export function setStoredToken(token: string | null): void {
  clearLegacyLocalStorageToken();
  if (typeof sessionStorage === 'undefined') return;
  try {
    if (token) sessionStorage.setItem(TOKEN_KEY, token);
    else {
      sessionStorage.removeItem(TOKEN_KEY);
      sessionStorage.removeItem(AUTH_PROFILE_KEY);
    }
  } catch {
    /* quota / private mode */
  }
}

export function apiUrl(path: string): string {
  const base = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '';
  const p = path.startsWith('/') ? path : `/${path}`;
  return `${base}${p}`;
}

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers);
  const token = getStoredToken();
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const body = init.body;
  if (body && !(body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  return fetch(apiUrl(path), { ...init, headers });
}

export async function readJson<T>(res: Response): Promise<T> {
  const text = await res.text();
  if (!text) return undefined as T;
  return JSON.parse(text) as T;
}

export async function parseErrorMessage(res: Response): Promise<string> {
  try {
    const text = await res.text();
    if (!text) return res.statusText || 'İstek başarısız';
    const data = JSON.parse(text) as {
      message?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };
    if (data.message) return data.message;
    if (data.errors) {
      const msgs = Object.values(data.errors).flat();
      if (msgs.length > 0) return msgs.join(' ');
    }
    if (data.title) return data.title;
  } catch {
    /* ignore */
  }
  return res.statusText || 'İstek başarısız';
}
