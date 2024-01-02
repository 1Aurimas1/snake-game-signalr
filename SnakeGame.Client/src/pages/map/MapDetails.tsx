import { useNavigate, useParams } from "react-router-dom";
import { UserRole, useAuth } from "../../hooks/useAuth";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { mapRatingValidation } from "../../utils/inputValidations";
import { MAPS } from "../../shared/constants/Routes";
import {
  ApiErrorResponse,
  ErrorOrSuccess,
  GameObstacle,
  MapDto,
  ObstacleDto,
  PointDto,
} from "../../shared/interfaces";
import {
  BasicDetails,
  BasicDetailsField,
  Button,
  ContentWrapper,
  DeletionModal,
  ErrorOrSuccessMessage,
  FormModal,
  Grid,
  Input,
} from "../../components";
import { transformToGameObstacle } from "../../utils/transformUtils";

interface UpdateMapDto {
  mapRating: number;
}

const MapDetails = () => {
  const { userId, mapId } = useParams();
  const endpoint = `/users/${userId}/maps/${mapId}`;
  const publishEndpoint = `${endpoint}/status`;
  const obstacleEndpoint = "/obstacles";

  const { userAuthData, authFetch } = useAuth();

  const [toRefetchDetails, setToRefetchDetails] = useState(false);
  const [mapData, setMapData] = useState<MapDto>();
  const [gameObstacles, setGameObstacles] = useState<GameObstacle[]>([]);
  const navigate = useNavigate();

  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);

  const [openUpdate, setOpenUpdate] = useState(false);
  const [openDelete, setOpenDelete] = useState(false);
  const handleOpenDelete = () => setOpenDelete(!openDelete);

  const methods = useForm<UpdateMapDto>();

  function handleOpenUpdate() {
    setOpenUpdate(!openUpdate);
    setErrorOrSuccess(null);
  }

  useEffect(() => {
    async function getObstaclesByIds(ids: number[]) {
      const params = new URLSearchParams();
      ids.forEach((id) => params.append("ids", id.toString()));

      const obstacleIdsEndpoint = `${obstacleEndpoint}?${params.toString()}`;

      const { apiData, apiError } = await authFetch(obstacleIdsEndpoint, {
        method: "GET",
      });
      if (apiError) {
        console.error(apiError);
      } else {
        const obstacles = apiData as ObstacleDto[];
        for (const obstacle of obstacles) {
          const positions: PointDto[] = [];
          for (const mapObstacle of mapData!.mapObstacles) {
            if (mapObstacle.obstacleId === obstacle.id) {
              positions.push(mapObstacle.position);
            }
          }
          gameObstacles.push(transformToGameObstacle(obstacle, positions));
        }

        setGameObstacles([...gameObstacles]);
      }
    }

    if (mapData) {
      const uniqueObstacleIds = Array.from(
        new Set(mapData.mapObstacles.map((o) => o.obstacleId)),
      );
      getObstaclesByIds(uniqueObstacleIds);
    }
  }, [mapData]);

  function renderMapDetails(map: MapDto): React.ReactNode {
    const nameValuePair: { [key: string]: any }[] = [
      { name: "Name", value: map.name },
      { name: "Creator", value: map.creator.userName },
      { name: "Rating", value: map.rating },
      {
        name: "Obstacle ids",
        value: map.mapObstacles.map((o) => o.mapObstacleId),
      },
    ];

    return nameValuePair.map((p, idx) => (
      <BasicDetailsField key={idx} fieldName={p.name} fieldValue={p.value} />
    ));
  }

  async function rateMap(updateMapDto: UpdateMapDto) {
    const { apiError } = await authFetch(
      endpoint,
      {
        method: "PATCH",
      },
      updateMapDto,
    );
    if (apiError) {
      setApiError(apiError);
    } else {
      setErrorOrSuccess({ success: "Successfully rated the map!" });
      setToRefetchDetails(true);
    }
  }

  async function publishMap() {
    const { apiError } = await authFetch(publishEndpoint, {
      method: "PATCH",
    });
    if (apiError) {
      setApiError(apiError);
    } else {
      setErrorOrSuccess({ success: "Successfully published the map!" });
      setToRefetchDetails(true);
    }
  }

  async function deleteMap() {
    const { apiError } = await authFetch(endpoint, {
      method: "DELETE",
    });
    if (apiError) {
      setApiError(apiError);
    } else {
      navigate(MAPS, { replace: true });
    }
  }

  return (
    <ContentWrapper title="Map details">
      <ErrorOrSuccessMessage errorOrSuccess={errorOrSuccess} />
      <BasicDetails
        endpoint={endpoint}
        renderItem={renderMapDetails}
        refetchTrigger={toRefetchDetails}
        setRefetchTrigger={setToRefetchDetails}
        setFetchData={setMapData}
      >
        <Grid
          cols={12}
          gameObjects={gameObstacles}
          className="m-1 bg-white p-1 drop-shadow-lg"
        />
      </BasicDetails>
      <div className="mt-8">
        <FormModal
          open={openUpdate}
          handleOpen={handleOpenUpdate}
          header="Rate Map"
          methods={methods}
          onSubmit={rateMap}
          errorOrSuccess={errorOrSuccess}
        >
          <Input {...mapRatingValidation} apiErrorResponse={apiError} />
        </FormModal>

        {mapData?.isPublished ? (
          <Button className="mr-2" type="button" onClick={handleOpenUpdate}>
            Add rating
          </Button>
        ) : (
          userAuthData?.roles.includes(UserRole.Admin) && (
            <Button className="mr-2" type="button" onClick={publishMap}>
              Publish
            </Button>
          )
        )}

        <DeletionModal
          open={openDelete}
          handleOpen={handleOpenDelete}
          onSubmit={deleteMap}
        />

        {userAuthData?.roles.includes(UserRole.Admin) && (
          <Button type="button" onClick={handleOpenDelete}>
            Delete
          </Button>
        )}
      </div>
    </ContentWrapper>
  );
};

export default MapDetails;
