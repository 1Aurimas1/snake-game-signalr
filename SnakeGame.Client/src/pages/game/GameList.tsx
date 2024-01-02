import { GAME_CREATION } from "../../shared/constants/Routes";
import { useAuth } from "../../hooks/useAuth";
import { BasicTable, ContentWrapper, CustomLink } from "../../components";
import { GameDto } from "../../shared/interfaces";

const GameList = () => {
  const endpoint = "/games";
  const { userAuthData } = useAuth();

  function renderHeader(thClasses: string): React.ReactNode {
    return (
      <>
        <th className={thClasses}>Creator</th>
        <th className={thClasses}>Name</th>
        <th className={thClasses}>Is open</th>
      </>
    );
  }

  function renderBody(game: GameDto, tdClasses: string): React.ReactNode {
    return (
      <>
        <td className={tdClasses}>{game.creator.userName}</td>
        <td className={tdClasses}>{game.name}</td>
        <td className={tdClasses + " text-gray-500"}>
          {game.isOpen ? "✔" : "✖"}
        </td>
      </>
    );
  }

  function getDetailsRoute(game: GameDto): string {
    return `/users/${game.creator.id}/games/${game.id}`;
  }

  return (
    <ContentWrapper title="Games">
      <BasicTable
        endpoint={endpoint}
        renderHeader={renderHeader}
        renderBody={renderBody}
        getDetailsRoute={getDetailsRoute}
      />
      {userAuthData && (
        <CustomLink to={GAME_CREATION} hasButtonStyle={true}>
          Create game
        </CustomLink>
      )}
    </ContentWrapper>
  );
};

export default GameList;
