import { Outlet } from "react-router-dom";
import Footer from "./Footer";
import NavBar from "./NavBar";
import { useAuth } from "../hooks/useAuth";

export const Layout = () => {
  const { userToken } = useAuth();

  return (
    <div className="flex min-h-screen w-full flex-col">
      {userToken && <NavBar />}
      <div className="m-10 flex flex-grow justify-center">
        <Outlet />
      </div>
      <Footer />
    </div>
  );
};

export default Layout;
