import { ReactNode, createContext, useContext, useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useLocalStorage } from "./useLocalStorage";

interface AuthContextType {
  userToken: string;
  login: (data: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [userToken, setUserToken] = useLocalStorage("userToken", null);
  const navigate = useNavigate();
  const location = useLocation();

  const redirectPath =
    location.pathname === "/login" || !location.pathname
      ? "/"
      : location.pathname;

  const login = async (data: string) => {
    setUserToken(data);
    navigate(redirectPath, { replace: true });
  };

  const logout = () => {
    sessionStorage.clear();
    navigate("/login", { replace: true });
  };

  const value = useMemo(
    () => ({
      userToken,
      login,
      logout,
    }),
    [userToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const auth = useContext(AuthContext);

  if (!auth) {
    throw new Error("useAuth can't be used outside a RequireAuth.");
  }

  return auth;
};
