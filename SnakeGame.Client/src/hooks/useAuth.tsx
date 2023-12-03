import {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useLocalStorage } from "./useLocalStorage";
import SuccessfulLoginDto from "../shared/interfaces/SuccessfulLoginDto";
import { useFetch } from "./useFetch";
import { ApiResponse } from "../shared/interfaces/ApiResponse";

interface AuthContextType {
  accessToken: string;
  refreshToken: string;
  authFetch: (
    endpoint: string,
    options: RequestInit,
    body?: any,
  ) => Promise<ApiResponse>;
  refreshAccessToken: () => Promise<string | null>;
  login: (loginDto: SuccessfulLoginDto) => void;
  logout: () => void;
}

function getRedirectPath(fromPath: string) {
  switch (fromPath) {
    case "/login":
    case "/logout":
    case "":
    case null:
      return "/";
    default:
      return fromPath;
  }
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const location = useLocation();
  const navigate = useNavigate();

  const [accessToken, setAccessToken] = useLocalStorage("accessToken", null);
  const [refreshToken, setRefreshToken] = useLocalStorage("refreshToken", null);
  const [previousJwt, setPreviousJwt] = useState({ accessToken, refreshToken });
  const isJwtUpdated = useRef(false);

  useEffect(() => {
    if (accessToken !== previousJwt.accessToken) {
      setPreviousJwt((prevState) => ({
        ...prevState,
        accessToken: accessToken,
      }));
    }
    if (refreshToken !== previousJwt.refreshToken) {
      setPreviousJwt((prevState) => ({
        ...prevState,
        refreshToken: refreshToken,
      }));
    }
    if (
      refreshToken === previousJwt.refreshToken &&
      accessToken === previousJwt.accessToken
    ) {
      isJwtUpdated.current = true;
    }
  }, [accessToken, refreshToken]);

  async function authFetch(
    endpoint: string,
    options: RequestInit,
    body?: any,
  ): Promise<ApiResponse> {
    addAuthHeader(accessToken);
    let responseInfo = await useFetch(endpoint, options, body);

    if (responseInfo.httpStatusCode === 401) {
      const newAccessToken = await refreshAccessToken();
      if (newAccessToken) {
        addAuthHeader(newAccessToken);
        responseInfo = await useFetch(endpoint, options, body);
      }
    }

    function addAuthHeader(token: string) {
      options.headers = {
        ...options.headers,
        Authorization: `Bearer ${token}`,
      };
    }

    return responseInfo;
  }

  async function refreshAccessToken(): Promise<string | null> {
    console.log("Refreshing tokens...");
    let newAccessToken: string | null = null;

    if (isJwtUpdated.current) {
      isJwtUpdated.current = false;
      const { apiData, apiError } = await useFetch(
        "/accessToken",
        { method: "POST" },
        { refreshToken } as RefreshAccessTokenDto,
      );

      if (apiError) {
        console.error("Token refresh error. Logging out...");
        logout();
      } else {
        const tokens = { ...apiData } as SuccessfulLoginDto;
        setAccessToken(tokens.accessToken);
        setRefreshToken(tokens.refreshToken);
        newAccessToken = tokens.accessToken;
      }
    }

    return newAccessToken;
  }

  function login(loginDto: SuccessfulLoginDto): void {
    setAccessToken(loginDto.accessToken);
    setRefreshToken(loginDto.refreshToken);
    navigate(getRedirectPath(location.pathname), { replace: true });
  }

  function logout() {
    sessionStorage.clear();
    localStorage.clear();
    navigate("/login", { replace: true });
  }

  const value = useMemo(
    () => ({
      accessToken,
      refreshToken,
      authFetch,
      refreshAccessToken,
      login,
      logout,
    }),
    [accessToken, refreshToken],
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
