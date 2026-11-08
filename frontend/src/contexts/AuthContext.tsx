import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { apiClient } from "../services/apiClient";
import {
  AUTH_EXPIRED_EVENT,
  clearStoredUser,
  readStoredUser,
  storeUser,
} from "../services/authStorage";
import type { LoginRequest, RegisterRequest, UserResponse } from "../types/api";

interface AuthContextValue {
  user: UserResponse | null;
  ready: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  register: (credentials: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserResponse | null>(() => readStoredUser());
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const handleExpiredToken = () => {
      setUser(null);
    };

    window.addEventListener(AUTH_EXPIRED_EVENT, handleExpiredToken);
    setUser(readStoredUser());
    setReady(true);

    return () => {
      window.removeEventListener(AUTH_EXPIRED_EVENT, handleExpiredToken);
    };
  }, []);

  const saveAuthenticatedUser = useCallback((authenticatedUser: UserResponse) => {
    storeUser(authenticatedUser);
    setUser(authenticatedUser);
  }, []);

  const login = useCallback(
    async (credentials: LoginRequest) => {
      saveAuthenticatedUser(await apiClient.login(credentials));
    },
    [saveAuthenticatedUser],
  );

  const register = useCallback(
    async (credentials: RegisterRequest) => {
      saveAuthenticatedUser(await apiClient.register(credentials));
    },
    [saveAuthenticatedUser],
  );

  const logout = useCallback(() => {
    clearStoredUser();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({ user, ready, login, register, logout }),
    [user, ready, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }
  return context;
}
