import type { UserResponse } from "../types/api";

const AUTH_STORAGE_KEY = "forma.auth";
export const AUTH_EXPIRED_EVENT = "forma:auth-expired";

export function readStoredUser(): UserResponse | null {
  const rawUser = window.localStorage.getItem(AUTH_STORAGE_KEY);
  if (!rawUser) {
    return null;
  }

  try {
    const user = JSON.parse(rawUser) as UserResponse;
    if (
      typeof user.token !== "string" ||
      typeof user.email !== "string" ||
      typeof user.displayName !== "string" ||
      isTokenExpired(user.token)
    ) {
      window.localStorage.removeItem(AUTH_STORAGE_KEY);
      if (typeof user.token === "string" && isTokenExpired(user.token)) {
        window.dispatchEvent(new Event(AUTH_EXPIRED_EVENT));
      }
      return null;
    }

    return user;
  } catch {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function getAccessToken(): string | null {
  return readStoredUser()?.token ?? null;
}

export function storeUser(user: UserResponse): void {
  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(user));
}

export function clearStoredUser(): void {
  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}

function isTokenExpired(token: string): boolean {
  const payload = token.split(".")[1];
  if (!payload) {
    return false;
  }

  try {
    const normalizedPayload = payload.replace(/-/g, "+").replace(/_/g, "/");
    const decodedPayload = window.atob(normalizedPayload);
    const claims = JSON.parse(decodedPayload) as { exp?: unknown };
    return typeof claims.exp === "number" && claims.exp * 1000 <= Date.now();
  } catch {
    return false;
  }
}
