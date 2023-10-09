import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

export const RequireAuth = () => {
  const { userToken } = useAuth();

  if (!userToken) {
    return <Navigate to="/login" />;
  }

  return <Outlet />;
};

export default RequireAuth;
