import { useEffect, useState } from "react";
import { useAuth } from "../../hooks/useAuth";
import infoSVG from "../../assets/info.svg";
import { Link } from "react-router-dom";

interface Props<T> {
  endpoint: string;
  renderHeader: (thClasses: string) => React.ReactNode;
  renderBody: (item: T, tdClasses: string) => React.ReactNode;
  getDetailsRoute: (item: T) => string;
}

const thClasses =
  "text-gray-50 leading-none bg-gray-500 border-b border-gray-400 p-4";
const tdClasses = "p-4 border-gray-400 bg-gray-50";

const BasicTable = <T extends {}>(props: Props<T>) => {
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
  }, [props.endpoint]);

  return (
    <table className="table-fixed border-separate border-spacing-y-2 border-gray-400 text-left drop-shadow-lg">
      <thead>
        <tr>
          {props.renderHeader(thClasses)}
          <th className={thClasses}></th>
        </tr>
      </thead>
      <tbody>
        {items.length != 0 ? (
          items.map((item, idx) => (
            <tr
              className="animate-fadeIn opacity-0"
              style={{ "--delay": idx * 0.4 + "s" } as any}
              key={idx}
            >
              {props.renderBody(item, tdClasses)}
              <td className={tdClasses}>
                <Link to={props.getDetailsRoute(item)}>
                  <div className="h-7 w-7 animate-shrink hover:animate-grow">
                    <img src={infoSVG} alt="more info" />
                  </div>
                </Link>
              </td>
            </tr>
          ))
        ) : (
          <tr>
            <td
              colSpan={100}
              className="border-gray-400 bg-gray-50 p-4 text-center"
            >
              No items found
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
};

export default BasicTable;
