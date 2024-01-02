import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import { BrowserRouter, Route, Routes } from "react-router-dom";

import { AuthProvider } from "./hooks/useAuth";

import * as R from "./shared/constants/Routes";
import {
  GameCreation,
  GameDetails,
  GameList,
  GameModeSelection,
  GameRoom,
  Login,
  Logout,
  MapCreation,
  MapDetails,
  MapList,
  Profile,
  Register,
  TournamentCreation,
  TournamentDetails,
  TournamentList,
  UserDetails,
  UserList,
} from "./pages";
import NotFound from "./pages/NotFound";
import { Layout, RequireAuth } from "./components";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<Layout />}>
            <Route path={R.LOGIN} element={<Login />} />
            <Route path={R.REGISTER} element={<Register />} />
            <Route path={R.GAMES} element={<GameList />} />

            <Route element={<RequireAuth />}>
              <Route path={R.HOME} element={<GameModeSelection />} />
              <Route path={R.PLAY} element={<GameRoom />} />

              <Route path={R.USERS} element={<UserList />} />
              <Route path={R.USER_DETAILS} element={<UserDetails />} />

              <Route path={R.MAPS} element={<MapList />} />
              <Route path={R.MAP_CREATION} element={<MapCreation />} />
              <Route path={R.MAP_DETAILS} element={<MapDetails />} />

              <Route path={R.GAME_CREATION} element={<GameCreation />} />
              <Route path={R.GAME_DETAILS} element={<GameDetails />} />

              <Route path={R.TOURNAMENTS} element={<TournamentList />} />
              <Route
                path={R.TOURNAMENT_CREATION}
                element={<TournamentCreation />}
              />
              <Route
                path={R.TOURNAMENT_DETAILS}
                element={<TournamentDetails />}
              />

              <Route path={R.PROFILE} element={<Profile />} />
              <Route path={R.LOGOUT} element={<Logout />} />
              <Route path={R.CATCH_ALL} element={<NotFound />} />
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
