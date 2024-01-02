import { useNavigate, useParams } from "react-router-dom";
import { useState } from "react";
import { useAuth } from "../../hooks/useAuth";
import getGameModeKey from "../../utils/getGameModeKey";
import { GAMES } from "../../shared/constants/Routes";
import {
  BasicDetails,
  BasicDetailsField,
  Button,
  ContentWrapper,
  DeletionModal,
  ErrorOrSuccessMessage,
} from "../../components";
import { ErrorOrSuccess, GameDto } from "../../shared/interfaces";

const GameDetails = () => {
  const { userId, gameId } = useParams();
  const endpoint = `/users/${userId}/games/${gameId}`;

  const [toRefetchDetails, setToRefetchDetails] = useState(false);
  const navigate = useNavigate();

  const { authFetch } = useAuth();
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );

  const [openDelete, setOpenDelete] = useState(false);
  const handleOpenDelete = () => setOpenDelete(!openDelete);

  function renderGame(game: GameDto): React.ReactNode {
    const nameValuePair: { [key: string]: any }[] = [
      { name: "Name", value: game.name },
      { name: "Creator", value: game.creator.userName },
      { name: "Mode", value: getGameModeKey(game.mode) },
      { name: "Map id", value: game.mapId },
      { name: "Is open", value: game.isOpen },
      { name: "Players", value: game.players.map((p) => p.userName) },
    ];

    return nameValuePair.map((p, idx) => (
      <BasicDetailsField key={idx} fieldName={p.name} fieldValue={p.value} />
    ));
  }

  async function joinGame() {
    const { apiError } = await authFetch(endpoint, {
      method: "PATCH",
    });
    if (apiError) {
      if (!Array.isArray(apiError)) {
        setErrorOrSuccess({ error: apiError.errorMessage });
      }
    } else {
      setErrorOrSuccess({ success: "Successfully joined the game!" });
      setToRefetchDetails(true);
    }
  }

  async function deleteGame() {
    const { apiError } = await authFetch(endpoint, {
      method: "DELETE",
    });
    if (apiError) {
      if (!Array.isArray(apiError)) {
        setErrorOrSuccess({ error: apiError.errorMessage });
      }
    } else {
      navigate(GAMES, { replace: true });
    }
  }

  return (
    <ContentWrapper title="Game details">
      <ErrorOrSuccessMessage errorOrSuccess={errorOrSuccess} />
      <BasicDetails
        endpoint={endpoint}
        renderItem={renderGame}
        refetchTrigger={toRefetchDetails}
        setRefetchTrigger={setToRefetchDetails}
      />

      <div className="mt-8">
        <Button className="mr-2" type="button" onClick={joinGame}>
          Join
        </Button>

        <DeletionModal
          open={openDelete}
          handleOpen={handleOpenDelete}
          onSubmit={deleteGame}
        />

        <Button type="button" onClick={handleOpenDelete}>
          Delete
        </Button>
      </div>
    </ContentWrapper>
  );
};

export default GameDetails;
