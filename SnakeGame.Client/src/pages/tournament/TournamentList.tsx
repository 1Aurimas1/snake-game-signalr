import { TOURNAMENT_CREATION } from "../../shared/constants/Routes";
import { getFormattedDate } from "../../utils/getFormattedDate";
import { UserRole, useAuth } from "../../hooks/useAuth";
import { TournamentDto } from "../../shared/interfaces";
import { BasicTable, ContentWrapper, CustomLink } from "../../components";

const TournamentList = () => {
  const endpoint = "/tournaments";
  const { userAuthData } = useAuth();

  function renderHeader(thClasses: string): React.ReactNode {
    return (
      <>
        <th className={thClasses}>Organizer</th>
        <th className={thClasses}>Name</th>
        <th className={thClasses}>Start date</th>
      </>
    );
  }

  function renderBody(
    tournament: TournamentDto,
    tdClasses: string,
  ): React.ReactNode {
    const formattedStartTime = getFormattedDate(tournament.startTime);

    return (
      <>
        <td className={tdClasses}>{tournament.organizer.userName}</td>
        <td className={tdClasses}>{tournament.name}</td>
        <td className={tdClasses}>{formattedStartTime}</td>
      </>
    );
  }

  function getDetailsRoute(tournament: TournamentDto): string {
    return `/users/${tournament.organizer.id}/tournaments/${tournament.id}`;
  }

  return (
    <ContentWrapper title="Tournaments">
      <BasicTable
        endpoint={endpoint}
        renderHeader={renderHeader}
        renderBody={renderBody}
        getDetailsRoute={getDetailsRoute}
      />

      {userAuthData?.roles.includes(UserRole.Admin) && (
        <CustomLink to={TOURNAMENT_CREATION} hasButtonStyle={true}>
          Create tournament
        </CustomLink>
      )}
    </ContentWrapper>
  );
};

export default TournamentList;
