import React, { useEffect, useRef, useState } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

interface GameDto {
  playerId: string;
  snakeParts: Point[];
  food?: Point;
}

interface Point {
  x: number;
  y: number;
}

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
type Direction = typeof Direction[keyof typeof Direction];

const MappedButtons = {
  UP: ["ArrowUp", "w"],
  DOWN: ["ArrowDown", "s"],
  LEFT: ["ArrowLeft", "a"],
  RIGHT: ["ArrowRight", "d"],
};

//const InputKeyMappings: { [key: string]: Direction } = {
//  ArrowUp: Direction.UP,
//  w: Direction.UP,
//  ArrowDown: Direction.DOWN,
//  s: Direction.DOWN,
//  ArrowLeft: Direction.LEFT,
//  a: Direction.LEFT,
//  ArrowRight: Direction.RIGHT,
//  d: Direction.RIGHT,
//} as const;

let playerId: string = Date.now().toString();
const inputBlockTime = 100;

const Game = () => {
  const hub = useRef<HubConnection | null>(null);

  const [grid, setGrid] = useState<Grid>({ rows: 12, columns: 12 });
  const [snake, setSnake] = useState<Point[]>([]);
  const [food, setFood] = useState<Point>({ x: 0, y: 0 });

  const direction = useRef<Direction>();
  const isInputBlocked = useRef(false);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
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

    connection.start()
      .then(() => {
        console.log("SignalR connection established.");
        connection.invoke("JoinGame", playerId).then((data) => {
          //playerId = data;
          connection.invoke("InitGrid").then((data) => {
            const gridDims: Grid = { rows: data.item1, columns: data.item2 };
            setGrid(gridDims);
          }).catch((e) => console.error(`Error initializing grid: ${e}`));
        }).catch((e) => console.error(`Error joining game: ${e}`));
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

    // TODO: handle off
    hub.current.on("ReceiveStateObjects", (message: GameDto[]) => {
      moveSnake(message[0].snakeParts);
      spawnFood(message[0].food);
    });
  }, [snake, food]);

  // TODO: send confirmation to server after successful snake & food render
  const moveSnake = (parts: Point[]) => {
    setSnake(parts);
  };

  const spawnFood = (position: Point | undefined) => {
    if (position) setFood(position);
  };

  const handleKeyDown = (event: KeyboardEvent) => {
    if (isInputBlocked.current) return;

    //let newDirection: Direction = InputKeyMappings[event.key];
    let newDirection: Direction | undefined = undefined;
    if (
      MappedButtons.UP.includes(event.key) &&
      direction.current !== Direction.DOWN &&
      direction.current !== Direction.UP
    ) {
      newDirection = Direction.UP;
    } else if (
      MappedButtons.DOWN.includes(event.key) &&
      direction.current !== Direction.UP &&
      direction.current !== Direction.DOWN
    ) {
      newDirection = Direction.DOWN;
    } else if (
      MappedButtons.LEFT.includes(event.key) &&
      direction.current !== Direction.RIGHT &&
      direction.current !== Direction.LEFT
    ) {
      newDirection = Direction.LEFT;
    } else if (
      MappedButtons.RIGHT.includes(event.key) &&
      direction.current !== Direction.LEFT &&
      direction.current !== Direction.RIGHT
    ) {
      newDirection = Direction.RIGHT;
    }

    if (newDirection === undefined || !hub.current) return;
    direction.current = newDirection;

    hub.current.invoke("SendInput", playerId, newDirection)
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

  // TODO: pre-allocate grid
  return (
    <div className="w-full h-full flex justify-center items-center">
      {(snake.length > 0 && food) &&
        (
          <div
            id="game-board"
            className="grid grid-cols-12 gap-1 max-w-[800px] max-h-[400px]"
          >
            {Array.from({ length: grid.rows }, (_, idx) => grid.rows - 1 - idx)
              .map(
                (y, _) =>
                  Array.from({ length: grid.columns }).map((_, x) => {
                    let snakePart;
                    for (let i = 0; i < snake.length; i++) {
                      const isSnake = snake[i].x === x && snake[i].y === y;
                      if (i === 0 && isSnake) {
                        snakePart = 1;
                      } else if (isSnake) {
                        snakePart = 2;
                      }
                    }
                    const isFood = food.x === x && food.y === y;

                    let cellColor;
                    if (snakePart === 2) {
                      cellColor = "bg-gray-500";
                    } else if (snakePart === 1) {
                      cellColor = "bg-black";
                    } else if (isFood) {
                      cellColor = "bg-red-500";
                    } else {
                      cellColor = "bg-gray-200";
                    }

                    return (
                      <div
                        key={`${x}-${y}`}
                        className={cellColor}
                        style={{
                          width: "20px",
                          height: "20px",
                        }}
                      />
                    );
                  }),
              )}
          </div>
        )}
    </div>
  );
};

export default Game;
