import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { apiPost, getToken, setToken } from "../api/client";

interface AuthUser {
  userId: string;
  email: string;
  displayName: string;
}

interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  displayName: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  isReady: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
}

const USER_STORAGE_KEY = "homeos.user";
const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function loadStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_STORAGE_KEY);
  if (!raw || !getToken()) return null;
  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    setUser(loadStoredUser());
    setIsReady(true);
  }, []);

  function persist(response: AuthResponse) {
    setToken(response.token);
    const authUser: AuthUser = { userId: response.userId, email: response.email, displayName: response.displayName };
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(authUser));
    setUser(authUser);
  }

  async function login(email: string, password: string) {
    const response = await apiPost<AuthResponse>("/api/auth/login", { email, password });
    persist(response);
  }

  async function register(email: string, password: string, displayName: string) {
    const response = await apiPost<AuthResponse>("/api/auth/register", { email, password, displayName });
    persist(response);
  }

  function logout() {
    setToken(null);
    localStorage.removeItem(USER_STORAGE_KEY);
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, isReady, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
