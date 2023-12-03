import { useEffect, useRef, useState } from "react";
import Button from "../components/Button";
import { ApiErrorResponse } from "../shared/interfaces/ApiResponse";
import { FormProvider, useForm } from "react-hook-form";
import { mapNameValidation } from "../utils/inputValidations";
import Input from "../components/Input";
import { useAuth } from "../hooks/useAuth";
import { CellType, paintCell } from "../components/Game";
import { Grid } from "../shared/interfaces/Grid";

interface Vector2 {
  X: number;
  Y: number;
}

interface Cell {
  X: number;
  Y: number;
  type: CellType;
}

interface Obstacle {
  cells: Cell[];
}

// TODO: fetch available obstacles from the server?
const obstacles: Obstacle[] = [
  {
    cells: [
      { X: 0, Y: 0, type: CellType.OBSTACLE },
      { X: 1, Y: 0, type: CellType.OBSTACLE },
    ],
  },
  {
    cells: [
      { X: -1, Y: -1, type: CellType.OBSTACLE },
      { X: 0, Y: 0, type: CellType.OBSTACLE },
      { X: 1, Y: 1, type: CellType.OBSTACLE },
    ],
  },
  {
    cells: [
      { X: 0, Y: -1, type: CellType.OBSTACLE },
      { X: -1, Y: 0, type: CellType.OBSTACLE },
      { X: 0, Y: 0, type: CellType.OBSTACLE },
      { X: 1, Y: 0, type: CellType.OBSTACLE },
      { X: 0, Y: 1, type: CellType.OBSTACLE },
    ],
  },
];

interface MapObstacle {
  obstacleId: number;
  position: Vector2;
}

//interface MapObstacleDto {
//  obstacleId: number;
//  position: Point;
//}

interface MapDto {
  name: string;
  mapObstacleDtos: MapObstacle[];
}

const grid: Grid = { rows: 12, columns: 12 };

// TODO: reset map functionality
const GameMapCreation = () => {
  const { authFetch } = useAuth();
  const [cells, setCells] = useState<number[][]>(
    Array.from({ length: grid.rows }, (_, idx) => idx).map((y, _) =>
      Array.from({ length: grid.columns }).map((_, x) => {
        return CellType.EMPTY;
      }),
    ),
  );
  const [selectedObstacleIndex, setSelectedObstacleIndex] = useState(0);
  const mapObstacles = useRef<MapObstacle[]>([]);

  const methods = useForm<MapDto>();
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);

  useEffect(() => {
      async function fetchData() {
          const response = await authFetch("/obstacles", { method: "GET" })
          console.log(response);
      }
      fetchData();
  }, []);

  function handleCellClick(x: number, y: number) {
    // TODO: handle 0 obstacles
    for (const o of obstacles[selectedObstacleIndex].cells) {
      const obstacleCellOnMap: Vector2 = { X: o.X + x, Y: o.Y + y };
      if (
        obstacleCellOnMap.X < grid.columns &&
        obstacleCellOnMap.Y < grid.rows &&
        obstacleCellOnMap.X >= 0 &&
        obstacleCellOnMap.Y >= 0
      ) {
        cells[obstacleCellOnMap.Y][obstacleCellOnMap.X] = CellType.OBSTACLE;
      }
    }

    setCells([...cells]);
    console.log(x, y);
    mapObstacles.current.push({
      obstacleId: selectedObstacleIndex,
      position: { X: x, Y: y },
    });
  }

  async function submitMap(mapDto: MapDto) {
    const { apiData, apiError } = await authFetch(
      "/maps",
      { method: "POST" },
      mapDto,
    );

    if (apiError) {
      setApiError(apiError);
    } else {
      console.log(apiData);
    }
  }

  const handleObstacleSelection = (idx: number) => {
    setSelectedObstacleIndex(idx);
  };

  const onSubmit = methods.handleSubmit(async (mapDto) => {
    mapDto.mapObstacleDtos = mapObstacles.current;
    const response = await submitMap(mapDto);
  });

  // TODO: refactor grid into separate component
  return (
    <div className="mr-32 flex flex-col items-center justify-center gap-5">
      <h1 className="mb-7 text-2xl font-semibold">Map creator</h1>
      <FormProvider {...methods}>
        <form
          onSubmit={(e) => e.preventDefault()}
          noValidate
          autoComplete="off"
          className="flex flex-col items-center justify-center gap-5"
        >
          <Input {...mapNameValidation} apiErrorResponse={apiError} />
          <div className="flex flex-row items-center justify-center gap-16">
            <div className="ml-[11rem] grid max-h-[400px] min-w-[285px] grid-cols-12 gap-1">
              {cells.map((row, y) =>
                row.map((cellType, x) => (
                  <button
                    onClick={() => handleCellClick(x, y)}
                    key={`${x}-${y}`}
                    className={`hover:bg-red-300 ${paintCell(cellType)}`}
                    style={{
                      width: "20px",
                      height: "20px",
                    }}
                  />
                )),
              )}
            </div>

            <div>
              <h1 className="text-center font-semibold">Select obstacle</h1>
              <ul>
                {obstacles.map((o, idx) => (
                  <li
                    key={idx}
                    className={`m-5 p-1 hover:bg-red-300 ${
                      selectedObstacleIndex === idx && "bg-red-500"
                    }`}
                  >
                    <button
                      onClick={() => handleObstacleSelection(idx)}
                      className="grid max-h-[75px] min-w-[67px] grid-cols-3 gap-1"
                    >
                      {Array.from({ length: 3 }).map((_, y) =>
                        Array.from({ length: 3 }).map((_, x) => {
                          let cellType: CellType = CellType.EMPTY;
                          for (const cell of o.cells) {
                            // cell point relative to grid center
                            if (cell.X + 1 === x && cell.Y + 1 === y) {
                              cellType = cell.type;
                            }
                          }
                          const cellColor = paintCell(cellType);

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
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          </div>
          <Button onClick={onSubmit}>Submit Map</Button>
        </form>
      </FormProvider>
    </div>
  );
};

export default GameMapCreation;
