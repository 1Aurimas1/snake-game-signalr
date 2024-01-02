import { useNavigate, useParams } from "react-router-dom";
import { useState } from "react";
import { UserRole, useAuth } from "../../hooks/useAuth";
import { useForm } from "react-hook-form";
import {
  editEmailValidation,
  editUserNameValidation,
} from "../../utils/inputValidations";
import { USERS } from "../../shared/constants/Routes";
import {
  ApiErrorResponse,
  ErrorOrSuccess,
  PrivateUserDto,
} from "../../shared/interfaces";
import {
  BasicDetails,
  BasicDetailsField,
  Button,
  ContentWrapper,
  DeletionModal,
  FormModal,
  Input,
} from "../../components";

interface UpdateUserDto {
  userName: string;
  email: string;
}

const UserDetails = () => {
  const { userId } = useParams();
  const endpoint = `/users/${userId}`;

  const [toRefetchDetails, setToRefetchDetails] = useState(false);
  const navigate = useNavigate();

  const { userAuthData, authFetch } = useAuth();
  const [errorOrSuccess, setErrorOrSuccess] = useState<ErrorOrSuccess | null>(
    null,
  );
  const [apiError, setApiError] = useState<ApiErrorResponse | null>(null);

  const [openUpdate, setOpenUpdate] = useState(false);
  const [openDelete, setOpenDelete] = useState(false);

  const methods = useForm<UpdateUserDto>();

  function handleOpenUpdate() {
    setOpenUpdate(!openUpdate);
    setErrorOrSuccess(null);
  }

  const handleOpenDelete = () => setOpenDelete(!openDelete);

  function renderUser(user: PrivateUserDto): React.ReactNode {
    const nameValuePair: { [key: string]: any }[] = [
      { name: "Id", value: user.id },
      { name: "Name", value: user.userName },
      { name: "Email", value: user.email },
    ];

    return nameValuePair.map((p, idx) => (
      <BasicDetailsField key={idx} fieldName={p.name} fieldValue={p.value} />
    ));
  }

  async function updateUser(updateUserDto: UpdateUserDto) {
    const { apiError } = await authFetch(
      endpoint,
      {
        method: "PATCH",
      },
      updateUserDto,
    );
    if (apiError) {
      setApiError(apiError);
    } else {
      setErrorOrSuccess({ success: "Successfully updated details!" });
      setToRefetchDetails(true);
    }
  }

  async function deleteUser() {
    const { apiError } = await authFetch(endpoint, {
      method: "DELETE",
    });
    if (apiError) {
      setApiError(apiError);
    } else {
      navigate(USERS, { replace: true });
    }
  }

  return (
    <ContentWrapper title="User details">
      <BasicDetails
        endpoint={endpoint}
        renderItem={renderUser}
        refetchTrigger={toRefetchDetails}
        setRefetchTrigger={setToRefetchDetails}
      />

      <div className="mt-8">
        <FormModal
          open={openUpdate}
          handleOpen={handleOpenUpdate}
          methods={methods}
          header="Edit"
          onSubmit={updateUser}
          errorOrSuccess={errorOrSuccess}
        >
          <Input {...editUserNameValidation} apiErrorResponse={apiError} />
          <Input {...editEmailValidation} apiErrorResponse={apiError} />
        </FormModal>

        {userAuthData?.id === userId && (
          <Button className="mr-2" type="button" onClick={handleOpenUpdate}>
            Edit
          </Button>
        )}

        <DeletionModal
          open={openDelete}
          handleOpen={handleOpenDelete}
          onSubmit={deleteUser}
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

export default UserDetails;
