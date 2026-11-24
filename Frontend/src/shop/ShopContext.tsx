import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import {
  apiJson,
  getStoredAuthProfile,
  getStoredToken,
  setStoredAuthProfile,
  setStoredToken,
  apiFetch,
} from '../api/client';
import type { AuthProfile } from '../api/client';
import type { LoginResponse } from '../api/types';
import type { Cart } from '../api/shopTypes';
import { AuthRoles } from '../lib/authRoles';

type ToastKind = 'success' | 'danger' | 'info';
type Toast = { id: number; message: string; kind: ToastKind };

type ShopContextValue = {
  profile: AuthProfile | null;
  isCustomer: boolean;
  isStaff: boolean;
  cart: Cart | null;
  favorites: number[];
  toasts: Toast[];
  notify: (message: string, kind?: ToastKind) => void;
  signIn: (data: LoginResponse) => Promise<void>;
  signOut: () => Promise<void>;
  refreshCart: () => Promise<void>;
  addToCart: (productId: number, quantity?: number) => Promise<boolean>;
  setQuantity: (productId: number, quantity: number) => Promise<void>;
  removeFromCart: (productId: number) => Promise<void>;
  toggleFavorite: (productId: number) => Promise<boolean>;
  isFavorite: (productId: number) => boolean;
};

const Ctx = createContext<ShopContextValue | null>(null);

let toastSeq = 1;

// eslint-disable-next-line react-refresh/only-export-components
export function useShop(): ShopContextValue {
  const v = useContext(Ctx);
  if (!v) throw new Error('useShop yalnızca ShopProvider içinde kullanılabilir.');
  return v;
}

export function ShopProvider({ children }: { children: ReactNode }) {
  const [profile, setProfile] = useState<AuthProfile | null>(() =>
    getStoredToken() ? getStoredAuthProfile() : null,
  );
  const [cart, setCart] = useState<Cart | null>(null);
  const [favorites, setFavorites] = useState<number[]>([]);
  const [toasts, setToasts] = useState<Toast[]>([]);

  const isCustomer = profile?.role === AuthRoles.Musteri;
  const isStaff = !!profile && !isCustomer;

  const notify = useCallback((message: string, kind: ToastKind = 'success') => {
    const id = toastSeq++;
    setToasts((t) => [...t, { id, message, kind }]);
    window.setTimeout(() => setToasts((t) => t.filter((x) => x.id !== id)), 3500);
  }, []);

  const refreshCart = useCallback(async () => {
    if (!isCustomer) {
      setCart(null);
      return;
    }
    try {
      setCart(await apiJson<Cart>('/api/cart'));
    } catch {
      /* sessiz */
    }
  }, [isCustomer]);

  useEffect(() => {
    if (!isCustomer) {
      setCart(null);
      setFavorites([]);
      return;
    }
    void refreshCart();
    void (async () => {
      try {
        setFavorites(await apiJson<number[]>('/api/favorites/ids'));
      } catch {
        /* sessiz */
      }
    })();
  }, [isCustomer, refreshCart]);

  useEffect(() => {
    const onExpired = () => {
      setProfile(null);
      notify('Oturumunuzun süresi doldu, lütfen tekrar giriş yapın.', 'info');
    };
    window.addEventListener('auth:expired', onExpired);
    return () => window.removeEventListener('auth:expired', onExpired);
  }, [notify]);

  const signIn = useCallback(async (data: LoginResponse) => {
    setStoredToken(data.accessToken);
    const p = { email: data.email, userName: data.userName, role: data.role };
    setStoredAuthProfile(p);
    setProfile(p);
  }, []);

  const signOut = useCallback(async () => {
    try {
      await apiFetch('/api/Auth/logout', { method: 'POST' });
    } catch {
      /* yine de çıkış yap */
    }
    setStoredToken(null);
    setProfile(null);
    setCart(null);
    setFavorites([]);
  }, []);

  const addToCart = useCallback(
    async (productId: number, quantity = 1) => {
      if (!isCustomer) return false;
      try {
        setCart(await apiJson<Cart>('/api/cart/items', {
          method: 'POST',
          body: JSON.stringify({ productId, quantity }),
        }));
        notify('Ürün sepete eklendi.');
        return true;
      } catch (e) {
        notify(e instanceof Error ? e.message : 'Sepete eklenemedi.', 'danger');
        return false;
      }
    },
    [isCustomer, notify],
  );

  const setQuantity = useCallback(
    async (productId: number, quantity: number) => {
      try {
        setCart(await apiJson<Cart>(`/api/cart/items/${productId}`, {
          method: 'PUT',
          body: JSON.stringify({ quantity }),
        }));
      } catch (e) {
        notify(e instanceof Error ? e.message : 'Güncellenemedi.', 'danger');
      }
    },
    [notify],
  );

  const removeFromCart = useCallback(
    async (productId: number) => {
      try {
        setCart(await apiJson<Cart>(`/api/cart/items/${productId}`, { method: 'DELETE' }));
      } catch (e) {
        notify(e instanceof Error ? e.message : 'Silinemedi.', 'danger');
      }
    },
    [notify],
  );

  const toggleFavorite = useCallback(
    async (productId: number) => {
      if (!isCustomer) return false;
      const has = favorites.includes(productId);
      try {
        await apiJson<void>(`/api/favorites/${productId}`, { method: has ? 'DELETE' : 'PUT' });
        setFavorites((f) => (has ? f.filter((x) => x !== productId) : [productId, ...f]));
        notify(has ? 'Favorilerden çıkarıldı.' : 'Favorilere eklendi.', 'info');
        return true;
      } catch (e) {
        notify(e instanceof Error ? e.message : 'İşlem başarısız.', 'danger');
        return false;
      }
    },
    [favorites, isCustomer, notify],
  );

  const isFavorite = useCallback((id: number) => favorites.includes(id), [favorites]);

  const value = useMemo<ShopContextValue>(
    () => ({
      profile, isCustomer, isStaff, cart, favorites, toasts, notify,
      signIn, signOut, refreshCart, addToCart, setQuantity, removeFromCart, toggleFavorite, isFavorite,
    }),
    [profile, isCustomer, isStaff, cart, favorites, toasts, notify, signIn, signOut, refreshCart,
      addToCart, setQuantity, removeFromCart, toggleFavorite, isFavorite],
  );

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}
