import { useState } from "react";
import Grid from "./Grid";
import { GameStateDto, GridSize, Point } from "../../shared/interfaces";

interface Props {
  gridSize: GridSize;
  state: GameStateDto;
  isControllable: boolean;
}

const Game = ({gridSize, state, isControllable}: Props) => {
  const snake = state.snakeParts;
  const playerName = state.playerName;

  const [food, setFood] = useState<Point | undefined>();
  const [score, setScore] = useState<number>();

  if (state.food != food && state.food != null) {
    setFood(state.food);
    setScore(state.score);
  }

  return (
    <div className="m-10 flex flex-col items-center justify-center gap-5">
      {snake.length > 0 && food && (
        <>
          <div className="flex w-full flex-row justify-between">
            <p className="font-semibold">Name: {playerName}</p>
            <p className="font-semibold">Score: {score}</p>
          </div>

          <div
            className={`${isControllable && "shadow-custom shadow-red-500"}`}
          >
            <Grid cols={12} gameObjects={[snake, food]} />
          </div>
        </>
      )}
    </div>
  );
};

export default Game;
