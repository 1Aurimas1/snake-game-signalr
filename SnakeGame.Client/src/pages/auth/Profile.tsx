import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { USERS } from "../../shared/constants/Routes";
import { useEffect } from "react";

export const Profile = () => {
  const { userAuthData } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    navigate(`${USERS}/${userAuthData?.id}`, { replace: true });
  }, []);

  return <></>;
};

export default Profile;
