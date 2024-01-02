import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { mapNameValidation } from "../../utils/inputValidations";
import { useAuth } from "../../hooks/useAuth";
import {
  ApiErrorResponse,
  ErrorOrSuccess,
  GameObstacle,
  ObstacleDto,
  Point,
} from "../../shared/interfaces";
import { Button, Form, Grid, Input } from "../../components";
import { transformToGameObstacle } from "../../utils/transformUtils";

interface CreateMapObstacleDto {
  obstacleId: number;
  position: Point;
}

interface CreateMapDto {
  name: string;
  mapObstacles: CreateMapObstacleDto[];
}

const MapCreation = () => {
  const { authFetch } = useAuth();
  const methods = useForm<CreateMapDto>();
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );

  const mapObstacles = useRef<CreateMapObstacleDto[]>([]);
  const [obstacles, setObstacles] = useState<ObstacleDto[]>([]);
  const [selectedObstacleId, setSelectedObstacleId] = useState<number>();
  const [gameObstacles, setGameObstacles] = useState<GameObstacle[]>([]);

  useEffect(() => {
    async function fetchData() {
      const response = await authFetch("/obstacles", { method: "GET" });

      if (response.apiError) {
        console.error(response.apiError);
        setApiError(response.apiError);
      } else {
        const obstacleDtos = response.apiData as ObstacleDto[];
        setObstacles(obstacleDtos);
      }
    }
    fetchData();
  }, []);

  function handleCellClick(x: number, y: number) {
    if (!selectedObstacleId) {
      setErrorOrSuccess({ error: "No obstacle was selected" });
      return;
    }

    const selectedObstacle = obstacles.find((o) => o.id == selectedObstacleId);
    if (!selectedObstacle) {
      setErrorOrSuccess({ error: "Selected obstacle was not found" });
      return;
    } else {
      setErrorOrSuccess(null);
    }

    setGameObstacles([
      ...gameObstacles,
      transformToGameObstacle(selectedObstacle, { X: x, Y: y }),
    ]);

    mapObstacles.current.push({
      obstacleId: selectedObstacleId,
      position: { X: x, Y: y },
    });
  }

  function resetInputs() {
    methods.reset();
    setGameObstacles([]);
  }

  function handleObstacleSelection(id: number) {
    setSelectedObstacleId(id);
  }

  async function onSubmit(createMapDto: CreateMapDto) {
    createMapDto.mapObstacles = mapObstacles.current;

    const { apiError } = await authFetch(
      "/maps",
      { method: "POST" },
      createMapDto,
    );

    if (apiError) {
      setApiError(apiError);
    } else {
      resetInputs();
      setErrorOrSuccess({ success: "Map submitted successfully" });
    }
  }

  return (
    <div className="mr-32 flex flex-col items-center justify-center gap-5">
      <h1 className="mb-7 text-2xl font-semibold">Map creator</h1>
      <Form
        methods={methods}
        onSubmit={onSubmit}
        errorOrSuccess={errorOrSuccess}
      >
        <Input {...mapNameValidation} apiErrorResponse={apiError} />

        <div className="ml-[11rem] flex flex-row items-center justify-center gap-16">
          <Grid
            gameObjects={gameObstacles}
            onCellClick={handleCellClick}
            cellClassName="hover:bg-red-300"
            cols={12}
          />

          <div>
            <h1 className="text-center font-semibold">Select obstacle</h1>
            <ul>
              {obstacles.map((o, idx) => (
                <li
                  key={idx}
                  className={`m-5 p-1 hover:bg-red-300 ${
                    o.id === selectedObstacleId && "bg-red-500"
                  }`}
                >
                  <button
                    type="button"
                    onClick={() => handleObstacleSelection(o.id)}
                  >
                    <Grid
                      gameObjects={transformToGameObstacle(o, { X: 1, Y: 1 })}
                      cols={3}
                    />
                  </button>
                </li>
              ))}
            </ul>
          </div>
        </div>

        <Button type="submit">Submit Map</Button>
      </Form>
    </div>
  );
};

export default MapCreation;
