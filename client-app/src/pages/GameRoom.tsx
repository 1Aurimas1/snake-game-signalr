import { useEffect, useRef, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { GameMode } from "../shared/constants/GameMode";
import { GameDto } from "../shared/interfaces/GameDto";
import Game from "../components/Game";
import Spinner from "../components/Spinner";
import ErrorComponent from "../components/ErrorComponent";

interface Grid {
  rows: number;
  columns: number;
}

const Direction = {
  UP: 0,
  DOWN: 1,
  LEFT: 2,
  RIGHT: 3,
} as const;
type Direction = (typeof Direction)[keyof typeof Direction];

const Controls = {
  UP: ["ArrowUp", "w"],
  DOWN: ["ArrowDown", "s"],
  LEFT: ["ArrowLeft", "a"],
  RIGHT: ["ArrowRight", "d"],
};

// TODO: if null ask user to relog?
let playerName: string | null = sessionStorage.getItem("username");

let roomId: string;
const inputBlockTime = 100;

const GameRoom = () => {
  const location = useLocation();
  const selectedMode: GameMode = location.state?.selectedMode;
  //const userToken: string = location.state?.user;

  if (selectedMode == null) {
    return (
      <ErrorComponent
        title="user input error"
        helperMessage="Before starting to play please select game mode!"
        redirectMessage="Go to game mode selection?"
      />
    );
  }

  const hub = useRef<HubConnection | null>(null);

  const [grid, setGrid] = useState<Grid>({ rows: 12, columns: 12 });
  const [isStarting, setIsStarting] = useState(true);
  const [countdown, setCountdown] = useState(0);
  const [gameStates, setGameStates] = useState<GameDto[]>([]);

  const direction = useRef<Direction>();
  const isInputBlocked = useRef(false);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      //.withUrl("/gamehub",  { accessTokenFactory: () => userToken })
      .withUrl("/gamehub")
      .configureLogging(LogLevel.Information)
      .build();

    connection.onclose((error) => {
      if (error) {
        console.error("SignalR connection closed with an error:", error);
      } else {
        console.log(
          "SignalR connection closed by the server or due to other reasons.",
        );
      }
    });

    connection
      .start()
      .then(() => {
        console.log("SignalR connection established.");
        connection
          .invoke("JoinGame", playerName, selectedMode)
          .then((data: string) => {
            roomId = data;
            connection
              .invoke("InitGrid")
              .then((data) => {
                const gridDims: Grid = {
                  rows: data.item1,
                  columns: data.item2,
                };
                setGrid(gridDims);
              })
              .catch((e) => console.error(`Error initializing grid: ${e}`));
          })
          .catch((e) => console.error(`Error joining game: ${e}`));
      })
      .catch((error) => {
        console.error("Error starting SignalR connection:", error);
      });

    if (!hub.current) hub.current = connection;

    return () => {
      connection.stop();
    };
  }, []);

  useEffect(() => {
    if (!hub.current) return;

    hub.current.on(
      "ReceiveStateObjects",
      (gameStates: GameDto[], initial: boolean) => {
        setIsStarting(initial);
        setGameStates(gameStates);
      },
    );

    return () => {
      hub.current?.off("ReceiveStateObjects");
    };
  }, [gameStates]);

  useEffect(() => {
    if (!hub.current) return;

    hub.current.on("ReceiveCountdown", (i: number) => {
      setCountdown(i);
    });

    return () => {
      hub.current?.off("ReceiveCountdown");
    };
  }, [countdown]);

  const handleKeyDown = (event: KeyboardEvent) => {
    if (isInputBlocked.current) return;

    let newDirection: Direction | undefined;
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
    }

    if (newDirection === undefined || !hub.current) return;
    direction.current = newDirection;

    hub.current
      .invoke("SendInput", roomId, playerName, newDirection)
      .catch((e) => console.error("[SendInput] error: ", e));

    isInputBlocked.current = true;
    setTimeout(() => {
      isInputBlocked.current = false;
    }, inputBlockTime);
  };

  useEffect(() => {
    window.addEventListener("keyup", handleKeyDown);

    return () => {
      window.removeEventListener("keyup", handleKeyDown);
    };
  }, []);

  return (
    <div className="relative h-screen">
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
        {gameStates.map((state, i) => {
          let isControllable = false;
          if (playerName === state.playerName) isControllable = true;
          return (
            <Game
              key={i}
              grid={grid}
              isControllable={isControllable}
              state={state}
            />
          );
        })}
      </div>
    </div>
  );
};

export default GameRoom;
