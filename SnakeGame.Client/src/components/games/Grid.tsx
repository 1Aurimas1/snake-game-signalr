import { useEffect, useState } from "react";
import { CellType } from "../../shared/constants/CellType";
import { GameObstacle, GridSize, Point } from "../../shared/interfaces";
import { paintCell } from "../../utils/paintCell";
import Cell from "./Cell";

const gridSizeDefaults: GridSize = { cols: 12, rows: 12 };

type Food = Point;
type Snake = Point[];
type GameObject = Food | Snake | GameObstacle;

function isPoint(obj: any): obj is Point {
  const point = obj as Point;
  return point.X !== undefined && point.Y !== undefined;
}

function isSnake(obj: GameObject): obj is Snake {
  const snake = obj as Snake;
  return Array.isArray(snake) && isPoint(snake[0]);
}

function isFood(obj: GameObject): obj is Food {
  const food = obj as Food;
  return !Array.isArray(food) && isPoint(food);
}

function isObstacles(obj: GameObject): obj is GameObstacle {
  const gameObstacle = obj as GameObstacle;
  return (
    gameObstacle.points !== undefined && gameObstacle.positions !== undefined
  );
}

function getColsClass(cols: number): string {
  switch (cols) {
    case 3:
      return "grid-cols-3";
    default:
      return "grid-cols-12";
  }
}

function generateEmptyGrid(
  cols = gridSizeDefaults.cols,
  rows = gridSizeDefaults.rows,
): CellType[][] {
  return Array.from({ length: rows }).map((_, _y) =>
    Array.from({ length: cols }).map((_, _x) => CellType.EMPTY),
  );
}

interface Props {
  cols: number;
  gameObjects?: GameObject[] | GameObject;
  onCellClick?: (x: number, y: number) => void;
  className?: string;
  cellClassName?: string;
}

const Grid = ({
  cols,
  gameObjects,
  onCellClick,
  className,
  cellClassName,
}: Props) => {
  const rows = cols;

  const [cells, setCells] = useState<CellType[][]>(
    generateEmptyGrid(cols, rows),
  );
  const [snakeTail, setSnakeTail] = useState<Point | null>(null);

  useEffect(() => {
    if (gameObjects) {
      if (Array.isArray(gameObjects)) {
        for (const gameObject of gameObjects) {
          handleGameObjectPlacement(gameObject);
        }
      } else {
        handleGameObjectPlacement(gameObjects);
      }

      function handleGameObjectPlacement(gameObject: GameObject) {
        if (isSnake(gameObject)) {
          placeSnake(gameObject);
        } else if (isFood(gameObject)) {
          placeFood(gameObject);
        } else if (isObstacles(gameObject)) {
          placeObstacles(gameObject);
        }

        setCells([...cells]);
      }
    }
  }, [gameObjects]);

  function invertYAxisCoord(point: Point): number {
    return gridSizeDefaults.rows - point.Y - 1;
  }

  function placeSnake(snake: Snake) {
    const head = snake[0];
    cells[invertYAxisCoord(head)][head.X] = CellType.SNAKE_HEAD;
    for (let i = 1; i < snake.length; i++) {
      const point = snake[i];
      cells[invertYAxisCoord(point)][point.X] = CellType.SNAKE_BODY;
    }

    const tail = snake[snake.length - 1];
    if (snakeTail) {
        cells[invertYAxisCoord(snakeTail)][snakeTail.X] = CellType.EMPTY;
    }
    setSnakeTail(tail);
  }

  function placeFood(food: Food) {
    cells[invertYAxisCoord(food)][food.X] = CellType.FOOD;
  }

  function isPointWithinGrid(point: Point) {
    return 0 <= point.X && point.X < cols && 0 <= point.Y && point.Y < rows;
  }

  function placeObstacles(obstacles: GameObstacle) {
    if (Array.isArray(obstacles)) {
      obstacles.forEach((o) => placeObstacle(o));
    } else {
      placeObstacle(obstacles);
    }

    function placeObstacle(obstacle: GameObstacle) {
      for (const position of obstacle.positions) {
        for (const point of obstacle.points) {
          const pointOnMap: Point = {
            X: point.X + position.X,
            Y: point.Y + position.Y,
          };
          if (isPointWithinGrid(pointOnMap)) {
            cells[pointOnMap.Y][pointOnMap.X] = CellType.OBSTACLE;
          }
        }
      }
    }
  }

  return (
    <div className={`grid ${getColsClass(cols)} min-w-max gap-1 ${className}`}>
      {cells.map((row, y) =>
        row.map((cellType, x) => {
          return (
            <Cell
              key={`${x}-${y}`}
              onClick={
                onCellClick
                  ? () => {
                      if (onCellClick) onCellClick(x, y);
                    }
                  : undefined
              }
              className={`${paintCell(cellType)} ${cellClassName}`}
            />
          );
        }),
      )}
    </div>
  );
};

export default Grid;
