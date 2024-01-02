import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { LOGIN } from "../../shared/constants/Routes";

export const RequireAuth = () => {
  const { accessToken } = useAuth();

  return !accessToken ? <Navigate to={LOGIN} /> : <Outlet />;
};

export default RequireAuth;
