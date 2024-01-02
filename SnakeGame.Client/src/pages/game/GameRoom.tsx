import { useEffect, useRef, useState } from "react";
import { useLocation } from "react-router-dom";
import { GameMode } from "../../shared/constants/GameMode";
import { useAuth } from "../../hooks/useAuth";
import { Direction } from "../../shared/constants/Directions";
import { LOGIN } from "../../shared/constants/Routes";
import { ErrorComponent, Game, Spinner } from "../../components";
import { GameStateDto, GridSize } from "../../shared/interfaces";
import GameHubConnector from "../../shared/classes/GameHubConnector";

const Controls = {
  UP: ["ArrowUp", "w"],
  DOWN: ["ArrowDown", "s"],
  LEFT: ["ArrowLeft", "a"],
  RIGHT: ["ArrowRight", "d"],
};

const GameRoom = () => {
  const { accessToken, refreshAccessToken, userAuthData } = useAuth();
  let roomId: string;

  const location = useLocation();
  const selectedMode: GameMode = location.state?.selectedMode;

  if (selectedMode == null) {
    return (
      <ErrorComponent
        title="user input error"
        helperMessage="Before starting to play please select game mode!"
        redirectMessage="Go to game mode selection?"
      />
    );
  } else if (!userAuthData) {
    return (
      <ErrorComponent
        title="auth error"
        helperMessage="Failed to authorize account. Please relogin..."
        redirectPath={LOGIN}
        redirectMessage="Go to login page?"
      />
    );
  }

  const gameHub = useRef<GameHubConnector | null>(null);

  const [gridSize, setGridSize] = useState<GridSize>({ rows: 12, cols: 12 });
  const [isStarting, setIsStarting] = useState(true);
  const [countdown, setCountdown] = useState(0);
  const [gameStates, setGameStates] = useState<GameStateDto[]>([]);

  const direction = useRef<Direction>();
  const wasInputProcessed = useRef(true);

  const { fetch: originalFetch } = window;
  window.fetch = async (...args) => {
    let [resource, config] = args;
    let response = await originalFetch(resource as RequestInfo, config);
    // response interceptor
    if (response.status === 401) {
      const newAccessToken = await refreshAccessToken();
      if (newAccessToken && config) {
        config.headers = {
          ...config?.headers,
          Authorization: `Bearer ${newAccessToken}`,
        };
        response = await fetch(resource as RequestInfo, config);
      }
    }
    return response;
  };

  const receiveGameStates = (
    newGameStates: GameStateDto[],
    initial: boolean,
  ) => {
    if (wasInputProcessed.current === false) {
      wasInputProcessed.current = true;
    }
    setIsStarting(initial);
    setGameStates(newGameStates);
  };

  const receiveCountdown = (i: number) => {
    setCountdown(i);
  };

  useEffect(() => {
    const gameHubConnector = new GameHubConnector(accessToken);

    const initConnection = async () => {
      gameHubConnector.on("receiveCountdown", receiveCountdown);
      gameHubConnector.on("receiveGameStates", receiveGameStates);

      await gameHubConnector.startConnection();

      roomId = await gameHubConnector.joinGame(userAuthData.name, selectedMode);
      setGridSize(await gameHubConnector.initGrid());

      gameHub.current = gameHubConnector;
    };

    initConnection();

    window.addEventListener("keyup", handleKeyDown);

    return () => {
      gameHubConnector.off("receiveCountdown");
      gameHubConnector.off("receiveGameStates");
      gameHubConnector.stopConnection();

      window.removeEventListener("keyup", handleKeyDown);
    };
  }, []);

  const handleKeyDown = (event: KeyboardEvent) => {
    if (!wasInputProcessed.current || !gameHub.current) return;

    let newDirection: Direction;
    if (
      Controls.UP.includes(event.key) &&
      direction.current !== Direction.DOWN &&
      direction.current !== Direction.UP
    ) {
      newDirection = Direction.UP;
    } else if (
      Controls.DOWN.includes(event.key) &&
      direction.current !== Direction.UP &&
      direction.current !== Direction.DOWN
    ) {
      newDirection = Direction.DOWN;
    } else if (
      Controls.LEFT.includes(event.key) &&
      direction.current !== Direction.RIGHT &&
      direction.current !== Direction.LEFT
    ) {
      newDirection = Direction.LEFT;
    } else if (
      Controls.RIGHT.includes(event.key) &&
      direction.current !== Direction.LEFT &&
      direction.current !== Direction.RIGHT
    ) {
      newDirection = Direction.RIGHT;
    } else {
      return;
    }

    gameHub.current.sendUserInput(roomId, userAuthData.name, newDirection);

    direction.current = newDirection;
    wasInputProcessed.current = false;
  };

  return (
    <div className="relative">
      {isStarting && (
        <Spinner
          className="absolute bottom-0 left-0 right-0 top-0 z-40"
          text={
            countdown === 0
              ? "Looking for a game..."
              : "Get ready! " + countdown
          }
        />
      )}
      <div
        className={`flex flex-col items-center justify-evenly md:flex-row ${
          isStarting && "blur-[2px]"
        }`}
      >
        {gameStates.map((state, i) => (
          <Game
            key={i}
            gridSize={gridSize}
            isControllable={state.playerName === userAuthData.name}
            state={state}
          />
        ))}
      </div>
    </div>
  );
};

export default GameRoom;
