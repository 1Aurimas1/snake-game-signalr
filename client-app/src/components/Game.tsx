import React, { useEffect, useRef, useState } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";

const ROWS = 12;
const COLS = 12;

const Direction = {
  UP: { x: 0, y: -1 },
  DOWN: { x: 0, y: 1 },
  LEFT: { x: -1, y: 0 },
  RIGHT: { x: 1, y: 0 },
};

const initialSnake = [
  { x: 2, y: 2 },
  { x: 2, y: 1 },
  { x: 2, y: 0 },
];

const generateFood = () => {
  const x = Math.floor(Math.random() * ROWS);
  const y = Math.floor(Math.random() * COLS);
  return { x, y };
};

const Game = () => {
  const [snake, setSnake] = useState(initialSnake);
  const [food, setFood] = useState(generateFood());
  const direction = useRef(Direction.RIGHT);
  const hub = useRef<HubConnection | null>(null);

  useEffect(() => {
    const interval = setInterval(moveSnake, 400);
    return () => clearInterval(interval);
  }, [snake]);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("/gamehub")
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("ReceiveMessage", (message) => {
      console.log("Received message:", message);
    });

    connection.start()
      .then(() => {
        console.log("SignalR connection established.");
        connection.invoke("SendMessage", "u").catch((e) =>
          console.error(`Error sending message: ${e}`)
        );
      })
      .catch((error) => {
        console.error("Error starting SignalR connection:", error);
      });

    if (!hub.current) hub.current = connection;

    return () => {
      connection.stop();
    };
  }, []);

  const moveSnake = () => {
    let head = { ...snake[0] };

    head.x += direction.current.x;
    head.y += direction.current.y;

    const newSnake = [head, ...snake.slice(0, -1)];
    setSnake(newSnake);

    if (head.x === food.x && head.y === food.y) {
      setFood(generateFood());
      setSnake([...newSnake, snake[snake.length - 1]]);
    }
  };

  const handleKeyDown = (event: KeyboardEvent) => {
    if (event.key === "ArrowUp" && direction.current !== Direction.DOWN) {
      direction.current = Direction.UP;
    } else if (
      event.key === "ArrowDown" && direction.current !== Direction.UP
    ) {
      direction.current = Direction.DOWN;
    } else if (
      event.key === "ArrowLeft" && direction.current !== Direction.RIGHT
    ) {
      direction.current = Direction.LEFT;
    } else if (
      event.key === "ArrowRight" && direction.current !== Direction.LEFT
    ) {
      direction.current = Direction.RIGHT;
    }
  };

  useEffect(() => {
    window.addEventListener("keydown", handleKeyDown);

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, []);

  return (
    <div className="w-full h-full flex justify-center items-center">
      <div
        id="game-board"
        className="grid grid-cols-12 gap-1 max-w-[800px] max-h-[400px]"
      >
        {Array.from({ length: ROWS }).map((_, y) =>
          Array.from({ length: COLS }).map((_, x) => {
            const isSnake = snake.some((s) => s.x === x && s.y === y);
            const isFood = food.x === x && food.y === y;

            return (
              <div
                key={`${x}-${y}`}
                className={`${
                  isSnake ? "bg-black" : isFood ? "bg-red-500" : "bg-gray-200"
                }`}
                style={{
                  width: "20px",
                  height: "20px",
                }}
              />
            );
          })
        )}
      </div>
    </div>
  );
};

export default Game;
