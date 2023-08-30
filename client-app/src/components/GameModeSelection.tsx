import { useNavigate } from "react-router-dom";
import { GameMode } from "../shared/constants/GameMode";
import { useAuth } from "../hooks/useAuth";

interface ModeSelection {
  title: string;
  mode: GameMode;
}

const modeSelections: ModeSelection[] = [
  { title: "Practice", mode: GameMode.SOLO },
  {
    title: "Duel",
    mode: GameMode.DUEL,
  },
];

const GameModeSelection = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const handleSelection = (e, selectedMode: GameMode) => {
    e.preventDefault();
    navigate("/play", { state: { selectedMode, user } });
  };

  return (
    <div className="m-auto flex items-center justify-center">
      <div className="flex flex-col items-center justify-center border border-black bg-gray-200 p-5">
        <h1 className="m-5 text-xl font-bold">Select game mode</h1>
        {modeSelections.map((selection) => (
          <button
            key={selection.mode}
            className="m-1 w-28 rounded border-2 border-black bg-gray-500 py-2 font-semibold text-white hover:bg-gray-200 hover:text-black active:bg-red-500 active:text-black"
            onClick={(e) => handleSelection(e, selection.mode)}
          >
            {selection.title}
          </button>
        ))}
      </div>
    </div>
  );
};

export default GameModeSelection;
