import { BasicTable, ContentWrapper } from "../../components";
import { PrivateUserDto } from "../../shared/interfaces";

const UserList = () => {
  const endpoint = "/users";

  function renderHeader(thClasses: string): React.ReactNode {
    return (
      <>
        <th className={thClasses}>Id</th>
        <th className={thClasses}>Name</th>
        <th className={thClasses}>Email</th>
      </>
    );
  }

  function renderBody(
    user: PrivateUserDto,
    tdClasses: string,
  ): React.ReactNode {
    return (
      <>
        <td className={tdClasses}>{user.id}</td>
        <td className={tdClasses}>{user.userName}</td>
        <td className={tdClasses}>{user.email}</td>
      </>
    );
  }

  function getDetailsRoute(user: PrivateUserDto): string {
    return `/users/${user.id}`;
  }

  return (
    <ContentWrapper title="Users">
      <BasicTable
        endpoint={endpoint}
        renderHeader={renderHeader}
        renderBody={renderBody}
        getDetailsRoute={getDetailsRoute}
      />
    </ContentWrapper>
  );
};

export default UserList;
