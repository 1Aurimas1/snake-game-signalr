import { useEffect, useState } from "react";
import { useAuth } from "../../hooks/useAuth";
import { useNavigate } from "react-router-dom";

interface Props<T> {
  endpoint: string;
  refetchTrigger: boolean;
  setRefetchTrigger: React.Dispatch<React.SetStateAction<boolean>>;
  setFetchData?: React.Dispatch<React.SetStateAction<T | undefined>>;
  renderItem: (item: T) => React.ReactNode;
  children?: React.ReactNode;
}

const BasicDetails = <T extends {}>(props: Props<T>) => {
  const { authFetch } = useAuth();
  const [item, setItem] = useState<T | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      const { apiData, apiError, httpStatusCode } = await authFetch(
        props.endpoint,
        {
          method: "GET",
        },
      );
      if (apiError) {
        console.error(apiError);
        if (httpStatusCode === 404) {
          navigate("/404", { replace: true });
        }
      } else {
        setItem(apiData);
        if (props.setFetchData) {
          props.setFetchData(apiData);
        }
      }
    };
    fetchData();
    props.setRefetchTrigger(false);
  }, [props.refetchTrigger]);

  return (
    <div className="border border-gray-300 bg-gray-100 px-4 py-2 shadow-lg">
      {item && props.renderItem(item)}
      {props.children}
    </div>
  );
};

export default BasicDetails;
