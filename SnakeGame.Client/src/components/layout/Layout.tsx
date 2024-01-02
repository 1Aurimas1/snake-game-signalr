import { Outlet } from "react-router-dom";
import Footer from "./Footer";
import NavBar from "./NavBar";

export const Layout = () => {
  return (
    <div className="flex min-h-screen w-full flex-col">
      <NavBar />
      <div className="m-10 flex flex-grow justify-center">
        <Outlet />
      </div>
      <Footer />
    </div>
  );
};

export default Layout;
