import { Outlet } from "react-router-dom";
import Footer from "./Footer";
import NavBar from "./NavBar";
import { useAuth } from "../hooks/useAuth";

export const Layout = () => {
  const { user } = useAuth();

  return (
    <div className="flex h-screen flex-col">
      {user && <NavBar />}
      <Outlet />
      <Footer />
    </div>
  );
};

export default Layout;
