import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from "../src/components/Login";
import Register from "../src/components/Register";
import { AuthProvider } from "./hooks/useAuth";
import RequireAuth from "../src/components/RequireAuth";
import GameModeSelection from "./components/GameModeSelection";
import GameRoom from "./components/GameRoom";
import NotFound from "./components/NotFound";
import Layout from "./components/Layout";
import Profile from "./components/Profile";

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route>
              <Route element={<Layout />}>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route element={<RequireAuth />}>
                <Route path="/" element={<GameModeSelection />} />
                <Route path="/play" element={<GameRoom />} />
                <Route path="/profile" element={<Profile />} />
                <Route path="*" element={<NotFound />} />
            </Route>
              </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
