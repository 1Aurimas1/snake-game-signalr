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
import { useFetch } from "./useFetch";
import { jwtDecode } from "jwt-decode";
import { HOME, LOGIN, LOGOUT } from "../shared/constants/Routes";
import { ApiResponse, SuccessfulLoginDto } from "../shared/interfaces";

interface AuthContextType {
  accessToken: string;
  refreshToken: string;
  userAuthData: UserAuthData | null;
  authFetch: (
    endpoint: string,
    options: RequestInit,
    body?: any,
  ) => Promise<ApiResponse>;
  refreshAccessToken: () => Promise<string | null>;
  login: (loginDto: SuccessfulLoginDto) => void;
  logout: () => void;
}

export enum UserRole {
  Admin = "Admin",
  Basic = "Basic",
  Guest = "Guest",
}

interface UserAuthData {
  id: string;
  name: string;
  roles: string[];
}

function tryGetUserAuthDataFromToken(
  accessToken: string | null,
): UserAuthData | null {
  if (!accessToken) {
    return null;
  }
  const decodedToken = jwtDecode(accessToken);
  const id = decodedToken.sub;
  const roles =
    decodedToken[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" as keyof typeof decodedToken
    ];
  const name =
    decodedToken[
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" as keyof typeof decodedToken
    ];

  return { id, roles, name } as UserAuthData;
}

function getRedirectPath(fromPath: string) {
  switch (fromPath) {
    case LOGIN:
    case LOGOUT:
    case "":
    case null:
      return HOME;
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

  const userAuthData = tryGetUserAuthDataFromToken(accessToken);

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
        console.log("Updating tokens...");
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
    setAccessToken("");
    setRefreshToken("");
    navigate(LOGIN, { replace: true });
  }

  const value = useMemo(
    () => ({
      accessToken,
      refreshToken,
      userAuthData,
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
