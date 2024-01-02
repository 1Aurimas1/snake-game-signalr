import { Link, To } from "react-router-dom";
import { ReactNode, useState } from "react";
import logo from "../../assets/snake_64_bw.png";
import { UserRole, useAuth } from "../../hooks/useAuth";
import {
  GAMES,
  HOME,
  LOGIN,
  LOGOUT,
  MAPS,
  PROFILE,
  TOURNAMENTS,
  USERS,
} from "../../shared/constants/Routes";

interface NavItem {
  name: string;
  path: string;
  role: UserRole;
}

const navItems: NavItem[] = [
  { name: "Mode selection", path: HOME, role: UserRole.Basic },
  { name: "Games", path: GAMES, role: UserRole.Guest },
  { name: "Maps", path: MAPS, role: UserRole.Basic },
  { name: "Tournaments", path: TOURNAMENTS, role: UserRole.Admin },
  { name: "Users", path: USERS, role: UserRole.Admin },
  { name: "Profile", path: PROFILE, role: UserRole.Basic },
];

interface NavLinkProps {
  to: To;
  onClick?: React.MouseEventHandler<HTMLAnchorElement> | undefined;
  children: ReactNode;
  isLast: boolean;
}

const NavLink = (props: NavLinkProps) => {
  return (
    <Link
      to={props.to}
      onClick={props.onClick}
      className={`mx-1 rounded bg-gray-500 p-1 ${
        props.isLast ? "font-bold text-red-500" : "font-semibold text-white"
      } hover:bg-gray-200 hover:text-black active:bg-red-500 active:text-black`}
    >
      {props.children}
    </Link>
  );
};

export const NavBar = () => {
  const { userAuthData } = useAuth();
  const [isOpen, setIsOpen] = useState(false);

  function filterNavItems(navItem: NavItem) {
    if (
      userAuthData?.roles.includes(navItem.role) ||
      navItem.role === UserRole.Guest
    ) {
      return true;
    }

    return false;
  }

  const mappedLinks = (
    <>
      {navItems.filter(filterNavItems).map((item, idx) => (
        <li
          key={idx}
          className="animate-slideIn py-2 opacity-0 sm:animate-none sm:opacity-100"
          style={{ "--delay": idx * 0.25 + "s" } as any}
        >
          <NavLink
            to={item.path}
            onClick={() => setIsOpen(false)}
            isLast={false}
          >
            {item.name}
          </NavLink>
        </li>
      ))}
      <li className="py-2">
        {userAuthData ? (
          <NavLink to={LOGOUT} onClick={() => setIsOpen(false)} isLast={true}>
            Log out
          </NavLink>
        ) : (
          <NavLink to={LOGIN} onClick={() => setIsOpen(false)} isLast={true}>
            Log in
          </NavLink>
        )}
      </li>
    </>
  );

  return (
    <nav className="bg-gray-500 p-4">
      <div className="grid grid-cols-[1fr_auto] items-center">
        <div className="min-w-[2.5rem]">
          <Link to="/">
            <img className="h-10 w-10" src={logo} alt="My logo" />
          </Link>
        </div>
        <ul className="hidden sm:flex sm:justify-self-end">{mappedLinks}</ul>

        <div className="flex flex-col sm:hidden">
          <button
            onClick={() => setIsOpen(!isOpen)}
            className="ml-auto text-gray-300 hover:text-white focus:text-white focus:outline-none"
          >
            <svg className="h-6 w-6" strokeWidth={2} stroke="currentColor">
              <path d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>
        </div>

        {isOpen && (
          <div className="mt-4 sm:hidden">
            <ul>{mappedLinks}</ul>
          </div>
        )}
      </div>
    </nav>
  );
};

export default NavBar;
