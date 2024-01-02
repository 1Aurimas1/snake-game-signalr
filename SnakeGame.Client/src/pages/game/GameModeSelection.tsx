import { useNavigate } from "react-router-dom";
import { GameMode } from "../../shared/constants/GameMode";
import { PLAY } from "../../shared/constants/Routes";
import { Button, ContentWrapper } from "../../components";

interface ModeSelection {
  title: string;
  mode: GameMode;
}

const modeSelections: ModeSelection[] = [
  { title: "Practice", mode: GameMode.SOLO },
  { title: "Duel", mode: GameMode.DUEL },
];

const GameModeSelection = () => {
  const navigate = useNavigate();

  const handleSelection = (
    e: React.MouseEvent<HTMLButtonElement, MouseEvent>,
    selectedMode: GameMode,
  ) => {
    e.preventDefault();
    navigate(PLAY, { state: { selectedMode } });
  };

  return (
    <ContentWrapper title="Select game mode">
      {modeSelections.map((selection) => (
        <Button
          key={selection.mode}
          onClick={(e) => handleSelection(e, selection.mode)}
          className="m-1 border-2 border-black font-semibold"
        >
          {selection.title}
        </Button>
      ))}
    </ContentWrapper>
  );
};

export default GameModeSelection;
