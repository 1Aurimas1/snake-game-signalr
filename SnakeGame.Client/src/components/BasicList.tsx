import { useEffect, useState } from "react";
import { useAuth } from "../hooks/useAuth";

interface Props<T> {
  endpoint: string;
  renderItem: (item: T) => React.ReactNode;
}

const BasicList = <T extends {}>(props: Props<T>) => {
  const { authFetch } = useAuth();
  const [items, setItems] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      const { apiData, apiError } = await authFetch(props.endpoint, {
        method: "GET",
      });
      if (apiError) {
        console.error(apiError);
      } else {
        if (apiData) {
          setItems(apiData);
        }
      }
    };
    fetchData();
  }, []);

  return (
    <ul>
      {items.length > 0 ? (
        items.map((item, idx) => (
          <li
            className="animate-fadeIn opacity-0"
            style={{ "--delay": idx * 0.50 + "s" } as any}
            key={idx}
          >
            {props.renderItem(item)}
          </li>
        ))
      ) : (
        <p>No items to display</p>
      )}
    </ul>
  );
};

export default BasicList;
