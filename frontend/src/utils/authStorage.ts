const STORAGE_KEY = 'banking.auth.session';

export interface StoredAuthState {
  token: string;
  username?: string;
  clienteId: number | null;
  roles: string[];
  expiresAt?: string;
}

const canUseStorage = () => typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';

export const loadAuthState = (): StoredAuthState | null => {
  if (!canUseStorage()) {
    return null;
  }

  const serialized = window.localStorage.getItem(STORAGE_KEY);
  if (!serialized) {
    return null;
  }

  try {
    const parsed = JSON.parse(serialized) as StoredAuthState;
    return parsed.token ? parsed : null;
  } catch {
    return null;
  }
};

export const saveAuthState = (state: StoredAuthState) => {
  if (!canUseStorage()) {
    return;
  }

  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
};

export const clearAuthState = () => {
  if (!canUseStorage()) {
    return;
  }

  window.localStorage.removeItem(STORAGE_KEY);
};
