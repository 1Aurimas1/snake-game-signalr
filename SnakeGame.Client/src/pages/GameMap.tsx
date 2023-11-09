import { useRef, useState } from "react";
import Button from "../components/Button";
import { ServerError } from "../shared/interfaces/ServerError";
import { FormProvider, useForm } from "react-hook-form";
import { mapNameValidation } from "../utils/inputValidations";
import Input from "../components/Input";
import { useAuth } from "../hooks/useAuth";
import { CellType, GridDimensions, paintCell } from "../components/Game";

interface Point {
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

interface FormInput {
  mapName: string;
  placedObstacles: Map<number, Point[]>;
}

// TODO: reset map
const GameMap = () => {
  const { userToken } = useAuth();
  // TODO: grid -> gridDimensions
  const [gridDims, setGridDims] = useState<GridDimensions>({ rows: 12, columns: 12 });
  const [cells, setCells] = useState<number[][]>(
    Array.from({ length: gridDims.rows }, (_, idx) => idx).map((y, _) =>
      Array.from({ length: gridDims.columns }).map((_, x) => {
        return CellType.EMPTY;
      }),
    ),
  );
  const [selectedObstacleIndex, setSelectedObstacleIndex] = useState(0);
  const placedUniqueObstacles = useRef<Map<number, Point[]>>(new Map());

  const methods = useForm<FormInput>();
  const [serverErrors, setServerErrors] = useState<ServerError[]>([]);

  const handleCellClick = (x: number, y: number) => {
    // TODO: handle 0 obstacles
    for (const o of obstacles[selectedObstacleIndex].cells) {
      const obstacleCellOnMap: Point = { X: o.X + x, Y: o.Y + y };
      if (
        obstacleCellOnMap.X < gridDims.columns &&
        obstacleCellOnMap.Y < gridDims.rows &&
        obstacleCellOnMap.X >= 0 &&
        obstacleCellOnMap.Y >= 0
      ) {
        cells[obstacleCellOnMap.Y][obstacleCellOnMap.X] = CellType.OBSTACLE;
      }
    }

    setCells([...cells]);
    if (placedUniqueObstacles.current.has(selectedObstacleIndex)) {
      placedUniqueObstacles.current
        .get(selectedObstacleIndex)
        ?.push({ X: x, Y: y });
    } else {
      placedUniqueObstacles.current.set(selectedObstacleIndex, [
        { X: x, Y: y },
      ]);
    }
  };

  async function submitMap(formInput: FormInput) {
    const headers = new Headers();
    headers.append("Content-Type", "application/json");
    if (userToken) headers.append("Authorization", `Bearer ${userToken}`);

    return await fetch("/api/map", {
      method: "POST",
      headers: headers,
      body: JSON.stringify(formInput),
    })
      .then(async (res) => {
        if (!res.ok) {
          const err = await res.json();

          const errs: ServerError[] = [];
          for (const field in err) {
            const messages = err[field];

            for (const message of messages) {
              errs.push({ field, message });
              break; // only single message per field is displayed
            }
          }
          setServerErrors(errs);

          return await Promise.reject(err);
        }

        return res.text();
      })
      .then((data) => {
        return data;
      })
      .catch((e) => {
        console.error("User login fetch error: ", e);
        return null;
      });
  }

  const handleObstacleSelection = (idx: number) => {
    setSelectedObstacleIndex(idx);
  };

  const onSubmit = methods.handleSubmit(async (formInput) => {
    formInput.placedObstacles = placedUniqueObstacles.current;
    const response = await submitMap(formInput);
  });

  // TODO: refactor grid into separate component
  return (
    <div className="m-10 flex flex-row items-center justify-center gap-5">
      <div className="m-10 flex flex-col items-center justify-center gap-5">
        <h1 className="text-2xl font-semibold">Map creation</h1>
        <FormProvider {...methods}>
          <form
            onSubmit={(e) => e.preventDefault()}
            noValidate
            autoComplete="off"
            className="m-10 flex flex-col items-center justify-center gap-5"
          >
            <Input {...mapNameValidation} serverError={serverErrors} />
            <div
              className={`grid max-h-[400px] max-w-[800px] grid-cols-12 gap-1`}
            >
              {cells.map((row, y) =>
                row.map((cellType, x) => {
                  const cellColor = paintCell(cellType);

                  return (
                    <button
                      onClick={() => handleCellClick(x, y)}
                      key={`${x}-${y}`}
                      className={`hover:bg-red-300 ${cellColor}`}
                      style={{
                        width: "20px",
                        height: "20px",
                      }}
                    />
                  );
                }),
              )}
            </div>
            <Button onClick={onSubmit}>Submit Map</Button>
          </form>
        </FormProvider>
      </div>

      <div>
        <h1>Choose obstacle</h1>
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
                className={`grid max-h-[75px] max-w-[67px] grid-cols-3 gap-1`}
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
  );
};

export default GameMap;
