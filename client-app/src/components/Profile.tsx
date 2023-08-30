import { useEffect, useState } from "react";
import { useAuth } from "../hooks/useAuth";
import Spinner from "./Spinner";
import Stat from "./Stat";

interface ProfileDto {
  wins: number;
  losses: number;
  highscore: number;
}

export const Profile = () => {
  const { user } = useAuth();
  const [profile, setProfile] = useState<ProfileDto>();

  async function getProfileInfo() {
    const headers = new Headers();
    headers.append("Content-Type", "application/json");
    if (user) headers.append("Authorization", `Bearer ${user}`);

    return fetch("/api/profile", {
      method: "GET",
      headers: headers,
    })
      .then(async (res) => {
        if (!res.ok) {
          const err = await res.text();
          return await Promise.reject(err);
        }

        return res.json();
      })
      .then((data: ProfileDto) => {
        return data;
      })
      .catch((e) => {
        console.error("User profile fetch error: ", e);
        return null;
      });
  }

  useEffect(() => {
    const fetchData = async () => {
      const response = await getProfileInfo();
      if (response != null) {
        setProfile(response);
      }
    };

    fetchData();
  }, []);

  return (
    <div className="flex h-screen items-center justify-center">
      <div className="flex flex-col items-center justify-center border border-black bg-gray-200 p-5">
        <h1 className="mx-5 mb-5 text-xl font-bold">Playtime statistics</h1>
        {profile ? (
          <div>
            <Stat name={"Wins:"} value={profile.wins} />
            <Stat name={"Losses:"} value={profile.losses} />
            <Stat name={"Highscore:"} value={profile.highscore} />
          </div>
        ) : (
          <Spinner text="Loading..." />
        )}
      </div>
    </div>
  );
};

export default Profile;
