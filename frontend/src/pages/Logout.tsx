import { useAuth } from "../hooks/useAuth";
import { useEffect } from "react";

export const Logout = () => {
  const { userToken, logout } = useAuth();

  async function logoutUser() {
    const headers = new Headers();
    headers.append("Content-Type", "application/json");
    if (userToken) headers.append("Authorization", `Bearer ${userToken}`);

    return fetch("/api/auth/logout", {
      method: "POST",
      headers: headers,
    })
      .then(async (res) => {
        if (!res.ok) {
          const err = await res.text();
          return await Promise.reject(err);
        }

        return res.text();
      })
      .then((data) => {
        return data;
      })
      .catch((e) => {
        console.error("User logout fetch error: ", e);
        return null;
      });
  }

  useEffect(() => {
    const fetchData = async () => {
      const response = await logoutUser();
      console.log(response);
      logout();
    };

    fetchData();
  }, []);

  return <h1>Logging out...</h1>;
};

export default Logout;
