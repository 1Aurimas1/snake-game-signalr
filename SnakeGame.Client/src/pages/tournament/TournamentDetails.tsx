import { useNavigate, useParams } from "react-router-dom";
import { UserRole, useAuth } from "../../hooks/useAuth";
import { useState } from "react";
import { TOURNAMENTS } from "../../shared/constants/Routes";
import { ErrorOrSuccess, TournamentDto } from "../../shared/interfaces";
import {
  BasicDetails,
  BasicDetailsField,
  Button,
  ContentWrapper,
  DeletionModal,
  ErrorOrSuccessMessage,
} from "../../components";

const TournamentDetails = () => {
  const { userId, tournamentId } = useParams();
  const endpoint = `/users/${userId}/tournaments/${tournamentId}`;

  const { userAuthData } = useAuth();

  const [toRefetchDetails, setToRefetchDetails] = useState(false);
  const navigate = useNavigate();

  const { authFetch } = useAuth();
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );

  const [openDelete, setOpenDelete] = useState(false);
  const handleOpenDelete = () => setOpenDelete(!openDelete);

  function renderTournamentDetails(tournament: TournamentDto): React.ReactNode {
    const nameValuePair: { [key: string]: any }[] = [
      { name: "Name", value: tournament.name },
      { name: "Organizer", value: tournament.organizer.userName },
      { name: "Start time", value: tournament.startTime },
      { name: "End time", value: tournament.endTime },
      { name: "Rounds", value: tournament.rounds.map((r) => r.index) },
      { name: "Current round", value: tournament.currentRound },
      { name: "Max participants", value: tournament.maxParticipants },
      {
        name: "Participants",
        value: tournament.participants.map((p) => p.userName),
      },
    ];

    return nameValuePair.map((p, idx) => (
      <BasicDetailsField key={idx} fieldName={p.name} fieldValue={p.value} />
    ));
  }

  async function joinTournament() {
    const { apiError } = await authFetch(endpoint, {
      method: "PATCH",
    });
    if (apiError) {
      if (!Array.isArray(apiError)) {
        setErrorOrSuccess({ error: apiError.errorMessage });
      }
    } else {
      setErrorOrSuccess({ success: "Successfully joined the tournament!" });
      setToRefetchDetails(true);
    }
  }

  async function deleteTournament() {
    const { apiError } = await authFetch(endpoint, {
      method: "DELETE",
    });
    if (apiError) {
      if (!Array.isArray(apiError)) {
        setErrorOrSuccess({ error: apiError.errorMessage });
      }
    } else {
      navigate(TOURNAMENTS, { replace: true });
    }
  }

  return (
    <ContentWrapper title="Tournament details">
      <ErrorOrSuccessMessage errorOrSuccess={errorOrSuccess} />
      <BasicDetails
        endpoint={endpoint}
        renderItem={renderTournamentDetails}
        refetchTrigger={toRefetchDetails}
        setRefetchTrigger={setToRefetchDetails}
      />
      <div className="mt-8">
        <Button className="mr-2" type="button" onClick={joinTournament}>
          Join
        </Button>

        <DeletionModal
          open={openDelete}
          handleOpen={handleOpenDelete}
          onSubmit={deleteTournament}
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

export default TournamentDetails;
