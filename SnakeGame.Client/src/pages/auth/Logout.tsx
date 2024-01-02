import { useAuth } from "../../hooks/useAuth";
import { useEffect } from "react";

export const Logout = () => {
  const { logout, authFetch, refreshToken } = useAuth();

  useEffect(() => {
    async function fetchData() {
      const response = await authFetch("/logout", { method: "POST" }, {
        refreshToken,
      } as RefreshAccessTokenDto);
      console.log(response);
      logout();
    }

    fetchData();
  }, []);

  return <h1>Logging out...</h1>;
};

export default Logout;
