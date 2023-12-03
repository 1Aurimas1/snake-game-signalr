import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

export const RequireAuth = () => {
  const { accessToken } = useAuth();

  return !accessToken ? <Navigate to="/login" /> : <Outlet />;
};

export default RequireAuth;
